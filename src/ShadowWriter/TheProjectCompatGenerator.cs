using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace ShadowWriter;

[Generator]
public sealed class TheProjectCompatGenerator : IIncrementalGenerator
{
    private readonly string generatorAssemblyVersion = "0.0.0";
    private const string Namespace = "ShadowWriter";
    private const string ObsoleteMessage =
        "TheProject is deprecated. Set <ShadowWriter_EnableProjectInfo>true</ShadowWriter_EnableProjectInfo> " +
        "in your .csproj and use ShadowWriter.ProjectInfo instead. See Details: https://github.com/StefanStolz/ShadowWriter/blob/main/docs/ProjectInfo.md";

    public TheProjectCompatGenerator()
    {
        var versionAttribute = this.GetType().Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
        if (versionAttribute != null)
            this.generatorAssemblyVersion = versionAttribute.Version;
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(context.CompilationProvider, (ctx, _) => this.GenerateCode(ctx));
    }

    private void GenerateCode(SourceProductionContext context)
    {
        var obs = $"[global::System.Obsolete(\"{ObsoleteMessage}\")]";
        var code =
            $$"""
              using System;
              using System.CodeDom.Compiler;
              using System.Runtime.CompilerServices;

              namespace {{Namespace}} {
                [CompilerGenerated]
                [GeneratedCode("ShadowWriter", "{{this.generatorAssemblyVersion}}")]
                internal static class TheProject
                {
                    {{obs}} public static string FullPath => "";
                    {{obs}} public static string ProjectDirectory => "";
                    {{obs}} public static string OutDir => "";
                    {{obs}} public static string Name => "";
                    {{obs}} public static string Version => "";
                    {{obs}} public static string RootNamespace => "";
                    {{obs}} public static DateTimeOffset BuildTimeUtc => default(DateTimeOffset);
                }
              }
              """;

        context.AddSource("ShadowWriter.TheProject.Compat.g.cs", SourceText.From(code, Encoding.UTF8));
    }
}
