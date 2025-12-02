using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ResultNet.Analyzers;

namespace ResultNet.CodeFixers;

/// <summary>
/// Code fixer for RN002: Do not use default with Result types
/// Replaces default with Result.Failure() or Result&lt;T&gt;.Failure()
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DefaultKeywordCodeFixer)), Shared]
public class DefaultKeywordCodeFixer : CodeFixProvider
{
    private const string Title = "Replace default with Result.Failure()";

    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("RN002");

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
        
        // Handle both default literal and default(T) expression
        ExpressionSyntax? defaultExpression = null;
        if (node is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.DefaultLiteralExpression))
        {
            defaultExpression = literal;
        }
        else if (node is DefaultExpressionSyntax defaultExpr)
        {
            defaultExpression = defaultExpr;
        }
        else
        {
            // Try to find ancestor
            defaultExpression = node.FirstAncestorOrSelf<LiteralExpressionSyntax>(n => n.IsKind(SyntaxKind.DefaultLiteralExpression)) 
                ?? (ExpressionSyntax?)node.FirstAncestorOrSelf<DefaultExpressionSyntax>();
        }

        if (defaultExpression == null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: Title,
                createChangedDocument: c => ReplaceDefaultWithFailureAsync(context.Document, defaultExpression, c),
                equivalenceKey: "ReplaceDefaultWithFailure"),
            diagnostic);
    }

    private static async Task<Document> ReplaceDefaultWithFailureAsync(
        Document document,
        ExpressionSyntax defaultExpression,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
            return document;

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel == null)
            return document;

        // Determine the type from context
        var typeSymbol = GetResultTypeFromContext(defaultExpression, semanticModel);
        if (typeSymbol == null)
            return document;

        // Track nodes that we'll need to transform
        var returnStatement = defaultExpression.FirstAncestorOrSelf<ReturnStatementSyntax>();
        var method = returnStatement?.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        var variableDeclarator = defaultExpression.FirstAncestorOrSelf<VariableDeclaratorSyntax>();
        var variableDeclaration = variableDeclarator?.Parent as VariableDeclarationSyntax;

        // Build list of replacements to make
        var replacements = new Dictionary<SyntaxNode, SyntaxNode>();

        // Replace default with Failure()
        var failureExpression = CodeFixHelpers.GenerateFailureExpression(typeSymbol);
        replacements[defaultExpression] = failureExpression.WithTriviaFrom(defaultExpression);

        // Transform method return type (if applicable)
        if (method?.ReturnType != null)
        {
            var resultTypeSyntax = CodeFixHelpers.TransformToResultType(typeSymbol);
            replacements[method.ReturnType] = resultTypeSyntax;
        }

        // Transform variable declaration type (if applicable)
        if (variableDeclaration?.Type != null)
        {
            var resultTypeSyntax = CodeFixHelpers.TransformToResultType(typeSymbol);
            replacements[variableDeclaration.Type] = resultTypeSyntax;
        }

        // Apply all replacements at once
        var newRoot = root.ReplaceNodes(replacements.Keys, (oldNode, newNode) => replacements[oldNode]);

        // Add using ResultNet
        newRoot = CodeFixHelpers.AddResultNetUsing(newRoot);

        return document.WithSyntaxRoot(newRoot);
    }

    private static ITypeSymbol? GetResultTypeFromContext(ExpressionSyntax defaultExpression, SemanticModel semanticModel)
    {
        // For default(Type), we can get the type from the expression itself
        if (defaultExpression is DefaultExpressionSyntax defaultExpr)
        {
            var typeInfo = semanticModel.GetTypeInfo(defaultExpr.Type);
            return typeInfo.Type;
        }

        // For default literal, infer from context
        // Check if this is a return statement
        var returnStatement = defaultExpression.FirstAncestorOrSelf<ReturnStatementSyntax>();
        if (returnStatement != null)
        {
            var method = returnStatement.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            if (method != null)
            {
                var methodSymbol = semanticModel.GetDeclaredSymbol(method);
                return methodSymbol?.ReturnType;
            }
        }

        // Check if this is a variable declaration
        var variableDeclarator = defaultExpression.FirstAncestorOrSelf<VariableDeclaratorSyntax>();
        if (variableDeclarator != null)
        {
            var variableSymbol = semanticModel.GetDeclaredSymbol(variableDeclarator) as ILocalSymbol;
            return variableSymbol?.Type;
        }

        // Check if this is an assignment expression
        var assignment = defaultExpression.FirstAncestorOrSelf<AssignmentExpressionSyntax>();
        if (assignment != null)
        {
            var leftType = semanticModel.GetTypeInfo(assignment.Left).Type;
            return leftType;
        }

        return null;
    }
}
