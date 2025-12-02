using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ResultNet.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DefaultKeywordAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.RN002_DefaultKeyword);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeDefaultExpression, SyntaxKind.DefaultExpression);
        context.RegisterSyntaxNodeAction(AnalyzeDefaultLiteral, SyntaxKind.DefaultLiteralExpression);
    }

    private static void AnalyzeDefaultExpression(SyntaxNodeAnalysisContext context)
    {
        var defaultExpression = (DefaultExpressionSyntax)context.Node;
        var typeInfo = context.SemanticModel.GetTypeInfo(defaultExpression);

        if (typeInfo.Type == null || !typeInfo.Type.IsReferenceType)
            return;

        var typeName = GetTypeName(typeInfo.Type);
        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.RN002_DefaultKeyword,
            defaultExpression.GetLocation(),
            typeName);

        context.ReportDiagnostic(diagnostic);
    }

    private static void AnalyzeDefaultLiteral(SyntaxNodeAnalysisContext context)
    {
        var defaultLiteral = (LiteralExpressionSyntax)context.Node;
        var typeInfo = context.SemanticModel.GetTypeInfo(defaultLiteral);

        if (typeInfo.ConvertedType == null || !typeInfo.ConvertedType.IsReferenceType)
            return;

        var typeArgName = GetTypeName(typeInfo.ConvertedType);
        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.RN002_DefaultKeyword,
            defaultLiteral.GetLocation(),
            typeArgName);

        context.ReportDiagnostic(diagnostic);
    }

    private static string GetTypeName(ITypeSymbol type)
    {
        return type switch
        {
            IArrayTypeSymbol arrayType => GetTypeName(arrayType.ElementType) + "[]",
            _ => type.Name
        };
    }
}
