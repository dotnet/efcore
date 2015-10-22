// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Design.Internal;
using Microsoft.Data.Entity.Scaffolding;
using Microsoft.Data.Entity.Scaffolding.Internal;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Data.Entity.Design
{
    public class DatabaseOperations
    {
        private readonly ILoggerProvider _loggerProvider;
        private readonly string _projectDir;
        private readonly string _rootNamespace;
        private readonly DesignTimeServicesBuilder _servicesBuilder;

        public DatabaseOperations(
            [NotNull] ILoggerProvider loggerProvider,
            [NotNull] string assemblyName,
            [NotNull] string startupAssemblyName,
            [CanBeNull] string environment,
            [NotNull] string projectDir,
            [NotNull] string rootNamespace)
        {
            Check.NotNull(loggerProvider, nameof(loggerProvider));
            Check.NotEmpty(assemblyName, nameof(assemblyName));
            Check.NotEmpty(startupAssemblyName, nameof(startupAssemblyName));
            Check.NotNull(projectDir, nameof(projectDir));
            Check.NotNull(rootNamespace, nameof(rootNamespace));

            _loggerProvider = loggerProvider;
            _projectDir = projectDir;
            _rootNamespace = rootNamespace;

            var startup = new StartupInvoker(startupAssemblyName, environment);
            _servicesBuilder = new DesignTimeServicesBuilder(startup);
        }

        public virtual Task<ReverseEngineerFiles> ReverseEngineerAsync(
            [NotNull] string provider,
            [NotNull] string connectionString,
            [CanBeNull] string outputDir,
            [CanBeNull] string dbContextClassName,
            [CanBeNull] List<string> schemas,
            [CanBeNull] List<string> tables,
            bool useDataAnnotations,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotEmpty(provider, nameof(provider));
            Check.NotEmpty(connectionString, nameof(connectionString));

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
                UseFluentApiOnly = !useDataAnnotations
            };

            return generator.GenerateAsync(configuration, cancellationToken);
        }
    }
}
