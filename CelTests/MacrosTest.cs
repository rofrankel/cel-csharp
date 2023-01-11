using NUnit.Framework;

namespace Cel.Tests.Unit;

[TestFixture]
public class MacrosTest
{
    [Test]
    public void Has()
    {
        Assert.AreEqual(
            @"true && (""bar"" in foo && ""baz"" in foo.bar && ""qux"" in foo.bar.baz) || true",
            Macros.Rewrite("true && has(foo.bar.baz.qux) || true")
        );
    }
}
