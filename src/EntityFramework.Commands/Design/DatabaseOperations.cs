// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Design.Internal;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
#if DNX451 || DNXCORE50
using Microsoft.Dnx.Runtime;
#endif

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
            [NotNull] Assembly assembly,
            [CanBeNull] string startupAssemblyName,
            [NotNull] string projectDir,
            [NotNull] string rootNamespace,
            [CanBeNull] IServiceProvider dnxServices = null)
        {
            Check.NotNull(loggerProvider, nameof(loggerProvider));
            Check.NotNull(assembly, nameof(assembly));
            Check.NotNull(projectDir, nameof(projectDir));
            Check.NotNull(rootNamespace, nameof(rootNamespace));

            _loggerProvider = loggerProvider;
            _projectDir = projectDir;
            _rootNamespace = rootNamespace;
            _servicesBuilder = new DesignTimeServicesBuilder(dnxServices);
        }

        public virtual Task<ReverseEngineerFiles> ReverseEngineerAsync(
            [NotNull] string provider,
            [NotNull] string connectionString,
            [CanBeNull] string outputDir,
            [CanBeNull] string dbContextClassName,
            bool useFluentApiOnly,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotEmpty(provider, nameof(provider));
            Check.NotEmpty(connectionString, nameof(connectionString));

            var services = _servicesBuilder.Build(provider);

            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            loggerFactory.AddProvider(_loggerProvider);

            var generator = services.GetRequiredService<ReverseEngineeringGenerator>();
            var configuration = new ReverseEngineeringConfiguration
            {
                ConnectionString = connectionString,
                ContextClassName = dbContextClassName,
                ProjectPath = _projectDir,
                ProjectRootNamespace = _rootNamespace,
                OutputPath = outputDir,
                UseFluentApiOnly = useFluentApiOnly
            };

            return generator.GenerateAsync(configuration, cancellationToken);
        }
    }
}
