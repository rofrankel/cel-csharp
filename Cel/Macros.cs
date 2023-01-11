using System.Text.RegularExpressions;

namespace Cel;

internal static class Macros
{
    // This matches the IDENTIFIER definition in the grammar.
    private const string Identifier = @"[a-zA-Z_][a-zA-Z0-9_]+";

    // This matches a subset of the Select definition in the grammar.
    private const string SimpleSelect = $@"{Identifier}(\.{Identifier})+";

    public static string Rewrite(string expr)
    {
        expr = RewriteHas(expr);

        return expr;
    }

    private static string RewriteHas(string expr)
    {
        return Regex.Replace(
            expr,
            $@"has\(({SimpleSelect})\)",
            match =>
            {
                var segments = match.Groups[1].Captures[0].Value.Split(".");

                var inChecks = new List<string>();
                var prefix = segments[0];
                for (int i = 0; i < segments.Length - 1; ++i)
                {
                    inChecks.Add($@"""{segments[i + 1]}"" in {prefix}");
                    prefix = $"{prefix}.{segments[i + 1]}";
                }

                return $"({string.Join(" && ", inChecks)})";
            }
        );
    }
}
