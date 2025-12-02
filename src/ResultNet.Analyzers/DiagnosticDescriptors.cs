using Microsoft.CodeAnalysis;

namespace ResultNet.Analyzers;

public static class DiagnosticDescriptors
{
    private const string Category = "ResultNet.Usage";

    public static readonly DiagnosticDescriptor RN001_NullLiteralReturn = new(
        id: "RN001",
        title: "Do not return null from Result-returning methods",
        messageFormat: "Return 'Result<{0}>.Failure()' instead of null",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Result types should never be null. Use Result.Failure() or Result<T>.Failure() to represent errors.");

    public static readonly DiagnosticDescriptor RN002_DefaultKeyword = new(
        id: "RN002",
        title: "Do not use default with Result types",
        messageFormat: "Use 'Result<{0}>.Failure()' instead of default",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Using default creates an invalid Result state. Use Result.Failure() or Result<T>.Failure() instead.");

    public static readonly DiagnosticDescriptor RN003_NullCoalescing = new(
        id: "RN003",
        title: "Do not use null-coalescing operator with Result types",
        messageFormat: "Use '.ValueOr()' or '.Match()' instead of '??'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Result types should not be used with the null-coalescing operator. Use .ValueOr() or .Match() for proper Result handling.");

    public static readonly DiagnosticDescriptor RN004_NullCheck = new(
        id: "RN004",
        title: "Do not check Result types for null",
        messageFormat: "Use '.IsFailure' or '.IsSuccess' instead of null checks",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Result types should not be checked for null. Use .IsSuccess or .IsFailure properties instead.");

    public static readonly DiagnosticDescriptor RN005_NullAssignment = new(
        id: "RN005",
        title: "Do not assign null to Result variables",
        messageFormat: "Assign 'Result<{0}>.Failure()' instead of null",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Result variables should never be assigned null. Use Result.Failure() or Result<T>.Failure() instead.");

    public static readonly DiagnosticDescriptor RN006_NullCoalescingAssignment = new(
        id: "RN006",
        title: "Do not use null-coalescing assignment with Result types",
        messageFormat: "Result types should not use '??=' operator",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Result types are value types and should not use the ??= operator. Use proper Result construction instead.");

    public static readonly DiagnosticDescriptor RN007_NullConditionalAccess = new(
        id: "RN007",
        title: "Do not use null-conditional operators with Result types",
        messageFormat: "Use '.IsSuccess' check or '.Match()' instead of '?.' or '?[]'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Result types should not use null-conditional operators (?. or ?[]). Use .IsSuccess or .Match() for proper Result handling.");

    public static readonly DiagnosticDescriptor RN008_NullForgivingOperator = new(
        id: "RN008",
        title: "Do not use null-forgiving operator with Result types",
        messageFormat: "Result types should not require null-forgiving operator",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Using the null-forgiving operator (!) on Result types suggests incorrect nullable Result usage.");

    public static readonly DiagnosticDescriptor RN009_NullInSwitchExpression = new(
        id: "RN009",
        title: "Do not check for null in switch expressions with Result types",
        messageFormat: "Use pattern matching on '.IsSuccess' or '.IsFailure' instead",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Switch expressions should not check for null on Result types. Use .IsSuccess or .IsFailure for proper Result handling.");

    public static readonly DiagnosticDescriptor RN010_NullDefaultParameter = new(
        id: "RN010",
        title: "Do not use null as default value for Result parameters",
        messageFormat: "Use 'Result<{0}>.Failure()' as default value instead of null",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Result type parameters should not have null as default value. Use Result.Failure() or Result<T>.Failure() instead.");
}
