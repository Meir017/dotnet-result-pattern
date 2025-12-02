using Microsoft.CodeAnalysis.Testing;
using ResultNet.CodeFixers;

namespace ResultNet.Analyzers.Tests;

public class NullCoalescingCodeFixerTests
{
    [Fact]
    public async Task ReplaceWithValueOr_SimpleVariable()
    {
        var source = """
            public class TestClass
            {
                public void Test(string? value)
                {
                    var result = value ?? "default";
                }
            }
            """;

        var fixedCode = """
            using ResultNet;
            
            public class TestClass
            {
                public void Test(Result<string> value)
                {
                    var result = value.ValueOr("default");
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullCoalescingAnalyzer, NullCoalescingCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN003_NullCoalescing)
            .WithSpan(5, 28, 5, 30);

        await CSharpCodeFixVerifier<NullCoalescingAnalyzer, NullCoalescingCodeFixer>
            .VerifyCodeFixAsync(source, fixedCode, expected);
    }

    [Fact]
    public async Task ReplaceWithValueOr_InReturnStatement()
    {
        var source = """
            public class TestClass
            {
                public string GetValue(string? value)
                {
                    return value ?? "default";
                }
            }
            """;

        var fixedCode = """
            using ResultNet;
            
            public class TestClass
            {
                public string GetValue(Result<string> value)
                {
                    return value.ValueOr("default");
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullCoalescingAnalyzer, NullCoalescingCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN003_NullCoalescing)
            .WithSpan(5, 22, 5, 24);

        await CSharpCodeFixVerifier<NullCoalescingAnalyzer, NullCoalescingCodeFixer>
            .VerifyCodeFixAsync(source, fixedCode, expected);
    }

    [Fact]
    public async Task ReplaceWithValueOr_CustomClass()
    {
        var source = """
            public class MyData
            {
                public string Name { get; set; }
            }
            
            public class TestClass
            {
                public void Test(MyData? data)
                {
                    var result = data ?? new MyData();
                }
            }
            """;

        var fixedCode = """
            using ResultNet;
            
            public class MyData
            {
                public string Name { get; set; }
            }
            
            public class TestClass
            {
                public void Test(Result<MyData> data)
                {
                    var result = data.ValueOr(new MyData());
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullCoalescingAnalyzer, NullCoalescingCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN003_NullCoalescing)
            .WithSpan(10, 27, 10, 29);

        await CSharpCodeFixVerifier<NullCoalescingAnalyzer, NullCoalescingCodeFixer>
            .VerifyCodeFixAsync(source, fixedCode, expected);
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
                    var result = value ?? "default";
                }
            }
            """;

        var fixedCode = """
            using ResultNet;
            
            public class TestClass
            {
                public void Test(Result<string> value)
                {
                    var result = value.ValueOr("default");
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullCoalescingAnalyzer, NullCoalescingCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN003_NullCoalescing)
            .WithSpan(7, 28, 7, 30);

        await CSharpCodeFixVerifier<NullCoalescingAnalyzer, NullCoalescingCodeFixer>
            .VerifyCodeFixAsync(source, fixedCode, expected);
    }
}
