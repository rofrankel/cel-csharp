using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using NUnit.Framework;

namespace Cel.Tests.Unit;

[TestFixture]
public class ParserTest
{
    private class TestCelVisitor : CommonExpressionLanguageBaseVisitor<object>
    {
        public List<string> ConditionalOrs { get; } = new List<string>();
        public List<string> Relations { get; } = new List<string>();

        public override object VisitConditionalOr(
            [NotNull] CommonExpressionLanguageParser.ConditionalOrContext context
        )
        {
            ConditionalOrs.Add(context.GetText());

            return VisitChildren(context);
        }

        public override object VisitRelation(
            [NotNull] CommonExpressionLanguageParser.RelationContext context
        )
        {
            Relations.Add(context.GetText());

            return VisitChildren(context);
        }
    }

    /// Create a parser for the given CEL expression.
    ///
    /// If valid is true, assert that there are no parsing errors; if false, assert that there's at least one.
    private CommonExpressionLanguageParser.ExprContext ParseExprAndAssertValidity(
        string input,
        bool valid
    )
    {
        var lexer = new CommonExpressionLanguageLexer(new AntlrInputStream(input));
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new CommonExpressionLanguageParser(tokenStream);
        var expr = parser.expr();

        Console.WriteLine("Parsed '{0}' as '{1}'", input, expr.ToStringTree(parser));

        if (valid)
        {
            Assert.AreEqual(0, parser.NumberOfSyntaxErrors);
        }
        else
        {
            Assert.AreNotEqual(0, parser.NumberOfSyntaxErrors);
        }

        return expr;
    }

    [Test]
    public void CanParseValidExpr()
    {
        var expr = ParseExprAndAssertValidity("a < 10 || a >= 100", valid: true);

        var visitor = new TestCelVisitor();
        visitor.Visit(expr);

        CollectionAssert.AreEqual(new List<string> { "a<10||a>=100" }, visitor.ConditionalOrs);

        CollectionAssert.AreEqual(
            new List<string> { "a<10", "a", "10", "a>=100", "a", "100" },
            visitor.Relations
        );
    }

    [Test]
    public void CannotParseInvalidExpr()
    {
        var expr = ParseExprAndAssertValidity("a || ||", valid: false);
    }
}
