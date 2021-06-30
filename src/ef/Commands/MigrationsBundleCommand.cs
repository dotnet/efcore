// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Tools.Generators;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands
{
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
        }

#if NET461
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
                    ["ContextType"] = context!,
                    ["Assembly"] = assembly,
                    ["StartupAssembly"] = startupAssembly
                }
            };
            programGenerator.Initialize();

            // TODO: We may not always have access to TEMP
            var directory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(directory);
            try
            {
                File.WriteAllText(
                    Path.Combine(directory, "bundle.csproj"),
                    projectGenerator.TransformText());

                File.WriteAllText(
                    Path.Combine(directory, "Program.cs"),
                    programGenerator.TransformText());

                var outputPath = Path.Combine(directory, "publish");
                Directory.CreateDirectory(outputPath);

                var publishArgs = new List<string> { "publish", "--output", outputPath };

                if (_selfContained!.HasValue())
                {
                    publishArgs.Add("--self-contained");
                }

                if (_runtime!.HasValue())
                {
                    publishArgs.Add("--runtime");
                    publishArgs.Add(_runtime!.Value()!);
                }

                if (_configuration!.HasValue())
                {
                    publishArgs.Add("--configuration");
                    publishArgs.Add(_configuration!.Value()!);
                }

                var exitCode = Exe.Run("dotnet", publishArgs, directory, interceptOutput: true);
                if (exitCode != 0)
                {
                    throw new CommandException(Resources.BuildBundleFailed);
                }

                var exe = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? ".exe"
                    : string.Empty;

                File.Move(
                    Path.Combine(outputPath, "bundle" + exe),
                    Path.Combine(WorkingDir!.Value()!, "bundle" + exe));

                Reporter.WriteInformation(Resources.BuildBundleSucceeded);

                var startupProjectDir = Path.GetDirectoryName(StartupProject.Value())!;
                if (File.Exists(Path.Combine(startupProjectDir, "appsettings.json")))
                {
                    Reporter.WriteWarning(Resources.AppSettingsTip);
                }
            }
            finally
            {
                Directory.Delete(directory, recursive: true);
            }

            return base.Execute(args);
        }
#endif
    }
}
