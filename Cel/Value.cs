using System.Collections;
using System.Globalization;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace Cel;

internal class EnumRegistry
{
    private readonly Dictionary<string, Dictionary<string, int>> _Enums = new();

    private EnumRegistry() { }

    private static readonly Lazy<EnumRegistry> _Instance = new Lazy<EnumRegistry>(
        () => new EnumRegistry()
    );

    public static EnumRegistry Instance => _Instance.Value;

    public void Register(string name, Dictionary<string, int> values)
    {
        if (!Exists(name))
        {
            _Enums[name] = values;
        }
    }

    public bool Exists(string name) => _Enums.ContainsKey(name);

    public Dictionary<string, int> GetValues(string name)
    {
        if (!Exists(name))
        {
            throw new ArgumentException($"Enum {name} is not registered.");
        }

        return _Enums[name];
    }
}

/// Represents a CEL value (see the spec: https://github.com/google/cel-spec/blob/master/doc/langdef.md)
public record Value
{
    public record Int(long Value) : Value;

    public record UInt(ulong Value) : Value;

    public record Double(double Value) : Value;

    public record String(string Value) : Value;

    public record Bool(bool Value) : Value;

    public record Bytes(byte[] Value) : Value;

    public record Duration(Google.Protobuf.WellKnownTypes.Duration Value) : Value
    {
        public static Duration Parse(string str)
        {
            throw new NotImplementedException("Duration parsing not yet implemented");
        }

        public string Format()
        {
            throw new NotImplementedException("Duration formatting not yet implemented");
        }
    }

    public record Timestamp(Google.Protobuf.WellKnownTypes.Timestamp Value) : Value
    {
        public static Timestamp Parse(string str)
        {
            return new Timestamp(
                DateTime.Parse(str, DateTimeFormatInfo.InvariantInfo).ToTimestamp()
            );
        }

        public string Format()
        {
            return Value
                .ToDateTime()
                .ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz", DateTimeFormatInfo.InvariantInfo);
        }
    }

    public record Enum(string Name, int Value) : Value;

    public record Map(Dictionary<Value, Value> Value) : Value;

    public record List(List<Value> Value) : Value;

    public record Null : Value;

    public static Value From(long value) => new Int(value);

    public static Value From(int value) => new Int(value);

    public static Value From(ulong value) => new UInt(value);

    public static Value From(uint value) => new UInt(value);

    public static Value From(double value) => new Double(value);

    public static Value From(float value) => new Double(value);

    public static Value From(string value) => new String(value);

    public static Value From(bool value) => new Bool(value);

    public static Value From(ByteString value) => new Bytes(value.ToArray());

    public static Value From(Google.Protobuf.WellKnownTypes.Duration value) => new Duration(value);

    public static Value From(Google.Protobuf.WellKnownTypes.Timestamp value) =>
        new Timestamp(value);

    public static Value From(Google.Protobuf.Reflection.EnumDescriptor descriptor, int value)
    {
        EnumRegistry.Instance.Register(
            descriptor.FullName,
            descriptor.Values.ToDictionary(option => option.Name, option => option.Number)
        );
        return new Enum(descriptor.FullName, value);
    }

    public static Value From(Dictionary<Value, Value> value) => new Map(value);

    public static Value From(List<Value> value) => new List(value);

    public static Value From(IMessage message) =>
        new Map(
            message.Descriptor.Fields
                .InFieldNumberOrder()
                .Where(field => field.Accessor.GetValue(message) != null)
                .ToDictionary(
                    field => From(field.Name),
                    field =>
                    {
                        var value = field.Accessor.GetValue(message);

                        Google.Protobuf.Reflection.EnumDescriptor enumType;
                        // Unfortunately, the C# reflection API for protobufs doesn't seem to have a way
                        // to check explicitly if a field is an enum.
                        try
                        {
                            enumType = field.EnumType;
                        }
                        catch (InvalidOperationException)
                        {
                            return FromObject(value);
                        }

                        if (field.IsRepeated)
                        {
                            var elements = new List<Value>();
                            foreach (var element in (IList)value)
                            {
                                elements.Add(From(enumType, (int)element));
                            }

                            return From(elements);
                        }

                        return From(enumType, (int)value);
                    }
                )
        );

