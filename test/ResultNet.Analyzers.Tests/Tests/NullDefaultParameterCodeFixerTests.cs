using Microsoft.CodeAnalysis.Testing;
using ResultNet.CodeFixers;

namespace ResultNet.Analyzers.Tests;

public class NullDefaultParameterCodeFixerTests
{
    [Fact]
    public async Task RemoveNullDefaultValue_FromParameter()
    {
        var source = """
            public class TestClass
            {
                public void Test(string? result = null)
                {
                }
            }
            """;

        var fixedSource = """
            using ResultNet;

            public class TestClass
            {
                public void Test(Result<string> result)
                {
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullDefaultParameterAnalyzer, NullDefaultParameterCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN010_NullDefaultParameter)
            .WithSpan(3, 39, 3, 43)
            .WithArguments("String");

        await CSharpCodeFixVerifier<NullDefaultParameterAnalyzer, NullDefaultParameterCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task RemoveNullDefaultValue_PreservesOtherParameters()
    {
        var source = """
            public class TestClass
            {
                public void Test(string name, object? result = null, int count = 0)
                {
                }
            }
            """;

        var fixedSource = """
            using ResultNet;

            public class TestClass
            {
                public void Test(string name, Result<object> result, int count = 0)
                {
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullDefaultParameterAnalyzer, NullDefaultParameterCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN010_NullDefaultParameter)
            .WithSpan(3, 52, 3, 56)
            .WithArguments("Object");

        await CSharpCodeFixVerifier<NullDefaultParameterAnalyzer, NullDefaultParameterCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }
}
