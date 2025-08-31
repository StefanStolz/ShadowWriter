using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynVerifier;

namespace RoslynVerifier.Tests;

[TestFixture]
[TestOf(typeof(TypeVerifier))]
public class TypeVerifierTests
{
    private static TypeVerifier CreateTypeVerifierForClass()
    {
        const string input = """
                             public class Test
                             {
                                public Test()
                                {
                                }

                                public void SomeMethod(){}

                                public void SomeMethod(int value){}
                             }
                             """;

        var syntaxTree = CSharpSyntaxTree.ParseText(input);

        var syntax = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Single();

        return SyntaxVerifier.From(syntax);
    }

    [Test]
    public void VerifyName()
    {
        var sut = CreateTypeVerifierForClass();
        sut.ShouldHaveName("Test");
    }

    [Test]
    public void VerifyNameFails()
    {
        var sut = CreateTypeVerifierForClass();
        Assert.Throws<VerifierException>(() => { sut.ShouldHaveName("Test2"); });
    }

    [Test]
    public void VerifyMethod()
    {
        var sut = CreateTypeVerifierForClass();
        sut.ShouldHaveMethod("SomeMethod")
            .WithReturnType("void");
    }

    [Test]
    public void VerifyMethodFailsIfNameNotExists()
    {
        var sut = CreateTypeVerifierForClass();

        Assert.Throws<VerifierException>(() => { sut.ShouldHaveMethod("NotExistingMethod"); });
    }

    [Test]
    public void VerifyMethodFailsIfReturnTypeNotExists()
    {
        var sut = CreateTypeVerifierForClass();
        var methodVerifier = sut.ShouldHaveMethod("SomeMethod");

        Assert.Throws<VerifierException>(() => { methodVerifier.WithReturnType("NotExistingReturnType"); });
    }


}