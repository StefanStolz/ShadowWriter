using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ShadowWriter;

public record NullObjectGeneratorArgs(
    bool ReportAttributeFound,
    string? ClassName,
    InterfaceDeclarationSyntax InterfaceDeclarationSyntax);