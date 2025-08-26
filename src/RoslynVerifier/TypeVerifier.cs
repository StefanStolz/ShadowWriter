using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynVerifier;

public sealed class TypeVerifier
{
    private readonly TypeDeclarationSyntax typeDeclarationSyntax;

    internal TypeVerifier(TypeDeclarationSyntax typeDeclarationSyntax)
    {
        this.typeDeclarationSyntax = typeDeclarationSyntax;
    }

    public void ShouldHaveName(string expectedName)
    {
        if (!this.typeDeclarationSyntax.Identifier.Text.Equals(expectedName, StringComparison.Ordinal))
        {
            throw new VerifierException(
                $"Expected class name to be '{expectedName}' but was '{this.typeDeclarationSyntax.Identifier.Text}'");
        }
    }

    public MethodVerifier ShouldHaveMethod(string methodName)
    {
        var methods = this.typeDeclarationSyntax.Members.OfType<MethodDeclarationSyntax>()
            .Where(m => m.Identifier.Text.Equals(methodName, StringComparison.Ordinal)).ToArray();

        if (methods.Length == 0)
        {
            throw new VerifierException($"Expected class to have method '{methodName}'");
        }

        return new MethodVerifier(methods);
    }

    public AttributeVerifier ShouldHaveAttribute(string attributeName)
    {
        var attributes = this.typeDeclarationSyntax.AttributeLists.SelectMany(a => a.Attributes)
            .Where(a => a.Name.ToString().Equals(attributeName, StringComparison.Ordinal))
            .ToArray();

        if (attributes.Length == 0)
        {
            throw new VerifierException($"Expected class to have attribute '{attributeName}'");
        }

        return new AttributeVerifier(attributes);
    }

    public PropertyVerifier ShouldHaveStaticProperty(string propertyName)
    {
        var properties = this.typeDeclarationSyntax.Members.OfType<PropertyDeclarationSyntax>()
            .Where(p => p.Modifiers.ToString().Contains("static")).ToArray();

        if (properties.Length == 0)
        {
            throw new VerifierException($"Expected class to have static property '{propertyName}'");
        }

        return new PropertyVerifier(properties);
    }

    public TypeVerifier ShouldHaveInnerClass(string className)
    {
        var clazz = this.typeDeclarationSyntax.Members.OfType<ClassDeclarationSyntax>()
            .Single(x => x.Identifier.Text.Equals(className, StringComparison.Ordinal));

        return new TypeVerifier(clazz);
    }

    public PropertyVerifier ShouldHaveProperty(string propertyName)
    {
        var properties = this.typeDeclarationSyntax.Members.OfType<PropertyDeclarationSyntax>()
            .Where(p => p.Identifier.Text.Equals(propertyName, StringComparison.Ordinal)).ToArray();
        return new PropertyVerifier(properties);
    }
}