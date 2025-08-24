using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ShadowWriter.NullObject;

public record ClassNullObjectGeneratorArgs(
    bool NullObjectAttributeFound,
    ClassDeclarationSyntax ClassDeclarationSyntax);