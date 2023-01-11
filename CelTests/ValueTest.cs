using Google.Protobuf;
using NUnit.Framework;
using Protobuf.Text;
using Value = Cel.Value;

namespace Cel.Tests.Unit;

[TestFixture]
public class ValueTest
{
    [Test]
    public void IntValue()
    {
        var one = Value.From(1);
        var two = Value.From(2);
        var alsoOne = Value.From(1);

        Assert.IsFalse(one.DeepEquals(two));
        Assert.IsTrue(one.DeepEquals(alsoOne));
        Assert.IsTrue(one < two);
        Assert.IsTrue(one <= two);
        Assert.IsTrue(two > one);
        Assert.IsTrue(two >= one);

        Assert.IsFalse(one < alsoOne);
        Assert.IsTrue(one <= alsoOne);
        Assert.IsFalse(one > alsoOne);
        Assert.IsTrue(one >= alsoOne);

        Assert.IsTrue((one + two).DeepEquals(Value.From(3)));
        Assert.IsTrue((one - two).DeepEquals(Value.From(-1)));
        Assert.IsTrue((one % two).DeepEquals(Value.From(1)));
        Assert.IsTrue((one / two).DeepEquals(Value.From(0)));
    }

    [Test]
    public void UIntValue()
    {
        var one = Value.From(1u);
        var two = Value.From(2u);
        var alsoOne = Value.From(1u);

        Assert.IsFalse(one.DeepEquals(two));
        Assert.IsTrue(one.DeepEquals(alsoOne));
        Assert.IsTrue(one < two);
        Assert.IsTrue(one <= two);
        Assert.IsTrue(two > one);
        Assert.IsTrue(two >= one);

        Assert.IsFalse(one < alsoOne);
        Assert.IsTrue(one <= alsoOne);
        Assert.IsFalse(one > alsoOne);
        Assert.IsTrue(one >= alsoOne);

        Assert.IsTrue((one + two).DeepEquals(Value.From(3u)));
        Assert.IsTrue((two - one).DeepEquals(Value.From(1u)));
        Assert.IsTrue((one % two).DeepEquals(Value.From(1u)));
        Assert.IsTrue((one / two).DeepEquals(Value.From(0u)));
    }

    [Test]
    public void DoubleValue()
    {
        var one = Value.From(1.0);
        var two = Value.From(2.0);
        var alsoOne = Value.From(1.0);

        Assert.IsFalse(one.DeepEquals(two));
        Assert.IsTrue(one.DeepEquals(alsoOne));
        Assert.IsTrue(one < two);
        Assert.IsTrue(one <= two);
        Assert.IsTrue(two > one);
        Assert.IsTrue(two >= one);

        Assert.IsFalse(one < alsoOne);
        Assert.IsTrue(one <= alsoOne);
        Assert.IsFalse(one > alsoOne);
        Assert.IsTrue(one >= alsoOne);

        Assert.IsTrue((one + two).DeepEquals(Value.From(3.0)));
        Assert.IsTrue((one - two).DeepEquals(Value.From(-1.0)));
        Assert.IsTrue((one / two).DeepEquals(Value.From(0.5)));
    }

    [Test]
    public void StringValue()
    {
        var foo = Value.From("foo");
        var bar = Value.From("bar");
        var alsoFoo = Value.From("foo");

        Assert.IsFalse(foo.DeepEquals(bar));
        Assert.IsTrue(foo.DeepEquals(alsoFoo));
        Assert.IsTrue(bar < foo);
        Assert.IsTrue(bar <= foo);
        Assert.IsTrue(foo > bar);
        Assert.IsTrue(foo >= bar);

        Assert.IsFalse(foo < alsoFoo);
        Assert.IsTrue(foo <= alsoFoo);
        Assert.IsFalse(foo > alsoFoo);
        Assert.IsTrue(foo >= alsoFoo);

        Assert.IsTrue(Value.From("foobar").DeepEquals(foo + bar));
    }

    [Test]
    public void BoolValue()
    {
        var @true = Value.From(true);
        var @false = Value.From(false);
        var alsoTrue = Value.From(true);

        Assert.IsFalse(@true.DeepEquals(@false));
        Assert.IsTrue(@true.DeepEquals(alsoTrue));
        Assert.IsTrue(@false < @true);
        Assert.IsTrue(@false <= @true);
        Assert.IsTrue(@true > @false);
        Assert.IsTrue(@true >= @false);

        Assert.IsFalse(@true < alsoTrue);
        Assert.IsTrue(@true <= alsoTrue);
        Assert.IsFalse(@true > alsoTrue);
        Assert.IsTrue(@true >= alsoTrue);

        Assert.IsTrue(true && true);
        Assert.IsFalse(true && false);
        Assert.IsTrue(true || true);
        Assert.IsTrue(true || false);
    }

