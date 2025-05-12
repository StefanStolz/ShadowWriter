using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ShadowWriter;

public record ClassNullObjectGeneratorArgs(
    bool ReportAttributeFound,
    ClassDeclarationSyntax ClassDeclarationSyntax);