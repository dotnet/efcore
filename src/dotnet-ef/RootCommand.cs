// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using System.Runtime.Versioning;
using System.Text.Json;
using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.EntityFrameworkCore.Tools.Commands;
using Microsoft.EntityFrameworkCore.Tools.Properties;
using EFCommand = Microsoft.EntityFrameworkCore.Tools.Commands.RootCommand;

namespace Microsoft.EntityFrameworkCore.Tools;

internal class RootCommand : CommandBase
{
    private CommandLineApplication? _command;
    private CommandOption? _project;
    private CommandOption? _startupProject;
    private CommandOption? _framework;
    private CommandOption? _configuration;
    private CommandOption? _runtime;
    private CommandOption? _msbuildprojectextensionspath;
    private CommandOption? _noBuild;
    private CommandOption? _help;
    private IList<string>? _args;
    private IList<string>? _applicationArgs;

    public override void Configure(CommandLineApplication command)
    {
        command.FullName = Resources.DotnetEfFullName;
        command.AllowArgumentSeparator = true;

        var options = new ProjectOptions();
        options.Configure(command);

        _project = options.Project;
        _startupProject = options.StartupProject;
        _framework = options.Framework;
        _configuration = options.Configuration;
        _runtime = options.Runtime;
        _msbuildprojectextensionspath = options.MSBuildProjectExtensionsPath;
        _noBuild = options.NoBuild;

        command.VersionOption("--version", GetVersion);
        _help = command.Option("-h|--help", description: null);

        _args = command.RemainingArguments;
        _applicationArgs = command.ApplicationArguments;

        base.Configure(command);

        _command = command;
    }

    protected override int Execute(string[] _)
    {
        var commands = _args!.TakeWhile(a => a[0] != '-').ToList();
        if (_help!.HasValue()
            || ShouldHelp(commands))
        {
            return ShowHelp(_help.HasValue(), commands);
        }

        var (projectFile, startupProjectFile) = ResolveProjects(
            _project!.Value(),
            _startupProject!.Value());

        Reporter.WriteVerbose(Resources.UsingProject(projectFile));
        Reporter.WriteVerbose(Resources.UsingStartupProject(startupProjectFile));

        var project = Project.FromFile(projectFile, _msbuildprojectextensionspath!.Value());
        var startupProject = Project.FromFile(
            startupProjectFile,
            _msbuildprojectextensionspath.Value(),
            _framework!.Value(),
            _configuration!.Value(),
            _runtime!.Value());

        if (!_noBuild!.HasValue())
        {
            Reporter.WriteInformation(Resources.BuildStarted);
            startupProject.Build();
            Reporter.WriteInformation(Resources.BuildSucceeded);
        }

        string executable;
        var args = new List<string>();

        var toolsPath = Path.Combine(
            Path.GetDirectoryName(typeof(Program).Assembly.Location)!,
            "tools");

        var targetDir = Path.GetFullPath(Path.Combine(startupProject.ProjectDir!, startupProject.OutputPath!));
        var targetPath = Path.Combine(targetDir, project.TargetFileName!);
        var startupTargetPath = Path.Combine(targetDir, startupProject.TargetFileName!);
        var depsFile = Path.Combine(
            targetDir,
            startupProject.AssemblyName + ".deps.json");
        var runtimeConfig = Path.Combine(
            targetDir,
            startupProject.AssemblyName + ".runtimeconfig.json");
        var projectAssetsFile = startupProject.ProjectAssetsFile;

        var targetFramework = new FrameworkName(startupProject.TargetFrameworkMoniker!);
        if (targetFramework.Identifier == ".NETFramework")
        {
            executable = Path.Combine(
                toolsPath,
                "net472",
                startupProject.PlatformTarget == "x86"
                    ? "win-x86"
                    : "any",
                "ef.exe");
        }
        else if (targetFramework.Identifier == ".NETCoreApp")
        {
            if (targetFramework.Version < new Version(2, 0))
            {
                throw new CommandException(
                    Resources.NETCoreApp1StartupProject(startupProject.ProjectName, targetFramework.Version));
            }

            var targetPlatformIdentifier = startupProject.TargetPlatformIdentifier!;
            if (targetPlatformIdentifier.Length != 0
                && !string.Equals(targetPlatformIdentifier, "Windows", StringComparison.OrdinalIgnoreCase))
            {
                executable = Path.Combine(
                    toolsPath,
                    "net472",
                    startupProject.PlatformTarget switch
                    {
                        "x86" => "win-x86",
                        "ARM64" => "win-arm64",
                        _ => "any"
                    },
                    "ef.exe");
            }

            executable = "dotnet";
            args.Add("exec");
            args.Add("--depsfile");
            args.Add(depsFile);

            if (!string.IsNullOrEmpty(projectAssetsFile))
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

            if (File.Exists(runtimeConfig))
            {
                args.Add("--runtimeconfig");
                args.Add(runtimeConfig);
            }
            else if (startupProject.RuntimeFrameworkVersion!.Length != 0)
            {
                args.Add("--fx-version");
                args.Add(startupProject.RuntimeFrameworkVersion);
            }

            args.Add(Path.Combine(toolsPath, "netcoreapp2.0", "any", "ef.dll"));
        }
        else if (targetFramework.Identifier == ".NETStandard")
        {
            throw new CommandException(Resources.NETStandardStartupProject(startupProject.ProjectName));
        }
        else
        {
            throw new CommandException(
                Resources.UnsupportedFramework(startupProject.ProjectName, targetFramework.Identifier));
        }

        args.AddRange(_args!);
        args.Add("--assembly");
        args.Add(targetPath);
        args.Add("--project");
        args.Add(projectFile);
        args.Add("--startup-assembly");
        args.Add(startupTargetPath);
        args.Add("--startup-project");
        args.Add(startupProjectFile);
        args.Add("--project-dir");
        args.Add(project.ProjectDir!);
        args.Add("--root-namespace");
        args.Add(project.RootNamespace!);
        args.Add("--language");
        args.Add(project.Language!);
        args.Add("--framework");
        args.Add(startupProject.TargetFramework!);

        if (_configuration.HasValue())
        {
            args.Add("--configuration");
            args.Add(_configuration.Value()!);
        }

        if (string.Equals(project.Nullable, "enable", StringComparison.OrdinalIgnoreCase)
            || string.Equals(project.Nullable, "annotations", StringComparison.OrdinalIgnoreCase))
        {
            args.Add("--nullable");
        }

        args.Add("--working-dir");
        args.Add(Directory.GetCurrentDirectory());

        if (Reporter.IsVerbose)
        {
            args.Add("--verbose");
        }

        if (Reporter.NoColor)
        {
            args.Add("--no-color");
        }

        if (Reporter.PrefixOutput)
        {
            args.Add("--prefix-output");
        }

        if (_applicationArgs!.Any())
        {
            args.Add("--");
            args.AddRange(_applicationArgs!);
        }

        return Exe.Run(executable, args, startupProject.ProjectDir);
    }

