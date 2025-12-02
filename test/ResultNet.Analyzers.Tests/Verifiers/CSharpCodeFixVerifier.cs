using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace ResultNet.Analyzers.Tests;

public static class CSharpCodeFixVerifier<TAnalyzer, TCodeFix>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    public static async Task VerifyCodeFixAsync(string source, string fixedSource, params DiagnosticResult[] expected)
    {
        var test = new Test
        {
            // Normalize line endings to LF for cross-platform compatibility
            TestCode = source.Replace("\r\n", "\n"),
            FixedCode = fixedSource.Replace("\r\n", "\n"),
        };

        test.ExpectedDiagnostics.AddRange(expected);
        await test.RunAsync();
    }

    public static async Task VerifyCodeFixAsync(string source, string fixedSource, int numberOfFixAllIterations, params DiagnosticResult[] expected)
    {
        var test = new Test
        {
            // Normalize line endings to LF for cross-platform compatibility
            TestCode = source.Replace("\r\n", "\n"),
            FixedCode = fixedSource.Replace("\r\n", "\n"),
            NumberOfFixAllIterations = numberOfFixAllIterations
        };

        test.ExpectedDiagnostics.AddRange(expected);
        await test.RunAsync();
    }

    public static async Task VerifyCodeFixAsync(string source, DiagnosticResult expected, string fixedSource)
    {
        var test = new Test
        {
            // Normalize line endings to LF for cross-platform compatibility
            TestCode = source.Replace("\r\n", "\n"),
            FixedCode = fixedSource.Replace("\r\n", "\n"),
        };

        test.ExpectedDiagnostics.Add(expected);
        await test.RunAsync();
    }

    public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor)
        => new DiagnosticResult(descriptor);

    private class Test : CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
    {
        public Test()
        {
            // Use latest .NET runtime for testing
            ReferenceAssemblies = new ReferenceAssemblies(
                "net10.0",
                new PackageIdentity("Microsoft.NETCore.App.Ref", "10.0.0"),
                Path.Combine("ref", "net10.0"));
            
            TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(
                typeof(Result).Assembly.Location));
            
            // Suppress compiler diagnostics to avoid conflicts with analyzer diagnostics
            CompilerDiagnostics = CompilerDiagnostics.None;
            
            // Support markup with diagnostic IDs for analyzers that report multiple diagnostics
            MarkupOptions = MarkupOptions.UseFirstDescriptor;
            
            // Use LF line endings for cross-platform compatibility
            TestState.AnalyzerConfigFiles.Add(("/.editorconfig", """
                root = true
                
                [*]
                end_of_line = lf
                """));
        }

        protected override CompilationOptions CreateCompilationOptions()
            => new Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions(
                Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: Microsoft.CodeAnalysis.NullableContextOptions.Enable);
    }
}
