using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using RoslynVerifier;

namespace ShadowWriter.Tests;

[TestFixture]
[TestOf(typeof(TheProjectCompatGenerator))]
public class TheProjectCompatGeneratorTests
{
    private static ImmutableArray<SyntaxTree> RunGenerator()
    {
        var sut = new TheProjectCompatGenerator();
        var provider = new FakeAnalyzerConfigOptionsProvider(new FakeAnalyzerConfigOptions(null));
        var compilation = CSharpCompilation.Create(nameof(TheProjectCompatGeneratorTests),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        return CSharpGeneratorDriver.Create(sut)
            .WithUpdatedAnalyzerConfigOptions(provider)
            .RunGeneratorsAndUpdateCompilation(compilation, out _, out _)
            .GetRunResult().GeneratedTrees;
    }

    private static ClassDeclarationSyntax GetTheProjectClass()
    {
        var trees = RunGenerator();
        var generated = trees.Single();
        return generated.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
    }

    [Test]
    public void AlwaysGeneratesTheProject()
    {
        var verifier = SyntaxVerifier.From(GetTheProjectClass());
        verifier.ShouldHaveName("TheProject");
    }

    [Test]
    public void AllPropertiesAreObsolete()
    {
        var classDecl = GetTheProjectClass();
        var properties = classDecl.Members.OfType<PropertyDeclarationSyntax>().ToList();
        properties.Count.ShouldBe(7);
        foreach (var property in properties)
        {
            var hasObsolete = property.AttributeLists
                .SelectMany(al => al.Attributes)
                .Any(a => a.Name.ToString().Contains("Obsolete"));
            hasObsolete.ShouldBeTrue($"Property '{property.Identifier.Text}' should have [Obsolete]");
        }
    }

    [Test]
    public void AllStringPropertiesReturnEmptyString()
    {
        var classDecl = GetTheProjectClass();
        var stringProps = classDecl.Members.OfType<PropertyDeclarationSyntax>()
            .Where(p => p.Type.ToString() == "string")
            .ToList();

        foreach (var property in stringProps)
        {
            var body = property.ExpressionBody?.Expression.ToString();
            body.ShouldBe("\"\"", $"Property '{property.Identifier.Text}' should return empty string");
        }
    }

    [Test]
    public void BuildTimeUtcReturnsDefaultDateTimeOffset()
    {
        var classDecl = GetTheProjectClass();
        var buildTimeProp = classDecl.Members.OfType<PropertyDeclarationSyntax>()
            .Single(p => p.Identifier.Text == "BuildTimeUtc");
        var body = buildTimeProp.ExpressionBody?.Expression.ToString();
        body.ShouldBe("default(DateTimeOffset)");
    }

    private sealed class FakeAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
    {
        private readonly AnalyzerConfigOptions globalOptions;

        public FakeAnalyzerConfigOptionsProvider(AnalyzerConfigOptions globalOptions)
        {
            this.globalOptions = globalOptions;
        }

        public override AnalyzerConfigOptions GlobalOptions => this.globalOptions;

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => this.globalOptions;

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => this.globalOptions;
    }

    private sealed class FakeAnalyzerConfigOptions : AnalyzerConfigOptions
    {
        private readonly Dictionary<string, string> options;

        public FakeAnalyzerConfigOptions(Dictionary<string, string>? options)
        {
            this.options = options ?? new Dictionary<string, string>();
        }

        public override bool TryGetValue(string key, out string value)
        {
            value = string.Empty;

            if (this.options.TryGetValue(key, out var val))
            {
                value = val;
                return true;
            }

            return false;
        }

        public override IEnumerable<string> Keys => this.options.Keys;
    }
}
