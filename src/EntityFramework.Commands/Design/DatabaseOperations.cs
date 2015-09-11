// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Design.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Storage;
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
        private readonly IServiceProvider _dnxServices;

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
            _dnxServices = dnxServices;
        }

        public virtual Task<ReverseEngineerFiles> ReverseEngineerAsync(
            [NotNull] string provider,
            [NotNull] string connectionString,
            [CanBeNull] string outputDir,
            bool useFluentApiOnly,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotEmpty(provider, nameof(provider));
            Check.NotEmpty(connectionString, nameof(connectionString));

            var designTimeMetadataProviderFactory =
                GetDesignTimeMetadataProviderFactory(provider);
            var serviceCollection = SetupInitialServices();
            designTimeMetadataProviderFactory.AddMetadataProviderServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var generator = serviceProvider.GetRequiredService<ReverseEngineeringGenerator>();
            var configuration = new ReverseEngineeringConfiguration
            {
                ConnectionString = connectionString,
                ProjectPath = _projectDir,
                ProjectRootNamespace = _rootNamespace,
                RelativeOutputPath = outputDir,
                UseFluentApiOnly = useFluentApiOnly
            };

            return generator.GenerateAsync(configuration, cancellationToken);
        }

        private IDesignTimeMetadataProviderFactory GetDesignTimeMetadataProviderFactory(
            string runtimeProviderAssemblyName)
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

            var designTimeServicesTypeAttribute = (DesignTimeProviderServicesAttribute)runtimeProviderAssembly
                .GetCustomAttribute(typeof(DesignTimeProviderServicesAttribute));
            if (designTimeServicesTypeAttribute == null)
            {
                throw new InvalidOperationException(
                    Strings.CannotFindDesignTimeProviderAssemblyAttribute(
                        nameof(DesignTimeProviderServicesAttribute), runtimeProviderAssemblyName));
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
                throw new OperationException(
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
            var manifest = _dnxServices.GetRequiredService<IRuntimeServices>();
            if (manifest != null)
            {
                foreach (var service in manifest.Services)
                {
                    serviceCollection.AddTransient(
                        service, sp => _dnxServices.GetService(service));
                }
            }
#endif

            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(_loggerProvider);
            var logger = loggerFactory.CreateLogger<DatabaseOperations>();
            serviceCollection.AddScoped(typeof(ILogger), sp => logger);
            serviceCollection.AddScoped<IFileService, FileSystemFileService>();

            return serviceCollection;
        }
    }
}
