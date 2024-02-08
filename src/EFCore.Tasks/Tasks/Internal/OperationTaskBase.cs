// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore.Tools;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tasks.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public abstract class OperationTaskBase : Build.Utilities.Task
{
    /// <summary>
    ///     The assembly to use.
    /// </summary>
    [Required]
    public ITaskItem Assembly { get; set; } = null!;

    /// <summary>
    ///     The startup assembly to use.
    /// </summary>
    [Required]
    public ITaskItem StartupAssembly { get; set; } = null!;

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
    ///    The project directory.
    /// </summary>
    public ITaskItem? ProjectDir { get; set; }

    /// <summary>
    ///     The root namespace to use.
    /// </summary>
    public string? RootNamespace { get; set; }

    /// <summary>
    ///    The language to use. Defaults to C#.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    ///     A flag indicating whether nullable reference types are enabled.
    /// </summary>
    public bool Nullable { get; set; }

    protected virtual bool Execute(IEnumerable<string> additionalArguments, out string? result)
    {
        var args = new List<string>();

        var startupAssemblyName = Path.GetFileNameWithoutExtension(StartupAssembly.ItemSpec);
        var targetDir = Path.GetDirectoryName(Path.GetFullPath(StartupAssembly.ItemSpec))!;
        var depsFile = Path.Combine(
            targetDir,
            startupAssemblyName + ".deps.json");
        var runtimeConfig = Path.Combine(
            targetDir,
            startupAssemblyName + ".runtimeconfig.json");
        var projectAssetsFile = MsBuildUtilities.TrimAndGetNullForEmpty(ProjectAssetsFile);

        string executable;
        var targetFramework = new FrameworkName(TargetFrameworkMoniker);
        if (targetFramework.Identifier == ".NETCoreApp")
        {
            if (targetFramework.Version < new Version(2, 0))
            {
                throw new InvalidOperationException(
                    Resources.NETCoreApp1StartupProject(startupAssemblyName, targetFramework.Version));
            }

            executable = "dotnet";
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

            args.Add(Path.Combine(
                Path.GetDirectoryName(typeof(OperationTaskBase).Assembly.Location)!,
                "..",
                "..",
                "tools",
                "netcoreapp2.0",
                "ef.dll"));
        }
        else if (targetFramework.Identifier == ".NETStandard")
        {
            throw new InvalidOperationException(Resources.NETStandardStartupProject(startupAssemblyName));
        }
        else
        {
            throw new InvalidOperationException(
                Resources.UnsupportedFramework(startupAssemblyName, targetFramework.Identifier));
        }

        args.AddRange(additionalArguments);
        args.Add("--assembly");
        args.Add(Assembly.ItemSpec);

        if (StartupAssembly != null)
        {
            args.Add("--startup-assembly");
            args.Add(StartupAssembly.ItemSpec);
        }

        if (ProjectDir != null)
        {
            args.Add("--project-dir");
            args.Add(ProjectDir.ItemSpec);
        }

        if (DataDir != null) {
            args.Add("--data-dir");
            args.Add(DataDir.ItemSpec);
        }

        var rootNamespace = MsBuildUtilities.TrimAndGetNullForEmpty(RootNamespace);
        if (rootNamespace != null) {
            args.Add("--root-namespace");
            args.Add(rootNamespace);
        }

        var language = MsBuildUtilities.TrimAndGetNullForEmpty(Language);
        if (language != null) {
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

        var resultBuilder = new StringBuilder();
        var exitCode = Exe.Run(executable, args, ProjectDir?.ItemSpec, HandleOutput, processCommandLine: Log.LogCommandLine);
        result = resultBuilder.Length > 0 ? resultBuilder.ToString() : null;

        return exitCode == 0;

        void HandleOutput(string? output)
        {
            if (output == null)
            {
                return;
            }

            if (output.StartsWith(Reporter.ErrorPrefix, StringComparison.InvariantCulture))
            {
                Log.LogError(output.Substring(Reporter.ErrorPrefix.Length));
            }
            else if (output.StartsWith(Reporter.WarningPrefix, StringComparison.InvariantCulture))
            {
                Log.LogWarning(output.Substring(Reporter.WarningPrefix.Length));
            }
            else if (output.StartsWith(Reporter.InfoPrefix, StringComparison.InvariantCulture))
            {
                Log.LogMessage(output.Substring(Reporter.InfoPrefix.Length));
            }
            else if (output.StartsWith(Reporter.VerbosePrefix, StringComparison.InvariantCulture))
            {
                Log.LogMessage(MessageImportance.Low, output.Substring(Reporter.VerbosePrefix.Length));
            }
            else if (output.StartsWith(Reporter.DataPrefix, StringComparison.InvariantCulture))
            {
                resultBuilder.AppendLine(output.Substring(Reporter.DataPrefix.Length));
            }
            else if(output.StartsWith("fail: ", StringComparison.InvariantCulture))
            {
                Log.LogError(output.Substring(6));
            }
            else if (output.StartsWith("warn: ", StringComparison.InvariantCulture))
            {
                Log.LogWarning(output.Substring(6));
            }
            else if (output.StartsWith("info: ", StringComparison.InvariantCulture))
            {
                Log.LogMessage(output.Substring(6));
            }
            else if (output.StartsWith("dbug: ", StringComparison.InvariantCulture)
                || output.StartsWith("trce: ", StringComparison.InvariantCulture))
            {
                Log.LogMessage(MessageImportance.Low, output.Substring(6));
            }
            else
            {
                Log.LogError(output);
            }
        }
    }
}
