using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ShadowWriter.Tests;

[TestFixture]
public class ClassNullObjectGeneratorTests
{
    [Test]
    [Explicit]
    public async Task DevelopmentTest()
    {
        var input = """
                    using System;
                    using System.Collections.Generic;
                    using System.Threading.Tasks;

                    namespace TestNamespace;

                    [ShadowWriter.NullObject]
                    public partial class NullShibby : IDisposable {

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

        var generated = runResult.GeneratedTrees.Single(x => x.FilePath.Contains("NullShibby"));

        var code = (await generated.GetTextAsync()).ToString();


        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var root = await syntaxTree.GetRootAsync();
        var clazz = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Single();

        clazz.Identifier.Value.ShouldBe("NullShibby");

        var method = clazz.Members.OfType<MethodDeclarationSyntax>().Single();

        method.Identifier.Value.ShouldBe("Dispose");
        var txt = method.ReturnType.GetText().ToString().Trim();

        txt.ShouldBe("void");
    }

    [Test]
    public async Task ClassWithPartialMethods()
    {
        const string input = """
                             using System;
                             using System.Collections.Generic;
                             using System.Threading.Tasks;

                             namespace TestNamespace;

                             public record Shibby(int Value);

                             public interface IShibby{
                                 Shibby GetShibby();
                             }

                             [ShadowWriter.NullObject]
                             public partial class NullShibby : IShibby {

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

        var generated = runResult.GeneratedTrees.Single(x => x.FilePath.Contains("NullShibby"));

        var code = (await generated.GetTextAsync()).ToString();

        code.ShouldBe("""
                      using System;
                      using System.CodeDom.Compiler;
                      using System.Runtime.CompilerServices;
                      using System.Threading.Tasks;

                      #nullable disable

                      namespace TestNamespace;

                      [CompilerGenerated]
                      [GeneratedCode("ShadowWriter", "0.0.25.0")]
                      public sealed partial class NullShibby
                      {
                        private NullShibby()
                        {}

                        public static TestNamespace.NullShibby Instance { get; } = new NullShibby();

                        // GetShibby
                        public partial TestNamespace.Shibby GetShibby()  ;
                      }
                      """, StringCompareShould.IgnoreLineEndings | StringCompareShould.IgnoreCase);
    }

    [Test]
    public async Task ClassWithWithProperties()
    {
        const string input = """
                             using System;
                             using System.Collections.Generic;
                             using System.Threading.Tasks;

                             namespace TestNamespace;


                             public interface IHaveProperties {
                                int Number { get; }
                                string Text { get; }

                                IEnumerable<string> AnEnumerable { get; }
                             }

                             [ShadowWriter.NullObject]
                             public partial class NullHaveProperties : IHaveProperties {

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

        var generated = runResult.GeneratedTrees.Single(x => x.FilePath.Contains("NullHaveProperties"));

        var code = (await generated.GetTextAsync()).ToString();

        await TestContext.Out.WriteLineAsync(code);

        var root = await generated.GetRootAsync();

        var generatedClass = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Single();

        generatedClass.Identifier.Value.ShouldBe("NullHaveProperties");

        var properties = generatedClass.Members.OfType<PropertyDeclarationSyntax>().Select(p => new
        {
            Name = p.Identifier.ValueText,
            Type = p.Type.ToString().Trim(),
            Modifiers = p.Modifiers.ToString().Trim()
        }).ToArray();

        var one = new { Name = "Instance", Type = "TestNamespace.NullHaveProperties", Modifiers = "public static" };
        var two = new { Name = "Number", Type = "int", Modifiers = "public" };
        var three = new { Name = "Text", Type = "string", Modifiers = "public" };
        var four = new { Name = "AnEnumerable", Type = "System.Collections.Generic.IEnumerable<string>", Modifiers = "public" };

        properties.ShouldBeEquivalentTo(new[] { one, two, three, four });
    }
}