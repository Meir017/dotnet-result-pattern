using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ResultNet.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NullLiteralAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
        ImmutableArray.Create(
            DiagnosticDescriptors.RN001_NullLiteralReturn,
            DiagnosticDescriptors.RN005_NullAssignment);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeReturnStatement, SyntaxKind.ReturnStatement);
        context.RegisterSyntaxNodeAction(AnalyzeVariableDeclaration, SyntaxKind.VariableDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeAssignment, SyntaxKind.SimpleAssignmentExpression);
    }

    private static void AnalyzeReturnStatement(SyntaxNodeAnalysisContext context)
    {
        var returnStatement = (ReturnStatementSyntax)context.Node;

        if (returnStatement.Expression is not LiteralExpressionSyntax { RawKind: (int)SyntaxKind.NullLiteralExpression })
            return;

        var containingMethod = returnStatement.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (containingMethod == null)
            return;

        var methodSymbol = context.SemanticModel.GetDeclaredSymbol(containingMethod);
        if (methodSymbol == null)
            return;

        var returnType = methodSymbol.ReturnType;
        
        if (returnType == null || !returnType.IsReferenceType)
            return;

        var typeArgName = returnType.Name;
        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.RN001_NullLiteralReturn,
            returnStatement.Expression.GetLocation(),
            typeArgName);

        context.ReportDiagnostic(diagnostic);
    }

    private static void AnalyzeVariableDeclaration(SyntaxNodeAnalysisContext context)
    {
        var variableDeclaration = (VariableDeclarationSyntax)context.Node;

        foreach (var variable in variableDeclaration.Variables)
        {
            if (variable.Initializer?.Value is not LiteralExpressionSyntax { RawKind: (int)SyntaxKind.NullLiteralExpression } nullLiteral)
                continue;

            var typeInfo = context.SemanticModel.GetTypeInfo(variableDeclaration.Type);
            
            if (typeInfo.Type == null || !typeInfo.Type.IsReferenceType)
                continue;

            var typeArgName = typeInfo.Type.Name;
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.RN005_NullAssignment,
                nullLiteral.GetLocation(),
                typeArgName);

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void AnalyzeAssignment(SyntaxNodeAnalysisContext context)
    {
        var assignment = (AssignmentExpressionSyntax)context.Node;

        if (assignment.Right is not LiteralExpressionSyntax { RawKind: (int)SyntaxKind.NullLiteralExpression } nullLiteral)
            return;

        var leftTypeInfo = context.SemanticModel.GetTypeInfo(assignment.Left);
        
        if (leftTypeInfo.Type == null || !leftTypeInfo.Type.IsReferenceType)
            return;

        var typeArgName = leftTypeInfo.Type.Name;
        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.RN005_NullAssignment,
            nullLiteral.GetLocation(),
            typeArgName);

        context.ReportDiagnostic(diagnostic);
    }
}