    [Test]
    public void BytesValue()
    {
        var foo = Value.From(ByteString.CopyFromUtf8("foo"));
        var bar = Value.From(ByteString.CopyFromUtf8("bar"));
        var alsoFoo = Value.From(ByteString.CopyFromUtf8("foo"));

        // Use these to test different length strings.
        var food = Value.From(ByteString.CopyFromUtf8("food"));
        var bard = Value.From(ByteString.CopyFromUtf8("bard"));

        Assert.IsFalse(foo.DeepEquals(bar));
        Assert.IsTrue(foo.DeepEquals(alsoFoo));

        Assert.IsTrue(bar < foo);
        Assert.IsTrue(bar <= foo);
        Assert.IsTrue(foo > bar);
        Assert.IsTrue(foo >= bar);

        // Compare arrays to equivalent arrays.
        Assert.IsFalse(foo < alsoFoo);
        Assert.IsTrue(foo <= alsoFoo);
        Assert.IsFalse(foo > alsoFoo);
        Assert.IsTrue(foo >= alsoFoo);

        // First array is longer.
        Assert.IsTrue(bard < foo);
        Assert.IsTrue(bard <= foo);
        Assert.IsTrue(foo > bard);
        Assert.IsTrue(foo >= bard);

        // Second array is longer.
        Assert.IsTrue(bar < food);
        Assert.IsTrue(bar <= food);
        Assert.IsTrue(food > bar);
        Assert.IsTrue(food >= bar);

        // Explicitly test the empty array.
        var empty = Value.From(ByteString.CopyFromUtf8(""));
        var alsoEmpty = Value.From(ByteString.CopyFromUtf8(""));

        Assert.IsTrue(empty < foo);
        Assert.IsTrue(empty <= foo);
        Assert.IsTrue(foo > empty);
        Assert.IsTrue(foo >= empty);

        Assert.IsFalse(empty < alsoEmpty);
        Assert.IsTrue(empty <= alsoEmpty);
        Assert.IsFalse(empty > alsoEmpty);
        Assert.IsTrue(empty >= alsoEmpty);

        // Test the `+` operator.
        Assert.IsTrue(Value.From(ByteString.CopyFromUtf8("foobar")).DeepEquals(foo + bar));
    }

    [Test]
    public void DurationValue()
    {
        var minute = Value.From(
            Google.Protobuf.WellKnownTypes.Duration.FromTimeSpan(TimeSpan.FromMinutes(1))
        );
        var hour = Value.From(
            Google.Protobuf.WellKnownTypes.Duration.FromTimeSpan(TimeSpan.FromHours(1))
        );
        var alsoMinute = Value.From(
            Google.Protobuf.WellKnownTypes.Duration.FromTimeSpan(TimeSpan.FromMinutes(1))
        );

        Assert.IsFalse(hour.DeepEquals(minute));
        Assert.IsTrue(minute.DeepEquals(alsoMinute));
        Assert.IsTrue(minute < hour);
        Assert.IsTrue(minute <= hour);
        Assert.IsTrue(hour > minute);
        Assert.IsTrue(hour >= minute);

        Assert.IsFalse(minute < alsoMinute);
        Assert.IsTrue(minute <= alsoMinute);
        Assert.IsFalse(minute > alsoMinute);
        Assert.IsTrue(minute >= alsoMinute);

        Assert.AreEqual(
            Value.From(Google.Protobuf.WellKnownTypes.Duration.FromTimeSpan(new TimeSpan(1, 1, 0))),
            minute + hour
        );
        Assert.AreEqual(
            Value.From(
                Google.Protobuf.WellKnownTypes.Duration.FromTimeSpan(new TimeSpan(0, 59, 0))
            ),
            hour - minute
        );
    }

