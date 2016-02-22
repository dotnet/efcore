// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public partial class DesignTimeServicesBuilder
    {
        private readonly StartupInvoker _startup;

        public DesignTimeServicesBuilder(
            [NotNull] StartupInvoker startupInvoker)
        {
            _startup = startupInvoker;
        }

        public virtual IServiceProvider Build([NotNull] DbContext context)
        {
            Check.NotNull(context, nameof(context));

            var services = new ServiceCollection();
            ConfigureServices(services);

            var contextServices = ((IInfrastructure<IServiceProvider>)context).Instance;
            ConfigureContextServices(contextServices, services);

            var databaseProviderServices = contextServices.GetRequiredService<IDatabaseProviderServices>();
            var provider = databaseProviderServices.InvariantName;
            ConfigureProviderServices(provider, services);

            ConfigureUserServices(services);

            return services.BuildServiceProvider();
        }

        public virtual IServiceProvider Build([NotNull] string provider)
        {
            Check.NotEmpty(provider, nameof(provider));

            var services = new ServiceCollection();
            ConfigureServices(services);
            ConfigureProviderServices(provider, services, throwOnError: true);
            ConfigureUserServices(services);

            return services.BuildServiceProvider();
        }

        protected virtual void ConfigureServices([NotNull] IServiceCollection services)
            => services
                .AddLogging()
                .AddSingleton<CSharpHelper>()
                .AddSingleton<CSharpMigrationOperationGenerator>()
                .AddSingleton<CSharpSnapshotGenerator>()
                .AddSingleton<MigrationsCodeGenerator, CSharpMigrationsGenerator>()
                .AddScaffolding();

        private void ConfigureProviderServices(string provider, IServiceCollection services, bool throwOnError = false)
            => _startup.ConfigureDesignTimeServices(GetProviderDesignTimeServices(provider, throwOnError), services);

        protected virtual void ConfigureContextServices(
            [NotNull] IServiceProvider contextServices,
            [NotNull] IServiceCollection services)
            => services
                .AddTransient<MigrationsScaffolder>()
                .AddTransient(_ => contextServices.GetService<DbContext>())
                .AddTransient(_ => contextServices.GetService<IDatabaseProviderServices>())
                .AddTransient(_ => contextServices.GetService<IDbContextOptions>())
                .AddTransient(_ => contextServices.GetService<IHistoryRepository>())
                .AddTransient(_ => contextServices.GetService<ILoggerFactory>())
                .AddTransient(_ => contextServices.GetService<IMigrationsAssembly>())
                .AddTransient(_ => contextServices.GetService<IMigrationsIdGenerator>())
                .AddTransient(_ => contextServices.GetService<IMigrationsModelDiffer>())
                .AddTransient(_ => contextServices.GetService<IMigrator>())
                .AddTransient(_ => contextServices.GetService<IModel>());

        private void ConfigureUserServices(IServiceCollection services)
            => _startup.ConfigureDesignTimeServices(services);

        private static Type GetProviderDesignTimeServices(string provider, bool throwOnError)
        {
            Assembly providerAssembly;
            try
            {
                providerAssembly = Assembly.Load(new AssemblyName(provider));
            }
            catch (Exception ex)
            {
                if (!throwOnError)
                {
                    return null;
                }

                throw new OperationException(CommandsStrings.CannotFindRuntimeProviderAssembly(provider), ex);
            }

            var providerServicesAttribute = providerAssembly.GetCustomAttribute<DesignTimeProviderServicesAttribute>();
            if (providerServicesAttribute == null)
            {
                if (!throwOnError)
                {
                    return null;
                }

                throw new InvalidOperationException(
                    CommandsStrings.CannotFindDesignTimeProviderAssemblyAttribute(
                        nameof(DesignTimeProviderServicesAttribute),
                        provider));
            }

            try
            {
                return Type.GetType(
                    providerServicesAttribute.FullyQualifiedTypeName,
                    throwOnError: true,
                    ignoreCase: false);
            }
            catch (Exception ex)
            when (ex is FileNotFoundException || ex is FileLoadException || ex is BadImageFormatException)
            {
                if (!throwOnError)
                {
                    return null;
                }

                throw new OperationException(
                    CommandsStrings.CannotFindDesignTimeProviderAssembly(providerServicesAttribute.PackageName), ex);
            }
        }
    }
}
