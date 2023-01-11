using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using NUnit.Framework;

namespace Cel.Tests.Unit;

[TestFixture]
public class BuiltInFunctionsTest
{
    [Test]
    public void Size()
    {
        var registry = FunctionRegistry.Instance;

        Assert.AreEqual(
            Value.From(3),
            registry.Evaluate("size", ReceiverStyle.Global, Value.From("foo"))
        );
        Assert.AreEqual(
            Value.From(3),
            registry.Evaluate(
                "size",
                ReceiverStyle.Global,
                Value.From(ByteString.CopyFromUtf8("foo"))
            )
        );
        Assert.AreEqual(
            Value.From(3),
            registry.Evaluate(
                "size",
                ReceiverStyle.Global,
                Value.From(new List<Value> { Value.From("f"), Value.From("o"), Value.From("o") })
            )
        );
        Assert.AreEqual(
            Value.From(3),
            registry.Evaluate(
                "size",
                ReceiverStyle.Global,
                Value.From(
                    new Dictionary<Value, Value>
                    {
                        { Value.From("a"), Value.From(true) },
                        { Value.From("b"), Value.From(true) },
                        { Value.From("c"), Value.From(true) }
                    }
                )
            )
        );
    }

    [Test]
    public void TimestampDecomposition()
    {
        var registry = FunctionRegistry.Instance;

        var march1St2004 = Value.From(
            new DateTime(2004, 3, 1, 0, 0, 0, kind: DateTimeKind.Utc).ToTimestamp()
        );

        Assert.AreEqual(
            Value.From(1),
            registry.Evaluate("getDate", ReceiverStyle.Receiver, march1St2004)
        );
        Assert.AreEqual(
            Value.From(1),
            registry.Evaluate("getDayOfWeek", ReceiverStyle.Receiver, march1St2004)
        );
        Assert.AreEqual(
            Value.From(60),
            registry.Evaluate("getDayOfYear", ReceiverStyle.Receiver, march1St2004)
        );
        Assert.AreEqual(
            Value.From(2004),
            registry.Evaluate("getFullYear", ReceiverStyle.Receiver, march1St2004)
        );
        Assert.AreEqual(
            Value.From(0),
            registry.Evaluate("getHours", ReceiverStyle.Receiver, march1St2004)
        );
        Assert.AreEqual(
            Value.From(0),
            registry.Evaluate("getMilliseconds", ReceiverStyle.Receiver, march1St2004)
        );
        Assert.AreEqual(
            Value.From(0),
            registry.Evaluate("getMinutes", ReceiverStyle.Receiver, march1St2004)
        );
        Assert.AreEqual(
            Value.From(2),
            registry.Evaluate("getMonth", ReceiverStyle.Receiver, march1St2004)
        );
        Assert.AreEqual(
            Value.From(0),
            registry.Evaluate("getSeconds", ReceiverStyle.Receiver, march1St2004)
        );
    }

    [Test]
    public void StringFunctions()
    {
        var registry = FunctionRegistry.Instance;

        var fooBarBaz = Value.From("foobarbaz");

        Assert.AreEqual(
            Value.From(true),
            registry.Evaluate("contains", ReceiverStyle.Receiver, fooBarBaz, Value.From("bar"))
        );
        Assert.AreEqual(
            Value.From(true),
            registry.Evaluate("endsWith", ReceiverStyle.Receiver, fooBarBaz, Value.From("baz"))
        );
        Assert.AreEqual(
            Value.From(true),
            registry.Evaluate("matches", ReceiverStyle.Receiver, fooBarBaz, Value.From("foo.*baz"))
        );
        Assert.AreEqual(
            Value.From(true),
            registry.Evaluate("startsWith", ReceiverStyle.Receiver, fooBarBaz, Value.From("foo"))
        );

        Assert.AreEqual(
            Value.From(false),
            registry.Evaluate("contains", ReceiverStyle.Receiver, fooBarBaz, Value.From("rab"))
        );
        Assert.AreEqual(
            Value.From(false),
            registry.Evaluate("endsWith", ReceiverStyle.Receiver, fooBarBaz, Value.From("rba"))
        );
        Assert.AreEqual(
            Value.From(false),
            registry.Evaluate("matches", ReceiverStyle.Receiver, fooBarBaz, Value.From("foo.*barz"))
        );
        Assert.AreEqual(
            Value.From(false),
            registry.Evaluate("startsWith", ReceiverStyle.Receiver, fooBarBaz, Value.From("oob"))
        );
    }
}
