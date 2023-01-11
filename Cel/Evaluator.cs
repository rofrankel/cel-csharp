using Antlr4.Runtime;
using Google.Protobuf;
using System.Diagnostics;

namespace Cel;

public class Evaluator
{
    private readonly CommonExpressionLanguageParser.ExprContext _Expr;

    public Evaluator(string expr)
    {
        expr = Macros.Rewrite(expr);
        var lexer = new CommonExpressionLanguageLexer(new AntlrInputStream(expr));
        var tokenStream = new CommonTokenStream(lexer);
        var parserOutput = new StringWriter();
        var parserErrors = new StringWriter();

        var parser = new CommonExpressionLanguageParser(tokenStream, parserOutput, parserErrors);
        _Expr = parser.expr();

        if (parser.NumberOfSyntaxErrors > 0)
        {
            throw new ArgumentException(
                $"expr '{expr}' is not valid: {parserErrors}",
                nameof(expr)
            );
        }

        Debug.WriteLine("Parsed `{0}` as `{1}`", expr, _Expr.ToStringTree(parser));
    }

    public Value Evaluate(IMessage message)
    {
        var data = Value.From(message);

        var visitor = new EvaluatorVisitor(data);
        return visitor.Visit(_Expr);
    }

    /// Wrapper for Evaluate that requires the result to be Boolean, and then returns a bool.
    public bool EvaluateBool(IMessage message)
    {
        var result = Evaluate(message);
        return result switch
        {
            Value.Bool boolValue => boolValue.Value,
            _
                => throw new ArgumentException(
                    $"Not a boolean expression: `{result}` has type `{result.GetType()}`"
                ),
        };
    }

    private class EvaluatorVisitor : CommonExpressionLanguageBaseVisitor<Value>
    {
        private readonly Value.Map _Data;

        public EvaluatorVisitor(Value data)
        {
            _Data = data switch
            {
                Value.Map fields => fields,
                _
                    => throw new ArgumentException(
                        $"Evaluation context must be a message or dictionary, not a `{data.GetType()}`",
                        "data"
                    ),
            };
        }

        public override Value VisitExpr(CommonExpressionLanguageParser.ExprContext context)
        {
            var e = Visit(context.e);

            if (context.e1 != null)
            {
                return e switch
                {
                    Value.Bool boolValue => boolValue.Value ? Visit(context.e1) : Visit(context.e2),
                    _
                        => throw new TypeException(
                            "Cannot use ternary operator (`?`) with non-Boolean condition"
                        ),
                };
            }

            return e;
        }

        public override Value VisitConditionalOr(
            CommonExpressionLanguageParser.ConditionalOrContext context
        )
        {
            // If there is an `||`, all relations must be Boolean; return their disjunction.
            if (context._ops.Count > 0)
            {
                foreach (var and in context.conditionalAnd())
                {
                    if (((Value.Bool)Visit(and)).Value)
                    {
                        return Value.From(true);
                    }
                }

                return Value.From(false);
            }

            return Visit(context.conditionalAnd()[0]);
        }

        public override Value VisitConditionalAnd(
            CommonExpressionLanguageParser.ConditionalAndContext context
        )
        {
            // If there is an `&&`, all relations must be Boolean; return their conjunction.
            if (context._ops.Count > 0)
            {
                foreach (var relation in context.relation())
                {
                    if (!((Value.Bool)Visit(relation)).Value)
                    {
                        return Value.From(false);
                    }
                }

                return Value.From(true);
            }

            return Visit(context.relation()[0]);
        }

