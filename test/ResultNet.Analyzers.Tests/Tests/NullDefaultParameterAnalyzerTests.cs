using Microsoft.CodeAnalysis.Testing;

namespace ResultNet.Analyzers.Tests;

public class NullDefaultParameterAnalyzerTests
{
    [Fact]
    public async Task NullDefaultParameter_WithResult_NoDiagnostic()
    {
        var source = """
            using ResultNet;

            public class TestClass
            {
                public void Process(Result<int> result = null)
                {
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullDefaultParameterAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NullDefaultParameter_WithNonGenericResult_NoDiagnostic()
    {
        var source = """
            using ResultNet;

            public class TestClass
            {
                public void Process(Result result = null)
                {
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullDefaultParameterAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NullDefaultParameter_WithString_ReportsDiagnostic()
    {
        var source = """
            public class TestClass
            {
                public void Process(string? value = [|null|])
                {
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullDefaultParameterAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NullDefaultParameter_WithCustomClass_ReportsDiagnostic()
    {
        var source = """
            public class MyClass
            {
                public string Name { get; set; }
            }

            public class TestClass
            {
                public void Process(MyClass? obj = [|null|])
                {
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullDefaultParameterAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NonNullDefaultParameter_WithResult_NoDiagnostic()
    {
        var source = """
            using ResultNet;

            public class TestClass
            {
                private static readonly Result<int> DefaultResult = Result<int>.Failure("Default");

                public void Process(Result<int> result = default)
                {
                }
            }
            """;

        // Note: This won't be caught by RN010, and RN002 won't trigger on Result types either
        await CSharpAnalyzerVerifier<NullDefaultParameterAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DefaultParameter_WithString_ReportsDiagnostic()
    {
        var source = """
            public class TestClass
            {
                public void Process(string value = [|default|])
                {
                }
            }
            """;

        await CSharpAnalyzerVerifier<DefaultKeywordAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task OptionalParameter_WithoutDefault_NoDiagnostic()
    {
        var source = """
            using ResultNet;

            public class TestClass
            {
                public void Process(Result<int> result)
                {
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullDefaultParameterAnalyzer>.VerifyAnalyzerAsync(source);
    }
}
