using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynVerifier;

public sealed class PropertyVerifier
{
    private readonly IReadOnlyCollection<PropertyDeclarationSyntax> properties;

    public PropertyVerifier(IReadOnlyCollection<PropertyDeclarationSyntax> properties)
    {
        this.properties = properties;
    }

    public void WithType(string expectedType)
    {
        var found = this.properties.Select(x => x.Type.ToString())
            .Any(x => x.Equals(expectedType));

        if (!found)
        {
            throw new VerifierException($"Expected property to have type '{expectedType}'");
        }
    }
}