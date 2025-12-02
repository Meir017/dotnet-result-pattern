using Microsoft.CodeAnalysis.Testing;

namespace ResultNet.Analyzers.Tests;

public class NullCheckAnalyzerTests
{
    [Fact]
    public async Task NullEquality_WithResult_NoDiagnostic()
    {
        var source = """
            using ResultNet;

            public class TestClass
            {
                public void Test(Result<int> result)
                {
                    if (result == null)
                    {
                    }
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullCheckAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NullInequality_WithResult_NoDiagnostic()
    {
        var source = """
            using ResultNet;

            public class TestClass
            {
                public void Test(Result<string> result)
                {
                    if (result != null)
                    {
                    }
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullCheckAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task IsNullPattern_WithResult_NoDiagnostic()
    {
        var source = """
            using ResultNet;

            public class TestClass
            {
                public void Test(Result<int> result)
                {
                    if (result is null)
                    {
                    }
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullCheckAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NullCheck_WithString_ReportsDiagnostic()
    {
        var source = """
            public class TestClass
            {
                public void Test(string? value)
                {
                    if ([|value == null|])
                    {
                    }
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullCheckAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NullCheck_WithCustomClass_ReportsDiagnostic()
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
                    if ([|obj == null|])
                    {
                    }
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullCheckAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NullCheck_WithValueType_NoDiagnostic()
    {
        var source = """
            public class TestClass
            {
                public void Test(int? value)
                {
                    if (value == null)
                    {
                    }
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullCheckAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task IsSuccessCheck_NoDiagnostic()
    {
        var source = """
            using ResultNet;

            public class TestClass
            {
                public void Test(Result<int> result)
                {
                    if (result.IsSuccess)
                    {
                    }
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullCheckAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NullCheckLeftSide_WithResult_NoDiagnostic()
    {
        var source = """
            using ResultNet;

            public class TestClass
            {
                public void Test(Result<int> result)
                {
                    if (null == result)
                    {
                    }
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullCheckAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NullCheckLeftSide_WithString_ReportsDiagnostic()
    {
        var source = """
            public class TestClass
            {
                public void Test(string? value)
                {
                    if ([|null == value|])
                    {
                    }
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullCheckAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task IsNotNullPattern_WithResult_NoDiagnostic()
    {
        var source = """
            using ResultNet;

            public class TestClass
            {
                public void Test(Result<int> result)
                {
                    if (result is not null)
                    {
                    }
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullCheckAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task IsNotNullPattern_WithString_ReportsDiagnostic()
    {
        var source = """
            public class TestClass
            {
                public void Test(string? value)
                {
                    if ([|value is not null|])
                    {
                    }
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullCheckAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task IsNullPattern_WithString_ReportsDiagnostic()
    {
        var source = """
            public class TestClass
            {
                public void Test(string? value)
                {
                    if ([|value is null|])
                    {
                    }
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullCheckAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NullCheckInWhileCondition_WithString_ReportsDiagnostic()
    {
        var source = """
            public class TestClass
            {
                public void Test(string? obj)
                {
                    while ([|obj != null|])
                    {
                        obj = null;
                    }
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullCheckAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NullCheckInTernaryExpression_WithString_ReportsDiagnostic()
    {
        var source = """
            public class TestClass
            {
                public void Test(string? obj)
                {
                    var result = [|obj == null|] ? "null" : "not null";
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullCheckAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NullCheckInLinqWhereClause_WithString_ReportsDiagnostic()
    {
        var source = """
            using System.Linq;
            using System.Collections.Generic;

            public class TestClass
            {
                public void Test(List<string?> items)
                {
                    var filtered = items.Where(x => [|x != null|]);
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullCheckAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NullCheckInLambda_WithString_ReportsDiagnostic()
    {
        var source = """
            using System;

            public class TestClass
            {
                public void Test(string? obj)
                {
                    Func<bool> f = () => [|obj == null|];
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullCheckAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task MultipleNullChecksInCompoundCondition_WithString_ReportsMultipleDiagnostics()
    {
        var source = """
            public class TestClass
            {
                public void Test(string? a, string? b)
                {
                    if ([|a == null|] && [|b != null|])
                    {
                    }
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullCheckAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NullCheckOnMethodReturn_WithString_ReportsDiagnostic()
    {
        var source = """
            public class TestClass
            {
                private string? GetValue() => null;

                public void Test()
                {
                    if ([|GetValue() == null|])
                    {
                    }
                }
            }
            """;

        await CSharpAnalyzerVerifier<NullCheckAnalyzer>.VerifyAnalyzerAsync(source);
    }
}
