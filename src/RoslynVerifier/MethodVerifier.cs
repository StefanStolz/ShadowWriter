using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynVerifier;

public sealed class MethodVerifier
{
    private readonly IReadOnlyCollection<MethodDeclarationSyntax> methodDeclarationSyntax;

    internal MethodVerifier(IReadOnlyCollection<MethodDeclarationSyntax> methodDeclarationSyntax)
    {
        this.methodDeclarationSyntax = methodDeclarationSyntax.ToArray();
    }

    public void WithReturnType(string expectedReturnType)
    {
        var found = this.methodDeclarationSyntax.Select(x => x.ReturnType.ToString())
            .Any(x => x.Equals(expectedReturnType));

        if (!found) throw new VerifierException($"Expected method to have return type '{expectedReturnType}'");
    }
}