// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.EntityFrameworkCore.Tools;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tasks.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public abstract class OperationTaskBase : ToolTask
{
    /// <summary>
    ///     The assembly to use.
    /// </summary>
    [Required]
    public ITaskItem Assembly { get; set; } = null!;

    /// <summary>
    ///     The startup assembly to use.
    /// </summary>
    public ITaskItem? StartupAssembly { get; set; }

    /// <summary>
    ///     The location of Microsoft.EntityFrameworkCore.Design.dll
    /// </summary>
    public ITaskItem? DesignAssembly { get; set; }

    /// <summary>
    ///     The target framework moniker.
    /// </summary>
    [Required]
    public string TargetFrameworkMoniker { get; set; } = null!;

    /// <summary>
    ///     The target runtime framework version.
    /// </summary>
    public string? RuntimeFrameworkVersion { get; set; }

    /// <summary>
    ///     The project assets file.
    /// </summary>
    public string? ProjectAssetsFile { get; set; }

    /// <summary>
    ///     The directory containing the database files.
    /// </summary>
    public ITaskItem? DataDir { get; set; }

    /// <summary>
    ///     The target project.
    /// </summary>
    public ITaskItem? Project { get; set; }

    /// <summary>
    ///     The project directory.
    /// </summary>
    public ITaskItem? ProjectDir { get; set; }

    /// <summary>
    ///     The root namespace to use.
    /// </summary>
    public string? RootNamespace { get; set; }

    /// <summary>
    ///     The language to use. Defaults to C#.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    ///     A flag indicating whether nullable reference types are enabled.
    /// </summary>
    public bool Nullable { get; set; }

    /// <summary>
    ///     The additional arguments to pass to the dotnet-ef command.
    /// </summary>
    protected List<string> AdditionalArguments { get; } = [];

    /// <summary>
    ///     The structured output from the executed command
    /// </summary>
    protected string Output { get; set; } = null!;

    protected override string ToolName
        => "dotnet";

    protected override string GenerateFullPathToTool()
        => ToolName;

    protected override bool ValidateParameters()
    {
        var startupAssemblyName = Path.GetFileNameWithoutExtension(StartupAssembly?.ItemSpec ?? Assembly.ItemSpec);

        var targetFramework = new FrameworkName(TargetFrameworkMoniker);
        if (targetFramework.Identifier == ".NETStandard")
        {
            Log.LogError(Resources.NETStandardStartupProject(startupAssemblyName));
            return false;
        }

        if (targetFramework.Identifier != ".NETCoreApp")
        {
            Log.LogError(Resources.UnsupportedFramework(startupAssemblyName, targetFramework.Identifier));
            return false;
        }

        if (targetFramework.Version < new Version(2, 0))
        {
            Log.LogError(Resources.NETCoreApp1StartupProject(startupAssemblyName, targetFramework.Version));
            return false;
        }

        if (StartupAssembly != null
            && Path.GetExtension(StartupAssembly.ItemSpec) != ".exe")
        {
            Log.LogError(Resources.NotExecutableStartupProject(startupAssemblyName));
            return false;
        }

        return true;
    }

    private readonly StringBuilder _resultBuilder = new();

    public override bool Execute()
    {
        _resultBuilder.Clear();
        var success = base.Execute();
        Output = _resultBuilder.ToString();

        return success;
    }

    protected override string? GetWorkingDirectory()
        => ProjectDir?.ItemSpec;

    protected override string GenerateCommandLineCommands()
    {
        var args = new List<string>();

        var startupAssemblyName = Path.GetFileNameWithoutExtension(StartupAssembly?.ItemSpec ?? Assembly.ItemSpec);
        var startupDir = Path.GetDirectoryName(Path.GetFullPath(StartupAssembly?.ItemSpec ?? Assembly.ItemSpec))!;
        var depsFile = Path.Combine(
            startupDir,
            startupAssemblyName + ".deps.json");
        var runtimeConfig = Path.Combine(
            startupDir,
            startupAssemblyName + ".runtimeconfig.json");
        var projectAssetsFile = MsBuildUtilities.TrimAndGetNullForEmpty(ProjectAssetsFile);

        args.Add("exec");

        if (File.Exists(depsFile))
        {
            args.Add("--depsfile");
            args.Add(depsFile);
        }

        if (projectAssetsFile != null
            && File.Exists(projectAssetsFile))
        {
            using var file = File.OpenRead(projectAssetsFile);
            using var reader = JsonDocument.Parse(file);
            var projectAssets = reader.RootElement;
            var packageFolders = projectAssets.GetProperty("packageFolders").EnumerateObject().Select(p => p.Name);

            foreach (var packageFolder in packageFolders)
            {
                args.Add("--additionalprobingpath");
                args.Add(packageFolder.TrimEnd(Path.DirectorySeparatorChar));
            }
        }

        var runtimeFrameworkVersion = MsBuildUtilities.TrimAndGetNullForEmpty(RuntimeFrameworkVersion);
        if (File.Exists(runtimeConfig))
        {
            args.Add("--runtimeconfig");
            args.Add(runtimeConfig);
        }
        else if (runtimeFrameworkVersion != null)
        {
            args.Add("--fx-version");
            args.Add(runtimeFrameworkVersion);
        }

#if NET472
#elif NET10_0
#else
#error Target framework needs to be updated here
#endif
        args.Add(
            Path.Combine(
                Path.GetDirectoryName(typeof(OperationTaskBase).Assembly.Location)!,
                "..",
                "..",
                "tools",
                "net10.0",
                "ef.dll"));

        args.AddRange(AdditionalArguments);
        args.Add("--assembly");
        args.Add(Path.ChangeExtension(Assembly.ItemSpec, ".dll"));

        if (StartupAssembly != null)
        {
            args.Add("--startup-assembly");
            args.Add(Path.ChangeExtension(StartupAssembly.ItemSpec, ".dll"));
        }

        if (DesignAssembly != null)
        {
            args.Add("--design-assembly");
            args.Add(DesignAssembly.ItemSpec);
        }

        if (Project != null)
        {
            args.Add("--project");
            args.Add(Project.ItemSpec);
        }

        if (ProjectDir != null)
        {
            args.Add("--project-dir");
            args.Add(ProjectDir.ItemSpec);
        }

        if (DataDir != null)
        {
            args.Add("--data-dir");
            args.Add(DataDir.ItemSpec);
        }

        var rootNamespace = MsBuildUtilities.TrimAndGetNullForEmpty(RootNamespace);
        if (rootNamespace != null)
        {
            args.Add("--root-namespace");
            args.Add(rootNamespace);
        }

        var language = MsBuildUtilities.TrimAndGetNullForEmpty(Language);
        if (language != null)
        {
            args.Add("--language");
            args.Add(language);
        }

        if (Nullable)
        {
            args.Add("--nullable");
        }

        args.Add("--working-dir");
        args.Add(Directory.GetCurrentDirectory());

        args.Add("--verbose");
        args.Add("--no-color");
        args.Add("--prefix-output");

        return Exe.ToArguments(args);
    }

    protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
    {
        if (singleLine == null)
        {
            return;
        }

        if (singleLine.StartsWith(Reporter.ErrorPrefix, StringComparison.InvariantCulture))
        {
            Log.LogError(singleLine.Substring(Reporter.ErrorPrefix.Length));
        }
        else if (singleLine.StartsWith(Reporter.WarningPrefix, StringComparison.InvariantCulture))
        {
            Log.LogWarning(singleLine.Substring(Reporter.WarningPrefix.Length));
        }
        else if (singleLine.StartsWith(Reporter.InfoPrefix, StringComparison.InvariantCulture))
        {
            Log.LogMessage(singleLine.Substring(Reporter.InfoPrefix.Length));
        }
        else if (singleLine.StartsWith(Reporter.VerbosePrefix, StringComparison.InvariantCulture))
        {
            Log.LogMessage(MessageImportance.Low, singleLine.Substring(Reporter.VerbosePrefix.Length));
        }
        else if (singleLine.StartsWith(Reporter.DataPrefix, StringComparison.InvariantCulture))
        {
            _resultBuilder.AppendLine(singleLine.Substring(Reporter.DataPrefix.Length));
        }
        else if (singleLine.StartsWith("fail: ", StringComparison.InvariantCulture))
        {
            Log.LogError(singleLine.Substring(6));
        }
        else if (singleLine.StartsWith("warn: ", StringComparison.InvariantCulture))
        {
            Log.LogWarning(singleLine.Substring(6));
        }
        else if (singleLine.StartsWith("info: ", StringComparison.InvariantCulture))
        {
            Log.LogMessage(singleLine.Substring(6));
        }
        else if (singleLine.StartsWith("dbug: ", StringComparison.InvariantCulture)
                 || singleLine.StartsWith("trce: ", StringComparison.InvariantCulture))
        {
            Log.LogMessage(MessageImportance.Low, singleLine.Substring(6));
        }
        else
        {
            base.LogEventsFromTextOutput(singleLine, messageImportance);
        }
    }
}
