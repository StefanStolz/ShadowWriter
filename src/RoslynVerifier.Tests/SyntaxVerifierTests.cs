using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynVerifier.Tests;

[TestFixture]
public class SyntaxVerifierTests
{
    [Test]
    public async Task FindClassWithName()
    {
        string input = """
                       public class Test { }
                       """;

        var syntaxTree = CSharpSyntaxTree.ParseText(input);

        var syntax = (await syntaxTree.GetRootAsync())
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Single();

        var sut = SyntaxVerifier.From(syntax);

        sut.ShouldNotBeNull();
    }

    [Test]
    public async Task FindRecordWithName()
    {
        string input = """
                       public record Test { }
                       """;

        var syntaxTree = CSharpSyntaxTree.ParseText(input);

        var syntax = (await syntaxTree.GetRootAsync())
            .DescendantNodes()
            .OfType<RecordDeclarationSyntax>()
            .Single();

        var sut = SyntaxVerifier.From(syntax);

        sut.ShouldNotBeNull();
    }
}