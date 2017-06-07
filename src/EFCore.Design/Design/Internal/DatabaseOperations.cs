// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public class DatabaseOperations
    {
        private readonly IOperationReporter _reporter;
        private readonly string _projectDir;
        private readonly string _rootNamespace;
        private readonly DesignTimeServicesBuilder _servicesBuilder;

        public DatabaseOperations(
            [NotNull] IOperationReporter reporter,
            [NotNull] Assembly startupAssembly,
            [NotNull] string projectDir,
            [NotNull] string rootNamespace)
        {
            Check.NotNull(reporter, nameof(reporter));
            Check.NotNull(startupAssembly, nameof(startupAssembly));
            Check.NotNull(projectDir, nameof(projectDir));
            Check.NotNull(rootNamespace, nameof(rootNamespace));

            _reporter = reporter;
            _projectDir = projectDir;
            _rootNamespace = rootNamespace;

            _servicesBuilder = new DesignTimeServicesBuilder(startupAssembly, reporter);
        }

        public virtual ReverseEngineerFiles ScaffoldContext(
            [NotNull] string provider,
            [NotNull] string connectionString,
            [CanBeNull] string outputDir,
            [CanBeNull] string dbContextClassName,
            [NotNull] IEnumerable<string> schemas,
            [NotNull] IEnumerable<string> tables,
            bool useDataAnnotations,
            bool overwriteFiles)
        {
            Check.NotEmpty(provider, nameof(provider));
            Check.NotEmpty(connectionString, nameof(connectionString));
            Check.NotNull(schemas, nameof(schemas));
            Check.NotNull(tables, nameof(tables));

            var services = _servicesBuilder.Build(provider);

            var loggerFactory = services.GetService<ILoggerFactory>();
#pragma warning disable CS0618 // Type or member is obsolete
            loggerFactory.AddProvider(new LoggerProvider(categoryName => new OperationLogger(categoryName, _reporter)));
#pragma warning restore CS0618 // Type or member is obsolete

            var generator = services.GetRequiredService<IModelScaffolder>();

            return generator.Generate(
                connectionString,
                new TableSelectionSet(tables, schemas),
                _projectDir,
                outputDir,
                _rootNamespace,
                dbContextClassName,
                useDataAnnotations,
                overwriteFiles);
        }
    }
}