    private static Value FromList(IList list)
    {
        var result = new List<Value>();
        foreach (var element in list)
        {
            result.Add(FromObject(element));
        }
        return From(result);
    }

    private static Value FromDictionary(IDictionary dict)
    {
        var result = new Dictionary<Value, Value>();
        foreach (var key in dict.Keys)
        {
            var keyAsValue = FromObject(key);
            var v = keyAsValue switch
            {
                Int => FromObject(dict[key]),
                UInt => FromObject(dict[key]),
                Bool => FromObject(dict[key]),
                String => FromObject(dict[key]),
                _
                    => throw new TypeException(
                        $"Map keys must be int, uint, bool, or string. Cannot be `{key}`"
                    )
            };
            result.Add(keyAsValue, v);
        }
        return From(result);
    }

    private static Value FromObject(object obj)
    {
        return obj switch
        {
            // C# collections
            Value value => value,
            IList list => FromList(list),
            IDictionary dict => FromDictionary(dict),

            //
            // Protobuf well-known types
            //

            // TODO(rfrankel): Support google.Protobuf.Any

            ListValue listValue
                => From(listValue.Values.Select(element => FromObject(element)).ToList()),

            Google.Protobuf.WellKnownTypes.Value value
                => value.KindCase switch
                {
                    Google.Protobuf.WellKnownTypes.Value.KindOneofCase.BoolValue
                        => FromObject(value.BoolValue),
                    Google.Protobuf.WellKnownTypes.Value.KindOneofCase.ListValue
                        => FromObject(value.ListValue),
                    Google.Protobuf.WellKnownTypes.Value.KindOneofCase.None => new Value.Null(),
                    Google.Protobuf.WellKnownTypes.Value.KindOneofCase.NullValue
                        => new Value.Null(),
                    Google.Protobuf.WellKnownTypes.Value.KindOneofCase.NumberValue
                        => FromObject(value.NumberValue),
                    Google.Protobuf.WellKnownTypes.Value.KindOneofCase.StringValue
                        => FromObject(value.StringValue),
                    Google.Protobuf.WellKnownTypes.Value.KindOneofCase.StructValue
                        => FromObject(value.StructValue),
                    _
                        => throw new ArgumentException(
                            $"Unrecognized Google.Protobuf.WellKnownTypes.Value subtype {value.KindCase}"
                        ),
                },

            Google.Protobuf.WellKnownTypes.Struct structValue => FromDictionary(structValue.Fields),

            // WKT wrappers
            BoolValue boolValue => From(boolValue.Value),
            BytesValue bytesValue => From(bytesValue.Value),
            DoubleValue doubleValue => From(doubleValue.Value),
            FloatValue floatValue => From(floatValue.Value),
            Int32Value int32Value => From(int32Value.Value),
            Int64Value int64Value => From(int64Value.Value),
            NullValue nullValue => new Value.Null(),
            StringValue stringValue => From(stringValue.Value),
            UInt32Value uint32Value => From(uint32Value.Value),
            UInt64Value uint64Value => From(uint64Value.Value),

            // Catch-all for messages that are not WKTs
            IMessage message => From(message),

            // If it's not one of the above known structured types, assume it's a primitive.
            _ => PrimitiveValue(obj)
        };
    }

    private static Value PrimitiveValue(object obj)
    {
        return obj switch
        {
            long o => From(o),
            int o => From(o),
            ulong o => From(o),
            uint o => From(o),
            double o => From(o),
            float o => From(o),
            string o => From(o),
            bool o => From(o),
            ByteString o => From(o),
            _ => throw new TypeException($"Value <{obj}> has unknown type `{obj.GetType()}`")
        };
    }

    public Decimal ToDecimal()
    {
        return new Decimal(
            this switch
            {
                Int value => value.Value,
                UInt value => value.Value,
                Double value => value.Value,
                _ => throw new TypeException($"`{this}` is not a numeric value")
            }
        );
    }

    public static Value operator !(Value value) =>
        value switch
        {
            Bool boolValue => From(!boolValue.Value),
            _ => throw new TypeException($"Cannot negate non-Boolean value `{value}`")
        };

    public static Value operator -(Value value) =>
        value switch
        {
            Int v => From(-v.Value),
            Double v => From(-v.Value),
            _ => throw new TypeException($"Cannot negate non-signed number value `{value}`")
        };

