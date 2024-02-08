// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Tools.Generators;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands;

internal partial class MigrationsBundleCommand
{
    protected override void Validate()
    {
        base.Validate();

        if (!Project!.HasValue())
        {
            throw new CommandException(Resources.MissingOption(Project.LongName));
        }

        if (!StartupProject!.HasValue())
        {
            throw new CommandException(Resources.MissingOption(StartupProject.LongName));
        }

        if (!Framework!.HasValue())
        {
            throw new CommandException(Resources.MissingOption(Framework.LongName));
        }
    }

#if NET472
    protected override int Execute(string[] args)
        => throw new CommandException(Resources.VersionRequired("6.0.0"));
#else
    protected override int Execute(string[] args)
    {
        if (new SemanticVersionComparer().Compare(EFCoreVersion, "6.0.0") < 0)
        {
            throw new CommandException(Resources.VersionRequired("6.0.0"));
        }

        string context;
        using (var executor = CreateExecutor(args))
        {
            context = (string)executor.GetContextInfo(Context!.Value())["Type"];
        }

        Reporter.WriteInformation(Resources.BuildBundleStarted);

        var projectGenerator = new BundleProjectGenerator
        {
            Session = new Dictionary<string, object>
            {
                ["TargetFramework"] = Framework!.Value()!,
                ["EFCoreVersion"] = EFCoreVersion!,
                ["Project"] = Project!.Value()!,
                ["StartupProject"] = StartupProject!.Value()!
            }
        };
        projectGenerator.Initialize();

        var assembly = Path.GetFileNameWithoutExtension(Assembly!.Value()!);
        var startupAssembly = StartupAssembly!.HasValue()
            ? Path.GetFileNameWithoutExtension(StartupAssembly.Value()!)
            : assembly;

        var programGenerator = new BundleProgramGenerator
        {
            Session = new Dictionary<string, object>
            {
                ["ContextType"] = context,
                ["Assembly"] = assembly,
                ["StartupAssembly"] = startupAssembly
            }
        };
        programGenerator.Initialize();

        // TODO: We may not always have access to TEMP
        var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDirectory);
        try
        {
            var directory = tempDirectory;

            var globalJson = default(string);
            var nugetConfigs = new Stack<string>();

            var searchPath = WorkingDir!.Value();
            do
            {
                foreach (var file in Directory.EnumerateFiles(searchPath))
                {
                    var fileName = Path.GetFileName(file);
                    if (fileName.Equals("NuGet.Config", StringComparison.OrdinalIgnoreCase))
                    {
                        nugetConfigs.Push(file);
                    }
                    else if (globalJson == null
                             && fileName.Equals("global.json", StringComparison.OrdinalIgnoreCase))
                    {
                        globalJson = file;
                    }
                }

                searchPath = Path.GetDirectoryName(searchPath);
            }
            while (searchPath != null);

            while (nugetConfigs.Count > 1)
            {
                var nugetConfig = nugetConfigs.Pop();
                File.Copy(nugetConfig, Path.Combine(directory, Path.GetFileName(nugetConfig)));

                directory = Path.Combine(directory, Path.GetRandomFileName());
                Directory.CreateDirectory(directory);
            }

            if (globalJson != null)
            {
                File.Copy(globalJson, Path.Combine(directory, Path.GetFileName(globalJson)));
            }

            if (nugetConfigs.Count > 0)
            {
                var nugetConfig = nugetConfigs.Pop();
                File.Copy(nugetConfig, Path.Combine(directory, Path.GetFileName(nugetConfig)));
            }

            var publishArgs = new List<string> { "publish" };

            var runtime = _runtime!.HasValue()
                ? _runtime!.Value()!
                : (string)AppContext.GetData("RUNTIME_IDENTIFIER");
            publishArgs.Add("--runtime");
            publishArgs.Add(runtime);

            var baseLength = runtime.IndexOfAny(['-', '.', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0']);
            var baseRid = runtime.Substring(0, baseLength);
            var exe = string.Equals(baseRid, "win", StringComparison.OrdinalIgnoreCase)
                ? ".exe"
                : null;

            var outputPath = _output!.HasValue()
                ? _output!.Value()!
                : "efbundle" + exe;
            var bundleName = Path.GetFileNameWithoutExtension(outputPath);

            File.WriteAllText(
                Path.Combine(directory, bundleName + ".csproj"),
                projectGenerator.TransformText());

            File.WriteAllText(
                Path.Combine(directory, "Program.cs"),
                programGenerator.TransformText());

            var publishPath = Path.Combine(directory, "publish");
            publishArgs.Add("--output");
            publishArgs.Add(publishPath);
            Directory.CreateDirectory(publishPath);

            publishArgs.Add(
                _selfContained!.HasValue()
                    ? "--self-contained"
                    : "--no-self-contained");

            var configuration = Configuration!.Value();
            if (string.Equals(configuration, "Debug", StringComparison.OrdinalIgnoreCase)
                || string.Equals(configuration, "Release", StringComparison.OrdinalIgnoreCase))
            {
                publishArgs.Add("--configuration");
                publishArgs.Add(configuration!);
            }

            publishArgs.Add("--disable-build-servers");

            var exitCode = Exe.Run("dotnet", publishArgs, directory, handleOutput: Reporter.WriteVerbose);
            if (exitCode != 0)
            {
                throw new CommandException(Resources.BuildBundleFailed);
            }

            var destination = Path.GetFullPath(Path.Combine(WorkingDir!.Value()!, outputPath));
            if (File.Exists(destination))
            {
                if (!_force!.HasValue())
                {
                    throw new CommandException(Resources.FileExists(destination));
                }

                File.Delete(destination);
            }

            var destinationDir = Path.GetDirectoryName(destination);
            if (!string.IsNullOrWhiteSpace(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            File.Move(
                Path.Combine(publishPath, bundleName + exe),
                destination);

            Reporter.WriteInformation(Resources.BuildBundleSucceeded(destination));

            var startupProjectDir = Path.GetDirectoryName(StartupProject.Value())!;
            if (File.Exists(Path.Combine(startupProjectDir, "appsettings.json")))
            {
                Reporter.WriteWarning(Resources.AppSettingsTip);
            }
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }

        return base.Execute(args);
    }
#endif
}
