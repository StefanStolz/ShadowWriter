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
    [Test]
    public void CreateProjectInfo()
    {
        var sut = new ProjectInfoGenerator();

        var configOptions = new FakeAnalyzerConfigOptions(new Dictionary<string, string>
        {
            { "build_property.MSBuildProjectFullPath", "C:\\Projects\\TestProject\\TestProject.csproj" },
            { "build_property.MSBuildProjectName", "TestProject" },
            { "build_property.OutDir", "bin\\Debug\\" },
            { "build_property.Version", "1.0.0" },
            { "build_property.VersionPrefix", "1.0" },
            { "build_property.VersionSuffix", "beta" },
            { "build_property.RootNamespace", "TestNamespace" },
            { "build_property.ShadowWriter_EnableEmbeddedResources", "true" },
            { "build_property.AllEmbeddedResources", "Resource1.txt|Resource2.txt" }
        });

        var configOptionsProvider = new FakeAnalyzerConfigOptionsProvider(configOptions);


        var driver = CSharpGeneratorDriver.Create(sut).WithUpdatedAnalyzerConfigOptions(configOptionsProvider);

        var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

        var compilation = CSharpCompilation.Create(nameof(ProjectInfoGeneratorTests), options: options);


        var runResult = driver
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation outputCompilation,
                out ImmutableArray<Diagnostic> array).GetRunResult();

        runResult.Diagnostics.ShouldBeEmpty();


        var generated = runResult.GeneratedTrees.Single();

        var verifier = SyntaxVerifier.From((generated.GetRoot()).DescendantNodes()
            .OfType<ClassDeclarationSyntax>().Single());

        verifier.ShouldHaveName("TheProject");
    }

    private class FakeAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
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

    private class FakeAnalyzerConfigOptions : AnalyzerConfigOptions
    {
        private readonly Dictionary<string, string> options;

        public FakeAnalyzerConfigOptions(Dictionary<string, string> options)
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