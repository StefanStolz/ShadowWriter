using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using RoslynVerifier;

namespace ShadowWriter.Tests;

[TestFixture]
[TestOf(typeof(ProjectInfoGenerator))]
public class ProjectInfoGeneratorTests
{
    private static readonly Dictionary<string, string> BaseOptions = new()
    {
        { "build_property.MSBuildProjectFullPath", "C:\\Projects\\TestProject\\TestProject.csproj" },
        { "build_property.MSBuildProjectName", "TestProject" },
        { "build_property.OutDir", "C:\\Projects\\TestProject\\bin\\Debug\\" },
        { "build_property.Version", "1.0.0" },
        { "build_property.RootNamespace", "TestNamespace" },
        { "build_property.ShadowWriter_EnableProjectInfo", "true" },
    };

    private static ImmutableArray<SyntaxTree> RunGenerator(Dictionary<string, string> options)
    {
        var sut = new ProjectInfoGenerator();
        var provider = new FakeAnalyzerConfigOptionsProvider(new FakeAnalyzerConfigOptions(options));
        var compilation = CSharpCompilation.Create(nameof(ProjectInfoGeneratorTests),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        return CSharpGeneratorDriver.Create(sut)
            .WithUpdatedAnalyzerConfigOptions(provider)
            .RunGeneratorsAndUpdateCompilation(compilation, out _, out _)
            .GetRunResult().GeneratedTrees;
    }

    private static TypeVerifier GetProjectInfoVerifier(Dictionary<string, string> options)
    {
        var trees = RunGenerator(options);
        var generated = trees.Single();
        return SyntaxVerifier.From(generated.GetRoot().DescendantNodes()
            .OfType<ClassDeclarationSyntax>().Single());
    }

    [Test]
    public void CreateProjectInfo()
    {
        var verifier = GetProjectInfoVerifier(new Dictionary<string, string>(BaseOptions));
        verifier.ShouldHaveName("ProjectInfo");
    }

    [Test]
    public void MasterSwitchFalseGeneratesNoSource()
    {
        var options = new Dictionary<string, string>(BaseOptions)
        {
            ["build_property.ShadowWriter_EnableProjectInfo"] = "false"
        };

        var trees = RunGenerator(options);
        trees.ShouldBeEmpty();
    }

    [Test]
    public void DefaultFlagsGenerateAllProperties()
    {
        var verifier = GetProjectInfoVerifier(new Dictionary<string, string>(BaseOptions));
        verifier.ShouldHaveStaticProperty("FullPath");
        verifier.ShouldHaveStaticProperty("Name");
        verifier.ShouldHaveStaticProperty("Version");
        verifier.ShouldHaveStaticProperty("RootNamespace");
        verifier.ShouldHaveStaticProperty("BuildTimeUtc");
    }

    [Test]
    public void DisablePathsOmitsPathProperties()
    {
        var options = new Dictionary<string, string>(BaseOptions)
        {
            { "build_property.ShadowWriter_ProjectInfo_IncludePaths", "false" }
        };

        var verifier = GetProjectInfoVerifier(options);
        verifier.ShouldNotHaveProperty("FullPath");
        verifier.ShouldNotHaveProperty("ProjectDirectory");
        verifier.ShouldNotHaveProperty("OutDir");
        verifier.ShouldHaveStaticProperty("Name");
    }

    [Test]
    public void DisableVersionOmitsVersionProperty()
    {
        var options = new Dictionary<string, string>(BaseOptions)
        {
            { "build_property.ShadowWriter_ProjectInfo_IncludeVersion", "false" }
        };

        var verifier = GetProjectInfoVerifier(options);
        verifier.ShouldNotHaveProperty("Version");
        verifier.ShouldHaveStaticProperty("Name");
    }

    [Test]
    public void DisableBuildTimeOmitsBuildTimeUtcProperty()
    {
        var options = new Dictionary<string, string>(BaseOptions)
        {
            { "build_property.ShadowWriter_ProjectInfo_IncludeBuildTime", "false" }
        };

        var verifier = GetProjectInfoVerifier(options);
        verifier.ShouldNotHaveProperty("BuildTimeUtc");
        verifier.ShouldHaveStaticProperty("Name");
    }

    [Test]
    public void DisableRootNamespaceOmitsRootNamespaceProperty()
    {
        var options = new Dictionary<string, string>(BaseOptions)
        {
            { "build_property.ShadowWriter_ProjectInfo_IncludeRootNamespace", "false" }
        };

        var verifier = GetProjectInfoVerifier(options);
        verifier.ShouldNotHaveProperty("RootNamespace");
        verifier.ShouldHaveStaticProperty("Name");
    }

    [Test]
    public void DisableAllSubFeaturesLeavesOnlyName()
    {
        var options = new Dictionary<string, string>(BaseOptions)
        {
            { "build_property.ShadowWriter_ProjectInfo_IncludePaths", "false" },
            { "build_property.ShadowWriter_ProjectInfo_IncludeVersion", "false" },
            { "build_property.ShadowWriter_ProjectInfo_IncludeBuildTime", "false" },
            { "build_property.ShadowWriter_ProjectInfo_IncludeRootNamespace", "false" },
        };

        var trees = RunGenerator(options);
        var generated = trees.Single();
        var classDecl = generated.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
        var properties = classDecl.Members.OfType<PropertyDeclarationSyntax>().ToList();
        properties.Count.ShouldBe(1);
        properties[0].Identifier.Text.ShouldBe("Name");
    }

    private sealed class FakeAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
    {
        private readonly AnalyzerConfigOptions globalOptions;

        public FakeAnalyzerConfigOptionsProvider(AnalyzerConfigOptions globalOptions)
        {
            this.globalOptions = globalOptions;
        }

        public override AnalyzerConfigOptions GlobalOptions => this.globalOptions;

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => this.globalOptions;

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => this.globalOptions;
    }

    private sealed class FakeAnalyzerConfigOptions : AnalyzerConfigOptions
    {
        private readonly Dictionary<string, string> options;

        public FakeAnalyzerConfigOptions(Dictionary<string, string>? options)
        {
            this.options = options ?? new Dictionary<string, string>();
        }

        public override bool TryGetValue(string key, out string value)
        {
            value = String.Empty;

            if (this.options.TryGetValue(key, out var val))
            {
                value = val;
                return true;
            }

            return false;
        }

        public override IEnumerable<string> Keys => this.options.Keys;
    }
}