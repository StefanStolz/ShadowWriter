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
                                             public string? Value { get; set; }
                                             public TheRecord Build()
                                             {
                                                 if (this.Value is null) throw new InvalidOperationException("'Value' must be set before calling Build().");
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
        builderClass.ShouldHaveProperty("Count").WithType("int?");
        builderClass.ShouldHaveProperty("OptionalCount").WithType("int?");
        builderClass.ShouldHaveMethod("Build").WithReturnType("TheRecord");

        Assert.That(code, Does.Contain("this.Count.Value"));
        Assert.That(code, Does.Contain("!this.Count.HasValue"));
        Assert.That(code, Does.Not.Contain("!this.OptionalCount.HasValue"));
    }
}