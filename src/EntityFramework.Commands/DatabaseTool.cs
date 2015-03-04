// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Commands.Utilities;
using Microsoft.Data.Entity.Relational.Design.CodeGeneration;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Relational.Design.Utilities;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Commands
{
    public class DatabaseTool
    {
        private readonly ServiceProvider _serviceProvider;

        public DatabaseTool(
            [CanBeNull] IServiceProvider serviceProvider,
            [NotNull] ILoggerProvider loggerProvider)
        {
            Check.NotNull(loggerProvider, nameof(loggerProvider));

            _serviceProvider = new ServiceProvider(serviceProvider);
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(loggerProvider);
            var logger = new LazyRef<ILogger>(() => loggerFactory.CreateLogger<DatabaseTool>());
            _serviceProvider.AddService(typeof(ILogger), logger.Value);
            _serviceProvider.AddService(typeof(CSharpCodeGeneratorHelper), new CSharpCodeGeneratorHelper());
            _serviceProvider.AddService(typeof(ModelUtilities), new ModelUtilities());
        }

        public virtual void ReverseEngineer(
            [NotNull] Assembly providerAssembly,
            [NotNull] string connectionString,
            [NotNull] string rootNamespace,
            [NotNull] string projectDir)
        {
            Check.NotNull(providerAssembly, nameof(providerAssembly));
            Check.NotEmpty(connectionString, nameof(connectionString));
            Check.NotEmpty(rootNamespace, nameof(rootNamespace));
            Check.NotEmpty(projectDir, nameof(projectDir));

            var configuration = new ReverseEngineeringConfiguration()
            {
                ProviderAssembly = providerAssembly,
                ConnectionString = connectionString,
                Namespace = rootNamespace,
                OutputPath = projectDir
            };

            var generator = new ReverseEngineeringGenerator(_serviceProvider);
            generator.GenerateAsync(configuration).Wait();
        }
    }
}
