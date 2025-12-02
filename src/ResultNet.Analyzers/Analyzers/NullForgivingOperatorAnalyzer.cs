using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ResultNet.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NullForgivingOperatorAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.RN008_NullForgivingOperator);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeSuppressNullableWarning, SyntaxKind.SuppressNullableWarningExpression);
    }

    private static void AnalyzeSuppressNullableWarning(SyntaxNodeAnalysisContext context)
    {
        var suppressNullableWarning = (PostfixUnaryExpressionSyntax)context.Node;

        var operandType = context.SemanticModel.GetTypeInfo(suppressNullableWarning.Operand).Type;
        if (operandType == null || operandType.IsValueType)
            return;

        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.RN008_NullForgivingOperator,
            suppressNullableWarning.OperatorToken.GetLocation());

        context.ReportDiagnostic(diagnostic);
    }
}
