using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ResultNet.CodeFixers;

/// <summary>
/// Code fixer for RN008: Do not use null-forgiving operator with Result types
/// Removes the ! operator from Result expressions.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NullForgivingOperatorCodeFixer)), Shared]
public class NullForgivingOperatorCodeFixer : CodeFixProvider
{
    private const string Title = "Remove null-forgiving operator";

    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("RN008");

    public sealed override FixAllProvider GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null)
            return;

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the suppress nullable warning expression (!)
        var node = root.FindNode(diagnosticSpan);
        var suppressExpression = node.FirstAncestorOrSelf<PostfixUnaryExpressionSyntax>();
        
        if (suppressExpression == null || !suppressExpression.IsKind(SyntaxKind.SuppressNullableWarningExpression))
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: Title,
                createChangedDocument: c => RemoveNullForgivingOperatorAsync(context.Document, suppressExpression, c),
                equivalenceKey: "RemoveNullForgiving"),
            diagnostic);
    }

    private static async Task<Document> RemoveNullForgivingOperatorAsync(
        Document document,
        PostfixUnaryExpressionSyntax suppressExpression,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
            return document;

        // Replace the suppress expression with just the operand, preserving trivia
        var operand = suppressExpression.Operand;
        var newOperand = operand.WithTriviaFrom(suppressExpression);

        var newRoot = root.ReplaceNode(suppressExpression, newOperand);
        return document.WithSyntaxRoot(newRoot);
    }
}
