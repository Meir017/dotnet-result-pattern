using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ResultNet.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NullCheckAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.RN004_NullCheck);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeEqualsExpression, 
            SyntaxKind.EqualsExpression, 
            SyntaxKind.NotEqualsExpression);
        context.RegisterSyntaxNodeAction(AnalyzeIsPattern, SyntaxKind.IsPatternExpression);
    }

    private static void AnalyzeEqualsExpression(SyntaxNodeAnalysisContext context)
    {
        var binaryExpression = (BinaryExpressionSyntax)context.Node;

        var leftIsNull = binaryExpression.Left is LiteralExpressionSyntax { RawKind: (int)SyntaxKind.NullLiteralExpression };
        var rightIsNull = binaryExpression.Right is LiteralExpressionSyntax { RawKind: (int)SyntaxKind.NullLiteralExpression };

        if (!leftIsNull && !rightIsNull)
            return;

        var valueExpression = leftIsNull ? binaryExpression.Right : binaryExpression.Left;
        var typeInfo = context.SemanticModel.GetTypeInfo(valueExpression);
        
        if (typeInfo.Type == null || typeInfo.Type.IsValueType)
            return;

        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.RN004_NullCheck,
            binaryExpression.GetLocation());

        context.ReportDiagnostic(diagnostic);
    }

    private static void AnalyzeIsPattern(SyntaxNodeAnalysisContext context)
    {
        var isPatternExpression = (IsPatternExpressionSyntax)context.Node;

        // Check for "is null" pattern
        var isNullPattern = isPatternExpression.Pattern is ConstantPatternSyntax { Expression: LiteralExpressionSyntax { RawKind: (int)SyntaxKind.NullLiteralExpression } };
        
        // Check for "is not null" pattern
        var isNotNullPattern = isPatternExpression.Pattern is UnaryPatternSyntax 
        { 
            RawKind: (int)SyntaxKind.NotPattern,
            Pattern: ConstantPatternSyntax { Expression: LiteralExpressionSyntax { RawKind: (int)SyntaxKind.NullLiteralExpression } }
        };

        if (!isNullPattern && !isNotNullPattern)
            return;

        var typeInfo = context.SemanticModel.GetTypeInfo(isPatternExpression.Expression);
        
        if (typeInfo.Type == null || typeInfo.Type.IsValueType)
            return;

        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.RN004_NullCheck,
            isPatternExpression.GetLocation());

        context.ReportDiagnostic(diagnostic);
    }
}
