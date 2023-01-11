namespace Cel;

/// Used for syntax errors.
///
/// These should generally only be thrown in "this should not happen" code, e.g. the default case in a pattern matching
/// expression.  If this is thrown, there is likely a grammar bug.
public class SyntaxException : Exception
{
    public SyntaxException(string message)
        : base($"Syntax error: {message}") { }
}

/// A value (typically an operand) is of the wrong type.
public class TypeException : Exception
{
    public TypeException(string message)
        : base($"Type error: {message}") { }
}

/// As defined by the CEL spec: a map or message does not contain the desired field.
public class NoSuchFieldException : Exception
{
    public NoSuchFieldException(Value name, Value.Map context)
        : base($"No such field `{name}` found in `{context}`") { }
}

/// The specified identifier does not exist in the global context.
public class NoSuchIdentifierException : Exception
{
    public NoSuchIdentifierException(string name)
        : base($"No such identifier `{name}` found in global evaluation context") { }
}

/// As defined by the CEL spec: this function has no overload for the types of the arguments.
public class NoMatchingOverloadException : Exception
{
    public NoMatchingOverloadException(Value name, string[] types)
        : base($"No matching overload of `{name}` found for types `{types}`.") { }
}
