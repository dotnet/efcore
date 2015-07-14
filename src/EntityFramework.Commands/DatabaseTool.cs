// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Commands.Utilities;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
#if DNX451 || DNXCORE50
using Microsoft.Framework.Runtime;
#endif

namespace Microsoft.Data.Entity.Commands
{
    public class DatabaseTool
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly ILoggerProvider _loggerProvider;

        public DatabaseTool(
            [CanBeNull] IServiceProvider serviceProvider,
            [NotNull] ILoggerProvider loggerProvider)
        {
            Check.NotNull(loggerProvider, nameof(loggerProvider));

            _serviceProvider = new ServiceProvider(serviceProvider);
            _loggerProvider = loggerProvider;
        }

        public virtual Task<IReadOnlyList<string>> ReverseEngineerAsync(
            [NotNull] string runtimeProviderAssemblyName,
            [NotNull] string connectionString,
            [NotNull] string rootNamespace,
            [NotNull] string projectDir,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(runtimeProviderAssemblyName, nameof(runtimeProviderAssemblyName));
            Check.NotEmpty(connectionString, nameof(connectionString));
            Check.NotEmpty(rootNamespace, nameof(rootNamespace));
            Check.NotEmpty(projectDir, nameof(projectDir));

            Assembly runtimeProviderAssembly = null;
            try
            {
                runtimeProviderAssembly = Assembly.Load(new AssemblyName(runtimeProviderAssemblyName));
            }
            catch(Exception exception)
            {
                throw new InvalidOperationException(
                    Strings.CannotFindRuntimeProviderAssembly(runtimeProviderAssemblyName), exception);
            }

            var designTimeServicesTypeAttribute = (ProviderDesignTimeServicesAttribute)runtimeProviderAssembly
                .GetCustomAttribute(typeof(ProviderDesignTimeServicesAttribute));
            if (designTimeServicesTypeAttribute == null)
            {
                throw new InvalidOperationException(
                    Strings.CannotFindDesignTimeProviderAssemblyAttribute(
                        nameof(ProviderDesignTimeServicesAttribute), runtimeProviderAssemblyName));
            }

            var designTimeTypeName = designTimeServicesTypeAttribute.TypeName;
            var designTimeAssemblyName =
                designTimeServicesTypeAttribute.AssemblyName ?? runtimeProviderAssemblyName;

            var designTimeMetadataProviderFactory =
                GetDesignTimeMetadataProviderFactory(designTimeTypeName, designTimeAssemblyName);
            var serviceCollection = SetupInitialServices();
            designTimeMetadataProviderFactory.AddMetadataProviderServices(serviceCollection);
            designTimeMetadataProviderFactory.Create(serviceCollection);
            var designTimeProvider = designTimeMetadataProviderFactory.Create(serviceCollection);

            var configuration = new ReverseEngineeringConfiguration
            {
                Provider = designTimeProvider,
                ConnectionString = connectionString,
                Namespace = rootNamespace,
                OutputPath = projectDir
            };

            var generator = serviceCollection.BuildServiceProvider().GetRequiredService<ReverseEngineeringGenerator>();
            return generator.GenerateAsync(configuration, cancellationToken);
        }

        public virtual IDesignTimeMetadataProviderFactory GetDesignTimeMetadataProviderFactory(
            [NotNull] string providerTypeFullName, [NotNull] string providerAssemblyName)
        {
            Check.NotNull(providerTypeFullName, nameof(providerTypeFullName));
            Check.NotNull(providerAssemblyName, nameof(providerAssemblyName));

            Assembly designTimeProviderAssembly = null;
            try
            {
                designTimeProviderAssembly = Assembly.Load(new AssemblyName(providerAssemblyName));
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    Strings.CannotFindDesignTimeProviderAssembly(providerAssemblyName), exception);
            }

            var designTimeMetadataProviderFactoryType =
                designTimeProviderAssembly.GetType(providerTypeFullName);
            if (designTimeMetadataProviderFactoryType == null)
            {
                throw new InvalidOperationException(
                    Strings.DesignTimeAssemblyProviderDoesNotContainSpecifiedType(
                        designTimeProviderAssembly.FullName,
                        providerTypeFullName));
            }

            return (IDesignTimeMetadataProviderFactory)Activator
                    .CreateInstance(designTimeMetadataProviderFactoryType);
        }

        private ServiceCollection SetupInitialServices()
        {
            var serviceCollection = new ServiceCollection();
#if DNX451 || DNXCORE50
            var manifest = _serviceProvider.GetRequiredService<IServiceManifest>();
            if (manifest != null)
            {
                foreach (var service in manifest.Services)
                {
                    serviceCollection.AddTransient(
                        service, sp => _serviceProvider.GetService(service));
                }
            }
#endif

            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(_loggerProvider);
            var logger = loggerFactory.CreateLogger<DatabaseTool>();
            serviceCollection.AddTransient(typeof(ILogger), sp => logger);
            serviceCollection.AddTransient<IFileService, FileSystemFileService>();

            return serviceCollection;
        }
    }
}
