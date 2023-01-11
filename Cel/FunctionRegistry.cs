namespace Cel;

public enum ReceiverStyle
{
    Global,
    Receiver,
    Both
}

/// A singleton class for registering functions to be interpreted by CEL.
public sealed class FunctionRegistry
{
    private static string ValueTypeName(Type valueType) => valueType.Name;

    // Represents the types of the return type and arguments of a function.
    public record Signature
    {
        public record Nullary(Type Result) : Signature
        {
            public ArgumentList Arguments => new ArgumentList.Nullary();

            public override string ToString() => $"{Arguments} -> {ValueTypeName(Result)}";
        }

        public record Unary(Type T1, Type Result, ReceiverStyle Style = ReceiverStyle.Global)
            : Signature
        {
            public ArgumentList Arguments => new ArgumentList.Unary(T1, Style);

            public override string ToString() => $"{Arguments} -> {ValueTypeName(Result)}";
        }

        public record Binary(
            Type T1,
            Type T2,
            Type TResult,
            ReceiverStyle Style = ReceiverStyle.Global
        ) : Signature
        {
            public ArgumentList Arguments => new ArgumentList.Binary(T1, T2, Style);

            public override string ToString() => $"{Arguments} -> {ValueTypeName(TResult)}";
        }

        public record Ternary(
            Type T1,
            Type T2,
            Type T3,
            Type TResult,
            ReceiverStyle Style = ReceiverStyle.Global
        ) : Signature
        {
            public ArgumentList Arguments => new ArgumentList.Ternary(T1, T2, T3, Style);

            public override string ToString() => $"{Arguments} -> {ValueTypeName(TResult)}";
        }

        public static Signature FromList(Type result, List<Type> types) =>
            types.Count switch
            {
                0 => new Nullary(result),
                1 => new Unary(types[0], result),
                2 => new Binary(types[0], types[1], result),
                3 => new Ternary(types[0], types[1], types[2], result),
                _
                    => throw new ArgumentException(
                        "Functions of more than three arguments are not yet supported."
                    )
            };
    }

    // Represents the types of the arguments (but not the return type) of a function.
    public record ArgumentList
    {
        private static string ToReceiverStyleString(params Type[] types) =>
            $"{ValueTypeName(types[0])}.{ToGlobalStyleString(types.Skip(1).ToArray())}";

        private static string ToGlobalStyleString(params Type[] types) =>
            $"({string.Join(", ", types.Select(type => ValueTypeName(type)))})";

        public record Nullary : ArgumentList
        {
            public override string ToString() => "()";
        }

        public record Unary(Type T1, ReceiverStyle Style = ReceiverStyle.Global) : ArgumentList
        {
            public override string ToString() =>
                Style == ReceiverStyle.Global ? ToGlobalStyleString(T1) : ToReceiverStyleString(T1);
        }

        public record Binary(Type T1, Type T2, ReceiverStyle Style = ReceiverStyle.Global)
            : ArgumentList
        {
            public override string ToString() =>
                Style == ReceiverStyle.Global
                    ? ToGlobalStyleString(T1, T2)
                    : ToReceiverStyleString(T1, T2);
        }

        public record Ternary(Type T1, Type T2, Type T3, ReceiverStyle Style = ReceiverStyle.Global)
            : ArgumentList
        {
            public override string ToString() =>
                Style == ReceiverStyle.Global
                    ? ToGlobalStyleString(T1, T2, T3)
                    : ToReceiverStyleString(T1, T2, T3);
        }

        public static ArgumentList FromList(List<Type> types, ReceiverStyle style) =>
            types.Count switch
            {
                0 => new Nullary(),
                1 => new Unary(types[0], style),
                2 => new Binary(types[0], types[1], style),
                3 => new Ternary(types[0], types[1], types[2], style),
                _
                    => throw new ArgumentException(
                        "Functions of more than three arguments are not yet supported."
                    ),
            };
    }

    // _functions is a map from function name to a map of overloads.
    //
    // The map of overloads is keyed by argument types; the values are the actual functions to evaluate.
    //
    // Example (based on the example in the spec:
    // https://github.com/google/cel-spec/blob/master/doc/langdef.md#functions):
    //
    // {
    //   size: {
    //     [Value.StringValue]: <impl>,
    //     [Value.BytesValue]: <impl>,
    //     [Value.Values]: <impl>,
    //     [Value.Fields]: <impl>,
    //   }
    // }
    private readonly Dictionary<
        string,
        Dictionary<ArgumentList, (Signature, Func<IList<Value>, Value>)>
    > _Functions = new();

    private FunctionRegistry() { }

    private static readonly Lazy<FunctionRegistry> _Instance = new Lazy<FunctionRegistry>(() =>
    {
        var instance = new FunctionRegistry();
        BuiltInFunctions.Register(instance);
        return instance;
    });

    public static FunctionRegistry Instance => _Instance.Value;

