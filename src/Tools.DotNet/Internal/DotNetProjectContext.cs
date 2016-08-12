// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using JetBrains.Annotations;
using Microsoft.DotNet.ProjectModel;
using Microsoft.EntityFrameworkCore.Utilities;
using NuGet.Frameworks;

namespace Microsoft.EntityFrameworkCore.Tools.DotNet.Internal
{
    public class DotNetProjectContext : IProjectContext
    {
        private readonly ProjectContext _project;
        private readonly OutputPaths _paths;
        private readonly bool _isExecutable;

        public DotNetProjectContext([NotNull] ProjectContext wrappedProject,
            [NotNull] string configuration,
            [CanBeNull] string outputPath)
        {
            Check.NotNull(wrappedProject, nameof(wrappedProject));
            Check.NotEmpty(configuration, nameof(configuration));

            _project = wrappedProject;
            _paths = wrappedProject.GetOutputPaths(configuration, /* buildBasePath: */ null, outputPath);

            // Workaround https://github.com/dotnet/cli/issues/3164
            _isExecutable = wrappedProject.ProjectFile.GetCompilerOptions(wrappedProject.TargetFramework, configuration).EmitEntryPoint
                            ?? wrappedProject.ProjectFile.GetCompilerOptions(null, configuration).EmitEntryPoint.GetValueOrDefault();

            Configuration = configuration;
        }

        public bool IsClassLibrary => !_isExecutable;

        public NuGetFramework TargetFramework => _project.TargetFramework;
        public string Config => _paths.RuntimeFiles.Config;
        public string DepsJson => _paths.RuntimeFiles.DepsJson;
        public string RuntimeConfigJson => _paths.RuntimeFiles.RuntimeConfigJson;
        public string PackagesDirectory => _project.PackagesDirectory;

        public string AssemblyFullPath =>
            _isExecutable && (_project.IsPortable || TargetFramework.IsDesktop())
                ? _paths.RuntimeFiles.Executable
                : _paths.RuntimeFiles.Assembly;

        public Project Project => _project.ProjectFile;
        public string ProjectFullPath => _project.ProjectFile.ProjectFilePath;
        public string ProjectName => _project.ProjectFile.Name;
        public string RootNamespace => _project.ProjectFile.Name;
        public string TargetDirectory => _paths.RuntimeOutputPath;

        public string Configuration { get; }
    }
}
