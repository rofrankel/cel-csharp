//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.11.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from c:\git\cel-csharp\Cel\CommonExpressionLanguage.g4 by ANTLR 4.11.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace Cel {
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete generic visitor for a parse tree produced
/// by <see cref="CommonExpressionLanguageParser"/>.
/// </summary>
/// <typeparam name="Result">The return type of the visit operation.</typeparam>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.11.1")]
[System.CLSCompliant(false)]
public interface ICommonExpressionLanguageVisitor<Result> : IParseTreeVisitor<Result> {
	/// <summary>
	/// Visit a parse tree produced by <see cref="CommonExpressionLanguageParser.start"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStart([NotNull] CommonExpressionLanguageParser.StartContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="CommonExpressionLanguageParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr([NotNull] CommonExpressionLanguageParser.ExprContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="CommonExpressionLanguageParser.conditionalOr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitConditionalOr([NotNull] CommonExpressionLanguageParser.ConditionalOrContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="CommonExpressionLanguageParser.conditionalAnd"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitConditionalAnd([NotNull] CommonExpressionLanguageParser.ConditionalAndContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="CommonExpressionLanguageParser.relation"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitRelation([NotNull] CommonExpressionLanguageParser.RelationContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="CommonExpressionLanguageParser.calc"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCalc([NotNull] CommonExpressionLanguageParser.CalcContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>MemberExpr</c>
	/// labeled alternative in <see cref="CommonExpressionLanguageParser.unary"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMemberExpr([NotNull] CommonExpressionLanguageParser.MemberExprContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>LogicalNot</c>
	/// labeled alternative in <see cref="CommonExpressionLanguageParser.unary"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLogicalNot([NotNull] CommonExpressionLanguageParser.LogicalNotContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Negate</c>
	/// labeled alternative in <see cref="CommonExpressionLanguageParser.unary"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitNegate([NotNull] CommonExpressionLanguageParser.NegateContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>MemberCall</c>
	/// labeled alternative in <see cref="CommonExpressionLanguageParser.member"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMemberCall([NotNull] CommonExpressionLanguageParser.MemberCallContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Select</c>
	/// labeled alternative in <see cref="CommonExpressionLanguageParser.member"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSelect([NotNull] CommonExpressionLanguageParser.SelectContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>PrimaryExpr</c>
	/// labeled alternative in <see cref="CommonExpressionLanguageParser.member"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPrimaryExpr([NotNull] CommonExpressionLanguageParser.PrimaryExprContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Index</c>
	/// labeled alternative in <see cref="CommonExpressionLanguageParser.member"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIndex([NotNull] CommonExpressionLanguageParser.IndexContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>IdentOrGlobalCall</c>
	/// labeled alternative in <see cref="CommonExpressionLanguageParser.primary"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIdentOrGlobalCall([NotNull] CommonExpressionLanguageParser.IdentOrGlobalCallContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Nested</c>
	/// labeled alternative in <see cref="CommonExpressionLanguageParser.primary"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitNested([NotNull] CommonExpressionLanguageParser.NestedContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>CreateList</c>
	/// labeled alternative in <see cref="CommonExpressionLanguageParser.primary"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCreateList([NotNull] CommonExpressionLanguageParser.CreateListContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>CreateStruct</c>
	/// labeled alternative in <see cref="CommonExpressionLanguageParser.primary"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCreateStruct([NotNull] CommonExpressionLanguageParser.CreateStructContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>CreateMessage</c>
	/// labeled alternative in <see cref="CommonExpressionLanguageParser.primary"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCreateMessage([NotNull] CommonExpressionLanguageParser.CreateMessageContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>ConstantLiteral</c>
	/// labeled alternative in <see cref="CommonExpressionLanguageParser.primary"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitConstantLiteral([NotNull] CommonExpressionLanguageParser.ConstantLiteralContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="CommonExpressionLanguageParser.exprList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExprList([NotNull] CommonExpressionLanguageParser.ExprListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="CommonExpressionLanguageParser.listInit"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitListInit([NotNull] CommonExpressionLanguageParser.ListInitContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="CommonExpressionLanguageParser.fieldInitializerList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFieldInitializerList([NotNull] CommonExpressionLanguageParser.FieldInitializerListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="CommonExpressionLanguageParser.optField"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitOptField([NotNull] CommonExpressionLanguageParser.OptFieldContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="CommonExpressionLanguageParser.mapInitializerList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMapInitializerList([NotNull] CommonExpressionLanguageParser.MapInitializerListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="CommonExpressionLanguageParser.optExpr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitOptExpr([NotNull] CommonExpressionLanguageParser.OptExprContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Int</c>
	/// labeled alternative in <see cref="CommonExpressionLanguageParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitInt([NotNull] CommonExpressionLanguageParser.IntContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Uint</c>
	/// labeled alternative in <see cref="CommonExpressionLanguageParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitUint([NotNull] CommonExpressionLanguageParser.UintContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Double</c>
	/// labeled alternative in <see cref="CommonExpressionLanguageParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDouble([NotNull] CommonExpressionLanguageParser.DoubleContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>String</c>
	/// labeled alternative in <see cref="CommonExpressionLanguageParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitString([NotNull] CommonExpressionLanguageParser.StringContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Bytes</c>
	/// labeled alternative in <see cref="CommonExpressionLanguageParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBytes([NotNull] CommonExpressionLanguageParser.BytesContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>BoolTrue</c>
	/// labeled alternative in <see cref="CommonExpressionLanguageParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBoolTrue([NotNull] CommonExpressionLanguageParser.BoolTrueContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>BoolFalse</c>
	/// labeled alternative in <see cref="CommonExpressionLanguageParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBoolFalse([NotNull] CommonExpressionLanguageParser.BoolFalseContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Null</c>
	/// labeled alternative in <see cref="CommonExpressionLanguageParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitNull([NotNull] CommonExpressionLanguageParser.NullContext context);
}
} // namespace Cel
