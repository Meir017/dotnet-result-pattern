using Microsoft.CodeAnalysis.Testing;
using ResultNet.CodeFixers;

namespace ResultNet.Analyzers.Tests;

public class NullConditionalAccessCodeFixerTests
{
    [Fact]
    public async Task ReplaceNullConditional_WithConditionalExpression()
    {
        var source = """
            public class MyData
            {
                public int Value { get; set; }
            }

            public class TestClass
            {
                public void Test(MyData? result)
                {
                    var value = result?.Value;
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
                public void Test(Result<MyData> result)
                {
                    var value = result.IsSuccess ? result.Value.Value : default;
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullConditionalAccessAnalyzer, NullConditionalAccessCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN007_NullConditionalAccess)
            .WithSpan(10, 27, 10, 28);

        await CSharpCodeFixVerifier<NullConditionalAccessAnalyzer, NullConditionalAccessCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task ReplaceNullConditional_OnProperty()
    {
        var source = """
            public class TestClass
            {
                public void Test(string? result)
                {
                    var length = result?.Length;
                }
            }
            """;

        var fixedSource = """
            using ResultNet;

            public class TestClass
            {
                public void Test(Result<string> result)
                {
                    var length = result.IsSuccess ? result.Value.Length : default;
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullConditionalAccessAnalyzer, NullConditionalAccessCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN007_NullConditionalAccess)
            .WithSpan(5, 28, 5, 29);

        await CSharpCodeFixVerifier<NullConditionalAccessAnalyzer, NullConditionalAccessCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task ReplaceNullConditional_MethodInvocation()
    {
        var source = """
            public class MyData
            {
                public string GetName() => "Test";
            }

            public class TestClass
            {
                public void Test(MyData? data)
                {
                    var name = data?.GetName();
                }
            }
            """;

        var fixedSource = """
            using ResultNet;

            public class MyData
            {
                public string GetName() => "Test";
            }

            public class TestClass
            {
                public void Test(Result<MyData> data)
                {
                    var name = data.IsSuccess ? data.Value.GetName() : default;
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullConditionalAccessAnalyzer, NullConditionalAccessCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN007_NullConditionalAccess)
            .WithSpan(10, 24, 10, 25);

        await CSharpCodeFixVerifier<NullConditionalAccessAnalyzer, NullConditionalAccessCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task ReplaceNullConditional_IndexerAccess()
    {
        var source = """
            public class TestClass
            {
                public void Test(string[]? arr)
                {
                    var first = arr?[0];
                }
            }
            """;

        var fixedSource = """
            using ResultNet;

            public class TestClass
            {
                public void Test(Result<string[]> arr)
                {
                    var first = arr.IsSuccess ? arr.Value[0] : default;
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullConditionalAccessAnalyzer, NullConditionalAccessCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN007_NullConditionalAccess)
            .WithSpan(5, 24, 5, 25);

        await CSharpCodeFixVerifier<NullConditionalAccessAnalyzer, NullConditionalAccessCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task ReplaceNullConditional_ChainedAccess()
    {
        var source = """
            public class Inner
            {
                public string Value { get; set; }
            }

            public class Outer
            {
                public Inner? InnerObj { get; set; }
            }

            public class TestClass
            {
                public void Test(Outer? obj)
                {
                    var value = obj?.InnerObj;
                }
            }
            """;

        var fixedSource = """
            using ResultNet;

            public class Inner
            {
                public string Value { get; set; }
            }

            public class Outer
            {
                public Inner? InnerObj { get; set; }
            }

            public class TestClass
            {
                public void Test(Result<Outer> obj)
                {
                    var value = obj.IsSuccess ? obj.Value.InnerObj : default;
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullConditionalAccessAnalyzer, NullConditionalAccessCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN007_NullConditionalAccess)
            .WithSpan(15, 24, 15, 25);

        await CSharpCodeFixVerifier<NullConditionalAccessAnalyzer, NullConditionalAccessCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task ReplaceNullConditional_InReturnStatement()
    {
        var source = """
            public class TestClass
            {
                public int? GetLength(string? value)
                {
                    return value?.Length;
                }
            }
            """;

        var fixedSource = """
            using ResultNet;

            public class TestClass
            {
                public int? GetLength(Result<string> value)
                {
                    return value.IsSuccess ? value.Value.Length : default;
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullConditionalAccessAnalyzer, NullConditionalAccessCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN007_NullConditionalAccess)
            .WithSpan(5, 21, 5, 22);

        await CSharpCodeFixVerifier<NullConditionalAccessAnalyzer, NullConditionalAccessCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task ReplaceNullConditional_GenericType()
    {
        var source = """
            using System.Collections.Generic;

            public class TestClass
            {
                public void Test(List<string>? list)
                {
                    var count = list?.Count;
                }
            }
            """;

        var fixedSource = """
            using ResultNet;
            using System.Collections.Generic;

            public class TestClass
            {
                public void Test(Result<List<string>> list)
                {
                    var count = list.IsSuccess ? list.Value.Count : default;
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullConditionalAccessAnalyzer, NullConditionalAccessCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN007_NullConditionalAccess)
            .WithSpan(7, 25, 7, 26);

        await CSharpCodeFixVerifier<NullConditionalAccessAnalyzer, NullConditionalAccessCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task PreserveExistingUsing()
    {
        var source = """
            using ResultNet;

            public class TestClass
            {
                public void Test(string? value)
                {
                    var length = value?.Length;
                }
            }
            """;

        var fixedSource = """
            using ResultNet;

            public class TestClass
            {
                public void Test(Result<string> value)
                {
                    var length = value.IsSuccess ? value.Value.Length : default;
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullConditionalAccessAnalyzer, NullConditionalAccessCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN007_NullConditionalAccess)
            .WithSpan(7, 27, 7, 28);

        await CSharpCodeFixVerifier<NullConditionalAccessAnalyzer, NullConditionalAccessCodeFixer>
            .VerifyCodeFixAsync(source, fixedSource, expected);
    }
}
