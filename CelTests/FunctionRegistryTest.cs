using NUnit.Framework;

namespace Cel.Tests.Unit;

[TestFixture]
public class FunctionRegistryTest
{
    [OneTimeSetUp]
    public void TestFixtureSetup()
    {
        var registry = FunctionRegistry.Instance;
        // Nullary
        registry.Register("theAnswer", () => new Value.Int(42));

        // Unary
        registry.Register(
            "double",
            (Value.Double doubleValue) => new Value.Double(doubleValue.Value * 2)
        );

        // Binary
        registry.Register(
            "concat",
            (Value.String a, Value.String b) => new Value.String(a.Value + b.Value)
        );

        // Ternary, with five overloads, two of which are receiver-style (one specified via Both).
        registry.Register(
            "clamp",
            (Value.UInt min, Value.UInt max, Value.UInt value) =>
                new Value.UInt(Math.Max(min.Value, Math.Min(value.Value, max.Value)))
        );
        registry.Register(
            "clamp",
            (Value.Int min, Value.Int max, Value.Int value) =>
                new Value.Int(Math.Max(min.Value, Math.Min(value.Value, max.Value)))
        );
        registry.Register(
            "clamp",
            (Value.Double min, Value.Double max, Value.Double value) =>
                new Value.Double(Math.Max(min.Value, Math.Min(value.Value, max.Value))),
            style: ReceiverStyle.Both
        );
        registry.Register(
            "clamp",
            (Value.UInt min, Value.UInt max, Value.UInt value) =>
                new Value.UInt(Math.Max(min.Value, Math.Min(value.Value, max.Value))),
            style: ReceiverStyle.Receiver
        );
    }

    [Test]
    public void RegisterAndEvaluateNullary()
    {
        var registry = FunctionRegistry.Instance;

        Assert.AreEqual(Value.From(42), registry.Evaluate("theAnswer", ReceiverStyle.Global));
    }

    [Test]
    public void RegisterAndEvaluateUnary()
    {
        var registry = FunctionRegistry.Instance;

        Assert.AreEqual(
            Value.From(4.0),
            registry.Evaluate("double", ReceiverStyle.Global, Value.From(2.0))
        );
    }

    [Test]
    public void RegisterAndEvaluateBinary()
    {
        var registry = FunctionRegistry.Instance;

        Assert.AreEqual(
            Value.From("foobar"),
            registry.Evaluate("concat", ReceiverStyle.Global, Value.From("foo"), Value.From("bar"))
        );
    }

    [Test]
    public void RegisterAndEvaluateTernary()
    {
        var registry = FunctionRegistry.Instance;

        Assert.AreEqual(
            Value.From(3),
            registry.Evaluate(
                "clamp",
                ReceiverStyle.Global,
                Value.From(3),
                Value.From(7),
                Value.From(0)
            )
        );
        Assert.AreEqual(
            Value.From(3u),
            registry.Evaluate(
                "clamp",
                ReceiverStyle.Global,
                Value.From(3u),
                Value.From(7u),
                Value.From(0u)
            )
        );
        Assert.AreEqual(
            Value.From(3.0),
            registry.Evaluate(
                "clamp",
                ReceiverStyle.Global,
                Value.From(3.0),
                Value.From(7.0),
                Value.From(0.0)
            )
        );

        // Check receiver-style invocations.
        Assert.AreEqual(
            Value.From(3.0),
            registry.Evaluate(
                "clamp",
                ReceiverStyle.Receiver,
                Value.From(3.0),
                Value.From(7.0),
                Value.From(0.0)
            )
        );
    }

    [Test]
    public void CannotInvokeWithIncorrectArgumentCount()
    {
        var registry = FunctionRegistry.Instance;

        var exception = Assert.Throws<ArgumentException>(
            () =>
                registry.Evaluate("theAnswer", ReceiverStyle.Global, Value.From("to the question"))
        );
        Assert.AreEqual(
            "There is no registered function with the name `theAnswer` taking the argument types (String). Valid overloads are: () -> Int",
            exception?.Message
        );
    }

    [Test]
    public void CannotInvokeWithIncorrectArgumentTypes()
    {
        var registry = FunctionRegistry.Instance;

        var exception = Assert.Throws<ArgumentException>(
            () =>
                registry.Evaluate(
                    "clamp",
                    ReceiverStyle.Global,
                    Value.From(0),
                    Value.From(10u),
                    Value.From(11)
                )
        );
        Assert.AreEqual(
            "There is no registered function with the name `clamp` taking the argument types (Int, UInt, Int). Valid overloads are: (UInt, UInt, UInt) -> UInt, (Int, Int, Int) -> Int, (Double, Double, Double) -> Double, Double.(Double, Double) -> Double, UInt.(UInt, UInt) -> UInt",
            exception?.Message
        );
    }

    [Test]
    public void CannotInvokeWithIncorrectStyle()
    {
        var registry = FunctionRegistry.Instance;

        var exception = Assert.Throws<ArgumentException>(
            () =>
                registry.Evaluate(
                    "clamp",
                    ReceiverStyle.Receiver,
                    Value.From(3),
                    Value.From(7),
                    Value.From(0)
                )
        );
        Assert.AreEqual(
            "There is no registered function with the name `clamp` taking the argument types Int.(Int, Int). Valid overloads are: (UInt, UInt, UInt) -> UInt, (Int, Int, Int) -> Int, (Double, Double, Double) -> Double, Double.(Double, Double) -> Double, UInt.(UInt, UInt) -> UInt",
            exception?.Message
        );
    }
}
