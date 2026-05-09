# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```sh
# Restore, build, test
dotnet restore ./ShadowWriter.sln
dotnet build ./ShadowWriter.sln
dotnet test ./ShadowWriter.sln

# Run a single test class
dotnet test ./ShadowWriter.Tests/ShadowWriter.Tests.csproj --filter "FullyQualifiedName~BuilderGeneratorTests"

# Pack the NuGet package
dotnet pack ./ShadowWriter/ShadowWriter.csproj -o ../publish
```

Build output goes to `../artifacts/` (one level above `src/`, configured in `Directory.Build.props`). Package versions are managed centrally in `Directory.Packages.props`.

## Architecture

**ShadowWriter** is a Roslyn `IIncrementalGenerator` NuGet package (`netstandard2.0`) with four generators in `ShadowWriter/`:

| Generator | Trigger | Output |
|---|---|---|
| `NullObjectGenerator` | `[NullObject]` on interface or class | Sealed `Null*` class implementing the interface with do-nothing members; or fills a partial class |
| `BuilderGenerator` | `[Builder]` on a `partial record` | Nested `Builder` class with mutable properties + `Build()` |
| `ProjectInfoGenerator` | Always runs | `ShadowWriter.TheProject` static class with `FullPath`, `Name`, `Version`, etc. baked in at compile time |
| `ProjectFilesGenerator` | MSBuild property `ShadowWriter_EnableEmbeddedResources=true` | `EmbeddedResources` static class with strongly typed accessors; optionally implements `ITransientFileManagerSource` from `ShadowKit.IO` |

Each generator injects its own attribute source in `RegisterPostInitializationOutput`, so consumers don't need a separate attributes package.

**NullObjectGenerator detail:** Attribute detection uses `CreateSyntaxProvider` (not `ForAttributeWithMetadataName`) for `[NullObject]`. When applied to an *interface*, a full `sealed partial class` is generated. When applied to a *class*, members are injected into the existing partial class via `GenerateNullObjectInPartialClass`. Return-type handling (void, value types, `string`, `Task`, `ValueTask`, `IEnumerable<T>`) is centralised in `NullObjectTypeInfoHandler`; unsupported types emit diagnostic `SHADOWWRITER0001` and generate a `partial` method requiring manual implementation.

**RoslynVerifier** (`netstandard2.0`) is an internal test-assertion library. Use `SyntaxVerifier.From(typeDeclarationSyntax)` to get a `TypeVerifier`, then chain `ShouldHaveName`, `ShouldHaveProperty`, `ShouldHaveMethod`, `ShouldHaveInnerClass`, etc. It is *not* published to NuGet.

**Testing approach:** Tests in `ShadowWriter.Tests/` and `RoslynVerifier.Tests/` use NUnit + Shouldly. Generator tests create a `CSharpCompilation` in-process, run the generator via `CSharpGeneratorDriver`, then assert on the generated `SyntaxTree` using `RoslynVerifier`. Framework: NUnit 4, NSubstitute 5, Shouldly 4. All test dependencies are wired through `Directory.Build.props` via `IsTestProject=true`.

**Versioning:** `VersionPrefix` is set in `ShadowWriter.csproj` (currently `0.0.25`). CI overrides it with `0.0.<run_number>`. The assembly version is stamped into all generated files via `AssemblyFileVersionAttribute` read at generator constructor time.
