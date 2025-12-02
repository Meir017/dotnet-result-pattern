using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ResultNet.CodeFixers;

/// <summary>
/// Code fixer for RN010: Do not use null as default value for Result parameters
/// Removes the null default value from the parameter
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NullDefaultParameterCodeFixer)), Shared]
public class NullDefaultParameterCodeFixer : CodeFixProvider
{
    private const string Title = "Remove null default value";

    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("RN010");

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
        var parameter = node.FirstAncestorOrSelf<ParameterSyntax>();

        if (parameter == null || parameter.Default == null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: Title,
                createChangedDocument: c => RemoveDefaultValueAsync(context.Document, parameter, c),
                equivalenceKey: "RemoveNullDefaultParameter"),
            diagnostic);
    }

    private static async Task<Document> RemoveDefaultValueAsync(
        Document document,
        ParameterSyntax parameter,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
            return document;

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel == null)
            return document;

        // Get the parameter symbol to get its type
        var parameterSymbol = semanticModel.GetDeclaredSymbol(parameter) as IParameterSymbol;
        if (parameterSymbol == null || parameterSymbol.Type == null)
            return document;

        // Remove the default value clause
        var newParameter = parameter.WithDefault(null);
        
        // Transform the parameter type to Result<T>
        var resultTypeSyntax = CodeFixHelpers.TransformToResultType(parameterSymbol.Type);
        newParameter = newParameter.WithType(resultTypeSyntax);
        
        // Clean up trailing whitespace from the identifier (which had ` = null` after it)
        if (newParameter.Identifier != null)
        {
            newParameter = newParameter.WithIdentifier(
                newParameter.Identifier.WithTrailingTrivia());
        }
        
        var newRoot = root.ReplaceNode(parameter, newParameter);

        // Add using ResultNet
        newRoot = CodeFixHelpers.AddResultNetUsing(newRoot);

        return document.WithSyntaxRoot(newRoot);
    }
}
