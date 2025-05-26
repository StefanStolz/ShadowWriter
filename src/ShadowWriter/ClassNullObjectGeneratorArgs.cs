using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ShadowWriter;

public record ClassNullObjectGeneratorArgs(
    bool NullObjectAttributeFound,
    ClassDeclarationSyntax ClassDeclarationSyntax);