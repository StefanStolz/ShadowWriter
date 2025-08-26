using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynVerifier
{
    public static class Verifier
    {
        public static TypeVerifier From(TypeDeclarationSyntax typeDeclarationSyntax)
        {
            return new TypeVerifier(typeDeclarationSyntax);
        }
    }
}