using Microsoft.CodeAnalysis.Testing;

namespace ResultNet.Analyzers.Tests;

public class DefaultKeywordAnalyzerTests
{
    [Fact]
    public async Task DefaultExpression_WithResultType_NoDiagnostic()
    {
        var source = """
            using ResultNet;

            public class TestClass
            {
                public void Test()
                {
                    Result<int> result = default(Result<int>);
                }
            }
            """;

        await CSharpAnalyzerVerifier<DefaultKeywordAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DefaultLiteral_WithResultType_NoDiagnostic()
    {
        var source = """
            using ResultNet;

            public class TestClass
            {
                public void Test()
                {
                    Result<string> result = default;
                }
            }
            """;

        await CSharpAnalyzerVerifier<DefaultKeywordAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task ReturningDefault_WithResultType_NoDiagnostic()
    {
        var source = """
            using ResultNet;

            public class TestClass
            {
                public Result<int> GetValue()
                {
                    return default;
                }
            }
            """;

        await CSharpAnalyzerVerifier<DefaultKeywordAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DefaultExpression_WithString_ReportsDiagnostic()
    {
        var source = """
            public class TestClass
            {
                public void Test()
                {
                    string value = [|default(string)|];
                }
            }
            """;

        await CSharpAnalyzerVerifier<DefaultKeywordAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DefaultLiteral_WithString_ReportsDiagnostic()
    {
        var source = """
            public class TestClass
            {
                public void Test()
                {
                    string? value = [|default|];
                }
            }
            """;

        await CSharpAnalyzerVerifier<DefaultKeywordAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DefaultExpression_WithValueType_NoDiagnostic()
    {
        var source = """
            public class TestClass
            {
                public void Test()
                {
                    int value = default(int);
                }
            }
            """;

        await CSharpAnalyzerVerifier<DefaultKeywordAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DefaultExpression_WithCustomClass_ReportsDiagnostic()
    {
        var source = """
            public class MyClass
            {
                public string Name { get; set; }
            }

            public class TestClass
            {
                public void Test()
                {
                    MyClass obj = [|default(MyClass)|];
                }
            }
            """;

        await CSharpAnalyzerVerifier<DefaultKeywordAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DefaultExpression_WithNonGenericResultType_NoDiagnostic()
    {
        var source = """
            using ResultNet;

            public class TestClass
            {
                public void Test()
                {
                    Result result = default(Result);
                }
            }
            """;

        await CSharpAnalyzerVerifier<DefaultKeywordAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DefaultLiteral_WithNonGenericResultType_NoDiagnostic()
    {
        var source = """
            using ResultNet;

            public class TestClass
            {
                public void Test()
                {
                    Result result = default;
                }
            }
            """;

        await CSharpAnalyzerVerifier<DefaultKeywordAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DefaultInFieldInitializer_WithNullableType_ReportsDiagnostic()
    {
        var source = """
            public class TestClass
            {
                private string? _field = [|default|];
            }
            """;

        await CSharpAnalyzerVerifier<DefaultKeywordAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DefaultInPropertyInitializer_WithNullableType_ReportsDiagnostic()
    {
        var source = """
            public class TestClass
            {
                public string? Property { get; set; } = [|default|];
            }
            """;

        await CSharpAnalyzerVerifier<DefaultKeywordAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DefaultAsMethodArgument_WithNullableType_ReportsDiagnostic()
    {
        var source = """
            public class TestClass
            {
                public void Process(string? value) { }

                public void Test()
                {
                    Process([|default|]);
                }
            }
            """;

        await CSharpAnalyzerVerifier<DefaultKeywordAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DefaultInCollectionInitializer_WithNullableType_ReportsDiagnostic()
    {
        var source = """
            public class TestClass
            {
                public void Test()
                {
                    var array = new[] { "value", [|default(string)|] };
                }
            }
            """;

        await CSharpAnalyzerVerifier<DefaultKeywordAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DefaultInTernaryExpression_WithNullableType_ReportsDiagnostic()
    {
        var source = """
            public class TestClass
            {
                public void Test(bool condition)
                {
                    string? value = condition ? "value" : [|default|];
                }
            }
            """;

        await CSharpAnalyzerVerifier<DefaultKeywordAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DefaultInSwitchExpressionArm_WithNullableType_ReportsDiagnostic()
    {
        var source = """
            public class TestClass
            {
                public void Test(int value)
                {
                    string? result = value switch
                    {
                        1 => "one",
                        _ => [|default|]
                    };
                }
            }
            """;

        await CSharpAnalyzerVerifier<DefaultKeywordAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DefaultInLambdaExpressionBody_WithNullableType_ReportsDiagnostic()
    {
        var source = """
            using System;

            public class TestClass
            {
                public void Test()
                {
                    Func<string?> getValue = () => [|default|];
                }
            }
            """;

        await CSharpAnalyzerVerifier<DefaultKeywordAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task MultipleDefaultExpressions_WithNullableType_ReportsMultipleDiagnostics()
    {
        var source = """
            public class TestClass
            {
                public void Test()
                {
                    string? value1 = [|default|];
                    string? value2 = [|default(string)|];
                    var value3 = [|default(string)|];
                }
            }
            """;

        await CSharpAnalyzerVerifier<DefaultKeywordAnalyzer>.VerifyAnalyzerAsync(source);
    }
}
