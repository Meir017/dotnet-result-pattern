using Microsoft.CodeAnalysis.Testing;

namespace ResultNet.Analyzers.Tests;

public class NullConditionalAccessAnalyzerTests
{
    [Fact]
    public async Task NullConditionalMemberAccess_WithResult_NoDiagnostic()
    {
        var source = """
            using ResultNet;

            public class TestClass
            {
                public void Test(Result<int> result)
                {
                    var value = result?.Value;
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullConditionalAccessAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NullConditionalElementAccess_WithResult_NoDiagnostic()
    {
        var source = """
            using ResultNet;

            public class TestClass
            {
                public void Test(Result<int[]> result)
                {
                    var value = result?.Value[0];
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullConditionalAccessAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NullConditionalAccess_WithString_ReportsDiagnostic()
    {
        var source = """
            public class TestClass
            {
                public void Test(string? value)
                {
                    var length = value[|?|].Length;
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullConditionalAccessAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NullConditionalAccess_WithCustomClass_ReportsDiagnostic()
    {
        var source = """
            public class MyClass
            {
                public string Name { get; set; }
            }

            public class TestClass
            {
                public void Test(MyClass? obj)
                {
                    var name = obj[|?|].Name;
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullConditionalAccessAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NullConditionalAccess_WithValueType_NoDiagnostic()
    {
        var source = """
            public class TestClass
            {
                public void Test(int? value)
                {
                    var str = value?.ToString();
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullConditionalAccessAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task RegularMemberAccess_WithResult_NoDiagnostic()
    {
        var source = """
            using ResultNet;

            public class TestClass
            {
                public void Test(Result<int> result)
                {
                    var isSuccess = result.IsSuccess;
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullConditionalAccessAnalyzer>.VerifyAnalyzerAsync(source);
    }
}