    public static bool operator <(Value first, Value second)
    {
        // Returns true if a is lexicographically "less than" b.
        bool CompareByteArrays(byte[] a, byte[] b)
        {
            var firstMismatch = a.Zip(b).FirstOrDefault(pair => pair.First != pair.Second);
            return firstMismatch.Item1 < firstMismatch.Item2
                || (firstMismatch == default((byte, byte)) && a.Length < b.Length);
        }

        return (first, second) switch
        {
            (Bool a, Bool b) => !a.Value && b.Value, // false < true
            (Int a, Int b) => a.Value < b.Value,
            (UInt a, UInt b) => a.Value < b.Value,
            (Double a, Double b) => a.Value < b.Value,
            (String a, String b) => string.Compare(a.Value, b.Value) < 0,
            (Bytes a, Bytes b) => CompareByteArrays(a.Value, b.Value),
            (Timestamp a, Timestamp b) => a.Value < b.Value,
            (Duration a, Duration b) => a.Value.ToTimeSpan() < b.Value.ToTimeSpan(),
            _
                => throw new TypeException(
                    $"Less than comparison is not supported for types `{first.GetType()}` and `{second.GetType()}`"
                )
        };
    }

    public static bool operator >(Value first, Value second) => second < first;

    public static bool operator >=(Value first, Value second) =>
        first.DeepEquals(second) || first > second;

    public static bool operator <=(Value first, Value second) =>
        first.DeepEquals(second) || first < second;

    public static Value operator *(Value first, Value second) =>
        (first, second) switch
        {
            (Int a, Int b) => From(a.Value * b.Value),
            (UInt a, UInt b) => From(a.Value * b.Value),
            (Double a, Double b) => From(a.Value * b.Value),
            _
                => throw new TypeException(
                    $"Multiplication is not supported for types `{first.GetType()}` and `{second.GetType()}`"
                )
        };

    public static Value operator /(Value first, Value second) =>
        (first, second) switch
        {
            (Int a, Int b) => From(a.Value / b.Value),
            (UInt a, UInt b) => From(a.Value / b.Value),
            (Double a, Double b) => From(a.Value / b.Value),
            _
                => throw new TypeException(
                    $"Division is not supported for types `{first.GetType()}` and `{second.GetType()}`"
                )
        };

    public static Value operator %(Value first, Value second) =>
        (first, second) switch
        {
            (Int a, Int b) => From(a.Value % b.Value),
            (UInt a, UInt b) => From(a.Value % b.Value),
            (Double a, Double b) => From(a.Value % b.Value),
            _
                => throw new TypeException(
                    $"Modulus is not supported for types `{first.GetType()}` and `{second.GetType()}`"
                )
        };

    public static Value operator +(Value first, Value second) =>
        (first, second) switch
        {
            (Int a, Int b) => From(a.Value + b.Value),
            (UInt a, UInt b) => From(a.Value + b.Value),
            (Double a, Double b) => From(a.Value + b.Value),
            (String a, String b) => From(a.Value + b.Value),
            (Bytes a, Bytes b) => From(ByteString.CopyFrom(a.Value.Concat(b.Value).ToArray())),
            (List a, List b) => From(a.Value.Concat(b.Value).ToList()),
            (Timestamp a, Duration b) => From(a.Value + b.Value),
            (Duration a, Timestamp b) => From(b.Value + a.Value),
            (Duration a, Duration b) => From(a.Value + b.Value),
            _
                => throw new TypeException(
                    $"Addition is not supported for types `{first.GetType()}` and `{second.GetType()}`"
                )
        };

    public static Value operator -(Value first, Value second) =>
        (first, second) switch
        {
            (Int a, Int b) => From(a.Value - b.Value),
            (UInt a, UInt b) => From(a.Value - b.Value),
            (Double a, Double b) => From(a.Value - b.Value),
            (Timestamp a, Timestamp b) => From(a.Value - b.Value),
            (Timestamp a, Duration b) => From(a.Value - b.Value),
            (Duration a, Duration b) => From(a.Value - b.Value),
            _
                => throw new TypeException(
                    $"Subtraction is not supported for types `{first.GetType()}` and `{second.GetType()}`"
                )
        };

