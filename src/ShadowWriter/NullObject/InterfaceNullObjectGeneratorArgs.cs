using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ShadowWriter.NullObject;

public record InterfaceNullObjectGeneratorArgs(
    bool NullObjectAttributeFound,
    InterfaceDeclarationSyntax InterfaceDeclarationSyntax);