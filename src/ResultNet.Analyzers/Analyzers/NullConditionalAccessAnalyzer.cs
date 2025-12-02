using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ResultNet.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NullConditionalAccessAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.RN007_NullConditionalAccess);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeConditionalAccess, SyntaxKind.ConditionalAccessExpression);
    }

    private static void AnalyzeConditionalAccess(SyntaxNodeAnalysisContext context)
    {
        var conditionalAccess = (ConditionalAccessExpressionSyntax)context.Node;

        var exprType = context.SemanticModel.GetTypeInfo(conditionalAccess.Expression).Type;
        if (exprType == null || exprType.IsValueType)
            return;

        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.RN007_NullConditionalAccess,
            conditionalAccess.OperatorToken.GetLocation());

        context.ReportDiagnostic(diagnostic);
    }
}
