// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Design
{
    public class DatabaseOperations
    {
        private readonly ILoggerProvider _loggerProvider;
        private readonly string _projectDir;
        private readonly string _rootNamespace;
        private readonly DesignTimeServicesBuilder _servicesBuilder;

        public DatabaseOperations(
            [NotNull] AssemblyLoader assemblyLoader,
            [NotNull] ILoggerProvider loggerProvider,
            [NotNull] Assembly startupAssembly,
            [CanBeNull] string environment,
            [NotNull] string projectDir,
            [NotNull] string startupProjectDir,
            [NotNull] string rootNamespace)
        {
            Check.NotNull(assemblyLoader, nameof(assemblyLoader));
            Check.NotNull(loggerProvider, nameof(loggerProvider));
            Check.NotNull(startupAssembly, nameof(startupAssembly));
            Check.NotNull(projectDir, nameof(projectDir));
            Check.NotEmpty(startupProjectDir, nameof(startupProjectDir));
            Check.NotNull(rootNamespace, nameof(rootNamespace));

            _loggerProvider = loggerProvider;
            _projectDir = projectDir;
            _rootNamespace = rootNamespace;

            var startup = new StartupInvoker(startupAssembly, environment, startupProjectDir);
            _servicesBuilder = new DesignTimeServicesBuilder(assemblyLoader, startup);
        }

        public virtual Task<ReverseEngineerFiles> ReverseEngineerAsync(
            [NotNull] string provider,
            [NotNull] string connectionString,
            [CanBeNull] string outputDir,
            [CanBeNull] string dbContextClassName,
            [NotNull] IEnumerable<string> schemas,
            [NotNull] IEnumerable<string> tables,
            bool useDataAnnotations,
            bool overwriteFiles,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotEmpty(provider, nameof(provider));
            Check.NotEmpty(connectionString, nameof(connectionString));
            Check.NotNull(schemas, nameof(schemas));
            Check.NotNull(tables, nameof(tables));

            var services = _servicesBuilder.Build(provider);

            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            loggerFactory.AddProvider(_loggerProvider);

            var generator = services.GetRequiredService<ReverseEngineeringGenerator>();
            var tableSelectionSet = new TableSelectionSet(tables, schemas);
            var configuration = new ReverseEngineeringConfiguration
            {
                ConnectionString = connectionString,
                ContextClassName = dbContextClassName,
                ProjectPath = _projectDir,
                ProjectRootNamespace = _rootNamespace,
                OutputPath = outputDir,
                TableSelectionSet = tableSelectionSet,
                UseFluentApiOnly = !useDataAnnotations,
                OverwriteFiles = overwriteFiles
            };

            return generator.GenerateAsync(configuration, cancellationToken);
        }
    }
}
