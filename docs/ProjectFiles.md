# Project Files Generator (Embedded Resources)

> ⚠️ **EXPERIMENTAL FEATURE**: This feature is currently experimental and may undergo significant changes in future versions.

## Overview
The Project Files Generator creates strongly-typed wrappers for embedded resources in your project, providing safe and convenient access to these resources at runtime. It generates a static class structure that mirrors your project's resource folder hierarchy.

## Enabling the Feature

This feature requires explicit opt-in. To enable it, add the following property to your `.csproj` file:

```xml  
<ShadowWriter_EnableEmbeddedResources>true</ShadowWriter_EnableEmbeddedResources>
```


## Generated Code Structure

The generator creates two main components:

1. `EmbeddedResourceInfo` class - Provides access to individual embedded resources
2. `EmbeddedResources` static class - Contains strongly-typed properties for all embedded resources

### Features

- **Strongly-typed Access**: Each embedded resource is represented by a property with the appropriate name
- **Hierarchical Organization**: Resources are organized in nested classes that mirror your folder structure
- **Runtime Safety**: Built-in null checks and proper exception handling
- **Debug Information**: Includes a `DebugInfo` property listing all available resources

## Usage Examples

### Basic Resource Access

```csharp 
// Access a resource at the root level 
var stream = EmbeddedResources.MyImage1Png.GetEmbeddedResourceStream();
// Access a resource in a subfolder 
var stream = EmbeddedResources.Images.Logo.CompanyLogoPng.GetEmbeddedResourceStream();
```


## Resource Naming Convention

The generator follows these naming conventions:

- File names are converted to PascalCase
- File extensions become part of the property name
- Special characters are removed or converted to valid C# identifiers
- Folder names become nested static classes

For example:
- `images/logo.png` becomes `EmbeddedResources.Images.LogoPng`
- `docs/user-guide.pdf` becomes `EmbeddedResources.Docs.UserGuidePdf`

## Integration with ShadowKit.IO

When the `ShadowKit.IO` package is referenced, the generated `EmbeddedResourceInfo` class implements `ITransientFileManagerSource`, enabling additional integration features.

## Best Practices

1. **Resource Organization**
   - Keep resources in meaningful folder structures
   - Use consistent naming conventions for files

2. **Error Handling**
   ```csharp
   try
   {
       using var stream = EmbeddedResources.MyResource.GetEmbeddedResourceStream();
       // Process the stream
   }
   catch (FileNotFoundException ex)
   {
       // Handle missing resource
   }
   ```

3. **Resource Management**
   - Always dispose of resource streams properly
   - Use `using` statements when working with resource streams

## Limitations

- The feature must be explicitly enabled via project properties
- Resource names must be valid C# identifiers after conversion
- Changes to embedded resources require recompilation to update the generated code

## Technical Notes

- Generated code is marked with `[CompilerGenerated]` attribute
- All generated classes are internal by default
- Resource streams are obtained via `Assembly.GetManifestResourceStream`
- The generator automatically handles resource name mangling for .NET resource naming conventions

## Future Considerations

As this is an experimental feature, future versions may include:
- Additional resource access patterns
- Enhanced metadata support
- Custom resource transformation pipelines
- External resource mapping capabilities