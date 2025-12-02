using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ResultNet.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NullDefaultParameterAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.RN010_NullDefaultParameter);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeParameter, SyntaxKind.Parameter);
    }

    private static void AnalyzeParameter(SyntaxNodeAnalysisContext context)
    {
        var parameter = (ParameterSyntax)context.Node;

        if (parameter.Default?.Value is not LiteralExpressionSyntax { RawKind: (int)SyntaxKind.NullLiteralExpression } nullDefault)
            return;

        var parameterSymbol = context.SemanticModel.GetDeclaredSymbol(parameter);
        if (parameterSymbol == null)
            return;

        if (parameterSymbol.Type == null || !parameterSymbol.Type.IsReferenceType)
            return;

        var typeArgName = parameterSymbol.Type.Name;
        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.RN010_NullDefaultParameter,
            nullDefault.GetLocation(),
            typeArgName);

        context.ReportDiagnostic(diagnostic);
    }
}
