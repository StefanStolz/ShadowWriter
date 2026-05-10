# Builder Feature Documentation

## Overview

The Builder feature automatically generates a nested `Builder` class for any `record` type annotated with `[Builder]`. It eliminates the boilerplate of manually writing builder classes, and is especially useful for records with many optional or nullable parameters where a fluent construction API improves readability.

## Quick Start

```csharp
[Builder]
public partial record WithBuilder(int Number);
```

```csharp
var builder = new WithBuilder.Builder();
builder.Number = 42;
WithBuilder instance = builder.Build();
```

## Usage

### Basic Record

A record with a single value-type parameter:

```csharp
[Builder]
public partial record WithBuilder(int Number);
```

### Nullable Parameters

Nullable value types are supported and generate a nullable property:

```csharp
[Builder]
public partial record WithBuilderNullableInt(int? Number);
```

### Multiple Parameters

Records with multiple parameters generate one property per parameter:

```csharp
[Builder]
public partial record WithBuilderMultiple(int Number, int Number2, bool Enabled);
```

### Non-Nullable Reference Types

All reference-type properties are generated as nullable (`T?`). When NRT is enabled, `Build()` validates that every non-nullable parameter has been set before constructing the record:

```csharp
[Builder]
public partial record WithBuilderWithNonNullableString(string Text, Stream Stream);
```

Generated builder (NRT enabled):

```csharp
public sealed class Builder
{
    public string? Text { get; set; }
    public Stream? Stream { get; set; }

    public WithBuilderWithNonNullableString Build()
    {
        if (this.Text is null) throw new InvalidOperationException("'Text' must be set before calling Build().");
        if (this.Stream is null) throw new InvalidOperationException("'Stream' must be set before calling Build().");
        return new(this.Text, this.Stream);
    }
}
```

## Generated Code

For each annotated record the generator emits a partial record extension containing a single nested `sealed class Builder`. The builder has:

- One mutable nullable property per record parameter
- A `Build()` method that validates required parameters and constructs the record via its primary constructor

Example — input:

```csharp
[Builder]
public partial record WithBuilder(int Number);
```

Generated output:

```csharp
public partial record WithBuilder
{
    public sealed class Builder
    {
        public int? Number { get; set; }

        public WithBuilder Build()
        {
            if (!this.Number.HasValue) throw new InvalidOperationException("'Number' must be set before calling Build().");
            return new(this.Number.Value);
        }
    }
}
```

## Build() Validation

The `Build()` method guards every non-nullable parameter before calling the constructor. The guard type depends on the parameter kind:

| Parameter kind | Builder property | Guard in `Build()` | Constructor argument |
|---|---|---|---|
| Non-nullable value type (`int`, `bool`, `DateTime`) | `int?` | `if (!this.P.HasValue) throw ...` | `this.P.Value` |
| Non-nullable reference type (`string`, `Stream`) | `string?` | `if (this.P is null) throw ...` | `this.P` |
| Nullable value type (`int?`, `bool?`) | `int?` | _(none — null is valid)_ | `this.P` |
| Nullable reference type (`string?`, `Stream?`) | `string?` | _(none — null is valid)_ | `this.P` |

The exception thrown is `System.InvalidOperationException` with message `'<PropertyName>' must be set before calling Build().`

## Requirements

- The record must be declared `partial`.
- `[Builder]` targets `record` types only (records are classes under the hood, so the attribute has `AttributeTargets.Class`).

```csharp
// Correct
[Builder]
public partial record MyRecord(int Value);

// Wrong — missing partial
[Builder]
public record MyRecord(int Value); // will not compile with generated partial
```

## Technical Details

- **File naming**: The generator emits `{CleanedNamespace}{RecordName}.g.cs` into the compilation.
- **Attribute visibility**: `[Builder]` is generated as an `internal sealed` marker attribute with no parameters — it is only visible within the consuming project.
- **Emitted attributes**: Generated code is decorated with `[CompilerGenerated]` and `[GeneratedCode("ShadowWriter", "...")]`.
- **NRT awareness**: The generator reads the compilation's `NullableContextOptions` to decide which reference-type properties require null guards in `Build()` and whether to emit `#nullable enable` in the generated file.

## Version Compatibility

- Requires C# 9.0 or later (records are a C# 9 language feature)

## See Also

- [Documentation Home](./Home.md)
- [Project README](../README.md)
