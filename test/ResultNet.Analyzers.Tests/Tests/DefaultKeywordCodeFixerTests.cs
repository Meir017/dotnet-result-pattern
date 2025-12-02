using Microsoft.CodeAnalysis.Testing;
using ResultNet.CodeFixers;

namespace ResultNet.Analyzers.Tests;

public class DefaultKeywordCodeFixerTests
{
    [Fact]
    public async Task ReplaceDefaultLiteral_WithGenericResultFailure()
    {
        var source = """
            public class TestClass
            {
                public void Test()
                {
                    string? result = default;
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

        var expected = CSharpCodeFixVerifier<DefaultKeywordAnalyzer, DefaultKeywordCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN002_DefaultKeyword)
            .WithSpan(5, 26, 5, 33)
            .WithArguments("String");

        await CSharpCodeFixVerifier<DefaultKeywordAnalyzer, DefaultKeywordCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task ReplaceDefaultExpression_WithGenericResultFailure()
    {
        var source = """
            public class TestClass
            {
                public void Test()
                {
                    string? result = default(string);
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

        var expected = CSharpCodeFixVerifier<DefaultKeywordAnalyzer, DefaultKeywordCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN002_DefaultKeyword)
            .WithSpan(5, 26, 5, 41)
            .WithArguments("String");

        await CSharpCodeFixVerifier<DefaultKeywordAnalyzer, DefaultKeywordCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task ReplaceDefaultInReturn_WithResultFailure()
    {
        var source = """
            public class TestClass
            {
                public string? GetValue()
                {
                    return default;
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

        var expected = CSharpCodeFixVerifier<DefaultKeywordAnalyzer, DefaultKeywordCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN002_DefaultKeyword)
            .WithSpan(5, 16, 5, 23)
            .WithArguments("String");

        await CSharpCodeFixVerifier<DefaultKeywordAnalyzer, DefaultKeywordCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task ReplaceDefault_WithNonGenericResultFailure()
    {
        var source = """
            public class MyData
            {
                public int Value { get; set; }
            }

            public class TestClass
            {
                public void Test()
                {
                    MyData? result = default;
                }
            }
            """;

        var fixedSource = """
            using ResultNet;

            public class MyData
            {
                public int Value { get; set; }
            }

            public class TestClass
            {
                public void Test()
                {
                    Result<MyData> result = Result<MyData>.Failure();
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<DefaultKeywordAnalyzer, DefaultKeywordCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN002_DefaultKeyword)
            .WithSpan(10, 26, 10, 33)
            .WithArguments("MyData");

        await CSharpCodeFixVerifier<DefaultKeywordAnalyzer, DefaultKeywordCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task ReplaceDefaultWithGenericType()
    {
        var source = """
            using System.Collections.Generic;

            public class TestClass
            {
                public void Test()
                {
                    List<string>? list = default(List<string>);
                }
            }
            """;

        var fixedSource = """
            using ResultNet;
            using System.Collections.Generic;

            public class TestClass
            {
                public void Test()
                {
                    Result<List<string>> list = Result<List<string>>.Failure();
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<DefaultKeywordAnalyzer, DefaultKeywordCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN002_DefaultKeyword)
            .WithSpan(7, 30, 7, 51)
            .WithArguments("List");

        await CSharpCodeFixVerifier<DefaultKeywordAnalyzer, DefaultKeywordCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task ReplaceDefaultWithArrayType()
    {
        var source = """
            public class TestClass
            {
                public void Test()
                {
                    string[]? array = default(string[]);
                }
            }
            """;

        var fixedSource = """
            using ResultNet;

            public class TestClass
            {
                public void Test()
                {
                    Result<string[]> array = Result<string[]>.Failure();
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<DefaultKeywordAnalyzer, DefaultKeywordCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN002_DefaultKeyword)
            .WithSpan(5, 27, 5, 44)
            .WithArguments("String[]");

        await CSharpCodeFixVerifier<DefaultKeywordAnalyzer, DefaultKeywordCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task ReplaceDefaultWithNestedGenericType()
    {
        var source = """
            using System.Collections.Generic;

            public class TestClass
            {
                public void Test()
                {
                    Dictionary<string, List<int>>? dict = default;
                }
            }
            """;

        var fixedSource = """
            using ResultNet;
            using System.Collections.Generic;

            public class TestClass
            {
                public void Test()
                {
                    Result<Dictionary<string, List<int>>> dict = Result<Dictionary<string, List<int>>>.Failure();
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<DefaultKeywordAnalyzer, DefaultKeywordCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN002_DefaultKeyword)
            .WithSpan(7, 47, 7, 54)
            .WithArguments("Dictionary");

        await CSharpCodeFixVerifier<DefaultKeywordAnalyzer, DefaultKeywordCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }
}