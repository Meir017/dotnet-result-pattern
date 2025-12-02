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
/// Code fixer for RN006: Do not use null-coalescing assignment with nullable types
/// Replaces `value ??= fallback` with `if (value.IsFailure) value = fallback`
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NullCoalescingAssignmentCodeFixer)), Shared]
public class NullCoalescingAssignmentCodeFixer : CodeFixProvider
{
    private const string Title = "Replace with explicit Result check";

    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("RN006");

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
        var assignmentExpression = node as AssignmentExpressionSyntax 
            ?? node.FirstAncestorOrSelf<AssignmentExpressionSyntax>();

        if (assignmentExpression == null || !assignmentExpression.IsKind(SyntaxKind.CoalesceAssignmentExpression))
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: Title,
                createChangedDocument: c => ReplaceWithExplicitCheckAsync(context.Document, assignmentExpression, c),
                equivalenceKey: "ReplaceNullCoalescingAssignment"),
            diagnostic);
    }

    private static async Task<Document> ReplaceWithExplicitCheckAsync(
        Document document,
        AssignmentExpressionSyntax assignmentExpression,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
            return document;

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel == null)
            return document;

        // Get the type of the left side
        var leftType = semanticModel.GetTypeInfo(assignmentExpression.Left).Type;
        if (leftType == null)
            return document;

        var replacements = new Dictionary<SyntaxNode, SyntaxNode>();

        // value ??= fallback -> if (value.IsFailure) value = fallback;
        var condition = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            assignmentExpression.Left,
            SyntaxFactory.IdentifierName("IsFailure"));

        var simpleAssignment = SyntaxFactory.AssignmentExpression(
            SyntaxKind.SimpleAssignmentExpression,
            assignmentExpression.Left,
            assignmentExpression.Right);

        var ifStatement = SyntaxFactory.IfStatement(
            condition,
            SyntaxFactory.ExpressionStatement(simpleAssignment));

        // Replace the expression statement containing the assignment
        var expressionStatement = assignmentExpression.FirstAncestorOrSelf<ExpressionStatementSyntax>();
        if (expressionStatement != null)
        {
            replacements[expressionStatement] = ifStatement.WithTriviaFrom(expressionStatement);
        }

        // Transform parameter or variable type if needed
        CodeFixHelpers.AddParameterTransformation(root, assignmentExpression.Left, leftType, semanticModel, replacements);

        // Apply all replacements
        var newRoot = root.ReplaceNodes(replacements.Keys, (oldNode, newNode) => replacements[oldNode]);

        // Add using ResultNet
        newRoot = CodeFixHelpers.AddResultNetUsing(newRoot);

        return document.WithSyntaxRoot(newRoot);
    }
}
