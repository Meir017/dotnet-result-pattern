using System.Text;
using System.Text.RegularExpressions;

var testProjectPath = args.Length > 0 
    ? args[0] 
    : Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "test", "ResultNet.Tests");

var templatePath = args.Length > 1
    ? args[1]
    : Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "README.template.md");

var outputPath = args.Length > 2
    ? args[2]
    : Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "README.md");

Console.WriteLine($"Test Project: {testProjectPath}");
Console.WriteLine($"Template: {templatePath}");
Console.WriteLine($"Output: {outputPath}");

if (!Directory.Exists(testProjectPath))
{
    Console.Error.WriteLine($"Test project not found: {testProjectPath}");
    return 1;
}

if (!File.Exists(templatePath))
{
    Console.Error.WriteLine($"Template not found: {templatePath}");
    return 1;
}

var generator = new ReadmeGenerator(testProjectPath, templatePath, outputPath);
await generator.GenerateAsync();

Console.WriteLine($"README generated successfully: {outputPath}");
return 0;

class ReadmeGenerator
{
    private readonly string _testProjectPath;
    private readonly string _templatePath;
    private readonly string _outputPath;

    public ReadmeGenerator(string testProjectPath, string templatePath, string outputPath)
    {
        _testProjectPath = testProjectPath;
        _templatePath = templatePath;
        _outputPath = outputPath;
    }

    public async Task GenerateAsync()
    {
        var examples = await ExtractExamplesAsync();
        var template = await File.ReadAllTextAsync(_templatePath);
        var readme = ReplaceExamples(template, examples);
        await File.WriteAllTextAsync(_outputPath, readme);
    }

    private async Task<Dictionary<string, List<ExampleCode>>> ExtractExamplesAsync()
    {
        var examples = new Dictionary<string, List<ExampleCode>>();
        var usageExamplesFile = Path.Combine(_testProjectPath, "UsageExamplesTests.cs");

        if (!File.Exists(usageExamplesFile))
            return examples;

        var content = await File.ReadAllTextAsync(usageExamplesFile);
        var regions = ExtractRegions(content);

        foreach (var (regionName, regionContent) in regions)
        {
            var exampleTests = ExtractExampleTests(regionContent);
            if (exampleTests.Any())
            {
                examples[regionName] = exampleTests;
            }
        }

        return examples;
    }

    private Dictionary<string, string> ExtractRegions(string content)
    {
        var regions = new Dictionary<string, string>();
        var regionPattern = @"#region\s+(.+?)\s*\r?\n(.*?)#endregion";
        var matches = Regex.Matches(content, regionPattern, RegexOptions.Singleline);

        foreach (Match match in matches)
        {
            var regionName = match.Groups[1].Value.Trim();
            var regionContent = match.Groups[2].Value;
            regions[regionName] = regionContent;
        }

        return regions;
    }

    private List<ExampleCode> ExtractExampleTests(string regionContent)
    {
        var examples = new List<ExampleCode>();
        var testMethodPattern = @"\[Fact\]\s*(?:public\s+(?:async\s+Task|void)\s+)(Example_\w+)\(\)\s*\{(.*?)\n\s{4}\}";
        var matches = Regex.Matches(regionContent, testMethodPattern, RegexOptions.Singleline);

        foreach (Match match in matches)
        {
            var methodName = match.Groups[1].Value;
            var methodBody = match.Groups[2].Value;
            var cleanedCode = CleanTestCode(methodBody);
            var title = FormatTitle(methodName);

            examples.Add(new ExampleCode
            {
                Title = title,
                Code = cleanedCode
            });
        }

        return examples;
    }

    private string CleanTestCode(string code)
    {
        var lines = code.Split('\n');
        var cleanedLines = new List<string>();
        var minIndent = int.MaxValue;

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (line.TrimStart().StartsWith("Assert.") || 
                line.TrimStart().StartsWith("//"))
                continue;

            var leadingSpaces = line.Length - line.TrimStart().Length;
            if (leadingSpaces < minIndent && line.Trim().Length > 0)
                minIndent = leadingSpaces;

            cleanedLines.Add(line);
        }

        var result = new StringBuilder();
        foreach (var line in cleanedLines)
        {
            if (line.Trim().Length == 0)
            {
                result.AppendLine();
                continue;
            }

            var unindented = line.Length >= minIndent ? line.Substring(minIndent) : line;
            result.AppendLine(unindented);
        }

        return result.ToString().Trim();
    }

    private string FormatTitle(string methodName)
    {
        var withoutPrefix = methodName.Replace("Example_", "");
        var words = Regex.Replace(withoutPrefix, "([a-z])([A-Z])", "$1 $2");
        return words;
    }

    private string ReplaceExamples(string template, Dictionary<string, List<ExampleCode>> examples)
    {
        var result = template;

        foreach (var (regionName, exampleList) in examples)
        {
            var placeholder = $"{{{regionName.ToUpper().Replace(" ", "_")}}}";
            var content = GenerateExampleSection(exampleList);
            result = result.Replace(placeholder, content);
        }

        return result;
    }

    private string GenerateExampleSection(List<ExampleCode> examples)
    {
        var sb = new StringBuilder();

        foreach (var example in examples)
        {
            sb.AppendLine($"### {example.Title}");
            sb.AppendLine();
            sb.AppendLine("```csharp");
            sb.AppendLine(example.Code);
            sb.AppendLine("```");
            sb.AppendLine();
        }

        return sb.ToString();
    }
}

record ExampleCode
{
    public required string Title { get; init; }
    public required string Code { get; init; }
}
