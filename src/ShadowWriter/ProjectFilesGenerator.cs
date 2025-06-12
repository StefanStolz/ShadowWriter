using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace ShadowWriter;

public sealed record EncodedFileInfos(bool Generate, string RootNamespace, string AllEmbeddedResources);

[Generator]
public sealed class ProjectFilesGenerator : IIncrementalGenerator
{
    private readonly string generatorAssemblyVersion = "0.0.0";
    private const string Namespace = "ShadowWriter";

    public ProjectFilesGenerator()
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
                x.GlobalOptions.TryGetValue("build_property.GenerateEmbeddedResources",
                    out var generateEmbeddedResourcesText);
                x.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace);
                x.GlobalOptions.TryGetValue("build_property.AllEmbeddedResources", out var allEmbeddedResources);

                bool.TryParse(generateEmbeddedResourcesText, out bool generateEmbeddedResources);

                return new EncodedFileInfos(generateEmbeddedResources, rootNamespace ?? "", allEmbeddedResources ?? "");
            });

        context.RegisterSourceOutput(properties, this.GenerateCode);
    }

    private void GenerateCode(SourceProductionContext context, EncodedFileInfos encodedFileInfos)
    {
        if (!encodedFileInfos.Generate) return;

        var body = CreateBody(encodedFileInfos);

        var code =
            $$"""
              using System;
              using System.CodeDom.Compiler;
              using System.Runtime.CompilerServices;
              using System.IO;
              using System.Reflection;

              namespace {{Namespace}};

              [CompilerGenerated]
              [GeneratedCode("ShadowWriter", "{{this.generatorAssemblyVersion}}")]
              internal sealed class EmbeddedResourceInfo
              {
                  private readonly Assembly assembly;
                  private readonly string rootNamespace;
                  private readonly string resourceName;

                  public EmbeddedResourceInfo(Assembly assembly, string rootNamespace, string resourceName)
                  {
                      this.assembly = assembly;
                      this.rootNamespace = rootNamespace;
                      this.resourceName = resourceName.Replace(Path.DirectorySeparatorChar, '.').Replace(Path.AltDirectorySeparatorChar, '.');
                  }

                  public string ResourceName => this.resourceName;

                  public string ManifestResourceName => $"{this.rootNamespace}.{this.resourceName}";

                  public Stream GetEmbeddedResourceStream()
                  {
                      return assembly.GetManifestResourceStream(this.ManifestResourceName) ?? throw new FileNotFoundException($"Embedded resource {this.resourceName} not found.");
                  }
              }

              [CompilerGenerated]
              [GeneratedCode("ShadowWriter", "{{this.generatorAssemblyVersion}}")]
              internal static class EmbeddedResources
              {
                    // {{encodedFileInfos.RootNamespace}}
                    // {{encodedFileInfos.AllEmbeddedResources}}

                    public static string DebugInfo => @"{{encodedFileInfos.AllEmbeddedResources}}";

                    {{body}}
              }
              """;

        context.AddSource("ShadowWriter.EmbeddedResources.g.cs", SourceText.From(code, Encoding.UTF8));
    }

    private string CreateBody(EncodedFileInfos encodedFileInfos)
    {
        var files = encodedFileInfos.AllEmbeddedResources.Split(['|'], StringSplitOptions.RemoveEmptyEntries);


        var builder = new IndentedStringBuilder("  ", 1);

        foreach (var file in files)
        {
            var name = Path.GetFileName(file);
            var nameParts = name.Split(['.'], StringSplitOptions.RemoveEmptyEntries).Select(this.PascalCase);
            var propertyName = string.Concat(nameParts);


            builder.AppendLine(
                $"public static EmbeddedResourceInfo {propertyName} = new(typeof(EmbeddedResourceInfo).Assembly,\"{encodedFileInfos.RootNamespace}\", \"{file}\");");
        }

        return builder.ToString();
    }

    private string PascalCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return "";
        if (name.Length == 1) return name;
        var buffer = name.ToLowerInvariant();
        var firstChar = char.ToUpperInvariant(buffer[0]);

        return string.Concat(new[] { firstChar }.Concat(name.ToLowerInvariant().Skip(1)));
    }
}