using System;
using System.IO;
using ShadowKit.IO;

namespace ShadowWriter.Sample;

public static class StaticRun
{
    public static void Main()
    {
        // Console.WriteLine($"FullPath: {TheProject.FullPath}");
        // Console.WriteLine($"Name: {TheProject.Name}");
        // Console.WriteLine($"OutDir: {TheProject.OutDir}");
        // Console.WriteLine($"Version: {TheProject.Version}");
        // Console.WriteLine($"RootNamespace: {TheProject.RootNamespace}");
        //
        // Console.WriteLine("*******************");
        // Console.WriteLine($"Files-Debug: {EmbeddedResources.DebugInfo}");
        //
        // // Console.WriteLine(EmbeddedResources.Image1Jpg.ResourceName);
        // // Console.WriteLine(EmbeddedResources.Image2Jpg.ResourceName);
        //
        // var names = typeof(EmbeddedResources).Assembly.GetManifestResourceNames();
        //
        // foreach (string name in names)
        // {
        //     Console.WriteLine($"  {name}");
        // }


        var builder = new WithBuilderMultiple.Builder();

        builder.Number = 1;
        builder.Number2 = 12;
        builder.Enabled = true;

        var item = builder.Build();

        Console.WriteLine(item);

        var b2 = new WithBuilderWithNonNullableString.Builder
        {
            Text = "a",
            Stream = Stream.Null
        };

        var item2 = b2.Build();
        Console.WriteLine(item2);
    }
}