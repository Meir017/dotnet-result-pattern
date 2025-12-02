using Microsoft.CodeAnalysis.Testing;

namespace ResultNet.Analyzers.Tests;

public class SwitchExpressionNullAnalyzerTests
{
    [Fact]
    public async Task SwitchExpression_WithNullPattern_NoDiagnostic()
    {
        var source = """
            using ResultNet;

            public class TestClass
            {
                public string Test(Result<int> result)
                {
                    return result switch
                    {
                        null => "No result",
                        _ => "Has result"
                    };
                }
            }
            """;

        await CSharpAnalyzerVerifier<SwitchExpressionNullAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task SwitchStatement_WithNullCase_NoDiagnostic()
    {
        var source = """
            using ResultNet;

            public class TestClass
            {
                public void Test(Result<int> result)
                {
                    switch (result)
                    {
                        case null:
                            break;
                        default:
                            break;
                    }
                }
            }
            """;

        await CSharpAnalyzerVerifier<SwitchExpressionNullAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task SwitchStatement_WithNullPatternCase_NoDiagnostic()
    {
        var source = """
            using ResultNet;

            public class TestClass
            {
                public void Test(Result<int> result)
                {
                    switch (result)
                    {
                        case null:
                            break;
                        default:
                            break;
                    }
                }
            }
            """;

        await CSharpAnalyzerVerifier<SwitchExpressionNullAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task SwitchExpression_WithString_ReportsDiagnostic()
    {
        var source = """
            public class TestClass
            {
                public string Test(string? value)
                {
                    return value switch
                    {
                        [|null|] => "No value",
                        _ => "Has value"
                    };
                }
            }
            """;

        await CSharpAnalyzerVerifier<SwitchExpressionNullAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task SwitchExpression_WithCustomClass_ReportsDiagnostic()
    {
        var source = """
            public class MyClass
            {
                public string Name { get; set; }
            }

            public class TestClass
            {
                public string Test(MyClass? obj)
                {
                    return obj switch
                    {
                        [|null|] => "No object",
                        _ => "Has object"
                    };
                }
            }
            """;

        await CSharpAnalyzerVerifier<SwitchExpressionNullAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task SwitchExpression_WithValueType_NoDiagnostic()
    {
        var source = """
            public class TestClass
            {
                public string Test(int? value)
                {
                    return value switch
                    {
                        null => "No value",
                        _ => "Has value"
                    };
                }
            }
            """;

        await CSharpAnalyzerVerifier<SwitchExpressionNullAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task SwitchExpression_WithoutNull_NoDiagnostic()
    {
        var source = """
            using ResultNet;

            public class TestClass
            {
                public string Test(Result<int> result)
                {
                    return result.IsSuccess switch
                    {
                        true => "Success",
                        false => "Failure"
                    };
                }
            }
            """;

        await CSharpAnalyzerVerifier<SwitchExpressionNullAnalyzer>.VerifyAnalyzerAsync(source);
    }
}
