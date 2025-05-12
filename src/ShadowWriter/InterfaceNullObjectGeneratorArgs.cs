using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ShadowWriter;

public record InterfaceNullObjectGeneratorArgs(
    bool ReportAttributeFound,
    InterfaceDeclarationSyntax InterfaceDeclarationSyntax);