    [Test]
    public void TimestampValue()
    {
        var jan1 = Value.From(
            Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(
                new DateTime(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
            )
        );
        var jan2 = Value.From(
            Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(
                new DateTime(2000, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc)
            )
        );
        var alsoJan1 = Value.From(
            Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(
                new DateTime(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
            )
        );

        Assert.IsFalse(jan2.DeepEquals(jan1));
        Assert.IsTrue(jan1.DeepEquals(alsoJan1));
        Assert.IsTrue(jan1 < jan2);
        Assert.IsTrue(jan1 <= jan2);
        Assert.IsTrue(jan2 > jan1);
        Assert.IsTrue(jan2 >= jan1);

        Assert.IsFalse(jan1 < alsoJan1);
        Assert.IsTrue(jan1 <= alsoJan1);
        Assert.IsFalse(jan1 > alsoJan1);
        Assert.IsTrue(jan1 >= alsoJan1);

        var oneDay = Google.Protobuf.WellKnownTypes.Duration.FromTimeSpan(new TimeSpan(24, 0, 0));

        Assert.IsTrue(Value.From(oneDay).DeepEquals(jan2 - jan1));
    }

    [Test]
    public void MapValue()
    {
        var map = Value.From(
            new Dictionary<Value, Value>
            {
                { Value.From("a"), Value.From(1) },
                { Value.From("b"), Value.From(true) },
                {
                    Value.From(false),
                    Value.From(
                        new Dictionary<Value, Value>
                        {
                            {
                                Value.From(
                                    new List<Value> { Value.From(1), Value.From(2), Value.From(3) }
                                ),
                                Value.From(1.0)
                            }
                        }
                    )
                },
            }
        );

        Assert.IsTrue(Value.From(1).DeepEquals(map[Value.From("a")]));
        Assert.Throws<TypeException>(() => Value.From(false).DeepEquals(map[Value.From("a")]));
    }

    [Test]
    public void ListValue()
    {
        Value oneTwoThree = Value.From(
            new List<Value> { Value.From(1), Value.From(2), Value.From(3) }
        );
        var abc = Value.From(new List<Value> { Value.From("a"), Value.From("b"), Value.From("c") });

        var easyAs = Value.From(
            new List<Value>
            {
                Value.From("a"),
                Value.From("b"),
                Value.From("c"),
                Value.From(1),
                Value.From(2),
                Value.From(3)
            }
        );

        Assert.IsTrue(easyAs.DeepEquals(abc + oneTwoThree));

        Assert.IsTrue(easyAs[Value.From(3)].DeepEquals(oneTwoThree[Value.From(0)]));
        Assert.IsTrue(easyAs[Value.From(-6)].DeepEquals(abc[Value.From(0)]));
        Assert.Throws<IndexOutOfRangeException>(() => _ = easyAs[Value.From(6)]);
        Assert.Throws<IndexOutOfRangeException>(() => _ = easyAs[Value.From(-7)]);
        Assert.Throws<IndexOutOfRangeException>(() => _ = easyAs[Value.From(long.MaxValue)]);
        Assert.Throws<IndexOutOfRangeException>(() => _ = easyAs[Value.From(long.MinValue)]);
    }

    [Test]
    public void MessageValue()
    {
        var alice = Value.From(
            Person.Parser.ParseText(
                @"{
            ""given_name"": ""Alice""
            ""family_name"": ""Smith""
            ""nicknames"": [""Ali"", ""Lis"", ""Allsmith""]
            ""age"": 30
            ""best_golf_score"": -3
            ""favorites"" {
                ""book"" {
                    ""title"": ""Accelerando""
                    ""author"": ""Charles Stross""
                }
                ""band"": ""Monster Magnet""
                ""number"": 7
                ""food"": ""POTATO""
            }
            ""pet"": DOG
            ""fear"": HORSE
            ""allergies"": [
                ""PEA"",
                ""CARROT""
            ]
            dislikes: [""fried food""],
            stream_of_consciousness: {
                color: ""yellow"",
                animal: ""elephant""
            }
            ssn: 123456
        }"
            )
        );

        Assert.IsTrue(alice is Value.Map);
        var fields = (Value.Map)alice;
        var favorites = fields[Value.From("favorites")];
        Assert.IsTrue(favorites[Value.From("number")].DeepEquals(Value.From(7)));

        var nicknames = (Value.List)fields[Value.From("nicknames")];
        Assert.AreEqual(3, nicknames.Value.Count);

        // Check that enums work.
        Assert.IsTrue(
            Value
                .From(Person.Descriptor.FindFieldByName("pet").EnumType, 2)
                .DeepEquals(fields[Value.From("pet")])
        );
        Assert.IsFalse(fields[Value.From("pet")].DeepEquals(fields[Value.From("fear")]));

        // We should get a TypeException comparing different enum types.
        var exception = Assert.Throws<TypeException>(
            () => fields[Value.From("pet")].DeepEquals(favorites[Value.From("food")])
        );
        Assert.AreEqual(
            "Type error: Cannot evaluate equality of different enum types `cel.tests.Animal` and `cel.tests.Person.Food`",
            exception?.Message
        );

        //
        // Check protobuf WKTs
        //

        // List
        var expectedDislikes = Value.From(new List<Value> { Value.From("fried food") });
        Assert.IsTrue(fields[Value.From("dislikes")].DeepEquals(expectedDislikes));

        // Struct
        Assert.IsTrue(
            fields[Value.From("stream_of_consciousness")][Value.From("color")].DeepEquals(
                Value.From("yellow")
            )
        );

        // Wrappers
        Assert.IsTrue(fields[Value.From("ssn")].DeepEquals(Value.From(123456)));
        Assert.IsTrue(fields[Value.From("ssn")] > (Value.From(123455)));
    }
}
