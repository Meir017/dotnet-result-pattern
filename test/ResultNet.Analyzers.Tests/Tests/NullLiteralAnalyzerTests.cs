namespace ResultNet.Analyzers.Tests;

public class NullLiteralAnalyzerTests
{
    [Fact]
    public async Task ReturnNull_WithResultType_NoDiagnostic()
    {
        var source = """
            using ResultNet;

            public class TestClass
            {
                public Result<int> GetValue()
                {
                    return null;
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullLiteralAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task ReturnNull_WithNonGenericResult_NoDiagnostic()
    {
        var source = """
            using ResultNet;

            public class TestClass
            {
                public Result GetValue()
                {
                    return null;
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullLiteralAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task ReturnNull_WithString_ReportsDiagnostic()
    {
        var source = """
            public class TestClass
            {
                public string? GetValue()
                {
                    return {|#0:null|};
                }
            }
            """;

        var expected = CSharpAnalyzerVerifier<NullLiteralAnalyzer>
            .Diagnostic(DiagnosticDescriptors.RN001_NullLiteralReturn)
            .WithLocation(0)
            .WithArguments("String");

        await CSharpAnalyzerVerifier<NullLiteralAnalyzer>.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task ReturnNull_WithCustomClass_ReportsDiagnostic()
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
                    return {|#0:null|};
                }
            }
            """;

        var expected = CSharpAnalyzerVerifier<NullLiteralAnalyzer>
            .Diagnostic(DiagnosticDescriptors.RN001_NullLiteralReturn)
            .WithLocation(0)
            .WithArguments("MyClass");

        await CSharpAnalyzerVerifier<NullLiteralAnalyzer>.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task VariableDeclarationWithNull_WithResultType_NoDiagnostic()
    {
        var source = """
            using ResultNet;

            public class TestClass
            {
                public void Test()
                {
                    Result<string> result = null;
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullLiteralAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task VariableDeclarationWithNull_WithString_ReportsDiagnostic()
    {
        var source = """
            public class TestClass
            {
                public void Test()
                {
                    string? value = {|#0:null|};
                }
            }
            """;

        var expected = CSharpAnalyzerVerifier<NullLiteralAnalyzer>
            .Diagnostic(DiagnosticDescriptors.RN005_NullAssignment)
            .WithLocation(0)
            .WithArguments("String");

        await CSharpAnalyzerVerifier<NullLiteralAnalyzer>.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task AssignmentWithNull_WithResultType_NoDiagnostic()
    {
        var source = """
            using ResultNet;

            public class TestClass
            {
                public void Test()
                {
                    Result<int> result = Result<int>.Success(42);
                    result = null;
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullLiteralAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task AssignmentWithNull_WithString_ReportsDiagnostic()
    {
        var source = """
            public class TestClass
            {
                public void Test()
                {
                    string? value = "test";
                    value = {|#0:null|};
                }
            }
            """;

        var expected = CSharpAnalyzerVerifier<NullLiteralAnalyzer>
            .Diagnostic(DiagnosticDescriptors.RN005_NullAssignment)
            .WithLocation(0)
            .WithArguments("String");

        await CSharpAnalyzerVerifier<NullLiteralAnalyzer>.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task AssignmentWithNull_WithCustomClass_ReportsDiagnostic()
    {
        var source = """
            public class MyClass
            {
                public string Name { get; set; }
            }

            public class TestClass
            {
                public void Test()
                {
                    MyClass? obj = new MyClass();
                    obj = {|#0:null|};
                }
            }
            """;

        var expected = CSharpAnalyzerVerifier<NullLiteralAnalyzer>
            .Diagnostic(DiagnosticDescriptors.RN005_NullAssignment)
            .WithLocation(0)
            .WithArguments("MyClass");

        await CSharpAnalyzerVerifier<NullLiteralAnalyzer>.VerifyAnalyzerAsync(source, expected);
    }
}

