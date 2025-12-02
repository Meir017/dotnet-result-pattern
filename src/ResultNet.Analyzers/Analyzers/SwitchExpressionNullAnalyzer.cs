using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ResultNet.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SwitchExpressionNullAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.RN009_NullInSwitchExpression);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeSwitchExpression, SyntaxKind.SwitchExpression);
        context.RegisterSyntaxNodeAction(AnalyzeSwitchStatement, SyntaxKind.SwitchStatement);
    }

    private static void AnalyzeSwitchExpression(SyntaxNodeAnalysisContext context)
    {
        var switchExpression = (SwitchExpressionSyntax)context.Node;

        var governingType = context.SemanticModel.GetTypeInfo(switchExpression.GoverningExpression).Type;
        if (governingType == null || governingType.IsValueType)
            return;

        foreach (var arm in switchExpression.Arms)
        {
            if (IsNullPattern(arm.Pattern))
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.RN009_NullInSwitchExpression,
                    arm.Pattern.GetLocation());

                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static void AnalyzeSwitchStatement(SyntaxNodeAnalysisContext context)
    {
        var switchStatement = (SwitchStatementSyntax)context.Node;

        var governingType = context.SemanticModel.GetTypeInfo(switchStatement.Expression).Type;
        if (governingType == null || governingType.IsValueType)
            return;

        foreach (var section in switchStatement.Sections)
        {
            foreach (var label in section.Labels)
            {
                if (label is CaseSwitchLabelSyntax caseLabel && 
                    caseLabel.Value is LiteralExpressionSyntax { RawKind: (int)SyntaxKind.NullLiteralExpression })
                {
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.RN009_NullInSwitchExpression,
                        caseLabel.Value.GetLocation());

                    context.ReportDiagnostic(diagnostic);
                }
                else if (label is CasePatternSwitchLabelSyntax patternLabel && 
                         IsNullPattern(patternLabel.Pattern))
                {
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.RN009_NullInSwitchExpression,
                        patternLabel.Pattern.GetLocation());

                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    private static bool IsNullPattern(PatternSyntax pattern)
    {
        // Check for 'null' pattern
        if (pattern is ConstantPatternSyntax 
            { 
                Expression: LiteralExpressionSyntax { RawKind: (int)SyntaxKind.NullLiteralExpression } 
            })
        {
            return true;
        }

        // Check for 'not null' pattern
        if (pattern is UnaryPatternSyntax
            {
                RawKind: (int)SyntaxKind.NotPattern,
                Pattern: ConstantPatternSyntax
                {
                    Expression: LiteralExpressionSyntax { RawKind: (int)SyntaxKind.NullLiteralExpression }
                }
            })
        {
            return true;
        }

        return false;
    }
}
