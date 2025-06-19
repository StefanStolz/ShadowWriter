# ShadowWriter

[![NuGet](https://img.shields.io/nuget/v/ShadowWriter.svg)](https://www.nuget.org/packages/ShadowWriter)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Build](https://img.shields.io/github/actions/workflow/status/StefanStolz/ShadowWriter/build.yml?branch=main)](https://github.com/StefanStolz/ShadowWriter/actions)

**ShadowWriter** is a Roslyn Source Generator designed to simplify and automate aspects of .NET development.  
It currently supports the following features:

## ✨ Features

Samples can be found in the [Source-Code](https://github.com/StefanStolz/ShadowWriter/tree/main/src/ShadowWriter.Sample).

### 1. Generate Null Objects
The NullObject feature in ShadowWriter provides a simple way to automatically generate null object implementations for interfaces and classes. This pattern is useful for providing default "do nothing" implementations that can help avoid null reference exceptions and simplify code.

#### Usage
To create a null object implementation, simply add the `[NullObject]` attribute to your class:

```csharp
[NullObject]
public partial class ImplementingMyInterface : IMyInterface
{
}
```

### 2. Inject Project Information
Embeds values from the project file (`*.csproj`) directly into your source code.  
This is useful for build metadata, version numbers, or project-specific configuration.

### 3. Experimental: Typed Access to EmbeddedResources
Generates strongly typed wrappers for `EmbeddedResources`, allowing safe and convenient access to resources at runtime.

> ⚠️ Feature #3 is experimental and may change significantly in future versions.

## 📦 Installation

You can install ShadowWriter via NuGet:

```sh
dotnet add package ShadowWriter
```

⚙️ Usage
ShadowWriter runs automatically during compilation.
No manual setup is needed. Documentation and configuration options will be expanded in future versions.

📄 License
This project is licensed under the MIT License.