using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace ResultNet.CodeFixers;

/// <summary>
/// Code fixer for RN009: Do not check for null in switch expressions with Result types
/// Replaces null patterns with { IsFailure: true } or { IsSuccess: false }
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SwitchExpressionNullCodeFixer)), Shared]
public class SwitchExpressionNullCodeFixer : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("RN009");

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
        
        // Find the pattern containing null
        PatternSyntax? pattern = node as PatternSyntax ?? node.FirstAncestorOrSelf<PatternSyntax>();
        
        if (pattern == null)
            return;

        // Check if this is a 'not null' pattern
        var isNotPattern = (pattern is UnaryPatternSyntax { RawKind: (int)SyntaxKind.NotPattern }) ||
                          (pattern.Parent is UnaryPatternSyntax { RawKind: (int)SyntaxKind.NotPattern });

        // Determine which pattern to replace (use parent if it's a UnaryPattern)
        var patternToReplace = pattern.Parent is UnaryPatternSyntax unaryParent ? unaryParent : pattern;

        if (isNotPattern)
        {
            // not null -> { IsSuccess: true }
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Replace with { IsSuccess: true }",
                    createChangedDocument: c => ReplaceWithPropertyPatternAsync(context.Document, patternToReplace, "IsSuccess", true, c),
                    equivalenceKey: "ReplaceNullPattern"),
                diagnostic);
        }
        else
        {
            // null -> { IsFailure: true }
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Replace with { IsFailure: true }",
                    createChangedDocument: c => ReplaceWithPropertyPatternAsync(context.Document, patternToReplace, "IsFailure", true, c),
                    equivalenceKey: "ReplaceNullPattern"),
                diagnostic);
        }
    }

    private static async Task<Document> ReplaceWithPropertyPatternAsync(
        Document document,
        PatternSyntax pattern,
        string propertyName,
        bool value,
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

        // Create property pattern: { IsFailure: true } or { IsSuccess: true }
        var propertyPattern = SyntaxFactory.RecursivePattern()
            .WithPropertyPatternClause(
                SyntaxFactory.PropertyPatternClause(
                    SyntaxFactory.SingletonSeparatedList<SubpatternSyntax>(
                        SyntaxFactory.Subpattern(
                            SyntaxFactory.NameColon(propertyName),
                            SyntaxFactory.ConstantPattern(
                                SyntaxFactory.LiteralExpression(
                                    value ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression))))))
            .NormalizeWhitespace()
            .WithTriviaFrom(pattern);

        replacements[pattern] = propertyPattern;

        // Transform parameter type from the switch expression
        var switchExpr = pattern.FirstAncestorOrSelf<SwitchExpressionSyntax>();
        if (switchExpr != null)
        {
            var typeInfo = semanticModel.GetTypeInfo(switchExpr.GoverningExpression);
            if (typeInfo.Type != null)
            {
                CodeFixHelpers.AddParameterTransformation(root, switchExpr.GoverningExpression, typeInfo.Type, semanticModel, replacements);
            }
        }

        // Apply all replacements
        var newRoot = root.ReplaceNodes(replacements.Keys, (oldNode, newNode) => replacements[oldNode]);

        // Add using ResultNet
        newRoot = CodeFixHelpers.AddResultNetUsing(newRoot);

        return document.WithSyntaxRoot(newRoot);
    }
}
