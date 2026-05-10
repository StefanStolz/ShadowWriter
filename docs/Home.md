# ShadowWriter

ShadowWriter is a Roslyn Source Generator for .NET that reduces boilerplate by generating code at compile time. No runtime dependencies required.

## Features

### [NullObject](./NullObject.md)

Automatically generates null-object implementations for interfaces and classes using the `[NullObject]` attribute, providing safe "do nothing" defaults that eliminate null reference exceptions.

### [ProjectInfo](./ProjectInfo.md)

Embeds project metadata from your `.csproj` file (name, version, paths, build timestamp) into a compile-time `ProjectInfo` class, making it available at runtime without manual maintenance.

### [ProjectFiles](./ProjectFiles.md)

*(Experimental)* Generates strongly-typed wrappers for embedded resources, giving you safe, refactor-friendly access to project assets without magic strings.

### [Builder](./Builder.md)

Automatically generates a nested `Builder` class for `record` types annotated with `[Builder]`, eliminating boilerplate for fluent object construction.
