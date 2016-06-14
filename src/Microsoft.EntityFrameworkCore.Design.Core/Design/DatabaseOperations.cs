// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Internal;
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
            [NotNull] ILoggerProvider loggerProvider,
            [NotNull] AssemblyLoader startupAssemblyLoader,
            [NotNull] Assembly startupAssembly,
            [CanBeNull] string environment,
            [NotNull] string projectDir,
            [NotNull] string contentRootPath,
            [NotNull] string rootNamespace)
        {
            Check.NotNull(startupAssemblyLoader, nameof(startupAssemblyLoader));
            Check.NotNull(loggerProvider, nameof(loggerProvider));
            Check.NotNull(startupAssembly, nameof(startupAssembly));
            Check.NotNull(projectDir, nameof(projectDir));
            Check.NotEmpty(contentRootPath, nameof(contentRootPath));
            Check.NotNull(rootNamespace, nameof(rootNamespace));

            _loggerProvider = loggerProvider;
            _projectDir = projectDir;
            _rootNamespace = rootNamespace;

            var logger = new LazyRef<ILogger>(() => loggerProvider.CreateCommandsLogger());
            var startup = new StartupInvoker(logger, startupAssembly, environment, contentRootPath);
            _servicesBuilder = new DesignTimeServicesBuilder(startupAssemblyLoader, startup);
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
