using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynVerifier
{
    public static class SyntaxVerifier
    {
        public static TypeVerifier From(TypeDeclarationSyntax typeDeclarationSyntax)
        {
            return new TypeVerifier(typeDeclarationSyntax);
        }
    }
}