using Microsoft.CodeAnalysis;

namespace ResultNet.Analyzers;

internal static class AnalyzerHelpers
{
    private const string ResultTypeName = "ResultNet.Result";
    private const string ResultTTypeName = "ResultNet.Result`1";

    public static bool IsResultType(ITypeSymbol? typeSymbol)
    {
        if (typeSymbol == null)
            return false;

        // Handle nullable types - check the underlying type
        if (typeSymbol is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullableType &&
            nullableType.TypeArguments.Length > 0)
        {
            typeSymbol = nullableType.TypeArguments[0];
        }

        var fullName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        
        return fullName.StartsWith("global::ResultNet.Result");
    }

    public static bool IsResultTType(ITypeSymbol? typeSymbol)
    {
        if (typeSymbol == null)
            return false;

        return typeSymbol is INamedTypeSymbol namedType &&
               namedType.IsGenericType &&
               namedType.ConstructedFrom.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::ResultNet.Result<T>";
    }

    public static bool IsNonGenericResultType(ITypeSymbol? typeSymbol)
    {
        if (typeSymbol == null)
            return false;

        return !IsResultTType(typeSymbol) && 
               typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::ResultNet.Result";
    }

    public static string GetTypeArgumentName(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol { IsGenericType: true } namedType && 
            namedType.TypeArguments.Length > 0)
        {
            return namedType.TypeArguments[0].Name;
        }

        return "T";
    }
}
