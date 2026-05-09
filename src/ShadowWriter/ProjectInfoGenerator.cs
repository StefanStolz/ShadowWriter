using System;
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
                x.GlobalOptions.TryGetValue("build_property.OutDir", out var outDir);
                x.GlobalOptions.TryGetValue("build_property.Version", out var version);
                x.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace);

                return new ProjectInfo(
                    fullPath ?? "",
                    name ?? "",
                    outDir ?? "",
                    version ?? "",
                    rootNamespace ?? "");
            });

        var config = context.AnalyzerConfigOptionsProvider
            .Select((x, _) =>
            {
                bool ParseFlag(string key, bool defaultValue = true)
                {
                    return x.GlobalOptions.TryGetValue(key, out var raw)
                        && bool.TryParse(raw, out var parsed) ? parsed : defaultValue;
                }

                return new ProjectInfoConfig(
                    EnableProjectInfo:    ParseFlag("build_property.ShadowWriter_EnableProjectInfo", false),
                    IncludePaths:         ParseFlag("build_property.ShadowWriter_ProjectInfo_IncludePaths"),
                    IncludeVersion:       ParseFlag("build_property.ShadowWriter_ProjectInfo_IncludeVersion"),
                    IncludeBuildTime:     ParseFlag("build_property.ShadowWriter_ProjectInfo_IncludeBuildTime"),
                    IncludeRootNamespace: ParseFlag("build_property.ShadowWriter_ProjectInfo_IncludeRootNamespace"));
            });

        context.RegisterSourceOutput(properties.Combine(config), this.GenerateCode);
    }

    private void GenerateCode(SourceProductionContext context,
        (ProjectInfo ProjectInfo, ProjectInfoConfig Config) arg)
    {
        var (projectInfo, config) = arg;

        if (!config.EnableProjectInfo) return;

        var sb = new StringBuilder();

        if (config.IncludePaths)
        {
            sb.AppendLine($"    public static string FullPath => @\"{projectInfo.FullPath}\";");
            sb.AppendLine($"    public static string ProjectDirectory => @\"{Path.GetDirectoryName(projectInfo.FullPath)}\";");
            sb.AppendLine($"    public static string OutDir => @\"{Path.GetFullPath(projectInfo.OutDir)}\";");
        }

        sb.AppendLine($"    public static string Name => @\"{projectInfo.Name}\";");

        if (config.IncludeVersion)
            sb.AppendLine($"    public static string Version => @\"{projectInfo.Version}\";");

        if (config.IncludeRootNamespace)
            sb.AppendLine($"    public static string RootNamespace => @\"{projectInfo.RootNamespace}\";");

        if (config.IncludeBuildTime)
        {
            var now = DateTimeOffset.UtcNow;
            sb.AppendLine($"    public static DateTimeOffset BuildTimeUtc => new DateTimeOffset({now.Ticks}L, TimeSpan.Zero);");
        }

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
              {{sb}}    }
              }
              """;

        context.AddSource("ShadowWriter.TheProject.g.cs", SourceText.From(code, Encoding.UTF8));
    }
}