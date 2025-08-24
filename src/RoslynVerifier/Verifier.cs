using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynVerifier
{
    public class VerifierException : Exception
    {
        public VerifierException(string message)
            : base(message)
        {
        }

        public VerifierException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public class Verifier
    {
        private readonly ClassDeclarationSyntax classDeclarationSyntax;

        private Verifier(ClassDeclarationSyntax classDeclarationSyntax)
        {
            this.classDeclarationSyntax = classDeclarationSyntax;
        }

        public static Verifier From(ClassDeclarationSyntax classDeclarationSyntax)
        {
            return new Verifier(classDeclarationSyntax);
        }

        public void ShouldHaveName(string expectedName)
        {
            if (!this.classDeclarationSyntax.Identifier.Text.Equals(expectedName, StringComparison.Ordinal))
            {
                throw new VerifierException(
                    $"Expected class name to be '{expectedName}' but was '{this.classDeclarationSyntax.Identifier.Text}'");
            }
        }

        public MethodVerifier ShouldHaveMethod(string methodName)
        {
            var methods = this.classDeclarationSyntax.Members.OfType<MethodDeclarationSyntax>()
                .Where(m => m.Identifier.Text.Equals(methodName, StringComparison.Ordinal)).ToArray();

            if (methods.Length == 0)
            {
                throw new VerifierException($"Expected class to have method '{methodName}'");
            }

            return new MethodVerifier(methods);
        }
    }

    public class MethodVerifier
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
}