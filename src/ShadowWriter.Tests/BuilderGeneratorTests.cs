using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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

        var generated =
            runResult.GeneratedTrees.Single(x => x.FilePath.Contains("TheRecord") && x.FilePath.Contains("Builder"));

        var code = (await generated.GetTextAsync()).ToString();

        await TestContext.Out.WriteLineAsync(code);

        Assert.That(code, Is.EqualTo("""
                                     using System;
                                     using System.Threading.Tasks;
                                     using System.CodeDom.Compiler;
                                     using System.Runtime.CompilerServices;

                                     #nullable disable

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
                                         return new(this.Value    );
                                       }

                                     }
                                     }
                                     """).IgnoreWhiteSpace);
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
                                         return new(this.Value    );
                                       }

                                     }
                                     }
                                     """).IgnoreWhiteSpace);
    }
}