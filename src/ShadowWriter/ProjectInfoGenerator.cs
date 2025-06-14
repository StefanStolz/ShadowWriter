using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace ShadowWriter;

[Generator]
public sealed class ProjectInfoGenerator : IIncrementalGenerator
{
    private readonly string generatorAssemblyVersion = "0.0.0";
    private const string Namespace = "ShadowWriter";

    public ProjectInfoGenerator()
    {
        var versionAttribute = this.GetType().Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
        if (versionAttribute != null)
        {
            this.generatorAssemblyVersion = versionAttribute.Version;
        }
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var properties = context.AnalyzerConfigOptionsProvider
            .Select((x, _) =>
            {
                x.GlobalOptions.TryGetValue("build_property.MSBuildProjectFullPath", out var fullPath);
                x.GlobalOptions.TryGetValue("build_property.MSBuildProjectName", out var name);
                x.GlobalOptions.TryGetValue("build_property.OutDir", out var outdir);
                x.GlobalOptions.TryGetValue("build_property.Version", out var version);
                x.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace);


                return new ProjectInfo(
                    fullPath ?? "",
                    name ?? "",
                    outdir ?? "",
                    version ?? "",
                    rootNamespace ?? "" );
            });

        // context.RegisterSourceOutput(context.AnalyzerConfigOptionsProvider,
        //     (spc, provider) =>
        //     {
        //         var builder = new StringBuilder();
        //         builder.AppendLine("// Verfügbare GlobalOptions:");
        //         foreach (var kvp in provider.GlobalOptions.Keys)
        //         {
        //             builder.AppendLine($"// {kvp}");
        //         }
        //
        //         spc.AddSource("BuildPropertiesDebug.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
        //     });

        context.RegisterSourceOutput(properties, this.GenerateCode);
    }

    private void GenerateCode(SourceProductionContext context, ProjectInfo projectInfo)
    {
        var code =
            $$"""
              using System;
              using System.CodeDom.Compiler;
              using System.Runtime.CompilerServices;

              namespace {{Namespace}};

              [CompilerGenerated]
              [GeneratedCode("ShadowWriter", "{{this.generatorAssemblyVersion}}")]
              internal static class TheProject
              {
                public static string FullPath => @"{{projectInfo.FullPath}}";
                public static string ProjectDirectory => @"{{Path.GetDirectoryName(projectInfo.FullPath)}}";
                public static string Name => @"{{projectInfo.Name}}";
                public static string OutDir => @"{{Path.GetFullPath(projectInfo.OutDir)}}";
                public static string Version => @"{{projectInfo.Version}}";
                public static string RootNamespace => @"{{projectInfo.RootNamespace}}";
              }
              """;

        context.AddSource("ShadowWriter.TheProject.g.cs", SourceText.From(code, Encoding.UTF8));
    }
}