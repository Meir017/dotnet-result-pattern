using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace ResultNet.CodeFixers;

internal static class CodeFixHelpers
{
    /// <summary>
    /// Adds a using directive for ResultNet if it doesn't already exist
    /// </summary>
    public static SyntaxNode AddResultNetUsing(SyntaxNode root)
    {
        if (root is not CompilationUnitSyntax compilationUnit)
            return root;

        // Check if using ResultNet already exists
        var hasResultNetUsing = compilationUnit.Usings
            .Any(u => u.Name?.ToString() == "ResultNet");

        if (hasResultNetUsing)
            return root;

        // Add using ResultNet
        var usingDirective = SyntaxFactory.UsingDirective(
            SyntaxFactory.IdentifierName("ResultNet"));

        // If there are existing usings, insert in alphabetical order
        if (compilationUnit.Usings.Count > 0)
        {
            usingDirective = usingDirective.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
            
            // Find the correct position to insert (alphabetically)
            int insertIndex = 0;
            for (int i = 0; i < compilationUnit.Usings.Count; i++)
            {
                var existingUsing = compilationUnit.Usings[i].Name?.ToString() ?? "";
                // If ResultNet should come after the existing using, increment insert index
                if (string.Compare("ResultNet", existingUsing, System.StringComparison.Ordinal) < 0)
                {
                    // ResultNet comes before this using, so insert here
                    break;
                }
                insertIndex = i + 1;
            }
            
            var updatedUsings = compilationUnit.Usings.Insert(insertIndex, usingDirective);
            return compilationUnit.WithUsings(updatedUsings);
        }

        // No existing usings - add with a blank line after and preserve any leading trivia on first member
        usingDirective = usingDirective.WithTrailingTrivia(
            SyntaxFactory.CarriageReturnLineFeed,
            SyntaxFactory.CarriageReturnLineFeed);

        var newUsings = SyntaxFactory.SingletonList(usingDirective);
        var newCompilationUnit = compilationUnit.WithUsings(newUsings);

        // Remove extra leading trivia from the first member to avoid double blank lines
        if (newCompilationUnit.Members.Count > 0)
        {
            var firstMember = newCompilationUnit.Members[0];
            var leadingTrivia = firstMember.GetLeadingTrivia();
            
            // Remove leading blank lines
            var trimmedTrivia = leadingTrivia.SkipWhile(t => 
                t.IsKind(SyntaxKind.EndOfLineTrivia) || 
                t.IsKind(SyntaxKind.WhitespaceTrivia));
            
            var newFirstMember = firstMember.WithLeadingTrivia(trimmedTrivia);
            newCompilationUnit = newCompilationUnit.ReplaceNode(firstMember, newFirstMember);
        }

        return newCompilationUnit;
    }

    /// <summary>
    /// Transforms a nullable type to a Result type
    /// For T? returns Result&lt;T&gt;
    /// </summary>
    public static TypeSyntax TransformToResultType(ITypeSymbol typeSymbol)
    {
        // Get the non-nullable underlying type
        var underlyingType = typeSymbol.IsReferenceType && typeSymbol.NullableAnnotation == NullableAnnotation.Annotated
            ? typeSymbol.WithNullableAnnotation(NullableAnnotation.NotAnnotated)
            : typeSymbol;

        var typeName = underlyingType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        
        // Create Result<T>
        var resultType = SyntaxFactory.GenericName(
            SyntaxFactory.Identifier("Result"),
            SyntaxFactory.TypeArgumentList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.ParseTypeName(typeName))));

        return resultType;
    }

    /// <summary>
    /// Transforms a method return type from T? to Result&lt;T&gt;
    /// </summary>
    public static SyntaxNode TransformMethodReturnType(SyntaxNode root, MethodDeclarationSyntax method, ITypeSymbol newReturnType)
    {
        var resultTypeSyntax = TransformToResultType(newReturnType);
        var newMethod = method.WithReturnType(resultTypeSyntax);
        return root.ReplaceNode(method, newMethod);
    }

    /// <summary>
    /// Transforms a variable declaration from T? to Result&lt;T&gt;
    /// </summary>
    public static SyntaxNode TransformVariableDeclaration(
        SyntaxNode root,
        VariableDeclarationSyntax variableDeclaration,
        ITypeSymbol newType)
    {
        var resultTypeSyntax = TransformToResultType(newType);
        var newDeclaration = variableDeclaration.WithType(resultTypeSyntax);
        return root.ReplaceNode(variableDeclaration, newDeclaration);
    }

    /// <summary>
    /// Transforms a parameter from T? to Result&lt;T&gt;
    /// </summary>
    public static SyntaxNode TransformParameter(
        SyntaxNode root,
        ParameterSyntax parameter,
        ITypeSymbol newType)
    {
        if (parameter.Type == null)
            return root;

        var resultTypeSyntax = TransformToResultType(newType);
        var newParameter = parameter.WithType(resultTypeSyntax);
        return root.ReplaceNode(parameter, newParameter);
    }

    /// <summary>
    /// Generates a Result.Failure() or Result&lt;T&gt;.Failure() expression
    /// </summary>
    public static ExpressionSyntax GenerateFailureExpression(ITypeSymbol typeSymbol)
    {
        var resultTypeSyntax = TransformToResultType(typeSymbol);
        
        return SyntaxFactory.InvocationExpression(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                resultTypeSyntax,
                SyntaxFactory.IdentifierName("Failure")));
    }

    /// <summary>
    /// Finds a parameter declaration node for a given identifier expression and adds it to the replacements dictionary
    /// </summary>
    public static void AddParameterTransformation(
        SyntaxNode root,
        ExpressionSyntax identifierExpression,
        ITypeSymbol typeSymbol,
        SemanticModel semanticModel,
        Dictionary<SyntaxNode, SyntaxNode> replacements)
    {
        if (identifierExpression is not IdentifierNameSyntax identifier)
            return;

        var symbol = semanticModel.GetSymbolInfo(identifier).Symbol;
        if (symbol is not IParameterSymbol parameterSymbol)
            return;

        // Find all parameter syntax nodes with matching name
        var parameters = root.DescendantNodes()
            .OfType<ParameterSyntax>()
            .Where(p => p.Identifier.Text == parameterSymbol.Name && p.Type != null);

        foreach (var parameter in parameters)
        {
            // Skip if already in replacements
            if (replacements.ContainsKey(parameter))
                continue;

            var resultTypeSyntax = TransformToResultType(typeSymbol);
            var newParameter = parameter.WithType(resultTypeSyntax);
            replacements[parameter] = newParameter;
        }
    }
}
