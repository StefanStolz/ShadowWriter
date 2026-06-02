using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynVerifier;

namespace ShadowWriter.Tests;

[TestFixture]
[TestOf(typeof(BuilderGenerator))]
public class BuilderGeneratorTests
{
    [Test]
    public async Task CreateBuilderForSimpleRecord()
    {
        var input = """
                    using System;
                    using System.Collections.Generic;
                    using System.Threading.Tasks;

                    namespace TestNamespace;

                    [ShadowWriter.Builder]
                    public partial record TheRecord(string Value);
                    """;

        var generator = new BuilderGenerator();

        var driver = CSharpGeneratorDriver.Create(generator);

        var compilation = CSharpCompilation.Create(nameof(BuilderGeneratorTests),
            [CSharpSyntaxTree.ParseText(input)]);

        var runResult = driver.RunGenerators(compilation).GetRunResult();

        // TODO Idee - den Verifier direct über eine Extension-Methode aus den Generatedtrees holen
        var generated =
            runResult.GeneratedTrees.Single(x => x.FilePath.Contains("TheRecord") && x.FilePath.Contains("Builder"));

        var code = (await generated.GetTextAsync()).ToString();

        await TestContext.Out.WriteLineAsync(code);

        var root = await generated.GetRootAsync();
        var generatedRecord = root.DescendantNodes().OfType<RecordDeclarationSyntax>().Single();
        var recordVerifier = SyntaxVerifier.From(generatedRecord);
        recordVerifier.ShouldHaveName("TheRecord");
        var builderClass = recordVerifier.ShouldHaveInnerClass("Builder");
        builderClass.ShouldHaveProperty("Value");
        builderClass.ShouldHaveMethod("Build").WithReturnType("TheRecord");


        // TODO  Microsoft.CodeAnalysis.Testing anschauen bzw. Microsoft.CodeAnalysis.CSharp.Testing anschauen

    }

    [Test]
    public async Task CreateBuilderForSimpleRecordNrt()
    {
        var input = """
                    using System;
                    using System.Collections.Generic;
                    using System.Threading.Tasks;

                    namespace TestNamespace;

                    [ShadowWriter.Builder]
                    public partial record TheRecord(string Value);
                    """;

        var generator = new BuilderGenerator();

        var driver = CSharpGeneratorDriver.Create(generator);

        var compilationOptions = new CSharpCompilationOptions(
            OutputKind.DynamicallyLinkedLibrary,
            nullableContextOptions: NullableContextOptions.Enable);

        var compilation = CSharpCompilation.Create(nameof(BuilderGeneratorTests),
            [CSharpSyntaxTree.ParseText(input)], options: compilationOptions);

        var runResult = driver.RunGenerators(compilation).GetRunResult();

        var generated =
            runResult.GeneratedTrees.Single(x => x.FilePath.Contains("TheRecord") && x.FilePath.Contains("Builder"));

        var code = (await generated.GetTextAsync()).ToString();

        await TestContext.Out.WriteLineAsync(code);

        var verifier = SyntaxVerifier.From((await generated.GetRootAsync()).DescendantNodes()
            .OfType<RecordDeclarationSyntax>().Single());

        verifier.ShouldHaveName("TheRecord");
        verifier.ShouldHaveInnerClass("Builder");

        Assert.That(code, Is.EqualTo("""
                                     using System;
                                     using System.Threading.Tasks;
                                     using System.CodeDom.Compiler;
                                     using System.Runtime.CompilerServices;

                                     #nullable enable

                                     namespace TestNamespace;

                                     [CompilerGenerated]
                                     [GeneratedCode("ShadowWriter", "0.0.25.0")]
                                     public partial record TheRecord
                                     {
                                         public sealed class Builder
                                         {
                                             // Parameter: Value: string
                                             public string Value { get; set; } = "";
                                             public TheRecord Build()
                                             {
                                                 return new(this.Value);
                                             }
                                         }
                                     }
                                     """).IgnoreWhiteSpace);
    }

