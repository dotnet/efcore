// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.DotNet.ProjectModel;
using Microsoft.DotNet.ProjectModel.Loader;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Commands
{
    public class OperationsFactory
    {
        private readonly Assembly _assembly;
        private readonly Assembly _startupAssembly;
        private readonly string _environment;
        private readonly string _projectDir;
        private readonly string _rootNamespace;

        public OperationsFactory([CanBeNull] string startupProject, [CanBeNull] string environment)
        {
            var project = Directory.GetCurrentDirectory();
            startupProject = startupProject ?? project;

            var startupProjectContext = ProjectContext.CreateContextForEachFramework(startupProject).First();
            var projectContext = ProjectContext.CreateContextForEachFramework(project).First();
            var startupAssemblyName = new AssemblyName(startupProjectContext.ProjectFile.Name);
            var assemblyName = new AssemblyName(projectContext.ProjectFile.Name);
            var assemblyLoadContext = startupProjectContext.CreateLoadContext();

            _startupAssembly = assemblyLoadContext.LoadFromAssemblyName(startupAssemblyName);

            try
            {
                _assembly = assemblyLoadContext.LoadFromAssemblyName(assemblyName);
            }
            catch (Exception ex)
            {
                throw new OperationException(
                    CommandsStrings.UnreferencedAssembly(
                        projectContext.ProjectFile.Name,
                        startupProjectContext.ProjectFile.Name),
                    ex);
            }
            _environment = environment;
            _projectDir = projectContext.ProjectDirectory;
            _rootNamespace = projectContext.ProjectFile.Name;
        }

        public virtual DatabaseOperations CreateDatabaseOperations()
            => new DatabaseOperations(
                new LoggerProvider(name => new ConsoleCommandLogger(name)),
                _assembly,
                _startupAssembly,
                _environment,
                _projectDir,
                _rootNamespace);

        public virtual DbContextOperations CreateDbContextOperations()
            => new DbContextOperations(
                new LoggerProvider(name => new ConsoleCommandLogger(name)),
                _assembly,
                _startupAssembly,
                _projectDir,
                _environment);

        public virtual MigrationsOperations CreateMigrationsOperations()
            => new MigrationsOperations(
                new LoggerProvider(name => new ConsoleCommandLogger(name)),
                _assembly,
                _startupAssembly,
                _environment,
                _projectDir,
                _rootNamespace);
    }


}