        public override Value VisitRelation(CommonExpressionLanguageParser.RelationContext context)
        {
            var calc = context.calc();
            if (calc != null)
            {
                return Visit(calc);
            }

            var relation = context.relation();
            var a = Visit(relation[0]);
            var b = Visit(relation[1]);

            return new Value.Bool(
                context.op.Text switch
                {
                    ">" => a > b,
                    "<" => a < b,
                    "<=" => a <= b,
                    ">=" => a >= b,
                    "==" => a.DeepEquals(b),
                    "!=" => !a.DeepEquals(b),
                    "in"
                        => b switch
                        {
                            Value.Map fields => fields.Value.ContainsKey(a),
                            Value.List values => values.Value.Contains(a),
                            _
                                => throw new TypeException(
                                    $"The `in` operator can only be used with lists and maps; `{b}` is neither"
                                ),
                        },
                    _
                        => throw new TypeException(
                            $"Unsupported operator `{context.op.Text}` between `{a.GetType()}` and `{b.GetType()}` in relation `{context.GetText()}`"
                        ),
                }
            );
        }

        public override Value VisitCalc(CommonExpressionLanguageParser.CalcContext context)
        {
            var unary = context.unary();
            if (unary != null)
            {
                return Visit(unary);
            }

            var calc = context.calc();
            var a = Visit(calc[0]);
            var b = Visit(calc[1]);

            return context.op.Text switch
            {
                "*" => a * b,
                "/" => a / b,
                "%" => a % b,
                "+" => a + b,
                "-" => a - b,
                _ => throw new SyntaxException($"Unrecognized operator `{context.op.Text}`")
            };
        }

        public override Value VisitMemberExpr(
            CommonExpressionLanguageParser.MemberExprContext context
        ) => Visit(context.member());

        public override Value VisitLogicalNot(
            CommonExpressionLanguageParser.LogicalNotContext context
        )
        {
            var member = Visit(context.member());
            if (context._ops.Count % 2 == 1)
            {
                return !member;
            }

            return member;
        }

        public override Value VisitNegate(CommonExpressionLanguageParser.NegateContext context)
        {
            var member = Visit(context.member());
            if (context._ops.Count % 2 == 1)
            {
                return !member;
            }

            return member;
        }

        public override Value VisitPrimaryExpr(
            CommonExpressionLanguageParser.PrimaryExprContext context
        ) => Visit(context.primary());

        public override Value VisitSelect(CommonExpressionLanguageParser.SelectContext context)
        {
            var member = Visit(context.member());

            // Handle the ".?" operator when the LHS is null.
            if (context.opt != null)
            {
                if (member is Value.Null)
                {
                    return member;
                }
            }

            return member switch
            {
                Value.Map fields => fields.Value[Value.From(context.id.Text)],
                _
                    => throw new TypeException(
                        $"Selection operator (`.`) not supported for value `{member}`"
                    ),
            };
        }

        public override Value VisitMemberCall(
            CommonExpressionLanguageParser.MemberCallContext context
        )
        {
            var registry = FunctionRegistry.Instance;

            var member = Visit(context.member());
            var name = context.IDENTIFIER().GetText();

            var args = new List<Value> { member };
            var exprList = context.exprList();
            if (exprList != null)
            {
                args.AddRange(((Value.List)Visit(exprList)).Value);
            }

            return registry.Evaluate(name, ReceiverStyle.Receiver, args);
        }

        public override Value VisitIndex(CommonExpressionLanguageParser.IndexContext context)
        {
            var member = Visit(context.member());

            // Handle the ".?" operator when the LHS is null.
            if (context.opt != null)
            {
                if (member is Value.Null)
                {
                    return member;
                }
            }

            var index = Visit(context.expr());
            return member[index];
        }

        public override Value VisitIdentOrGlobalCall(
            CommonExpressionLanguageParser.IdentOrGlobalCallContext context
        )
        {
            if (context.op != null)
            {
                var registry = FunctionRegistry.Instance;
                var name = context.IDENTIFIER().GetText();
                var exprList = context.exprList();
                var args = new List<Value>();
                if (exprList != null)
                {
                    args.AddRange(((Value.List)Visit(exprList)).Value);
                }

                return registry.Evaluate(name, ReceiverStyle.Global, args);
            }

            var identifier = context.IDENTIFIER().GetText();
            if (!_Data.Value.ContainsKey(Value.From(identifier)))
            {
                throw new NoSuchIdentifierException(identifier);
            }

            return _Data.Value[Value.From(identifier)];
        }

