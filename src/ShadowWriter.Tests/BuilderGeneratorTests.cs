using System.Linq;
using System.Threading.Tasks;
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
    }
}