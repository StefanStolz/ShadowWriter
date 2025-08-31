using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynVerifier.Tests;

[TestFixture]
[TestOf(typeof(AttributeVerifier))]
public class AttributeVerifierTests
{
    private const string ClassAttributeWithoutArguments =
        """
        [System.ObsoleteAttribute]
        public class Test { }
        """;


    private static TypeVerifier CreateTypeVerifierForClass(string sourceCode)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

        var syntax = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Single();

        return SyntaxVerifier.From(syntax);
    }


    [Test]
    public void VerifyClassAttributeWithoutParameters()
    {
        var sut = CreateTypeVerifierForClass(ClassAttributeWithoutArguments);

        sut.ShouldHaveAttribute("System.ObsoleteAttribute");
    }

    [Test]
    public void VerifyClassAttributeWithBoolArgument()
    {
        var typeVerifier = CreateTypeVerifierForClass("""
                                                      [Obsolete("Message", true)]
                                                      public class Test { }
                                                      """);

        var attributeVerifier =  typeVerifier.ShouldHaveAttribute("Obsolete");
        attributeVerifier.WithArguments("Message", true);
    }

    [Test]
    public void VerifyClassAttributeWithInt32Argument()
    {
        var typeVerifier = CreateTypeVerifierForClass("""
                                                      [Obsolete("Message", 32)]
                                                      public class Test { }
                                                      """);

        var attributeVerifier =  typeVerifier.ShouldHaveAttribute("Obsolete");
        attributeVerifier.WithArguments("Message", 32);
    }

    [Test]
    public void VerifyClassAttributeWithInt32ArgumentThatFails()
    {
        var typeVerifier = CreateTypeVerifierForClass("""
                                                      [Obsolete("Message", 32)]
                                                      public class Test { }
                                                      """);

        var attributeVerifier =  typeVerifier.ShouldHaveAttribute("Obsolete");
        Assert.Throws<VerifierException>(() => attributeVerifier.WithArguments("Message", 33));
    }
}