        public override Value VisitNested(CommonExpressionLanguageParser.NestedContext context) =>
            Visit(context.expr());

        public override Value VisitCreateList(
            CommonExpressionLanguageParser.CreateListContext context
        ) => Visit(context.elems);

        public override Value VisitCreateStruct(
            CommonExpressionLanguageParser.CreateStructContext context
        ) => Visit(context.entries);

        public override Value VisitCreateMessage(
            CommonExpressionLanguageParser.CreateMessageContext context
        )
        {
            throw new NotImplementedException("Message initialization is not yet supported");
        }

        public override Value VisitConstantLiteral(
            CommonExpressionLanguageParser.ConstantLiteralContext context
        ) => Visit(context.literal());

        public override Value VisitExprList(
            CommonExpressionLanguageParser.ExprListContext context
        ) => Value.From(context._e.Select(Visit).ToList());

        public override Value VisitListInit(
            CommonExpressionLanguageParser.ListInitContext context
        ) => Value.From(context._elems.Select(Visit).ToList());

        public override Value VisitFieldInitializerList(
            CommonExpressionLanguageParser.FieldInitializerListContext context
        )
        {
            var pairs = context._fields.Select(Visit).Zip(context._values.Select(Visit));

            return Value.From(
                pairs.ToDictionary<(Value, Value), Value, Value>(kv => kv.Item1, kv => kv.Item2)
            );
        }

        public override Value VisitOptField(CommonExpressionLanguageParser.OptFieldContext context)
        {
            // TODO: What is the right way to handle the optional leading `?` here?
            return Visit(context.IDENTIFIER());
        }

        public override Value VisitMapInitializerList(
            CommonExpressionLanguageParser.MapInitializerListContext context
        )
        {
            var pairs = context._keys.Select(Visit).Zip(context._values.Select(Visit));

            return Value.From(pairs.ToDictionary(kv => kv.Item1, kv => kv.Item2));
        }

        public override Value VisitOptExpr(CommonExpressionLanguageParser.OptExprContext context)
        {
            // TODO: What is the right way to handle the optional leading `?` here?
            return Visit(context.expr());
        }

        // Start literals

        public override Value VisitInt(CommonExpressionLanguageParser.IntContext context)
        {
            var value = long.Parse(context.NUM_INT().GetText());
            if (context.MINUS() != null)
            {
                value = -value;
            }

            return Value.From(value);
        }

        public override Value VisitUint(CommonExpressionLanguageParser.UintContext context) =>
            Value.From(ulong.Parse(context.NUM_UINT().GetText().TrimEnd('u', 'U')));

        public override Value VisitDouble(CommonExpressionLanguageParser.DoubleContext context)
        {
            var value = double.Parse(context.NUM_FLOAT().GetText());
            if (context.MINUS() != null)
            {
                value = -value;
            }

            return Value.From(value);
        }

        public override Value VisitString(CommonExpressionLanguageParser.StringContext context) =>
            Value.From(context.STRING().GetText().Trim('"'));

        public override Value VisitBytes(CommonExpressionLanguageParser.BytesContext context) =>
            Value.From(ByteString.CopyFromUtf8(context.BYTES().GetText()));

        public override Value VisitBoolTrue(
            CommonExpressionLanguageParser.BoolTrueContext context
        ) => Value.From(true);

        public override Value VisitBoolFalse(
            CommonExpressionLanguageParser.BoolFalseContext context
        ) => Value.From(false);

        public override Value VisitNull(CommonExpressionLanguageParser.NullContext context)
        {
            return new Value.Null();
        }

        // End literals
    }
}
