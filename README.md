# ShadowWriter

[![NuGet](https://img.shields.io/nuget/v/ShadowWriter.svg)](https://www.nuget.org/packages/ShadowWriter)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Build](https://img.shields.io/github/actions/workflow/status/StefanStolz/ShadowWriter/build.yml?branch=main)](https://github.com/StefanStolz/ShadowWriter/actions)

**ShadowWriter** is a Roslyn Source Generator designed to simplify and automate aspects of .NET development.  
It currently supports the following features:

## ✨ Features

### 1. Generate Null Objects
Automatically generates implementations of interfaces or abstract classes that follow the *Null Object Pattern*.  
These objects act as safe defaults and help avoid `null` references.

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