// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Design;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;

#if DNX451 || DNXCORE50
using Microsoft.Dnx.Runtime;
#endif

namespace Microsoft.Data.Entity.Design.Internal
{
    // TODO: Allow design-time services to be overridden by users
    public partial class DesignTimeServicesBuilder
    {
        private readonly IServiceProvider _dnxServices;

        public DesignTimeServicesBuilder([CanBeNull] IServiceProvider dnxServices)
        {
            _dnxServices = dnxServices;
        }

        public virtual IServiceProvider Build([NotNull] DbContext context)
        {
            Check.NotNull(context, nameof(context));

            var services = new ServiceCollection();
            ConfigureServices(services);
            ConfigureDnxServices(services);

            var contextServices = ((IAccessor<IServiceProvider>)context).Service;
            ConfigureContextServices(contextServices, services);

            // TODO: Add design-time provider services
            return services.BuildServiceProvider();
        }

        public virtual IServiceProvider Build([NotNull] string provider)
        {
            Check.NotEmpty(provider, nameof(provider));

            var services = new ServiceCollection();
            ConfigureServices(services);
            ConfigureDnxServices(services);
            ConfigureProviderServices(provider, services);

            return services.BuildServiceProvider();
        }

        protected virtual void ConfigureServices([NotNull] IServiceCollection services)
        {
            services
                .AddLogging()
                .AddSingleton<CSharpHelper>()
                .AddSingleton<CSharpMigrationOperationGenerator>()
                .AddSingleton<CSharpSnapshotGenerator>()
                .AddSingleton<MigrationsCodeGenerator, CSharpMigrationsGenerator>();
        }

        partial void ConfigureDnxServices(IServiceCollection services);

#if DNX451 || DNXCORE50
        partial void ConfigureDnxServices(IServiceCollection services)
        {
            if (_dnxServices == null)
            {
                return;
            }

            var runtimeServices = _dnxServices.GetRequiredService<IRuntimeServices>();
            foreach (var service in runtimeServices.Services)
            {
                services.AddTransient(service, _ => _dnxServices.GetService(service));
            }
        }
#endif

        private static void ConfigureProviderServices(string provider, IServiceCollection services)
            => GetProviderDesignTimeServicesBuilder(provider).AddMetadataProviderServices(services);

        protected virtual void ConfigureContextServices(
            [NotNull] IServiceProvider contextServices,
            [NotNull] IServiceCollection services)
            => services
                .AddTransient<MigrationsScaffolder>()
                .AddTransient(_ => contextServices.GetService<DbContext>())
                .AddTransient(_ => contextServices.GetService<IDatabaseProviderServices>())
                .AddTransient(_ => contextServices.GetService<IHistoryRepository>())
                .AddTransient(_ => contextServices.GetService<ILoggerFactory>())
                .AddTransient(_ => contextServices.GetService<IMigrationsAssembly>())
                .AddTransient(_ => contextServices.GetService<IMigrationsIdGenerator>())
                .AddTransient(_ => contextServices.GetService<IMigrationsModelDiffer>())
                .AddTransient(_ => contextServices.GetService<IMigrator>())
                .AddTransient(_ => contextServices.GetService<IModel>());

        private static IDesignTimeMetadataProviderFactory GetProviderDesignTimeServicesBuilder(string provider)
        {
            Assembly providerAssembly;
            try
            {
                providerAssembly = Assembly.Load(new AssemblyName(provider));
            }
            catch (Exception ex)
            {
                throw new OperationException(Strings.CannotFindRuntimeProviderAssembly(provider), ex);
            }

            var providerServicesAttribute = providerAssembly.GetCustomAttribute<DesignTimeProviderServicesAttribute>();
            if (providerServicesAttribute == null)
            {
                throw new InvalidOperationException(
                    Strings.CannotFindDesignTimeProviderAssemblyAttribute(
                        nameof(DesignTimeProviderServicesAttribute),
                        provider));
            }

            var providerServicesAssemblyName = providerServicesAttribute.AssemblyName;
            Assembly providerServicesAssembly;
            if (providerServicesAssemblyName != null)
            {
                try
                {
                    providerServicesAssembly = Assembly.Load(new AssemblyName(providerServicesAssemblyName));
                }
                catch (Exception ex)
                {
                    throw new OperationException(
                        Strings.CannotFindDesignTimeProviderAssembly(providerServicesAssemblyName), ex);
                }
            }
            else
            {
                providerServicesAssembly = providerAssembly;
            }

            var providerServicesType = providerServicesAssembly.GetType(
                providerServicesAttribute.TypeName,
                throwOnError: true,
                ignoreCase: false);

            return (IDesignTimeMetadataProviderFactory)Activator.CreateInstance(providerServicesType);
        }
    }
}
