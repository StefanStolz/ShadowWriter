using System;

namespace ShadowWriter.Sample;

public static class StaticRun
{
    public static void Main()
    {
        Console.WriteLine($"FullPath: {TheProject.FullPath}");
        Console.WriteLine($"Name: {TheProject.Name}");
        Console.WriteLine($"OutDir: {TheProject.OutDir}");
        Console.WriteLine($"Version: {TheProject.Version}");
        Console.WriteLine($"RootNamespace: {TheProject.RootNamespace}");

    }
}