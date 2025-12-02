using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace ResultNet.Analyzers.Tests;

public static class AnalyzerTestHelper<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    public static async Task VerifyNoDiagnosticAsync(string source)
    {
        var test = new Test
        {
            TestCode = source
        };
        
        await test.RunAsync();
    }

    public static async Task VerifyDiagnosticAsync(string source, DiagnosticDescriptor descriptor, params string[] messageArgs)
    {
        var test = new Test
        {
            TestCode = source
        };

        var expected = new DiagnosticResult(descriptor);
        if (messageArgs.Length > 0)
        {
            expected = expected.WithArguments(messageArgs);
        }
        test.ExpectedDiagnostics.Add(expected);

        await test.RunAsync();
    }

    private class Test : CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
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
            
            // Allow compiler errors since Result<T> is a struct and null operations cause compiler errors
            CompilerDiagnostics = CompilerDiagnostics.Errors;
        }

        protected override CompilationOptions CreateCompilationOptions()
            => new Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions(
                Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: Microsoft.CodeAnalysis.NullableContextOptions.Enable);
    }
}