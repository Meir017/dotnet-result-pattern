using Microsoft.CodeAnalysis.Testing;
using ResultNet.CodeFixers;

namespace ResultNet.Analyzers.Tests;

public class NullCheckCodeFixerTests
{
    [Fact]
    public async Task ReplaceEqualsNull_WithIsFailure()
    {
        var source = """
            public class TestClass
            {
                public void Test(string? result)
                {
                    if (result == null)
                    {
                    }
                }
            }
            """;

        var fixedSource = """
            using ResultNet;

            public class TestClass
            {
                public void Test(Result<string> result)
                {
                    if (result.IsFailure)
                    {
                    }
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullCheckAnalyzer, NullCheckCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN004_NullCheck)
            .WithSpan(5, 13, 5, 27);

        await CSharpCodeFixVerifier<NullCheckAnalyzer, NullCheckCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task ReplaceNotEqualsNull_WithIsSuccess()
    {
        var source = """
            public class TestClass
            {
                public void Test(string? result)
                {
                    if (result != null)
                    {
                    }
                }
            }
            """;

        var fixedSource = """
            using ResultNet;

            public class TestClass
            {
                public void Test(Result<string> result)
                {
                    if (result.IsSuccess)
                    {
                    }
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullCheckAnalyzer, NullCheckCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN004_NullCheck)
            .WithSpan(5, 13, 5, 27);

        await CSharpCodeFixVerifier<NullCheckAnalyzer, NullCheckCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task ReplaceIsNull_WithIsFailure()
    {
        var source = """
            public class TestClass
            {
                public void Test(string? result)
                {
                    if (result is null)
                    {
                    }
                }
            }
            """;

        var fixedSource = """
            using ResultNet;

            public class TestClass
            {
                public void Test(Result<string> result)
                {
                    if (result.IsFailure)
                    {
                    }
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullCheckAnalyzer, NullCheckCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN004_NullCheck)
            .WithSpan(5, 13, 5, 27);

        await CSharpCodeFixVerifier<NullCheckAnalyzer, NullCheckCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task ReplaceIsNotNull_WithIsSuccess()
    {
        var source = """
            public class TestClass
            {
                public void Test(string? result)
                {
                    if (result is not null)
                    {
                    }
                }
            }
            """;

        var fixedSource = """
            using ResultNet;

            public class TestClass
            {
                public void Test(Result<string> result)
                {
                    if (result.IsSuccess)
                    {
                    }
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullCheckAnalyzer, NullCheckCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN004_NullCheck)
            .WithSpan(5, 13, 5, 31);

        await CSharpCodeFixVerifier<NullCheckAnalyzer, NullCheckCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task ReplaceNullEqualsResult_WithIsFailure()
    {
        var source = """
            public class TestClass
            {
                public void Test(string? result)
                {
                    if (null == result)
                    {
                    }
                }
            }
            """;

        var fixedSource = """
            using ResultNet;

            public class TestClass
            {
                public void Test(Result<string> result)
                {
                    if (result.IsFailure)
                    {
                    }
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullCheckAnalyzer, NullCheckCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN004_NullCheck)
            .WithSpan(5, 13, 5, 27);

        await CSharpCodeFixVerifier<NullCheckAnalyzer, NullCheckCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task ReplaceNullCheckInWhileLoop()
    {
        var source = """
            public class TestClass
            {
                public void Test(string? obj)
                {
                    while (obj != null)
                    {
                        obj = null;
                    }
                }
            }
            """;

        var fixedSource = """
            using ResultNet;

            public class TestClass
            {
                public void Test(Result<string> obj)
                {
                    while (obj.IsSuccess)
                    {
                        obj = null;
                    }
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullCheckAnalyzer, NullCheckCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN004_NullCheck)
            .WithSpan(5, 16, 5, 27);

        await CSharpCodeFixVerifier<NullCheckAnalyzer, NullCheckCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task ReplaceNullCheckInTernaryExpression()
    {
        var source = """
            public class TestClass
            {
                public void Test(string? obj)
                {
                    var x = obj == null ? "null" : "not null";
                }
            }
            """;

        var fixedSource = """
            using ResultNet;

            public class TestClass
            {
                public void Test(Result<string> obj)
                {
                    var x = obj.IsFailure ? "null" : "not null";
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullCheckAnalyzer, NullCheckCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN004_NullCheck)
            .WithSpan(5, 17, 5, 28);

        await CSharpCodeFixVerifier<NullCheckAnalyzer, NullCheckCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task ReplaceNullCheckInReturnStatement()
    {
        var source = """
            public class TestClass
            {
                public bool Test(string? obj)
                {
                    return obj == null;
                }
            }
            """;

        var fixedSource = """
            using ResultNet;

            public class TestClass
            {
                public bool Test(Result<string> obj)
                {
                    return obj.IsFailure;
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullCheckAnalyzer, NullCheckCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN004_NullCheck)
            .WithSpan(5, 16, 5, 27);

        await CSharpCodeFixVerifier<NullCheckAnalyzer, NullCheckCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task ReplaceMultipleNullChecksInCompoundCondition()
    {
        var source = """
            public class TestClass
            {
                public void Test(string? a, string? b)
                {
                    if (a == null && b != null)
                    {
                    }
                }
            }
            """;

        var fixedSource = """
            using ResultNet;

            public class TestClass
            {
                public void Test(Result<string> a, Result<string> b)
                {
                    if (a.IsFailure && b.IsSuccess)
                    {
                    }
                }
            }
            """;

        var expected = new[]
        {
            CSharpCodeFixVerifier<NullCheckAnalyzer, NullCheckCodeFixer>
                .Diagnostic(DiagnosticDescriptors.RN004_NullCheck)
                .WithSpan(5, 13, 5, 22),
            CSharpCodeFixVerifier<NullCheckAnalyzer, NullCheckCodeFixer>
                .Diagnostic(DiagnosticDescriptors.RN004_NullCheck)
                .WithSpan(5, 26, 5, 35)
        };

        await CSharpCodeFixVerifier<NullCheckAnalyzer, NullCheckCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, 2, expected);
    }

    [Fact]
    public async Task ReplaceNullCheckNestedInExpression()
    {
        var source = """
            public class TestClass
            {
                public void Test(string? obj)
                {
                    var result = (obj == null);
                }
            }
            """;

        var fixedSource = """
            using ResultNet;

            public class TestClass
            {
                public void Test(Result<string> obj)
                {
                    var result = (obj.IsFailure);
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullCheckAnalyzer, NullCheckCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN004_NullCheck)
            .WithSpan(5, 23, 5, 34);

        await CSharpCodeFixVerifier<NullCheckAnalyzer, NullCheckCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task PreserveExistingUsingStatement()
    {
        var source = """
            using ResultNet;

            public class TestClass
            {
                public void Test(string? result)
                {
                    if (result == null)
                    {
                    }
                }
            }
            """;

        var fixedSource = """
            using ResultNet;

            public class TestClass
            {
                public void Test(Result<string> result)
                {
                    if (result.IsFailure)
                    {
                    }
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullCheckAnalyzer, NullCheckCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN004_NullCheck)
            .WithSpan(7, 13, 7, 27);

        await CSharpCodeFixVerifier<NullCheckAnalyzer, NullCheckCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task ReplaceNullCheckInExpressionBody()
    {
        var source = """
            public class TestClass
            {
                public bool IsNull(string? obj) => obj == null;
            }
            """;

        var fixedSource = """
            using ResultNet;

            public class TestClass
            {
                public bool IsNull(Result<string> obj) => obj.IsFailure;
            }
            """;

        var expected = CSharpCodeFixVerifier<NullCheckAnalyzer, NullCheckCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN004_NullCheck)
            .WithSpan(3, 40, 3, 51);

        await CSharpCodeFixVerifier<NullCheckAnalyzer, NullCheckCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task ReplaceNullCheckWithGenericType()
    {
        var source = """
            using System.Collections.Generic;

            public class TestClass
            {
                public void Test(List<string>? obj)
                {
                    if (obj == null)
                    {
                    }
                }
            }
            """;

        var fixedSource = """
            using ResultNet;
            using System.Collections.Generic;

            public class TestClass
            {
                public void Test(Result<List<string>> obj)
                {
                    if (obj.IsFailure)
                    {
                    }
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullCheckAnalyzer, NullCheckCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN004_NullCheck)
            .WithSpan(7, 13, 7, 24);

        await CSharpCodeFixVerifier<NullCheckAnalyzer, NullCheckCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task ReplaceNullCheckWithArrayType()
    {
        var source = """
            public class TestClass
            {
                public void Test(string[]? arr)
                {
                    if (arr == null)
                    {
                    }
                }
            }
            """;

        var fixedSource = """
            using ResultNet;

            public class TestClass
            {
                public void Test(Result<string[]> arr)
                {
                    if (arr.IsFailure)
                    {
                    }
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullCheckAnalyzer, NullCheckCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN004_NullCheck)
            .WithSpan(5, 13, 5, 24);

        await CSharpCodeFixVerifier<NullCheckAnalyzer, NullCheckCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }
}
