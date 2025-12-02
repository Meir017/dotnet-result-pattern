using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ResultNet.CodeFixers;

/// <summary>
/// Code fixer for RN003: Do not use null-coalescing operator with nullable types
/// Transforms nullable types to Result types and replaces ?? with .ValueOr() or conditional expression
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NullCoalescingCodeFixer)), Shared]
public class NullCoalescingCodeFixer : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("RN003");

    public sealed override FixAllProvider GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null)
            return;

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var node = root.FindNode(diagnosticSpan);
        var binaryExpression = node as BinaryExpressionSyntax 
            ?? node.FirstAncestorOrSelf<BinaryExpressionSyntax>();

        if (binaryExpression == null || !binaryExpression.IsKind(SyntaxKind.CoalesceExpression))
            return;

        // Option 1: Use .ValueOr() extension method
        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Replace with .ValueOr()",
                createChangedDocument: c => ReplaceWithValueOrAsync(context.Document, binaryExpression, c),
                equivalenceKey: "ReplaceWithValueOr"),
            diagnostic);

        // Option 2: Replace with conditional expression
        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Replace with conditional expression",
                createChangedDocument: c => ReplaceWithConditionalAsync(context.Document, binaryExpression, c),
                equivalenceKey: "ReplaceWithConditional"),
            diagnostic);
    }

    private static async Task<Document> ReplaceWithValueOrAsync(
        Document document,
        BinaryExpressionSyntax binaryExpression,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
            return document;

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel == null)
            return document;

        // Get the type of the left side
        var leftType = semanticModel.GetTypeInfo(binaryExpression.Left).Type;
        if (leftType == null)
            return document;

        var replacements = new Dictionary<SyntaxNode, SyntaxNode>();

        // value ?? fallback -> value.ValueOr(fallback)
        var invocation = SyntaxFactory.InvocationExpression(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                binaryExpression.Left,
                SyntaxFactory.IdentifierName("ValueOr")),
            SyntaxFactory.ArgumentList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Argument(binaryExpression.Right))))
            .WithTriviaFrom(binaryExpression);

        replacements[binaryExpression] = invocation;

        // Transform parameter or variable type if needed
        CodeFixHelpers.AddParameterTransformation(root, binaryExpression.Left, leftType, semanticModel, replacements);

        // Apply all replacements
        var newRoot = root.ReplaceNodes(replacements.Keys, (oldNode, newNode) => replacements[oldNode]);

        // Add using ResultNet
        newRoot = CodeFixHelpers.AddResultNetUsing(newRoot);

        return document.WithSyntaxRoot(newRoot);
    }

    private static async Task<Document> ReplaceWithConditionalAsync(
        Document document,
        BinaryExpressionSyntax binaryExpression,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
            return document;

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel == null)
            return document;

        // Get the type of the left side
        var leftType = semanticModel.GetTypeInfo(binaryExpression.Left).Type;
        if (leftType == null)
            return document;

        var replacements = new Dictionary<SyntaxNode, SyntaxNode>();

        // value ?? fallback -> value.IsFailure ? fallback : value.Value
        var condition = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            binaryExpression.Left,
            SyntaxFactory.IdentifierName("IsFailure"));

        var valueAccess = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            binaryExpression.Left,
            SyntaxFactory.IdentifierName("Value"));

        var conditionalExpression = SyntaxFactory.ConditionalExpression(
            condition,
            binaryExpression.Right,
            valueAccess)
            .WithTriviaFrom(binaryExpression);

        replacements[binaryExpression] = conditionalExpression;

        // Transform parameter or variable type if needed
        CodeFixHelpers.AddParameterTransformation(root, binaryExpression.Left, leftType, semanticModel, replacements);

        // Apply all replacements
        var newRoot = root.ReplaceNodes(replacements.Keys, (oldNode, newNode) => replacements[oldNode]);

        // Add using ResultNet
        newRoot = CodeFixHelpers.AddResultNetUsing(newRoot);

        return document.WithSyntaxRoot(newRoot);
    }
}
