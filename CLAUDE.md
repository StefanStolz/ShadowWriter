# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with this repository.

## Repo layout

```
ShadowWriter/
├── src/                    # All source code — see src/CLAUDE.md for details
│   ├── ShadowWriter.sln
│   ├── ShadowWriter/       # The Roslyn generator NuGet package
│   ├── RoslynVerifier/     # Internal test-assertion library (not published)
│   ├── ShadowWriter.Sample/# Usage examples / integration smoke tests
│   ├── ShadowWriter.Tests/ # Generator unit tests
│   ├── RoslynVerifier.Tests/
│   ├── Directory.Build.props
│   └── Directory.Packages.props
├── docs/                   # Generator documentation (Home, NullObject, ProjectFiles, ProjectInfo)
├── artifacts/              # Build output (bin/, obj/) — gitignored, produced by dotnet build
├── .github/workflows/      # CI: build.yml (PR gate), deploy_to_nuget.yml (release)
├── .claude/                # Claude Code project settings
├── .editorconfig
├── LICENSE
├── README.md
└── NOTES.md
```

## Source code

All commands, architecture, and development guidance live in **[src/CLAUDE.md](src/CLAUDE.md)**.

## CI workflows

| File | Purpose |
|---|---|
| `build.yml` | Runs on every push/PR — restore, build, test |
| `deploy_to_nuget.yml` | Publishes the NuGet package on release; overrides `VersionPrefix` with `0.0.<run_number>` |
