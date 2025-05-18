using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StefanStolz.TestHelpers;

namespace ShadowWriter.Tests;

[TestFixture]
public class InterfaceNullObjectGeneratorTests {
    private const string EmptyInterfaceText =
        """
        namespace TestNamespace;

        [ShadowWriter.NullObject]
        public interface IEmptyInterface {}
        """;

    private const string ExpectedGeneratedCode =
        """
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

    private static readonly IEqualityComparer<string> codeComparer = new StringComparerBuilder().IgnoreLineEndings().TrimLines().Build();

    [Test]
    [TestCase(EmptyInterfaceText, ExpectedGeneratedCode, "NullEmptyInterface")]
    [TestCase(InterfaceWithMethod, ExpectedWithMethod, "NullSomeInterface")]
    [TestCase(InputWithMultipleMembers, ExpectedWithMultipleMembers, "NullSomeInterface")]
    public async Task GenerateInterface(string input, string expected, string fileName) {
        var generator = new NullObjectGenerator();

        var driver = CSharpGeneratorDriver.Create(generator);

        var compilation = CSharpCompilation.Create(
            nameof(InterfaceNullObjectGeneratorTests),
            [CSharpSyntaxTree.ParseText(input)],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]
        );

        var runResult = driver.RunGenerators(compilation).GetRunResult();

        var generated = runResult.GeneratedTrees.Single(x => x.FilePath.Contains(fileName));
     //   var text = (await generated.GetTextAsync()).ToString();
        var code = (await generated.GetTextAsync()).ToString();

        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var root = await syntaxTree.GetRootAsync();
        var clazz = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Single();

        clazz = clazz.WithAttributeLists(new SyntaxList<AttributeListSyntax>());

        clazz.ToFullString().ShouldBe(expected, codeComparer);



       // text.ShouldBe(expected, codeComparer);
    }

    [Test]
    public async Task GenerateMethodWithValueTypeReturn() {
        var input = """
                    using System;
                    using System.Collections.Generic;
                    using System.Threading.Tasks;

                    namespace TestNamespace;

                    [ShadowWriter.NullObject]
                    public interface ISut{
                        int Method(int value);
                    }
                    """;

        var generator = new NullObjectGenerator();

        var driver = CSharpGeneratorDriver.Create(generator);

        var compilation = CSharpCompilation.Create(
            nameof(InterfaceNullObjectGeneratorTests),
            [CSharpSyntaxTree.ParseText(input)],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]
        );

        var runResult = driver.RunGenerators(compilation).GetRunResult();

        var generated = runResult.GeneratedTrees.Single(x => x.FilePath.Contains("NullSut"));

        var code = (await generated.GetTextAsync()).ToString();

        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var root = await syntaxTree.GetRootAsync();
        var clazz = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Single();

        clazz.Identifier.Value.ShouldBe("NullSut");

        var method = clazz.Members.OfType<MethodDeclarationSyntax>().Single();

        method.Identifier.Value.ShouldBe("Method");
        var txt = method.ReturnType.GetText().ToString().Trim();

        txt.ShouldBe("int");
    }

    [Test]
    public async Task GenerateMethodWithTaskReturn() {
        var input = """
                    using System;
                    using System.Collections.Generic;
                    using System.Threading.Tasks;

                    namespace TestNamespace;

                    [ShadowWriter.NullObject]
                    public interface ISut{
                        Task Method(int value);
                    }
                    """;

        var generator = new NullObjectGenerator();

        var driver = CSharpGeneratorDriver.Create(generator);

        var compilation = CSharpCompilation.Create(
            nameof(InterfaceNullObjectGeneratorTests),
            [CSharpSyntaxTree.ParseText(input)],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]
        );

        var runResult = driver.RunGenerators(compilation).GetRunResult();

        var generated = runResult.GeneratedTrees.Single(x => x.FilePath.Contains("NullSut"));

        var code = (await generated.GetTextAsync()).ToString();

        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var root = await syntaxTree.GetRootAsync();
        var clazz = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Single();

        clazz.Identifier.Value.ShouldBe("NullSut");

        var method = clazz.Members.OfType<MethodDeclarationSyntax>().Single();

        method.Identifier.Value.ShouldBe("Method");
        var txt = method.ReturnType.GetText().ToString().Trim();

        txt.ShouldBe("System.Threading.Tasks.Task");
    }

    [Test]
    public async Task GenerateMethodWithValueTaskReturn() {
        var input = """
                    using System;
                    using System.Collections.Generic;
                    using System.Threading.Tasks;

                    namespace TestNamespace;

                    [ShadowWriter.NullObject]
                    public interface ISut{
                        ValueTask Method(int value);
                    }
                    """;

        var generator = new NullObjectGenerator();

        var driver = CSharpGeneratorDriver.Create(generator);

        var compilation = CSharpCompilation.Create(
            nameof(InterfaceNullObjectGeneratorTests),
            [CSharpSyntaxTree.ParseText(input)],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]
        );

        var runResult = driver.RunGenerators(compilation).GetRunResult();

        var generated = runResult.GeneratedTrees.Single(x => x.FilePath.Contains("NullSut"));

        var code = (await generated.GetTextAsync()).ToString();

        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var root = await syntaxTree.GetRootAsync();
        var clazz = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Single();

        clazz.Identifier.Value.ShouldBe("NullSut");

        var method = clazz.Members.OfType<MethodDeclarationSyntax>().Single();

        method.Identifier.Value.ShouldBe("Method");
        var txt = method.ReturnType.GetText().ToString().Trim();

        txt.ShouldBe("System.Threading.Tasks.ValueTask");
    }

    [Test]
    public async Task SetClassName() {
        var input = """
                    using System;
                    using System.Collections.Generic;
                    using System.Threading.Tasks;

                    namespace TestNamespace;

                    [ShadowWriter.NullObject]
                    [ShadowWriter.ClassName("abcd")]
                    public interface ISut{
                        ValueTask Method(int value);
                    }
                    """;

        var generator = new NullObjectGenerator();

        var driver = CSharpGeneratorDriver.Create(generator);

        var compilation = CSharpCompilation.Create(
            nameof(InterfaceNullObjectGeneratorTests),
            [CSharpSyntaxTree.ParseText(input)],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]
        );

        var runResult = driver.RunGenerators(compilation).GetRunResult();

        var generated = runResult.GeneratedTrees.Single(x => x.FilePath.Contains("abcd"));

        var code = (await generated.GetTextAsync()).ToString();

        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var root = await syntaxTree.GetRootAsync();
        var clazz = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Single();

        clazz.Identifier.Value.ShouldBe("abcd");

        var method = clazz.Members.OfType<MethodDeclarationSyntax>().Single();

        method.Identifier.Value.ShouldBe("Method");
        var txt = method.ReturnType.GetText().ToString().Trim();

        txt.ShouldBe("System.Threading.Tasks.ValueTask");
    }

}