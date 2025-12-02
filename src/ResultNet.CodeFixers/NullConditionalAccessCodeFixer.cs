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
/// Code fixer for RN007: Do not use null-conditional operators with Result types
/// Offers two fixes: conditional expression or Match() method
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NullConditionalAccessCodeFixer)), Shared]
public class NullConditionalAccessCodeFixer : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("RN007");

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
        
        ConditionalAccessExpressionSyntax? conditionalAccess = node as ConditionalAccessExpressionSyntax 
            ?? node.FirstAncestorOrSelf<ConditionalAccessExpressionSyntax>();

        if (conditionalAccess == null)
            return;

        // Option 1: Replace with conditional expression
        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Replace with conditional expression",
                createChangedDocument: c => ReplaceWithConditionalAsync(context.Document, conditionalAccess, c),
                equivalenceKey: "ReplaceWithConditional"),
            diagnostic);
    }

    private static async Task<Document> ReplaceWithConditionalAsync(
        Document document,
        ConditionalAccessExpressionSyntax conditionalAccess,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
            return document;

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel == null)
            return document;

        // Build list of replacements
        var replacements = new Dictionary<SyntaxNode, SyntaxNode>();

        // result?.Property -> result.IsSuccess ? result.Value.Property : default
        var condition = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            conditionalAccess.Expression,
            SyntaxFactory.IdentifierName("IsSuccess"));

        // Build the when-true expression (access through .Value)
        var whenTrue = BuildAccessExpression(conditionalAccess);

        // For when-false, use default
        var whenFalse = SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression);

        var conditionalExpression = SyntaxFactory.ConditionalExpression(
            condition,
            whenTrue,
            whenFalse)
            .WithTriviaFrom(conditionalAccess);

        replacements[conditionalAccess] = conditionalExpression;

        // Transform parameter types
        var typeInfo = semanticModel.GetTypeInfo(conditionalAccess.Expression);
        if (typeInfo.Type != null)
        {
            CodeFixHelpers.AddParameterTransformation(root, conditionalAccess.Expression, typeInfo.Type, semanticModel, replacements);
        }

        // Apply all replacements
        var newRoot = root.ReplaceNodes(replacements.Keys, (oldNode, newNode) => replacements[oldNode]);

        // Add using ResultNet
        newRoot = CodeFixHelpers.AddResultNetUsing(newRoot);

        return document.WithSyntaxRoot(newRoot);
    }

    private static ExpressionSyntax BuildAccessExpression(ConditionalAccessExpressionSyntax conditionalAccess)
    {
        // Convert result?.Property to result.Value.Property (since result is now Result<T>)
        var resultValue = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            conditionalAccess.Expression,
            SyntaxFactory.IdentifierName("Value"));

        if (conditionalAccess.WhenNotNull is MemberBindingExpressionSyntax memberBinding)
        {
            // result?.Property -> result.Value.Property
            return SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                resultValue,
                memberBinding.Name);
        }
        else if (conditionalAccess.WhenNotNull is ElementBindingExpressionSyntax elementBinding)
        {
            // result?[0] -> result.Value[0]
            return SyntaxFactory.ElementAccessExpression(
                resultValue,
                elementBinding.ArgumentList);
        }
        else if (conditionalAccess.WhenNotNull is InvocationExpressionSyntax invocation)
        {
            // Handle result?.Method() -> result.Value.Method()
            // The invocation expression will be something like .Method()
            // where Expression is a MemberBindingExpressionSyntax
            if (invocation.Expression is MemberBindingExpressionSyntax methodBinding)
            {
                var methodAccess = SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    resultValue,
                    methodBinding.Name);
                
                return SyntaxFactory.InvocationExpression(
                    methodAccess,
                    invocation.ArgumentList);
            }
            
            // Fallback for other invocation patterns
            return SyntaxFactory.InvocationExpression(
                resultValue,
                invocation.ArgumentList);
        }

        // Fallback
        return resultValue;
    }
}