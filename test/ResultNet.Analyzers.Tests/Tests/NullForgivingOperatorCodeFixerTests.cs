using Microsoft.CodeAnalysis.Testing;
using ResultNet.CodeFixers;

namespace ResultNet.Analyzers.Tests;

public class NullForgivingOperatorCodeFixerTests
{
    [Fact]
    public async Task RemovesNullForgivingOperator_FromStringVariable()
    {
        var source = """
            public class TestClass
            {
                public void Test(string? value)
                {
                    var result = value!;
                }
            }
            """;

        var fixedSource = """
            public class TestClass
            {
                public void Test(string? value)
                {
                    var result = value;
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullForgivingOperatorAnalyzer, NullForgivingOperatorCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN008_NullForgivingOperator)
            .WithSpan(5, 27, 5, 28);

        await CSharpCodeFixVerifier<NullForgivingOperatorAnalyzer, NullForgivingOperatorCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task RemovesNullForgivingOperator_FromMemberAccess()
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
                    var name = obj!.Name;
                }
            }
            """;

        var fixedSource = """
            public class MyClass
            {
                public string Name { get; set; }
            }

            public class TestClass
            {
                public void Test(MyClass? obj)
                {
                    var name = obj.Name;
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullForgivingOperatorAnalyzer, NullForgivingOperatorCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN008_NullForgivingOperator)
            .WithSpan(10, 23, 10, 24);

        await CSharpCodeFixVerifier<NullForgivingOperatorAnalyzer, NullForgivingOperatorCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task RemovesNullForgivingOperator_PreservesWhitespace()
    {
        var source = """
            public class TestClass
            {
                public void Test(string? value)
                {
                    // Comment before
                    var result = value!; // Comment after
                }
            }
            """;

        var fixedSource = """
            public class TestClass
            {
                public void Test(string? value)
                {
                    // Comment before
                    var result = value; // Comment after
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullForgivingOperatorAnalyzer, NullForgivingOperatorCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN008_NullForgivingOperator)
            .WithSpan(6, 27, 6, 28);

        await CSharpCodeFixVerifier<NullForgivingOperatorAnalyzer, NullForgivingOperatorCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task RemovesNullForgivingOperator_InMethodCall()
    {
        var source = """
            public class TestClass
            {
                public void Test(string? value)
                {
                    var length = value!.Length;
                }
            }
            """;

        var fixedSource = """
            public class TestClass
            {
                public void Test(string? value)
                {
                    var length = value.Length;
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullForgivingOperatorAnalyzer, NullForgivingOperatorCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN008_NullForgivingOperator)
            .WithSpan(5, 27, 5, 28);

        await CSharpCodeFixVerifier<NullForgivingOperatorAnalyzer, NullForgivingOperatorCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task RemovesNullForgivingOperator_InNestedExpression()
    {
        var source = """
            public class TestClass
            {
                public void Test(string? value1, string? value2)
                {
                    var result = value1! + value2;
                }
            }
            """;

        var fixedSource = """
            public class TestClass
            {
                public void Test(string? value1, string? value2)
                {
                    var result = value1 + value2;
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullForgivingOperatorAnalyzer, NullForgivingOperatorCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN008_NullForgivingOperator)
            .WithSpan(5, 28, 5, 29);

        await CSharpCodeFixVerifier<NullForgivingOperatorAnalyzer, NullForgivingOperatorCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }
}
