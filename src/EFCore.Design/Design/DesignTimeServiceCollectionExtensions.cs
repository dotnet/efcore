// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Design
{
    /// <summary>
    ///     Extension methods for adding Entity Framework Core design-time services to an
    ///     <see cref="IServiceCollection" />.
    /// </summary>
    public static class DesignTimeServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds the Entity Framework Core design-time services.
        /// </summary>
        /// <param name="services"> The <see cref="IServiceCollection" /> the services will be added to. </param>
        /// <param name="reporter"> Used to report design-time messages. </param>
        /// <param name="applicationServiceProviderAccessor"> An accessor to the application service provider. </param>
        /// <returns> The <paramref name="services" />. This enables chaining additional method calls. </returns>
        public static IServiceCollection AddEntityFrameworkDesignTimeServices(
            this IServiceCollection services,
            IOperationReporter? reporter = null,
            Func<IServiceProvider>? applicationServiceProviderAccessor = null)
        {
            if (reporter == null)
            {
                reporter = new OperationReporter(handler: null);
            }

            return services
                .AddSingleton<CSharpMigrationOperationGeneratorDependencies>()
                .AddSingleton<CSharpMigrationsGeneratorDependencies>()
                .AddSingleton<CSharpSnapshotGeneratorDependencies>()
                .AddSingleton<ICandidateNamingService, CandidateNamingService>()
                .AddSingleton<ICSharpDbContextGenerator, CSharpDbContextGenerator>()
                .AddSingleton<ICSharpEntityTypeGenerator, CSharpEntityTypeGenerator>()
                .AddSingleton<ICSharpHelper, CSharpHelper>()
                .AddSingleton<ICSharpMigrationOperationGenerator, CSharpMigrationOperationGenerator>()
                .AddSingleton<ICSharpSnapshotGenerator, CSharpSnapshotGenerator>()
                .AddSingleton<ICSharpUtilities, CSharpUtilities>()
                .AddSingleton(reporter)
                .AddSingleton<IMigrationsCodeGenerator, CSharpMigrationsGenerator>()
                .AddSingleton<IMigrationsCodeGeneratorSelector, MigrationsCodeGeneratorSelector>()
                .AddSingleton<IModelCodeGenerator, CSharpModelGenerator>()
                .AddSingleton<IModelCodeGeneratorSelector, ModelCodeGeneratorSelector>()
                .AddSingleton<INamedConnectionStringResolver>(
                    new DesignTimeConnectionStringResolver(applicationServiceProviderAccessor))
                .AddSingleton<IPluralizer, HumanizerPluralizer>()
                .AddSingleton<IReverseEngineerScaffolder, ReverseEngineerScaffolder>()
                .AddSingleton<IScaffoldingModelFactory, RelationalScaffoldingModelFactory>()
                .AddSingleton<IScaffoldingTypeMapper, ScaffoldingTypeMapper>()
                .AddSingleton<MigrationsCodeGeneratorDependencies>()
                .AddSingleton<ModelCodeGeneratorDependencies>()
                .AddScoped<MigrationsScaffolderDependencies>()
                .AddScoped<IMigrationsScaffolder, MigrationsScaffolder>()
                .AddScoped<ISnapshotModelProcessor, SnapshotModelProcessor>()
                .AddLogging(b => b.SetMinimumLevel(LogLevel.Debug).AddProvider(new OperationLoggerProvider(reporter)));
        }

        /// <summary>
        ///     Adds services from the <see cref="DbContext" /> which are used at design time.
        /// </summary>
        /// <param name="services"> The <see cref="IServiceCollection" /> the services will be added to. </param>
        /// <param name="context"> The <see cref="DbContext" /> the services will be added from. </param>
        /// <returns> The <paramref name="services" />. This enables chaining additional method calls. </returns>
        public static IServiceCollection AddDbContextDesignTimeServices(
            this IServiceCollection services,
            DbContext context)
        {
            new EntityFrameworkRelationalServicesBuilder(services)
                .TryAdd(context.GetService<IDatabaseProvider>())
                .TryAdd(_ => context.GetService<IMigrationsIdGenerator>())
                .TryAdd(_ => context.GetService<IRelationalTypeMappingSource>())
                .TryAdd(_ => context.GetService<IModelRuntimeInitializer>())
                .TryAdd(_ => context.GetService<LoggingDefinitions>())
                .TryAdd(_ => context.GetService<ICurrentDbContext>())
                .TryAdd(_ => context.GetService<IDbContextOptions>())
                .TryAdd(_ => context.GetService<IHistoryRepository>())
                .TryAdd(_ => context.GetService<IMigrationsAssembly>())
                .TryAdd(_ => context.GetService<IMigrationsModelDiffer>())
                .TryAdd(_ => context.GetService<IMigrator>())
                .TryAdd(_ => context.GetService<IDesignTimeModel>().Model);
            return services;
        }
    }
}
