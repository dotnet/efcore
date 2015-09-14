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
using Microsoft.Dnx.Runtime;
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
            [NotNull] string projectRootNamespace,
            [NotNull] string projectDir,
            [CanBeNull] string relativeOutputDir,
            bool useFluentApiOnly,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotEmpty(runtimeProviderAssemblyName, nameof(runtimeProviderAssemblyName));
            Check.NotEmpty(connectionString, nameof(connectionString));
            Check.NotEmpty(projectRootNamespace, nameof(projectRootNamespace));
            Check.NotEmpty(projectDir, nameof(projectDir));

            var designTimeMetadataProviderFactory =
                GetDesignTimeMetadataProviderFactory(runtimeProviderAssemblyName);
            var serviceCollection = SetupInitialServices();
            designTimeMetadataProviderFactory.AddMetadataProviderServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var generator = serviceProvider.GetRequiredService<ReverseEngineeringGenerator>();
            var configuration = new ReverseEngineeringConfiguration
            {
                ConnectionString = connectionString,
                ProjectPath = projectDir,
                ProjectRootNamespace = projectRootNamespace,
                RelativeOutputPath = relativeOutputDir,
                UseFluentApiOnly = useFluentApiOnly
            };

            return generator.GenerateAsync(configuration, cancellationToken);
        }

        public virtual IDesignTimeMetadataProviderFactory GetDesignTimeMetadataProviderFactory(
            [NotNull] string runtimeProviderAssemblyName)
        {
            Check.NotEmpty(runtimeProviderAssemblyName, nameof(runtimeProviderAssemblyName));

            Assembly runtimeProviderAssembly = null;
            try
            {
                runtimeProviderAssembly = Assembly.Load(new AssemblyName(runtimeProviderAssemblyName));
            }
            catch (Exception exception)
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

            Assembly designTimeProviderAssembly = null;
            try
            {
                designTimeProviderAssembly = Assembly.Load(new AssemblyName(designTimeAssemblyName));
            }
            catch (Exception exception)
            {
                throw new CommandException(
                    Strings.CannotFindDesignTimeProviderAssembly(designTimeAssemblyName), exception);
            }

            var designTimeMetadataProviderFactoryType =
                designTimeProviderAssembly.GetType(designTimeTypeName);
            if (designTimeMetadataProviderFactoryType == null)
            {
                throw new InvalidOperationException(
                    Strings.DesignTimeAssemblyProviderDoesNotContainSpecifiedType(
                        designTimeProviderAssembly.FullName,
                        designTimeTypeName));
            }

            return (IDesignTimeMetadataProviderFactory)Activator
                    .CreateInstance(designTimeMetadataProviderFactoryType);
        }

        private ServiceCollection SetupInitialServices()
        {
            var serviceCollection = new ServiceCollection();
#if DNX451 || DNXCORE50
            var manifest = _serviceProvider.GetRequiredService<IRuntimeServices>();
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
            serviceCollection.AddScoped(typeof(ILogger), sp => logger);
            serviceCollection.AddScoped<IFileService, FileSystemFileService>();

            return serviceCollection;
        }
    }
}
