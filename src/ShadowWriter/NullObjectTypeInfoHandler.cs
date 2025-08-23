using Microsoft.CodeAnalysis;

namespace ShadowWriter;

public sealed record NullObjectTypeInfo(bool Supported, string ReturnValue, bool IncludeReturn = true);

public sealed class NullObjectTypeInfoHandler
{
    private readonly Compilation compilation;

    public NullObjectTypeInfoHandler(Compilation compilation)
    {
        this.compilation = compilation;
    }

    private bool IsIEnumerableOfT(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
        {
            return namedTypeSymbol.ConstructedFrom.SpecialType ==
                   SpecialType.System_Collections_Generic_IEnumerable_T;
        }

        return false;
    }

    private bool IsValueTask(ITypeSymbol typeSymbol)
    {
        INamedTypeSymbol? taskType = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask");

        return SymbolEqualityComparer.Default.Equals(typeSymbol, taskType);
    }


    private bool IsTask(ITypeSymbol typeSymbol)
    {
        INamedTypeSymbol? taskType = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");

        return SymbolEqualityComparer.Default.Equals(typeSymbol, taskType);
    }

    public NullObjectTypeInfo GetTypeInfo(ITypeSymbol typeSymbol)
    {
        if (IsValueTask(typeSymbol))
        {
            return new NullObjectTypeInfo(true, "ValueTask.CompletedTask");
        }

        switch ((typeSymbol.IsValueType, typeSymbol.SpecialType))
        {
            case (true, _): return new NullObjectTypeInfo(true, "default");
            case (_, SpecialType.System_String): return new NullObjectTypeInfo(true, "string.Empty");
        }

        if (IsIEnumerableOfT(typeSymbol))
        {
            return new NullObjectTypeInfo(true, "yield break", IncludeReturn: false);
        }

        if (IsTask(typeSymbol))
        {
            return new NullObjectTypeInfo(true, "Task.CompletedTask");
        }

        return new NullObjectTypeInfo(false, "");
    }
}