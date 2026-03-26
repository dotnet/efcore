// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools;

internal class Project
{
    private readonly string _file;
    private readonly string? _framework;
    private readonly string? _configuration;
    private readonly string? _runtime;

    public Project(string file, string? framework, string? configuration, string? runtime)
    {
        Debug.Assert(!string.IsNullOrEmpty(file), "file is null or empty.");

        _file = file;
        _framework = framework;
        _configuration = configuration;
        _runtime = runtime;
        ProjectName = Path.GetFileName(file);
    }

    public string ProjectName { get; }

    public string? AssemblyName { get; set; }
    public string? DesignAssembly { get; set; }
    public string? Language { get; set; }
    public string? OutputPath { get; set; }
    public string? PlatformTarget { get; set; }
    public string? ProjectAssetsFile { get; set; }
    public string? ProjectDir { get; set; }
    public string? RootNamespace { get; set; }
    public string? RuntimeFrameworkVersion { get; set; }
    public string? TargetFileName { get; set; }
    public string? TargetFrameworkMoniker { get; set; }
    public string? Nullable { get; set; }
    public string? TargetFramework { get; set; }
    public string? TargetPlatformIdentifier { get; set; }

    public static Project FromFile(
        string file,
        string? framework = null,
        string? configuration = null,
        string? runtime = null)
    {
        Debug.Assert(!string.IsNullOrEmpty(file), "file is null or empty.");

        var args = new List<string> { "msbuild", };

        if (framework != null)
        {
            args.Add($"/property:TargetFramework={framework}");
        }

        if (configuration != null)
        {
            args.Add($"/property:Configuration={configuration}");
        }

        if (runtime != null)
        {
            args.Add($"/property:RuntimeIdentifier={runtime}");
        }

        foreach (var property in typeof(Project).GetProperties())
        {
            args.Add($"/getProperty:{property.Name}");
        }

        args.Add("/getProperty:Platform");

        args.Add("/t:ResolvePackageAssets");
        args.Add("/getItem:RuntimeCopyLocalItems");

        args.Add(file);

        var output = new StringBuilder();

        Reporter.WriteVerbose(Resources.RunningCommand("dotnet " + string.Join(" ", args)));

        var exitCode = Exe.Run("dotnet", args, handleOutput: line =>
        {
            output.AppendLine(line);
            Reporter.WriteVerbose(line);
        });
        if (exitCode != 0)
        {
            if (framework == null && HasMultipleTargetFrameworks(file))
            {
                throw new CommandException(Resources.MultipleTargetFrameworks);
            }

            throw new CommandException(Resources.GetMetadataFailed);
        }

        var metadata = JsonSerializer.Deserialize<ProjectMetadata>(output.ToString())!;

        var runtimeCopyLocalItems = metadata.Items["RuntimeCopyLocalItems"];

        var designAssembly = runtimeCopyLocalItems
            .Select(i => i["FullPath"])
            .FirstOrDefault(i => i.Contains("Microsoft.EntityFrameworkCore.Design", StringComparison.InvariantCulture))
            ?.Replace('\\', Path.DirectorySeparatorChar);
        var properties = metadata.Properties;

        var normalizedOutputPath = properties[nameof(OutputPath)]!.Replace('\\', Path.DirectorySeparatorChar);
        var normalizedProjectDir = properties[nameof(ProjectDir)]!.Replace('\\', Path.DirectorySeparatorChar);
        var normalizedProjectAssetsFile = properties[nameof(ProjectAssetsFile)]?.Replace('\\', Path.DirectorySeparatorChar);
        var outputPath = Path.GetFullPath(Path.Combine(normalizedProjectDir, normalizedOutputPath));
        CopyBuildHost(runtimeCopyLocalItems, outputPath);

        var platformTarget = properties[nameof(PlatformTarget)];
        if (platformTarget.Length == 0)
        {
            platformTarget = properties["Platform"];
        }

        return new Project(file, framework, configuration, runtime)
        {
            AssemblyName = properties[nameof(AssemblyName)],
            DesignAssembly = designAssembly,
            Language = properties[nameof(Language)],
            OutputPath = normalizedOutputPath,
            PlatformTarget = platformTarget,
            ProjectAssetsFile = normalizedProjectAssetsFile,
            ProjectDir = normalizedProjectDir,
            RootNamespace = properties[nameof(RootNamespace)],
            RuntimeFrameworkVersion = properties[nameof(RuntimeFrameworkVersion)],
            TargetFileName = properties[nameof(TargetFileName)],
            TargetFrameworkMoniker = properties[nameof(TargetFrameworkMoniker)],
            Nullable = properties[nameof(Nullable)],
            TargetFramework = properties[nameof(TargetFramework)],
            TargetPlatformIdentifier = properties[nameof(TargetPlatformIdentifier)]
        };
    }

