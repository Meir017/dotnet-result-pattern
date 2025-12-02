using Microsoft.CodeAnalysis.Testing;

namespace ResultNet.Analyzers.Tests;

public class NullCoalescingAnalyzerTests
{
    [Fact]
    public async Task NullCoalescing_WithResult_NoDiagnostic()
    {
        var source = """
            using ResultNet;

            public class TestClass
            {
                public void Test(Result<int> result)
                {
                    var value = result ?? Result<int>.Success(0);
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullCoalescingAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NullCoalescing_WithString_ReportsDiagnostic()
    {
        var source = """
            public class TestClass
            {
                public void Test(string? value)
                {
                    var result = value [|??|] "default";
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullCoalescingAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NullCoalescing_WithCustomClass_ReportsDiagnostic()
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
                    var result = obj [|??|] new MyClass();
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullCoalescingAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NullCoalescing_WithValueType_NoDiagnostic()
    {
        var source = """
            public class TestClass
            {
                public void Test(int? value)
                {
                    var result = value ?? 0;
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullCoalescingAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NullCoalescingChain_WithResult_NoDiagnostic()
    {
        var source = """
            using ResultNet;

            public class TestClass
            {
                public void Test(Result<int> r1, Result<int> r2, Result<int> r3)
                {
                    var value = r1 ?? r2 ?? r3;
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullCoalescingAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NullCoalescingChain_WithString_ReportsDiagnostics()
    {
        var source = """
            public class TestClass
            {
                public void Test(string? s1, string? s2, string? s3)
                {
                    var value = s1 [|??|] s2 [|??|] s3;
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullCoalescingAnalyzer>.VerifyAnalyzerAsync(source);
    }
}
