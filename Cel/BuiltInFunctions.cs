using System.Text.RegularExpressions;
using Google.Protobuf;

namespace Cel;

public static class BuiltInFunctions
{
    public static void Register(FunctionRegistry registry)
    {
        //
        // Type casting functions
        //

        // bytes
        registry.Register<Value.String, Value.Bytes>(
            "bytes",
            value => new Value.Bytes(ByteString.CopyFromUtf8(value.Value).ToArray())
        );

        // double
        registry.Register<Value.Int, Value.Double>(
            "double",
            value => new Value.Double(value.Value)
        );
        registry.Register<Value.UInt, Value.Double>(
            "double",
            value => new Value.Double(value.Value)
        );
        registry.Register<Value.String, Value.Double>(
            "double",
            value => new Value.Double(double.Parse(value.Value))
        );

        // duration
        registry.Register<Value.String, Value.Duration>(
            "duration",
            value => Value.Duration.Parse(value.Value)
        );

        // int
        registry.Register<Value.Double, Value.Int>(
            "int",
            value => new Value.Int((long)value.Value)
        );
        registry.Register<Value.UInt, Value.Int>("int", value => new Value.Int((long)value.Value));
        registry.Register<Value.String, Value.Int>(
            "int",
            value => new Value.Int(long.Parse(value.Value))
        );
        registry.Register<Value.Timestamp, Value.Int>(
            "int",
            value => new Value.Int(value.Value.ToDateTimeOffset().ToUnixTimeMilliseconds())
        );

        // string
        registry.Register<Value.Int, Value.String>(
            "string",
            value => new Value.String(value.Value.ToString())
        );
        registry.Register<Value.UInt, Value.String>(
            "string",
            value => new Value.String(value.Value.ToString())
        );
        registry.Register<Value.Double, Value.String>(
            "string",
            value => new Value.String(value.Value.ToString())
        );
        registry.Register<Value.Bytes, Value.String>(
            "string",
            value => new Value.String(System.Text.Encoding.UTF8.GetString(value.Value))
        );
        registry.Register<Value.Duration, Value.String>(
            "string",
            value => new Value.String(value.Format())
        );
        registry.Register<Value.Timestamp, Value.String>(
            "string",
            value => new Value.String(value.Format())
        );

        // timestamp
        registry.Register<Value.String, Value.Timestamp>(
            "timestamp",
            value => Value.Timestamp.Parse(value.Value)
        );

        // uint
        registry.Register<Value.Int, Value.UInt>(
            "uint",
            value => new Value.UInt((ulong)value.Value)
        );
        registry.Register<Value.Double, Value.UInt>(
            "uint",
            value => new Value.UInt((ulong)value.Value)
        );
        registry.Register<Value.String, Value.UInt>(
            "uint",
            value => new Value.UInt(ulong.Parse(value.Value))
        );

        //
        // Timestamp decomposition functions
        //

        registry.Register<Value.Timestamp, Value.Int>(
            "getDate",
            value => new Value.Int(value.Value.ToDateTime().Day),
            style: ReceiverStyle.Receiver
        );
        registry.Register<Value.Timestamp, Value.Int>(
            "getDayOfWeek",
            value => new Value.Int((long)value.Value.ToDateTime().DayOfWeek),
            style: ReceiverStyle.Receiver
        );
        registry.Register<Value.Timestamp, Value.Int>(
            "getDayOfYear",
            value => new Value.Int(value.Value.ToDateTime().DayOfYear - 1),
            style: ReceiverStyle.Receiver
        );
        registry.Register<Value.Timestamp, Value.Int>(
            "getFullYear",
            value => new Value.Int(value.Value.ToDateTime().Year),
            style: ReceiverStyle.Receiver
        );
        registry.Register<Value.Timestamp, Value.Int>(
            "getHours",
            value => new Value.Int(value.Value.ToDateTime().Hour),
            style: ReceiverStyle.Receiver
        );
        registry.Register<Value.Timestamp, Value.Int>(
            "getMilliseconds",
            value => new Value.Int(value.Value.ToDateTime().Millisecond),
            style: ReceiverStyle.Receiver
        );
        registry.Register<Value.Timestamp, Value.Int>(
            "getMinutes",
            value => new Value.Int(value.Value.ToDateTime().Minute),
            style: ReceiverStyle.Receiver
        );
        registry.Register<Value.Timestamp, Value.Int>(
            "getMonth",
            value => new Value.Int(value.Value.ToDateTime().Month - 1),
            style: ReceiverStyle.Receiver
        );
        registry.Register<Value.Timestamp, Value.Int>(
            "getSeconds",
            value => new Value.Int(value.Value.ToDateTime().Second),
            style: ReceiverStyle.Receiver
        );

        //
        // string functions
        //

        registry.Register<Value.String, Value.String, Value.Bool>(
            "contains",
            (str, substring) => new Value.Bool(str.Value.Contains(substring.Value)),
            style: ReceiverStyle.Receiver
        );

        registry.Register<Value.String, Value.String, Value.Bool>(
            "startsWith",
            (str, prefix) => new Value.Bool(str.Value.StartsWith(prefix.Value)),
            style: ReceiverStyle.Receiver
        );

        registry.Register<Value.String, Value.String, Value.Bool>(
            "endsWith",
            (str, prefix) => new Value.Bool(str.Value.EndsWith(prefix.Value)),
            style: ReceiverStyle.Receiver
        );

        registry.Register<Value.String, Value.String, Value.Bool>(
            "matches",
            (str, pattern) => new Value.Bool(new Regex(pattern.Value).IsMatch(str.Value)),
            style: ReceiverStyle.Receiver
        );

        // size
        registry.Register<Value.String, Value.Int>(
            "size",
            value => new Value.Int(value.Value.Length)
        );
        registry.Register<Value.Bytes, Value.Int>(
            "size",
            value => new Value.Int(value.Value.Length)
        );
        registry.Register<Value.List, Value.Int>("size", value => new Value.Int(value.Value.Count));
        registry.Register<Value.Map, Value.Int>("size", value => new Value.Int(value.Value.Count));
    }
}