    private record class ProjectMetadata
    {
        public Dictionary<string, string> Properties { get; set; } = null!;
        public Dictionary<string, Dictionary<string, string>[]> Items { get; set; } = null!;
    }

    private static bool HasMultipleTargetFrameworks(string file)
    {
        var args = new List<string> { "msbuild", "/getProperty:TargetFrameworks", file };

        var output = new StringBuilder();
        var exitCode = Exe.Run("dotnet", args, handleOutput: line => output.AppendLine(line));
        if (exitCode != 0)
        {
            return false;
        }

        var outputString = output.ToString();
        return !string.IsNullOrWhiteSpace(outputString);
    }

    private static void CopyBuildHost(
        Dictionary<string, string>[] runtimeCopyLocalItems,
        string targetDir)
    {
        var msbuildWorkspacesItem = runtimeCopyLocalItems.FirstOrDefault(item =>
            string.Equals(item["Filename"], "Microsoft.CodeAnalysis.Workspaces.MSBuild", StringComparison.OrdinalIgnoreCase));

        if (msbuildWorkspacesItem == null
            || !msbuildWorkspacesItem.TryGetValue("CopyLocal", out var copyLocal)
            || !string.Equals(copyLocal, "true", StringComparison.OrdinalIgnoreCase)
            || !msbuildWorkspacesItem.TryGetValue("FullPath", out var fullPath)
            || string.IsNullOrEmpty(fullPath))
        {
            return;
        }

        var contentFilesPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(fullPath)!, "..", "..", "contentFiles", "any", "any"));
            CopyDirectoryRecursive(contentFilesPath, targetDir);
    }

    private static void CopyDirectoryRecursive(string sourceDir, string targetDir)
    {
        var directory = new DirectoryInfo(sourceDir);
        if (!directory.Exists)
        {
            return;
        }

        Directory.CreateDirectory(targetDir);
        foreach (var file in directory.GetFiles())
        {
            var filePath = Path.Combine(targetDir, file.Name);
            if (!File.Exists(filePath))
            {
                file.CopyTo(filePath, overwrite: false);
            }
        }

        foreach (var subDir in directory.GetDirectories())
        {
            CopyDirectoryRecursive(subDir.FullName, Path.Combine(targetDir, subDir.Name));
        }
    }

    public void Build(IEnumerable<string>? additionalArgs)
    {
        var args = new List<string> { "build" };

        if (_file != null)
        {
            args.Add(_file);
        }

        // TODO: Only build for the first framework when unspecified
        if (_framework != null)
        {
            args.Add("--framework");
            args.Add(_framework);
        }

        if (_configuration != null)
        {
            args.Add("--configuration");
            args.Add(_configuration);
        }

        if (_runtime != null)
        {
            args.Add("--runtime");
            args.Add(_runtime);
        }

        args.Add("/verbosity:quiet");
        args.Add("/nologo");
        args.Add("/p:PublishAot=false"); // Avoid NativeAOT warnings
        if (additionalArgs != null)
        {
            args.AddRange(additionalArgs);
        }

        var exitCode = Exe.Run("dotnet", args, handleOutput: Reporter.WriteVerbose);
        if (exitCode != 0)
        {
            throw new CommandException(Resources.BuildFailed);
        }
    }
}
