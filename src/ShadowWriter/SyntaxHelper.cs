using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ShadowWriter;

public sealed class SyntaxHelper
{
    private readonly Compilation compilation;

    public SyntaxHelper(Compilation compilation)
    {
        this.compilation = compilation;
    }

    public bool ClassContainsProperty(ClassDeclarationSyntax classDeclarationSyntax, IPropertySymbol propertySymbol)
    {
        var semanticModel = this.compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);

        var properties = classDeclarationSyntax.Members.OfType<PropertyDeclarationSyntax>()
            .Where(m => m.Identifier.Text.Equals(propertySymbol.Name, StringComparison.Ordinal));

        return properties.Any(EnsureSymbols);


        bool EnsureSymbols(PropertyDeclarationSyntax propertyDeclaration)
        {
            var declaredPropertySymbol = semanticModel.GetDeclaredSymbol(propertyDeclaration) as IPropertySymbol;

            if (declaredPropertySymbol == null)
            {
                return false;
            }

            if (!SymbolEqualityComparer.Default.Equals(declaredPropertySymbol.Type, propertySymbol.Type))
            {
                return false;
            }

            // TODO test get and set method availability

            return true;
        }
    }

    public bool ClassContainsMethod(ClassDeclarationSyntax classDeclarationSyntax, IMethodSymbol expectedMethodSymbol)
    {
        var semanticModel = this.compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);

        var methods = classDeclarationSyntax.Members.OfType<MethodDeclarationSyntax>()
            .Where(m => m.Identifier.Text.Equals(expectedMethodSymbol.Name, StringComparison.Ordinal));

        return methods.Any(EnsureSymbols);

        bool EnsureSymbols(MethodDeclarationSyntax methodDeclaration)
        {
            var declaredMethodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration) as IMethodSymbol;

            if (declaredMethodSymbol == null)
            {
                return false;
            }

            if (!SymbolEqualityComparer.Default.Equals(declaredMethodSymbol.ReturnType,
                    expectedMethodSymbol.ReturnType))
            {
                return false;
            }

            if (declaredMethodSymbol.Parameters.Length != expectedMethodSymbol.Parameters.Length)
            {
                return false;
            }

            var parameters = declaredMethodSymbol.Parameters.Zip(expectedMethodSymbol.Parameters,
                (declared, expected) => (declared.Type, expected.Type));

            foreach (var (declaredParameterType, expectedParameterType) in parameters)
            {
                if (!SymbolEqualityComparer.Default.Equals(declaredParameterType, expectedParameterType))
                {
                    return false;
                }
            }

            return true;
        }
    }
}