    [Test]
    public async Task CreateBuilderForRecordWithValueTypes()
    {
        var input = """
                    using System;

                    namespace TestNamespace;

                    [ShadowWriter.Builder]
                    public partial record TheRecord(int Count, int? OptionalCount);
                    """;

        var generator = new BuilderGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        var compilationOptions = new CSharpCompilationOptions(
            OutputKind.DynamicallyLinkedLibrary,
            nullableContextOptions: NullableContextOptions.Enable);

        var runtimeRef = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        var compilation = CSharpCompilation.Create(nameof(BuilderGeneratorTests),
            [CSharpSyntaxTree.ParseText(input)],
            references: [runtimeRef],
            options: compilationOptions);

        var runResult = driver.RunGenerators(compilation).GetRunResult();

        var generated = runResult.GeneratedTrees.Single(x => x.FilePath.Contains("TheRecord") && x.FilePath.Contains("Builder"));
        var code = (await generated.GetTextAsync()).ToString();

        await TestContext.Out.WriteLineAsync(code);

        var root = await generated.GetRootAsync();
        var generatedRecord = root.DescendantNodes().OfType<RecordDeclarationSyntax>().Single();
        var recordVerifier = SyntaxVerifier.From(generatedRecord);
        var builderClass = recordVerifier.ShouldHaveInnerClass("Builder");
        builderClass.ShouldHaveProperty("Count").WithType("int");
        builderClass.ShouldHaveProperty("OptionalCount").WithType("int?");
        builderClass.ShouldHaveMethod("Build").WithReturnType("TheRecord");

        // By default non-nullable value types are initialized, not required.
        Assert.That(code, Does.Contain("public int Count { get; set; } = default;"));
        Assert.That(code, Does.Not.Contain("this.Count.Value"));
        Assert.That(code, Does.Not.Contain("!this.Count.HasValue"));
        Assert.That(code, Does.Not.Contain("!this.OptionalCount.HasValue"));
    }

    [Test]
    public async Task DefaultInitializesStrings()
    {
        var code = await GenerateBuilder("public partial record TheRecord(string Text);");

        Assert.That(code, Does.Contain("public string Text { get; set; } = \"\";"));
        Assert.That(code, Does.Not.Contain("'Text' must be set"));
        Assert.That(code, Does.Contain("return new(this.Text"));
    }

    [Test]
    public async Task DefaultInitializesValueTypes()
    {
        var code = await GenerateBuilder("public partial record TheRecord(int Count);");

        Assert.That(code, Does.Contain("public int Count { get; set; } = default;"));
        Assert.That(code, Does.Not.Contain("'Count' must be set"));
        Assert.That(code, Does.Not.Contain("this.Count.HasValue"));
        Assert.That(code, Does.Not.Contain("this.Count.Value"));
        Assert.That(code, Does.Contain("return new(this.Count"));
    }

    [Test]
    public async Task RequireValueTypesRestoresRequiredBehavior()
    {
        var code = await GenerateBuilder(
            "[ShadowWriter.Builder(RequireValueTypes = true)] public partial record TheRecord(int Count);",
            applyAttribute: false);

        Assert.That(code, Does.Contain("public int? Count { get; set; }"));
        Assert.That(code, Does.Contain("!this.Count.HasValue"));
        Assert.That(code, Does.Contain("this.Count.Value"));
    }

    [Test]
    public async Task RequireStringsRestoresRequiredBehavior()
    {
        var code = await GenerateBuilder(
            "[ShadowWriter.Builder(RequireStrings = true)] public partial record TheRecord(string Text);",
            applyAttribute: false);

        Assert.That(code, Does.Contain("public string? Text { get; set; }"));
        Assert.That(code, Does.Contain("'Text' must be set"));
    }

    [Test]
    public async Task FlagsAreIndependentAndStringIsNotAValueType()
    {
        // RequireStrings only: Text stays required, Count is still default-initialized.
        var code = await GenerateBuilder(
            "[ShadowWriter.Builder(RequireStrings = true)] public partial record TheRecord(string Text, int Count);",
            applyAttribute: false);

        Assert.That(code, Does.Contain("public string? Text { get; set; }"));
        Assert.That(code, Does.Contain("'Text' must be set"));
        Assert.That(code, Does.Contain("public int Count { get; set; } = default;"));
        Assert.That(code, Does.Not.Contain("'Count' must be set"));
    }

