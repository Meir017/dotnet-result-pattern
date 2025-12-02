using Microsoft.CodeAnalysis.Testing;
using ResultNet.CodeFixers;

namespace ResultNet.Analyzers.Tests;

public class NullCoalescingAssignmentCodeFixerTests
{
    [Fact]
    public async Task ReplaceWithIfStatement_LocalVariable()
    {
        var source = """
            public class TestClass
            {
                public void Test(string? value)
                {
                    value ??= "default";
                }
            }
            """;

        var fixedCode = """
            using ResultNet;
            
            public class TestClass
            {
                public void Test(Result<string> value)
                {
                    if (value.IsFailure)
                        value = "default";
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullCoalescingAssignmentAnalyzer, NullCoalescingAssignmentCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN006_NullCoalescingAssignment)
            .WithSpan(5, 15, 5, 18);

        await CSharpCodeFixVerifier<NullCoalescingAssignmentAnalyzer, NullCoalescingAssignmentCodeFixer>
            .VerifyCodeFixAsync(source, fixedCode, expected);
    }

    [Fact]
    public async Task ReplaceWithIfStatement_Field()
    {
        var source = """
            public class TestClass
            {
                private string? _value;
                
                public void Test()
                {
                    _value ??= "default";
                }
            }
            """;

        var fixedCode = """
            using ResultNet;
            
            public class TestClass
            {
                private string? _value;
                
                public void Test()
                {
                    if (_value.IsFailure)
                        _value = "default";
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullCoalescingAssignmentAnalyzer, NullCoalescingAssignmentCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN006_NullCoalescingAssignment)
            .WithSpan(7, 16, 7, 19);

        await CSharpCodeFixVerifier<NullCoalescingAssignmentAnalyzer, NullCoalescingAssignmentCodeFixer>
            .VerifyCodeFixAsync(source, fixedCode, expected);
    }

    [Fact]
    public async Task ReplaceWithIfStatement_Property()
    {
        var source = """
            public class TestClass
            {
                public string? Value { get; set; }
                
                public void Test()
                {
                    Value ??= "default";
                }
            }
            """;

        var fixedCode = """
            using ResultNet;
            
            public class TestClass
            {
                public string? Value { get; set; }
                
                public void Test()
                {
                    if (Value.IsFailure)
                        Value = "default";
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullCoalescingAssignmentAnalyzer, NullCoalescingAssignmentCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN006_NullCoalescingAssignment)
            .WithSpan(7, 15, 7, 18);

        await CSharpCodeFixVerifier<NullCoalescingAssignmentAnalyzer, NullCoalescingAssignmentCodeFixer>
            .VerifyCodeFixAsync(source, fixedCode, expected);
    }

    [Fact]
    public async Task ReplaceWithIfStatement_CustomClass()
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
                    data ??= new MyData();
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
                    if (data.IsFailure)
                        data = new MyData();
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullCoalescingAssignmentAnalyzer, NullCoalescingAssignmentCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN006_NullCoalescingAssignment)
            .WithSpan(10, 14, 10, 17);

        await CSharpCodeFixVerifier<NullCoalescingAssignmentAnalyzer, NullCoalescingAssignmentCodeFixer>
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
                    value ??= "default";
                }
            }
            """;

        var fixedCode = """
            using ResultNet;
            
            public class TestClass
            {
                public void Test(Result<string> value)
                {
                    if (value.IsFailure)
                        value = "default";
                }
            }
            """;

        var expected = CSharpCodeFixVerifier<NullCoalescingAssignmentAnalyzer, NullCoalescingAssignmentCodeFixer>
            .Diagnostic(DiagnosticDescriptors.RN006_NullCoalescingAssignment)
            .WithSpan(7, 15, 7, 18);

        await CSharpCodeFixVerifier<NullCoalescingAssignmentAnalyzer, NullCoalescingAssignmentCodeFixer>
            .VerifyCodeFixAsync(source, fixedCode, expected);
    }
}