    private static (string, string) ResolveProjects(
        string? projectPath,
        string? startupProjectPath)
    {
        var projects = ResolveProjects(projectPath);
        var startupProjects = ResolveProjects(startupProjectPath);

        if (projects.Count > 1)
        {
            throw new CommandException(
                projectPath != null
                    ? Resources.MultipleProjectsInDirectory(projectPath)
                    : Resources.MultipleProjects);
        }

        if (startupProjects.Count > 1)
        {
            throw new CommandException(
                startupProjectPath != null
                    ? Resources.MultipleProjectsInDirectory(startupProjectPath)
                    : Resources.MultipleStartupProjects);
        }

        if (projectPath != null
            && projects.Count == 0)
        {
            throw new CommandException(Resources.NoProjectInDirectory(projectPath));
        }

        if (startupProjectPath != null
            && startupProjects.Count == 0)
        {
            throw new CommandException(Resources.NoProjectInDirectory(startupProjectPath));
        }

        if (projectPath == null
            && startupProjectPath == null)
        {
            return projects.Count == 0
                ? throw new CommandException(Resources.NoProject)
                : (projects[0], startupProjects[0]);
        }

        if (projects.Count == 0)
        {
            return (startupProjects[0], startupProjects[0]);
        }

        if (startupProjects.Count == 0)
        {
            return (projects[0], projects[0]);
        }

        return (projects[0], startupProjects[0]);
    }

    private static List<string> ResolveProjects(string? path)
    {
        if (path == null)
        {
            path = Directory.GetCurrentDirectory();
        }
        else
        {
            path = Path.GetFullPath(path);

            if (!Directory.Exists(path)) // It's not a directory
            {
                return [path];
            }
        }

        var projectFiles = Directory.EnumerateFiles(path, "*.*proj", SearchOption.TopDirectoryOnly)
            .Where(f => !string.Equals(Path.GetExtension(f), ".xproj", StringComparison.OrdinalIgnoreCase))
            .Take(2).ToList();

        return projectFiles;
    }

    private static string GetVersion()
        => typeof(RootCommand).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
            .InformationalVersion;

    private static bool ShouldHelp(IReadOnlyList<string> commands)
        => commands.Count == 0
            || (commands.Count == 1
                && (commands[0] == "database"
                    || commands[0] == "dbcontext"
                    || commands[0] == "migrations"));

    private int ShowHelp(bool help, IEnumerable<string> commands)
    {
        var app = new CommandLineApplication { Name = _command!.Name };

        new EFCommand().Configure(app);

        app.FullName = _command.FullName;

        var args = new List<string>(commands);
        if (help)
        {
            args.Add("--help");
        }

        return app.Execute(args.ToArray());
    }
}
