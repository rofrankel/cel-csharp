using Protobuf.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Cel.Tests.Unit;

[TestFixture]
public class EvaluatorTest
{
    private readonly Person _Alice = Person.Parser.ParseText(
        @"{
            ""given_name"": ""Alice""
            ""family_name"": ""Smith""
            ""age"": 30
            ""best_golf_score"": -3
            ""nicknames"": [ ""A"", ""b"", ""c"" ]
            ""favorites"" {
                ""book"" {
                    ""title"": ""Accelerando""
                    ""author"": ""Charles Stross""
                }
                ""band"": ""Monster Magnet""
                ""number"": 7
            }
            goals_by_day { Monday: ""Work"" Tuesday: ""Rest"" }
            ssn: 123456
        }"
    );

    [Test]
    public void RejectsInvalidExpression()
    {
        var expr = "a || ||";
        var exception = Assert.Throws<ArgumentException>(() => new Evaluator(expr));
        var regex = new Regex($"expr '{expr}' is not valid: .*mismatched input");
        Assert.IsTrue(regex.IsMatch(exception!.Message));
    }

    [Test]
    public void NumberEquals()
    {
        var filter = new Evaluator("age == 30U");
        Assert.IsTrue(filter.EvaluateBool(_Alice));

        filter = new Evaluator("age == 29U");
        Assert.IsFalse(filter.EvaluateBool(_Alice));
    }

    [Test]
    public void NumberGreaterThan()
    {
        var filter = new Evaluator("age > 29U");
        Assert.IsTrue(filter.EvaluateBool(_Alice));

        filter = new Evaluator("age > 30U");
        Assert.IsFalse(filter.EvaluateBool(_Alice));

        filter = new Evaluator("31U > age");
        Assert.IsTrue(filter.EvaluateBool(_Alice));

        filter = new Evaluator("30U > age");
        Assert.IsFalse(filter.EvaluateBool(_Alice));

        filter = new Evaluator("ssn > 123455");
        Assert.IsTrue(filter.EvaluateBool(_Alice));
    }

    [Test]
    public void NumberGreaterThanOrEquals()
    {
        var filter = new Evaluator("age >= 30U");
        Assert.IsTrue(filter.EvaluateBool(_Alice));

        filter = new Evaluator("age >= 31U");
        Assert.IsFalse(filter.EvaluateBool(_Alice));

        filter = new Evaluator("30U >= age");
        Assert.IsTrue(filter.EvaluateBool(_Alice));

        filter = new Evaluator("29U >= age");
        Assert.IsFalse(filter.EvaluateBool(_Alice));
    }

    [Test]
    public void NumberLessThan()
    {
        var filter = new Evaluator("age < 31U");
        Assert.IsTrue(filter.EvaluateBool(_Alice));

        filter = new Evaluator("age < 30U");
        Assert.IsFalse(filter.EvaluateBool(_Alice));

        filter = new Evaluator("29U < age");
        Assert.IsTrue(filter.EvaluateBool(_Alice));

        filter = new Evaluator("30U < age");
        Assert.IsFalse(filter.EvaluateBool(_Alice));
    }

    [Test]
    public void NumberLessThanOrEquals()
    {
        var filter = new Evaluator("age <= 30U");
        Assert.IsTrue(filter.EvaluateBool(_Alice));

        filter = new Evaluator("age <= 29U");
        Assert.IsFalse(filter.EvaluateBool(_Alice));

        filter = new Evaluator("30U <= age");
        Assert.IsTrue(filter.EvaluateBool(_Alice));

        filter = new Evaluator("31U <= age");
        Assert.IsFalse(filter.EvaluateBool(_Alice));
    }

    [Test]
    public void NumberNegative()
    {
        var filter = new Evaluator("-3 == best_golf_score");
        Assert.IsTrue(filter.EvaluateBool(_Alice));

        filter = new Evaluator("-2 == best_golf_score");
        Assert.IsFalse(filter.EvaluateBool(_Alice));

        filter = new Evaluator("best_golf_score == -3");
        Assert.IsTrue(filter.EvaluateBool(_Alice));

        filter = new Evaluator("best_golf_score == -2");
        Assert.IsFalse(filter.EvaluateBool(_Alice));
    }

    [Test]
    public void String()
    {
        var filter = new Evaluator(@"given_name == ""Alice""");
        Assert.IsTrue(filter.EvaluateBool(_Alice));

        filter = new Evaluator(@"given_name == ""Bob""");
        Assert.IsFalse(filter.EvaluateBool(_Alice));
    }

    [Test]
    public void LogicalNot()
    {
        var filter = new Evaluator("!(3 == best_golf_score)");
        Assert.IsTrue(filter.EvaluateBool(_Alice));

        filter = new Evaluator("!(-3 == best_golf_score)");
        Assert.IsFalse(filter.EvaluateBool(_Alice));
    }

    [Test]
    public void CompositeLogicalExpression()
    {
        var filter = new Evaluator("(true || false) && (false || true)");
        Assert.IsTrue(filter.EvaluateBool(_Alice));

        filter = new Evaluator("(true || false) && (false || (false && true)))");
        Assert.IsFalse(filter.EvaluateBool(_Alice));
    }

    [Test]
    public void DotTraversal()
    {
        var filter = new Evaluator(@"favorites.book.title == ""Accelerando""");
        Assert.IsTrue(filter.EvaluateBool(_Alice));

        filter = new Evaluator(@"favorites.book.title == ""Permutation City""");
        Assert.IsFalse(filter.EvaluateBool(_Alice));
    }

    [Test]
    public void ListEquality()
    {
        var filter = new Evaluator(@"[1u, 2u] == [1u, 2u]}");
        Assert.IsTrue(filter.EvaluateBool(_Alice));

        filter = new Evaluator(@"[1u, 2u] != [1u, 2u]}");
        Assert.IsFalse(filter.EvaluateBool(_Alice));

        filter = new Evaluator(@"[1u, 2u] == [1u, 2u, 3u]}");
        Assert.IsFalse(filter.EvaluateBool(_Alice));

        filter = new Evaluator(@"[1u, 2u] != [1u, 2u, 3u]}");
        Assert.IsTrue(filter.EvaluateBool(_Alice));
    }

    [Test]
    public void ListInOperator()
    {
        var filter = new Evaluator(@"1u in [1u, 2u]}");
        Assert.IsTrue(filter.EvaluateBool(_Alice));

        filter = new Evaluator(@"2u in [1u, 2u]}");
        Assert.IsTrue(filter.EvaluateBool(_Alice));

        filter = new Evaluator(@"3u in [1u, 2u]}");
        Assert.IsFalse(filter.EvaluateBool(_Alice));
    }

    [Test]
    public void MapEquality()
    {
        var filter = new Evaluator(@"{1u: 1, 2u: 4} == {1u: 1, 2u: 4}");
        Assert.IsTrue(filter.EvaluateBool(_Alice));

        filter = new Evaluator(@"{1u: 1, 2u: 4} != {1u: 1, 2u: 4}");
        Assert.IsFalse(filter.EvaluateBool(_Alice));

        filter = new Evaluator(@"{1u: 1, 2u: 4} == {1u: 1, 2u: 4, 3u: 9}");
        Assert.IsFalse(filter.EvaluateBool(_Alice));

        filter = new Evaluator(@"{1u: 1, 2u: 4} != {1u: 1, 2u: 4, 3u: 9}");
        Assert.IsTrue(filter.EvaluateBool(_Alice));
    }

    [Test]
    public void MapInOperator()
    {
        var filter = new Evaluator(@"1u in {1u: 1, 2u: 4}");
        Assert.IsTrue(filter.EvaluateBool(_Alice));

        filter = new Evaluator(@"3u in {1u: 1, 2u: 4}");
        Assert.IsFalse(filter.EvaluateBool(_Alice));
    }

    [Test]
    public void MapRead()
    {
        var filter = new Evaluator(
            @"goals_by_day[""Monday""] == ""Work"" && goals_by_day[""Tuesday""] == ""Rest"""
        );
        Assert.IsTrue(filter.EvaluateBool(_Alice));
    }

    [Test]
    public void MapNegativeTest()
    {
        var filter = new Evaluator(
            "\"Saturday\" in goals_by_day && goals_by_day.Saturday == \"Sleep\""
        );
        Assert.IsFalse(filter.EvaluateBool(_Alice));
    }

    [Test]
    public void MapReadInvalidKey()
    {
        Assert.Throws<NoSuchFieldException>(() =>
        {
            var filter = new Evaluator(@"goals_by_day[""Foo""] == ""Bar""");
            Assert.IsTrue(filter.EvaluateBool(_Alice));
        });
    }

    [TestCase("\"A\" in nicknames", true)]
    [TestCase("\"Z\" in nicknames", false)]
    public void ReadList(string expression, bool expectedValue)
    {
        var filter = new Evaluator(expression);
        Assert.AreEqual(expectedValue, filter.EvaluateBool(_Alice));
    }

    [Test]
    public void Null()
    {
        var filter = new Evaluator(@"null == null");
        Assert.IsTrue(filter.EvaluateBool(_Alice));

        filter = new Evaluator(@"null != null");
        Assert.IsFalse(filter.EvaluateBool(_Alice));
    }

    [Test]
    public void GlobalStyleFunction()
    {
        FunctionRegistry.Instance.Register(
            "decrement",
            Value.Int (Value.Int value) => new Value.Int(value.Value - 1),
            style: ReceiverStyle.Global
        );
        var filter = new Evaluator("decrement(2)");
        Assert.AreEqual(Value.From(1), filter.Evaluate(_Alice));
    }

    [Test]
    public void ReceiverStyleFunction()
    {
        FunctionRegistry.Instance.Register(
            "increment",
            Value.Int (Value.Int value) => new Value.Int(value.Value + 1),
            style: ReceiverStyle.Receiver
        );
        var filter = new Evaluator("1.increment()");
        Assert.AreEqual(Value.From(2), filter.Evaluate(_Alice));
    }

    [Test]
    public void BuiltInFunction()
    {
        var filter = new Evaluator(@"""foo"".startsWith(""f"")");
        Assert.IsTrue(filter.EvaluateBool(_Alice));
    }

    [Test]
    public void UnsetMessageFields()
    {
        // Make sure that we are covering the case where the message field is actually unset.
        Assert.IsNull(_Alice.Job);

        var filter = new Evaluator(@"true");
        Assert.IsTrue(filter.EvaluateBool(_Alice));
    }

    [TestCase("has(favorites.book.title)", true)]
    [TestCase("has(favorites.book.isbn_number)", false)]
    [TestCase("has(goals_by_day.Monday)", true)]
    [TestCase("has(goals_by_day.Saturday)", false)]
    public void Macros(string expression, bool result)
    {
        var filter = new Evaluator(expression);
        Assert.AreEqual(result, filter.EvaluateBool(_Alice));
    }

    public void Macro()
    {
        var filter = new Evaluator(@"has(favorites.book.title)");
        Assert.IsTrue(filter.EvaluateBool(_Alice));

        filter = new Evaluator(@"has(favorites.book.isbn_number)");
        Assert.IsFalse(filter.EvaluateBool(_Alice));
    }
}
