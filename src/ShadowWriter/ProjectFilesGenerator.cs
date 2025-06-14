using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
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

                  public EmbeddedResourceInfo(Assembly assembly, string manifestResourceName)
                  {
                      this.assembly = assembly;
                      this.ManifestResourceName = manifestResourceName;
                  }

                  public string ManifestResourceName { get; }

                  public Stream GetEmbeddedResourceStream()
                  {
                      return assembly.GetManifestResourceStream(this.ManifestResourceName) ?? throw new FileNotFoundException($"Embedded resource {this.ManifestResourceName} not found.");
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
        var buildOutputModel = new BuildEmbeddedResourceOutputModel(encodedFileInfos.RootNamespace);

        var outputModel = buildOutputModel.GeneratedClasses(files);

        WriteEmbeddedClassInfo(outputModel.InnerClasses);

        foreach (var embeddedResourceItem in outputModel.Items)
        {
            builder.AppendLine(
                $"// {embeddedResourceItem.ManifestResourceName} - {embeddedResourceItem.PropertyName}");
            builder.AppendLine(
                $"public static EmbeddedResourceInfo {embeddedResourceItem.PropertyName} = new(typeof(EmbeddedResourceInfo).Assembly,\"{embeddedResourceItem.ManifestResourceName}\");");
        }

        void WriteEmbeddedClassInfo(IEnumerable<EmbeddedResourceClassInfo> infos)
        {
            using var _ = builder.BeginBlock();
            foreach (var embeddedResourceClassInfo in infos)
            {
                builder.AppendLine($"public static class {embeddedResourceClassInfo.Name} {{");
                if (embeddedResourceClassInfo.InnerClasses.Count > 0)
                {
                    WriteEmbeddedClassInfo(embeddedResourceClassInfo.InnerClasses);
                }

                foreach (var embeddedResourceItem in embeddedResourceClassInfo.Items)
                {
                    builder.AppendLine(
                        $"// {embeddedResourceItem.ManifestResourceName} - {embeddedResourceItem.PropertyName}");

                    builder.AppendLine(
                        $"public static EmbeddedResourceInfo {embeddedResourceItem.PropertyName} = new(typeof(EmbeddedResourceInfo).Assembly,\"{embeddedResourceItem.ManifestResourceName}\");");
                }

                builder.AppendLine("}");
            }
        }


        // foreach (var file in files)
        // {
        //     var name = Path.GetFileName(file);
        //     var nameParts = name.Split(['.'], StringSplitOptions.RemoveEmptyEntries).Select(x => x.ToPascalCase());
        //     var propertyName = string.Concat(nameParts);
        //
        //     builder.AppendLine(
        //         $"public static EmbeddedResourceInfo {propertyName} = new(typeof(EmbeddedResourceInfo).Assembly,\"{encodedFileInfos.RootNamespace}\", \"{file}\");");
        // }

        return builder.ToString();
    }
}

internal sealed class BuildEmbeddedResourceOutputModel
{
    private readonly string rootNamespace;

    public BuildEmbeddedResourceOutputModel(string rootNamespace)
    {
        this.rootNamespace = rootNamespace;
    }

    public EmbeddedResourceClassInfo GeneratedClasses(IEnumerable<string> files)
    {
        var result = new EmbeddedResourceClassInfo.Builder();
        result.Name = "EmbeddedResources";

        foreach (var file in files)
        {
            IList<EmbeddedResourceClassInfo.Builder> current = result.InnerClasses;
            var dir = Path.GetDirectoryName(file) ?? "";

            var parts = new Queue<string>(dir.Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                StringSplitOptions.RemoveEmptyEntries));

            var nsx = string.Concat(parts.Select(p => $"{p}."));

            EmbeddedResourceClassInfo.Builder? classInfo = result;
            while (parts.Count > 0)
            {
                var part = parts.Dequeue();

                classInfo = current.FirstOrDefault(x => x.Name.Equals(part, StringComparison.Ordinal));
                if (classInfo is null)
                {
                    classInfo = new EmbeddedResourceClassInfo.Builder();
                    classInfo.Name = part;
                    current.Add(classInfo);
                }

                current = classInfo.InnerClasses;
            }

            var name = Path.GetFileName(file) ?? "";

            var propertyName =
                $"{Path.GetFileNameWithoutExtension(file).ToPascalCase()}{Path.GetExtension(file).TrimStart('.').ToPascalCase()}"
                    .ToValidPropertyName();

            classInfo.Items.Add(new EmbeddedResourceItem.Builder
            {
                ManifestResourceName = $"{this.rootNamespace}.{nsx}{name}",
                PropertyName = propertyName,
            });
        }

        return result.Build();
    }
}

internal sealed record EmbeddedResourceItem(string PropertyName, string ManifestResourceName)
{
    public sealed class Builder
    {
        public string PropertyName { get; set; } = "";
        public string ManifestResourceName { get; set; } = "";

        public EmbeddedResourceItem Build()
        {
            if (string.IsNullOrWhiteSpace(this.PropertyName)) throw new InvalidOperationException();
            if (string.IsNullOrWhiteSpace(this.ManifestResourceName)) throw new InvalidOperationException();

            return new EmbeddedResourceItem(PropertyName, ManifestResourceName);
        }
    }
}

internal sealed record EmbeddedResourceClassInfo(
    string Name,
    IReadOnlyList<EmbeddedResourceClassInfo> InnerClasses,
    IReadOnlyList<EmbeddedResourceItem> Items)
{
    public sealed class Builder
    {
        public string Name { get; set; } = "";

        public List<Builder> InnerClasses { get; } = new();

        public List<EmbeddedResourceItem.Builder> Items { get; } = new();

        public EmbeddedResourceClassInfo Build()
        {
            if (string.IsNullOrWhiteSpace(this.Name)) throw new InvalidOperationException();

            return new EmbeddedResourceClassInfo(
                this.Name,
                this.InnerClasses.ConvertAll(i => i.Build()).AsReadOnly(),
                this.Items.ConvertAll(i => i.Build()).AsReadOnly());
        }
    }
};