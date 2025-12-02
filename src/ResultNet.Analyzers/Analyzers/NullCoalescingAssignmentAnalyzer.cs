using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ResultNet.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NullCoalescingAssignmentAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.RN006_NullCoalescingAssignment);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeCoalesceAssignment, SyntaxKind.CoalesceAssignmentExpression);
    }

    private static void AnalyzeCoalesceAssignment(SyntaxNodeAnalysisContext context)
    {
        var assignmentExpression = (AssignmentExpressionSyntax)context.Node;

        var leftType = context.SemanticModel.GetTypeInfo(assignmentExpression.Left).Type;
        if (leftType == null || leftType.IsValueType)
            return;

        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.RN006_NullCoalescingAssignment,
            assignmentExpression.OperatorToken.GetLocation());

        context.ReportDiagnostic(diagnostic);
    }
}
