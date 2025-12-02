using Microsoft.CodeAnalysis.Testing;

namespace ResultNet.Analyzers.Tests;

public class NullCoalescingAssignmentAnalyzerTests
{
    [Fact]
    public async Task NullCoalescingAssignment_WithResult_NoDiagnostic()
    {
        var source = """
            using ResultNet;

            public class TestClass
            {
                public void Test()
                {
                    Result<int> result = Result<int>.Success(1);
                    result ??= Result<int>.Success(0);
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullCoalescingAssignmentAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NullCoalescingAssignment_WithString_ReportsDiagnostic()
    {
        var source = """
            public class TestClass
            {
                public void Test()
                {
                    string? value = "test";
                    value [|??=|] "default";
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullCoalescingAssignmentAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NullCoalescingAssignment_WithCustomClass_ReportsDiagnostic()
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
                    MyClass? obj = new MyClass();
                    obj [|??=|] new MyClass();
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullCoalescingAssignmentAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NullCoalescingAssignment_WithValueType_NoDiagnostic()
    {
        var source = """
            public class TestClass
            {
                public void Test()
                {
                    int? value = 5;
                    value ??= 0;
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullCoalescingAssignmentAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NullCoalescingAssignment_WithNonGenericResult_NoDiagnostic()
    {
        var source = """
            using ResultNet;

            public class TestClass
            {
                public void Test()
                {
                    Result result = Result.Success();
                    result ??= Result.Failure("error");
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullCoalescingAssignmentAnalyzer>.VerifyAnalyzerAsync(source);
    }
}
