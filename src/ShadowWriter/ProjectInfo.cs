namespace ShadowWriter;

public sealed record ProjectInfo(
    string FullPath,
    string Name,
    string OutDir,
    string Version,
    string RootNamespace);

public sealed record ProjectInfoConfig(
    bool EnableProjectInfo,
    bool IncludePaths,
    bool IncludeVersion,
    bool IncludeBuildTime,
    bool IncludeRootNamespace);