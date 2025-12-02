using Microsoft.CodeAnalysis.Testing;

namespace ResultNet.Analyzers.Tests;

public class NullForgivingOperatorAnalyzerTests
{
    [Fact]
    public async Task NullForgivingOperator_WithResult_NoDiagnostic()
    {
        var source = """
            using ResultNet;

            public class TestClass
            {
                public void Test(Result<int>? result)
                {
                    var value = result!;
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullForgivingOperatorAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NullForgivingOperator_WithString_ReportsDiagnostic()
    {
        var source = """
            public class TestClass
            {
                public void Test(string? value)
                {
                    var result = value[|!|];
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullForgivingOperatorAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NullForgivingOperator_WithCustomClass_ReportsDiagnostic()
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
                    var name = obj[|!|].Name;
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullForgivingOperatorAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NullForgivingOperator_WithValueType_NoDiagnostic()
    {
        var source = """
            public class TestClass
            {
                public void Test(int? value)
                {
                    var result = value!.ToString();
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullForgivingOperatorAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NullForgivingOperator_OnResultMemberAccess_NoDiagnostic()
    {
        var source = """
            using ResultNet;

            public class TestClass
            {
                public void Test(Result<int>? result)
                {
                    var isSuccess = result!.IsSuccess;
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullForgivingOperatorAnalyzer>.VerifyAnalyzerAsync(source);
    }
}
