# Project Info Generator

## Overview
The Project Info Generator is a feature of ShadowWriter that automatically embeds project-specific information from your `.csproj` file directly into your source code. This feature provides convenient, compile-time access to important project metadata.

## Enabling the Generator
The generator is **opt-in** and must be enabled in your `.csproj` file:

```xml
<PropertyGroup>
  <ShadowWriter_EnableProjectInfo>true</ShadowWriter_EnableProjectInfo>
</PropertyGroup>
```

Once enabled, ShadowWriter generates a static class called `ProjectInfo` in the `ShadowWriter` namespace.

## Available Properties
The generated `ProjectInfo` class provides the following static properties:

| Property | Description | Example | Enabled by default |
|----------|-------------|---------|-------------------|
| `FullPath` | The complete path to the project file | `/path/to/YourProject.csproj` | Yes |
| `ProjectDirectory` | The directory containing the project file | `/path/to/` | Yes |
| `OutDir` | The output directory for compiled artifacts | `/path/to/artifacts/bin/YourProject/debug/` | Yes |
| `Name` | The name of the project | `YourProject` | Always included |
| `Version` | The current version of the project | `1.0.0` | Yes |
| `RootNamespace` | The root namespace of the project | `YourProject` | Yes |
| `BuildTimeUtc` | The UTC timestamp of the compilation | `2026-05-09T10:30:00Z` | Yes |

## Configuration
Each group of properties can be individually disabled:

```xml
<PropertyGroup>
  <ShadowWriter_EnableProjectInfo>true</ShadowWriter_EnableProjectInfo>

  <!-- Omit FullPath, ProjectDirectory, and OutDir -->
  <ShadowWriter_ProjectInfo_IncludePaths>false</ShadowWriter_ProjectInfo_IncludePaths>

  <!-- Omit Version -->
  <ShadowWriter_ProjectInfo_IncludeVersion>false</ShadowWriter_ProjectInfo_IncludeVersion>

  <!-- Omit RootNamespace -->
  <ShadowWriter_ProjectInfo_IncludeRootNamespace>false</ShadowWriter_ProjectInfo_IncludeRootNamespace>

  <!-- Omit BuildTimeUtc -->
  <ShadowWriter_ProjectInfo_IncludeBuildTime>false</ShadowWriter_ProjectInfo_IncludeBuildTime>
</PropertyGroup>
```

`Name` is always included regardless of configuration.

## Example Usage

```csharp
using ShadowWriter;

// Access project information anywhere in your code
Console.WriteLine($"Project Name: {ProjectInfo.Name}");
Console.WriteLine($"Project Version: {ProjectInfo.Version}");
Console.WriteLine($"Project Output Directory: {ProjectInfo.OutDir}");
Console.WriteLine($"Built at: {ProjectInfo.BuildTimeUtc}");
```

## Common Use Cases
- Accessing project version information at runtime
- Logging and diagnostics that require project metadata
- Build-time configuration and environment setup
- Resource path resolution relative to project directory
- Stamping build time into log output or about screens

## Benefits
- **Compile-time Safety**: Access project information with full type safety
- **Zero Runtime Dependencies**: All information is embedded at compile time
- **Always Up-to-date**: Values are updated automatically during compilation
- **Granular Control**: Enable only the properties your project needs

## Technical Details
- Values are extracted from the MSBuild project file during compilation
- The generated class lives in the `ShadowWriter` namespace
- The generated class is marked with `[CompilerGenerated]` attribute
- All properties are static and immutable

## Notes
- The generated class is `internal` by default
- Values reflect the state of the project at compile time
- Changes to the project file require recompilation to be reflected in the generated code

## Migrating from `TheProject`
Previous versions of ShadowWriter generated a class named `TheProject` automatically (no opt-in required). That class is now deprecated.

To migrate:

1. Enable `ProjectInfo` in your `.csproj`:
   ```xml
   <ShadowWriter_EnableProjectInfo>true</ShadowWriter_EnableProjectInfo>
   ```
2. Replace all references to `TheProject` with `ProjectInfo`.

The old `TheProject` class is still generated for backward compatibility, but all its properties return default values and are marked `[Obsolete]`. It will be removed in a future version.
