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

    private const string InterfaceWithMethod =
        """
        namespace TestNamespace;

        [ShadowWriter.NullObject]
        public interface ISomeInterface {
            void Method(int value);
        }
        """;

    private const string ExpectedWithMethod =
        """
        using System;
        using System.Threading.Tasks;

        namespace TestNamespace;

        public sealed partial class NullSomeInterface : ISomeInterface
        {
          private NullSomeInterface()
          {}

          public static ISomeInterface Instance { get; } = new NullSomeInterface();

        public void Method(int value)
        { }


        }
        """;

    private const string InputWithMultipleMembers =
        """
        using System;
        using System.Collections.Generic;
        using System.Threading.Tasks;

        namespace TestNamespace;

        [ShadowWriter.NullObject]
        public interface ISomeInterface {
            Task Method(int value);

            IEnumerable<string> GetItems();
        }
        """;

    private const string ExpectedWithMultipleMembers =
        """
        using System;
        using System.Threading.Tasks;

        namespace TestNamespace;

        public sealed partial class NullSomeInterface : ISomeInterface
        {
          private NullSomeInterface()
          {}

          public static ISomeInterface Instance { get; } = new NullSomeInterface();

        public System.Threading.Tasks.Task Method(int value)
        {
           return Task.CompletedTask;
        }

        public System.Collections.Generic.IEnumerable<string> GetItems()
        {
           yield break;
        }


        }
        """;

    [Test]
    [TestCase(EmptyInterfaceText, ExpectedGeneratedCode, "NullEmptyInterface")]
    [TestCase(InterfaceWithMethod, ExpectedWithMethod, "NullSomeInterface")]
    [TestCase(InputWithMultipleMembers, ExpectedWithMultipleMembers, "NullSomeInterface")]
    public void GenerateInterface(string input, string expected, string fileName) {
        var generator = new NullObjectGenerator();

        var driver = CSharpGeneratorDriver.Create(generator);

        var compilation = CSharpCompilation.Create(
            nameof(NullObjectGeneratorTests),
            new[] { CSharpSyntaxTree.ParseText(input) },
            new[] {
                // To support 'System.Attribute' inheritance, add reference to 'System.Private.CoreLib'.
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            });

        var runResult = driver.RunGenerators(compilation).GetRunResult();

        var generated = runResult.GeneratedTrees.Single(x => x.FilePath.Contains(fileName));
        var x = generated.GetText().ToString();
        generated.GetText().ToString().ShouldBe(expected, StringCompareShould.IgnoreLineEndings);
    }
}