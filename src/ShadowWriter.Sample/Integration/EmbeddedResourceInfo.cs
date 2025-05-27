using System.IO;
using System.Reflection;

namespace ShadowWriter.Sample.Integration;

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

    public string ResourceName => resourceName;

    public Stream GetEmbeddedResourceStream()
    {
        return assembly.GetManifestResourceStream($"{this.rootNamespace}.{this.resourceName}") ?? throw new FileNotFoundException($"Embedded resource {this.resourceName} not found.");
    }
}