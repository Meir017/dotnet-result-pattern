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
/// Code fixer for RN001 (return null) and RN005 (assign null): Replace null with Result.Failure()
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NullLiteralCodeFixer)), Shared]
public class NullLiteralCodeFixer : CodeFixProvider
{
    private const string Title = "Replace null with Result.Failure()";

    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("RN001", "RN005");

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
        var literalExpression = node as LiteralExpressionSyntax;
        
        if (literalExpression == null || !literalExpression.IsKind(SyntaxKind.NullLiteralExpression))
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: Title,
                createChangedDocument: c => ReplaceNullWithFailureAsync(context.Document, literalExpression, c),
                equivalenceKey: "ReplaceNullWithFailure"),
            diagnostic);
    }

    private static async Task<Document> ReplaceNullWithFailureAsync(
        Document document,
        LiteralExpressionSyntax nullLiteral,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
            return document;

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel == null)
            return document;

        // Determine the type from context
        var typeSymbol = GetTypeFromContext(nullLiteral, semanticModel);
        if (typeSymbol == null)
            return document;

        // Track nodes that we'll need to transform
        var returnStatement = nullLiteral.FirstAncestorOrSelf<ReturnStatementSyntax>();
        var method = returnStatement?.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        var variableDeclarator = nullLiteral.FirstAncestorOrSelf<VariableDeclaratorSyntax>();
        var variableDeclaration = variableDeclarator?.Parent as VariableDeclarationSyntax;

        // For assignment expressions, find the original variable declaration
        var assignment = nullLiteral.FirstAncestorOrSelf<AssignmentExpressionSyntax>();
        VariableDeclarationSyntax? assignmentTargetDeclaration = null;
        if (assignment != null && variableDeclaration == null)
        {
            // Get the symbol being assigned to
            var assignedSymbol = semanticModel.GetSymbolInfo(assignment.Left).Symbol;
            if (assignedSymbol is ILocalSymbol localSymbol)
            {
                // Find the variable declaration syntax
                var declarationSyntax = localSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
                if (declarationSyntax is VariableDeclaratorSyntax declarator)
                {
                    assignmentTargetDeclaration = declarator.Parent as VariableDeclarationSyntax;
                }
            }
        }

        // Build list of replacements to make
        var replacements = new Dictionary<SyntaxNode, SyntaxNode>();

        // Replace null literal with Failure()
        var failureExpression = CodeFixHelpers.GenerateFailureExpression(typeSymbol);
        replacements[nullLiteral] = failureExpression.WithTriviaFrom(nullLiteral);

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
        // Transform assignment target declaration if found
        else if (assignmentTargetDeclaration?.Type != null)
        {
            var resultTypeSyntax = CodeFixHelpers.TransformToResultType(typeSymbol);
            replacements[assignmentTargetDeclaration.Type] = resultTypeSyntax;
        }

        // Apply all replacements at once
        var newRoot = root.ReplaceNodes(replacements.Keys, (oldNode, newNode) => replacements[oldNode]);

        // Add using ResultNet
        newRoot = CodeFixHelpers.AddResultNetUsing(newRoot);

        return document.WithSyntaxRoot(newRoot);
    }

    private static ITypeSymbol? GetTypeFromContext(LiteralExpressionSyntax nullLiteral, SemanticModel semanticModel)
    {
        // Check if this is a return statement
        var returnStatement = nullLiteral.FirstAncestorOrSelf<ReturnStatementSyntax>();
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
        var variableDeclarator = nullLiteral.FirstAncestorOrSelf<VariableDeclaratorSyntax>();
        if (variableDeclarator != null)
        {
            var variableSymbol = semanticModel.GetDeclaredSymbol(variableDeclarator) as ILocalSymbol;
            return variableSymbol?.Type;
        }

        // Check if this is an assignment expression
        var assignment = nullLiteral.FirstAncestorOrSelf<AssignmentExpressionSyntax>();
        if (assignment != null)
        {
            var leftType = semanticModel.GetTypeInfo(assignment.Left).Type;
            return leftType;
        }

        return null;
    }
}
