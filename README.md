# CEL for C#

This library contains a generated C# parser, and an evaluator, for [CEL][]. The
parser is generated with [ANTLR4](https://www.antlr.org/).

An ANTLR4-compatible version of the CEL grammar is included at
`Cel/CommonExpressionLanguage.g4`.

## Evaluator spec compliance

The evaluator currently supports most of the CEL spec.

Known omissions include:

- Conformance testing.
- Support for converting `string`s and `int`s to enum types.
- Support for the `dyn` type.
- Support for type values and the standard function `type()`.
- Any kind of lazy object evaluation.

In addition, most macros (optional in the spec) are not supported. `has()` is
supported, but only for a subset of the functionality in the spec. In
particular, it only supports `map` semantics (as opposed to proto2 or proto3
semantics), and is implemented as a simple rewrite, before any type checking or
evaluation.

## Regenerating parser

In order to regenerate the parser with VS Code:

1. Install the
   [ANTLR4 grammar syntax support](https://marketplace.visualstudio.com/items?itemName=mike-lischke.vscode-antlr4)
   extension, if not already installed.

2. Simply edit and save `Cel/CommonExpressionLanguage.g4`.

This, combined with the settings in `.vscode/settings.json`, should regenerate
all the generated C# files.

## Repo status

This is the first .NET code I've ever written, and the first time I've tried to
implement CEL. Expect newbie mistakes in both domains. This code is complete
enough to be useful, while also having the significant known gaps described
above. It was a "hack week" style project I don't have time to maintain; it's
provided in the hope that someone else will find it a useful starting point for
a more complete implementation.

<!-- prettier-ignore-start -->

[CEL]: https://github.com/google/cel-spec

<!-- prettier-ignore-end -->
