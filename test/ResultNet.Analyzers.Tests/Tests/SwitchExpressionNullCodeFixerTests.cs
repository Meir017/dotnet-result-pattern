using Microsoft.CodeAnalysis.Testing;
using ResultNet.CodeFixers;

namespace ResultNet.Analyzers.Tests;

public class SwitchExpressionNullCodeFixerTests
{
    [Fact]
    public async Task ReplaceNullPattern_WithIsFailurePattern()
    {
        var source = """
            public class TestClass
            {
                public string Test(string? result)
                {
                    return result switch
                    {
                        null => "failure",
                        _ => "success"
                    };
                }
            }
            """;

        var fixedSource = """
            using ResultNet;

            public class TestClass
            {
                public string Test(Result<string> result)
                {
                    return result switch
                    {
                        { IsFailure: true } => "failure",
                        _ => "success"
                    };
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<SwitchExpressionNullAnalyzer, SwitchExpressionNullCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN009_NullInSwitchExpression)
            .WithSpan(7, 13, 7, 17);

        await CSharpCodeFixVerifier<SwitchExpressionNullAnalyzer, SwitchExpressionNullCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task ReplaceNotNullPattern_WithIsSuccessPattern()
    {
        var source = """
            public class TestClass
            {
                public string Test(string? result)
                {
                    return result switch
                    {
                        not null => "success",
                        _ => "failure"
                    };
                }
            }
            """;

        var fixedSource = """
            using ResultNet;

            public class TestClass
            {
                public string Test(Result<string> result)
                {
                    return result switch
                    {
                        { IsSuccess: true } => "success",
                        _ => "failure"
                    };
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<SwitchExpressionNullAnalyzer, SwitchExpressionNullCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN009_NullInSwitchExpression)
            .WithSpan(7, 13, 7, 21);

        await CSharpCodeFixVerifier<SwitchExpressionNullAnalyzer, SwitchExpressionNullCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }
}
