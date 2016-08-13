// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.ProjectModel;
using Microsoft.DotNet.ProjectModel.Resolution;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Tools.DotNet.FunctionalTests.Utilities
{
    public abstract class DotnetEf : Command
    {
        private readonly string[] _startupArgs;
        private readonly string _nugetPackageDir;
        private readonly string _runtimeConfig;
        private readonly string _depsJson;

        protected DotnetEf(string targetProject, ITestOutputHelper output, string startupProject = null)
            : base(new Muxer().MuxerPath, output)
        {
            _startupArgs = startupProject != null
                ? new[] { "--startup-project", startupProject }
                : Array.Empty<string>();

            // always executes on startup project
            WorkingDirectory = Path.GetDirectoryName(targetProject);

            var rootDirectory = ProjectRootResolver.ResolveRootDirectory(WorkingDirectory);
            GlobalSettings globalSettings;
            GlobalSettings.TryGetGlobalSettings(rootDirectory, out globalSettings);

            rootDirectory = globalSettings?.DirectoryPath ?? rootDirectory;
            _nugetPackageDir = PackageDependencyProvider.ResolvePackagesPath(rootDirectory, globalSettings);
            var appName = GetType().GetTypeInfo().Assembly.GetName().Name;
            _runtimeConfig = Path.Combine(AppContext.BaseDirectory, appName + FileNameSuffixes.RuntimeConfigJson);
            _depsJson = Path.Combine(AppContext.BaseDirectory, appName + FileNameSuffixes.DepsJson);
        }

        private static readonly string s_dotnetEfPath = Path.Combine(AppContext.BaseDirectory, "dotnet-ef.dll");

        private IEnumerable<string> BuildArgsImpl(string[] args)
            => new List<string>
            {
                "exec",
                "--runtimeconfig", _runtimeConfig,
                "--depsfile", _depsJson,
                "--additionalprobingpath", _nugetPackageDir,
                s_dotnetEfPath,
                "--verbose"
            }
                .Concat(_startupArgs)
                .Concat(BuildArgs())
                .Concat(args);

        public override CommandResult Execute(params string[] args)
            => base.Execute(BuildArgsImpl(args).ToArray());

        public override CommandResult ExecuteWithCapturedOutput(params string[] args)
            => base.ExecuteWithCapturedOutput(BuildArgsImpl(args).ToArray());

        protected abstract IEnumerable<string> BuildArgs();
    }
}
