using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace ShadowWriter;

public sealed class SourceLinkInfo
{
    public string RepositoryUrl { get; init; } = string.Empty;
    public  string Branch { get; init; } = string.Empty;
    public  string Commit { get; init; } = string.Empty;
    public  string SourceRevisionId { get; init; } = string.Empty;
}


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

        var configProvider = context.AnalyzerConfigOptionsProvider
            .Select((options, _) => new SourceLinkInfo
            {
                RepositoryUrl = options.GlobalOptions.TryGetValue("build_property.RepositoryUrl", out var url) ? url : string.Empty,
                Branch = options.GlobalOptions.TryGetValue("build_property.RepositoryBranch", out var branch) ? branch : string.Empty,
                Commit = options.GlobalOptions.TryGetValue("build_property.repositorycommit", out var commit) ? commit : string.Empty,
                SourceRevisionId = options.GlobalOptions.TryGetValue("build_property.SourceRevisionId", out var revId) ? revId :string.Empty,
            });

        context.RegisterSourceOutput(configProvider, GenerateSourceLinkCode);


        context.RegisterSourceOutput(context.AnalyzerConfigOptionsProvider,
            (spc, provider) =>
            {
                var values = string.Join(" | ", provider.GlobalOptions.Keys);

                var code =
                    $$"""
                      using System;
                      using System.CodeDom.Compiler;
                      using System.Runtime.CompilerServices;

                      namespace {{Namespace}};

                      [CompilerGenerated]
                      [GeneratedCode("ShadowWriter", "{{this.generatorAssemblyVersion}}")]
                      internal static class Debug
                      {
                        public static string DebugInfo = "{{values}}";
                      }
                      """;

                spc.AddSource("BuildPropertiesDebug.g.cs", SourceText.From(code, Encoding.UTF8));
            });

        context.RegisterSourceOutput(properties, this.GenerateCode);
    }

    private void GenerateSourceLinkCode(SourceProductionContext context, SourceLinkInfo sourceLinkInfo)
    {
        var code =
            $$"""
              using System;
              using System.CodeDom.Compiler;
              using System.Runtime.CompilerServices;

              namespace {{Namespace}};

              [CompilerGenerated]
              [GeneratedCode("ShadowWriter", "{{this.generatorAssemblyVersion}}")]
              internal static class Git
              {
                public static string RepositoryUrl => @"{{sourceLinkInfo.RepositoryUrl}}";
                public static string Branch => @"{{sourceLinkInfo.Branch}}";
                public static string Commit => @"{{sourceLinkInfo.Commit}}";
                public static string SourceRevisionId => @"{{sourceLinkInfo.SourceRevisionId}}";
              }
              """;

        context.AddSource("ShadowWriter.GitInfo.g.cs", SourceText.From(code, Encoding.UTF8));
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