    public static bool operator true(Value value) =>
        value switch
        {
            Bool boolValue => boolValue.Value,
            _ => throw new TypeException($"Cannot evaluate truth of non-boolean value `{value}`")
        };

    public static bool operator false(Value value) =>
        value switch
        {
            Bool boolValue => !boolValue.Value,
            _
                => throw new TypeException(
                    $"Cannot evaluate falsity status of non-boolean value `{value}`"
                )
        };

    // We can't override Equals because Value is a record, but the default Equals doesn't work properly with a nested
    // Dictionary. We'll use DeepEquals for the CEL equality operator.
    public bool DeepEquals(Value other) =>
        (this, other) switch
        {
            (Int a, Int b) => a.Value == b.Value,
            (UInt a, UInt b) => a.Value == b.Value,
            (Double a, Double b) => a.Value == b.Value,
            (String a, String b) => a.Value == b.Value,
            (Bool a, Bool b) => a.Value == b.Value,
            (Bytes a, Bytes b) => a.Value.SequenceEqual(b.Value),
            (Duration a, Duration b) => a.Value.ToTimeSpan() == b.Value.ToTimeSpan(),
            (Timestamp a, Timestamp b) => a.Value.ToDateTime() == b.Value.ToDateTime(),
            (Enum a, Enum b)
                => a.Name == b.Name
                    ? a.Value == b.Value
                    : throw new TypeException(
                        $"Cannot evaluate equality of different enum types `{a.Name}` and `{b.Name}`"
                    ),
            (Map a, Map b)
                => a.Value.Count == b.Value.Count
                    && a.Value.Keys.All(
                        key =>
                            b.Value.ContainsKey(key) && a.Value[key].PermissiveEquals(b.Value[key])
                    ),
            (List a, List b) => a.Value.SequenceEqual(b.Value),
            (Null, Null) => true,
            _
                => throw new TypeException(
                    $"Cannot evaluate equality of values `{this}` and `{other}` with incompatible types (`{GetType()}` vs. `{other.GetType()}`)"
                )
        };

    // Use the same general logic as `DeepEquals`, but return `false` rather than throwing for incompatible types.
    private bool PermissiveEquals(Value other)
    {
        try
        {
            return DeepEquals(other);
        }
        catch (TypeException)
        {
            return false;
        }
    }

    public Value this[Value index]
    {
        get
        {
            if (this is Map fields)
            {
                Value IndexOrThrow()
                {
                    if (fields.Value.ContainsKey(index))
                    {
                        return fields.Value[index];
                    }

                    throw new NoSuchFieldException(index, fields);
                }

                return index switch
                {
                    Int => IndexOrThrow(),
                    UInt => IndexOrThrow(),
                    Bool => IndexOrThrow(),
                    String => IndexOrThrow(),
                    _ => throw new TypeException($"Invalid map key of type `{index.GetType()}`")
                };
            }

            if (this is List values)
            {
                int numericIndex = index switch
                {
                    Int intValue
                        => (intValue.Value <= int.MaxValue && intValue.Value >= int.MinValue)
                            ? (int)intValue.Value
                            : throw new IndexOutOfRangeException(
                                $"List indices cannot exceed the range of an 32-bit signed integer (got `{intValue.Value}`)"
                            ),
                    UInt uintValue
                        => uintValue.Value <= int.MaxValue
                            ? (int)uintValue.Value
                            : throw new IndexOutOfRangeException(
                                $"List indices cannot exceed the range of an 32-bit signed integer (got `{uintValue.Value}`)"
                            ),
                    _
                        => throw new TypeException(
                            $"Invalid list index `{index}` of type `{index.GetType()}`"
                        )
                };

                // Support Python-style negative indexing.
                if (numericIndex < 0)
                {
                    numericIndex = values.Value.Count + numericIndex;
                }

                // Now that we've handled negative indexing, if numericIndex is still negative, it's
                // out of range (e.g. values.value.Count == 3 and the original index value was -5; 3 - 5 is -2).
                if (numericIndex < 0 || numericIndex >= values.Value.Count)
                {
                    throw new IndexOutOfRangeException($"Index `{numericIndex}` is out of range");
                }

                return values.Value[numericIndex];
            }

            throw new TypeException(
                $"The index operator (`[]`) is not valid for value `{this}` of type `{GetType()}`"
            );
        }
    }
}
