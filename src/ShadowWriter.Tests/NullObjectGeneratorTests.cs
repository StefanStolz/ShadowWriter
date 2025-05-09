using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ShadowWriter.Tests;

[TestFixture]
public class NullObjectGeneratorTests {
    private const string EmptyInterfaceText =
        """
        namespace TestNamespace;

        [ShadowWriter.NullObject]
        public interface IEmptyInterface {}
        """;

    private const string ExpectedGeneratedCode =
        """
        using System;
        using System.Threading.Tasks;

        namespace TestNamespace;

        public sealed partial class NullEmptyInterface : IEmptyInterface
        {
          private NullEmptyInterface()
          {}

          public static IEmptyInterface Instance { get; } = new NullEmptyInterface();


        }
        """;

    [Test]
    public void GenerateInterface() {
        var generator = new NullObjectGenerator();

        var driver = CSharpGeneratorDriver.Create(generator);

        var compilation = CSharpCompilation.Create(
            nameof(NullObjectGeneratorTests),
            new[] { CSharpSyntaxTree.ParseText(EmptyInterfaceText) },
            new[] {
                // To support 'System.Attribute' inheritance, add reference to 'System.Private.CoreLib'.
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            });

        var runResult = driver.RunGenerators(compilation).GetRunResult();

        var generated = runResult.GeneratedTrees.Single(x => x.FilePath.Contains("NullEmptyInterface"));

        generated.GetText().ToString().ShouldBe(ExpectedGeneratedCode);
    }
}