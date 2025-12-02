using Microsoft.CodeAnalysis.Testing;
using ResultNet.CodeFixers;

namespace ResultNet.Analyzers.Tests;

public class NullLiteralCodeFixerTests
{
    [Fact]
    public async Task RN001_ReplaceNullReturn_WithGenericResultFailure()
    {
        var source = """
            using ResultNet;

            public class TestClass
            {
                public string? GetValue()
                {
                    return null;
                }
            }
            """;

        var fixedSource = """
            using ResultNet;

            public class TestClass
            {
                public Result<string> GetValue()
                {
                    return Result<string>.Failure();
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullLiteralAnalyzer, NullLiteralCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN001_NullLiteralReturn)
            .WithSpan(7, 16, 7, 20)
            .WithArguments("String");

        await CSharpCodeFixVerifier<NullLiteralAnalyzer, NullLiteralCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task RN001_ReplaceNullReturn_WithNonGenericResultFailure()
    {
        var source = """
            public class MyClass
            {
                public string Name { get; set; }
            }

            public class TestClass
            {
                public MyClass? GetValue()
                {
                    return null;
                }
            }
            """;

        var fixedSource = """
            using ResultNet;

            public class MyClass
            {
                public string Name { get; set; }
            }

            public class TestClass
            {
                public Result<MyClass> GetValue()
                {
                    return Result<MyClass>.Failure();
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullLiteralAnalyzer, NullLiteralCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN001_NullLiteralReturn)
            .WithSpan(10, 16, 10, 20)
            .WithArguments("MyClass");

        await CSharpCodeFixVerifier<NullLiteralAnalyzer, NullLiteralCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task RN005_ReplaceNullAssignment_InVariableDeclaration()
    {
        var source = """
            public class TestClass
            {
                public void Test()
                {
                    string? result = null;
                }
            }
            """;

        var fixedSource = """
            using ResultNet;

            public class TestClass
            {
                public void Test()
                {
                    Result<string> result = Result<string>.Failure();
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullLiteralAnalyzer, NullLiteralCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN005_NullAssignment)
            .WithSpan(5, 26, 5, 30)
            .WithArguments("String");

        await CSharpCodeFixVerifier<NullLiteralAnalyzer, NullLiteralCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task RN005_ReplaceNullAssignment_InAssignmentExpression()
    {
        var source = """
            public class TestClass
            {
                public void Test()
                {
                    string? result;
                    result = null;
                }
            }
            """;

        var fixedSource = """
            using ResultNet;

            public class TestClass
            {
                public void Test()
                {
                    Result<string> result;
                    result = Result<string>.Failure();
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullLiteralAnalyzer, NullLiteralCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN005_NullAssignment)
            .WithSpan(6, 18, 6, 22)
            .WithArguments("String");

        await CSharpCodeFixVerifier<NullLiteralAnalyzer, NullLiteralCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }
}
