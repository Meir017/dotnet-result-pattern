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
/// Code fixer for RN004: Do not check Result types for null
/// Offers two fixes: replace with .IsFailure or .IsSuccess
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NullCheckCodeFixer)), Shared]
public class NullCheckCodeFixer : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("RN004");

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
        
        // Find the comparison expression
        BinaryExpressionSyntax? binaryExpression = node as BinaryExpressionSyntax 
            ?? node.FirstAncestorOrSelf<BinaryExpressionSyntax>();
        
        IsPatternExpressionSyntax? isPatternExpression = node as IsPatternExpressionSyntax 
            ?? node.FirstAncestorOrSelf<IsPatternExpressionSyntax>();

        if (binaryExpression != null)
        {
            RegisterBinaryExpressionFixes(context, binaryExpression, diagnostic);
        }
        else if (isPatternExpression != null)
        {
            RegisterPatternFixes(context, isPatternExpression, diagnostic);
        }
    }

    private void RegisterBinaryExpressionFixes(CodeFixContext context, BinaryExpressionSyntax binaryExpression, Diagnostic diagnostic)
    {
        var isEqualsNull = binaryExpression.IsKind(SyntaxKind.EqualsExpression);
        var isNotEqualsNull = binaryExpression.IsKind(SyntaxKind.NotEqualsExpression);

        if (isEqualsNull)
        {
            // == null -> .IsFailure
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Replace with .IsFailure",
                    createChangedDocument: c => ReplaceWithPropertyAccessAsync(context.Document, binaryExpression, "IsFailure", c),
                    equivalenceKey: "ReplaceWithIsFailure"),
                diagnostic);
        }
        else if (isNotEqualsNull)
        {
            // != null -> .IsSuccess
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Replace with .IsSuccess",
                    createChangedDocument: c => ReplaceWithPropertyAccessAsync(context.Document, binaryExpression, "IsSuccess", c),
                    equivalenceKey: "ReplaceWithIsSuccess"),
                diagnostic);
        }
    }

    private void RegisterPatternFixes(CodeFixContext context, IsPatternExpressionSyntax isPatternExpression, Diagnostic diagnostic)
    {
        var pattern = isPatternExpression.Pattern;
        
        if (pattern is ConstantPatternSyntax { Expression: LiteralExpressionSyntax literal } 
            && literal.IsKind(SyntaxKind.NullLiteralExpression))
        {
            // is null -> .IsFailure
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Replace with .IsFailure",
                    createChangedDocument: c => ReplacePatternWithPropertyAccessAsync(context.Document, isPatternExpression, "IsFailure", c),
                    equivalenceKey: "ReplaceWithIsFailure"),
                diagnostic);
        }
        else if (pattern is UnaryPatternSyntax { RawKind: (int)SyntaxKind.NotPattern } notPattern
            && notPattern.Pattern is ConstantPatternSyntax { Expression: LiteralExpressionSyntax notLiteral }
            && notLiteral.IsKind(SyntaxKind.NullLiteralExpression))
        {
            // is not null -> .IsSuccess
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Replace with .IsSuccess",
                    createChangedDocument: c => ReplacePatternWithPropertyAccessAsync(context.Document, isPatternExpression, "IsSuccess", c),
                    equivalenceKey: "ReplaceWithIsSuccess"),
                diagnostic);
        }
    }

    private static async Task<Document> ReplaceWithPropertyAccessAsync(
        Document document,
        BinaryExpressionSyntax binaryExpression,
        string propertyName,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
            return document;

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel == null)
            return document;

        // Determine which side is the Result expression
        var resultExpression = binaryExpression.Left;
        if (binaryExpression.Right is not LiteralExpressionSyntax rightLiteral || !rightLiteral.IsKind(SyntaxKind.NullLiteralExpression))
        {
            // null is on the left
            resultExpression = binaryExpression.Right;
        }

        // Build list of replacements
        var replacements = new Dictionary<SyntaxNode, SyntaxNode>();

        // Create property access expression
        var propertyAccess = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            resultExpression,
            SyntaxFactory.IdentifierName(propertyName))
            .WithTriviaFrom(binaryExpression);

        replacements[binaryExpression] = propertyAccess;

        // Transform parameter types
        var typeInfo = semanticModel.GetTypeInfo(resultExpression);
        if (typeInfo.Type != null)
        {
            CodeFixHelpers.AddParameterTransformation(root, resultExpression, typeInfo.Type, semanticModel, replacements);
        }

        // Apply all replacements
        var newRoot = root.ReplaceNodes(replacements.Keys, (oldNode, newNode) => replacements[oldNode]);

        // Add using ResultNet
        newRoot = CodeFixHelpers.AddResultNetUsing(newRoot);

        return document.WithSyntaxRoot(newRoot);
    }

    private static async Task<Document> ReplacePatternWithPropertyAccessAsync(
        Document document,
        IsPatternExpressionSyntax isPatternExpression,
        string propertyName,
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

        // Create property access expression
        var propertyAccess = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            isPatternExpression.Expression,
            SyntaxFactory.IdentifierName(propertyName))
            .WithTriviaFrom(isPatternExpression);

        replacements[isPatternExpression] = propertyAccess;

        // Transform parameter types
        var typeInfo = semanticModel.GetTypeInfo(isPatternExpression.Expression);
        if (typeInfo.Type != null)
        {
            CodeFixHelpers.AddParameterTransformation(root, isPatternExpression.Expression, typeInfo.Type, semanticModel, replacements);
        }

        // Apply all replacements
        var newRoot = root.ReplaceNodes(replacements.Keys, (oldNode, newNode) => replacements[oldNode]);

        // Add using ResultNet
        newRoot = CodeFixHelpers.AddResultNetUsing(newRoot);

        return document.WithSyntaxRoot(newRoot);
    }
}