    /// Register a function that takes no arguments.
    public void Register<TResult>(string name, Func<TResult> function)
        where TResult : Value
    {
        if (!_Functions.ContainsKey(name))
        {
            _Functions[name] =
                new Dictionary<ArgumentList, (Signature, Func<IList<Value>, Value>)>();
        }

        var signature = new Signature.Nullary(function.Method.ReturnType);

        if (_Functions[name].ContainsKey(signature.Arguments))
        {
            throw new ArgumentException(
                $"There is already a registered function named `{name}` taking arguments of types `{signature}`"
            );
        }

        _Functions[name][signature.Arguments] = (signature, _ => function());
    }

    /// Register a function that takes one argument.
    public void Register<T1, TResult>(
        string name,
        Func<T1, TResult> function,
        ReceiverStyle style = ReceiverStyle.Global
    )
        where TResult : Value
        where T1 : Value
    {
        if (style == ReceiverStyle.Both)
        {
            Register(name, function, ReceiverStyle.Global);
            Register(name, function, ReceiverStyle.Receiver);
            return;
        }

        if (!_Functions.ContainsKey(name))
        {
            _Functions[name] =
                new Dictionary<ArgumentList, (Signature, Func<IList<Value>, Value>)>();
        }

        var parameters = function.Method.GetParameters();
        var signature = new Signature.Unary(
            parameters[0].ParameterType,
            function.Method.ReturnType,
            Style: style
        );

        if (_Functions[name].ContainsKey(signature.Arguments))
        {
            throw new ArgumentException(
                $"There is already a registered function named `{name}` taking arguments of types `{signature}`"
            );
        }

        _Functions[name][signature.Arguments] = (signature, (args) => function((T1)args[0]));
    }

    /// Register a function that takes two arguments.
    public void Register<T1, T2, TResult>(
        string name,
        Func<T1, T2, TResult> function,
        ReceiverStyle style = ReceiverStyle.Global
    )
        where TResult : Value
        where T1 : Value
        where T2 : Value
    {
        if (style == ReceiverStyle.Both)
        {
            Register(name, function, ReceiverStyle.Global);
            Register(name, function, ReceiverStyle.Receiver);
            return;
        }

        if (!_Functions.ContainsKey(name))
        {
            _Functions[name] =
                new Dictionary<ArgumentList, (Signature, Func<IList<Value>, Value>)>();
        }

        var parameters = function.Method.GetParameters();
        var signature = new Signature.Binary(
            parameters[0].ParameterType,
            parameters[1].ParameterType,
            function.Method.ReturnType,
            Style: style
        );

        if (_Functions[name].ContainsKey(signature.Arguments))
        {
            throw new ArgumentException(
                $"There is already a registered function named `{name}` taking arguments of types `{signature}`"
            );
        }

        _Functions[name][signature.Arguments] = (
            signature,
            (args) => function((T1)args[0], (T2)args[1])
        );
    }

    /// Register a function that takes three arguments.
    public void Register<T1, T2, T3, TResult>(
        string name,
        Func<T1, T2, T3, TResult> function,
        ReceiverStyle style = ReceiverStyle.Global
    )
        where TResult : Value
        where T1 : Value
        where T2 : Value
        where T3 : Value
    {
        if (style == ReceiverStyle.Both)
        {
            Register(name, function, ReceiverStyle.Global);
            Register(name, function, ReceiverStyle.Receiver);
            return;
        }

        if (!_Functions.ContainsKey(name))
        {
            _Functions[name] =
                new Dictionary<ArgumentList, (Signature, Func<IList<Value>, Value>)>();
        }

        var parameters = function.Method.GetParameters();
        var signature = new Signature.Ternary(
            parameters[0].ParameterType,
            parameters[1].ParameterType,
            parameters[2].ParameterType,
            function.Method.ReturnType,
            Style: style
        );

        if (_Functions[name].ContainsKey(signature.Arguments))
        {
            throw new ArgumentException(
                $"There is already a registered function named `{name}` taking arguments of types `{signature}`"
            );
        }

        _Functions[name][signature.Arguments] = (
            signature,
            (args) => function((T1)args[0], (T2)args[1], (T3)args[2])
        );
    }

    /// Evaluate a function.
    public Value Evaluate(string name, ReceiverStyle style, params Value[] args)
    {
        if (!_Functions.ContainsKey(name))
        {
            throw new ArgumentException($"There is no registered function with the name `{name}`.");
        }

        var argumentList = ArgumentList.FromList(
            args.Select(value => value.GetType()).ToList(),
            style
        );

        var overloads = _Functions[name];

        if (!overloads.ContainsKey(argumentList))
        {
            var overloadSignatures = overloads.Values
                .Select(overload => overload.Item1.ToString())
                .ToList();
            throw new ArgumentException(
                $"There is no registered function with the name `{name}` taking the argument types {argumentList}. Valid overloads are: {string.Join(", ", overloadSignatures)}"
            );
        }

        var (_, function) = overloads[argumentList];
        return function(args);
    }

    /// Evaluate a function.
    public Value Evaluate(string name, ReceiverStyle style, IList<Value> args)
    {
        return Evaluate(name, style, args.ToArray());
    }
}