    [Test]
    public async Task DefaultInitializesArrays()
    {
        var code = await GenerateBuilder("public partial record TheRecord(int[] Tags);");

        Assert.That(code, Does.Contain("public int[] Tags { get; set; } = [];"));
        Assert.That(code, Does.Not.Contain("'Tags' must be set"));
        Assert.That(code, Does.Contain("return new(this.Tags"));
    }

    [Test]
    public async Task DefaultInitializesLists()
    {
        var code = await GenerateBuilder("public partial record TheRecord(List<string> Names);");

        Assert.That(code, Does.Contain("Names { get; set; } = [];"));
        Assert.That(code, Does.Not.Contain("'Names' must be set"));
    }

    [Test]
    public async Task DefaultInitializesEnumerables()
    {
        var code = await GenerateBuilder("public partial record TheRecord(IEnumerable<int> Ids);");

        Assert.That(code, Does.Contain("Ids { get; set; } = [];"));
        Assert.That(code, Does.Not.Contain("'Ids' must be set"));
    }

    [Test]
    public async Task RequireCollectionsRestoresRequiredBehavior()
    {
        var code = await GenerateBuilder(
            "[ShadowWriter.Builder(RequireCollections = true)] public partial record TheRecord(int[] Tags);",
            applyAttribute: false);

        Assert.That(code, Does.Contain("public int[]? Tags { get; set; }"));
        Assert.That(code, Does.Contain("'Tags' must be set"));
        Assert.That(code, Does.Not.Contain("= [];"));
    }

    [Test]
    public async Task DictionariesStayRequiredByDefault()
    {
        var code = await GenerateBuilder("public partial record TheRecord(Dictionary<int, string> Map);");

        // [] cannot construct a dictionary, so dictionaries are not treated as collections.
        Assert.That(code, Does.Not.Contain("Map { get; set; } = [];"));
        Assert.That(code, Does.Contain("'Map' must be set"));
    }

    [Test]
    public async Task RequireCollectionsIsIndependentOfOtherFlags()
    {
        // RequireCollections only: Tags required, Text still default-initialized.
        var code = await GenerateBuilder(
            "[ShadowWriter.Builder(RequireCollections = true)] public partial record TheRecord(string Text, int[] Tags);",
            applyAttribute: false);

        Assert.That(code, Does.Contain("public string Text { get; set; } = \"\";"));
        Assert.That(code, Does.Not.Contain("'Text' must be set"));
        Assert.That(code, Does.Contain("public int[]? Tags { get; set; }"));
        Assert.That(code, Does.Contain("'Tags' must be set"));
    }

    private static async Task<string> GenerateBuilder(string declaration, bool applyAttribute = true)
    {
        var typeDeclaration = applyAttribute ? $"[ShadowWriter.Builder]\n{declaration}" : declaration;

        var input = $$"""
                      using System;
                      using System.Collections.Generic;

                      namespace TestNamespace;

                      {{typeDeclaration}}
                      """;

        var generator = new BuilderGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        var compilationOptions = new CSharpCompilationOptions(
            OutputKind.DynamicallyLinkedLibrary,
            nullableContextOptions: NullableContextOptions.Enable);

        var runtimeRef = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        var compilation = CSharpCompilation.Create(nameof(BuilderGeneratorTests),
            [CSharpSyntaxTree.ParseText(input)],
            references: [runtimeRef],
            options: compilationOptions);

        var runResult = driver.RunGenerators(compilation).GetRunResult();

        var generated = runResult.GeneratedTrees.Single(x =>
            x.FilePath.Contains("TheRecord") && x.FilePath.Contains("Builder"));
        var code = (await generated.GetTextAsync()).ToString();

        await TestContext.Out.WriteLineAsync(code);

        return code;
    }
}