// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;

namespace Microsoft.EntityFrameworkCore.Design;

/// <summary>
///     Extension methods for adding Entity Framework Core design-time services to an
///     <see cref="IServiceCollection" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-design-time-services">EF Core design-time services</see> for more information and examples.
/// </remarks>
public static class DesignTimeServiceCollectionExtensions
{
    /// <summary>
    ///     Adds the Entity Framework Core design-time services.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-design-time-services">EF Core design-time services</see> for more information and examples.
    /// </remarks>
    /// <param name="services">The <see cref="IServiceCollection" /> the services will be added to.</param>
    /// <param name="reporter">Used to report design-time messages.</param>
    /// <param name="applicationServiceProviderAccessor">An accessor to the application service provider.</param>
    /// <returns>The <paramref name="services" />. This enables chaining additional method calls.</returns>
    public static IServiceCollection AddEntityFrameworkDesignTimeServices(
        this IServiceCollection services,
        IOperationReporter? reporter = null,
        Func<IServiceProvider>? applicationServiceProviderAccessor = null)
    {
        reporter ??= new OperationReporter(handler: null);

        new EntityFrameworkRelationalDesignServicesBuilder(services)
            .TryAddProviderSpecificServices(
                services => services
                    .TryAddSingleton<CSharpMigrationOperationGeneratorDependencies, CSharpMigrationOperationGeneratorDependencies>()
                    .TryAddSingleton<CSharpMigrationsGeneratorDependencies, CSharpMigrationsGeneratorDependencies>()
                    .TryAddSingleton<CSharpSnapshotGeneratorDependencies, CSharpSnapshotGeneratorDependencies>()
                    .TryAddSingleton<ICandidateNamingService, CandidateNamingService>()
                    .TryAddSingleton<ICSharpHelper, CSharpHelper>()
                    .TryAddSingleton<ICSharpMigrationOperationGenerator, CSharpMigrationOperationGenerator>()
                    .TryAddSingleton<ICSharpSnapshotGenerator, CSharpSnapshotGenerator>()
                    .TryAddSingleton<ICSharpUtilities, CSharpUtilities>()
                    .TryAddSingleton(reporter)
                    .TryAddSingleton<IMigrationsCodeGenerator, CSharpMigrationsGenerator>()
                    .TryAddSingleton<IMigrationsCodeGeneratorSelector, MigrationsCodeGeneratorSelector>()
                    .TryAddSingletonEnumerable<IModelCodeGenerator, TextTemplatingModelGenerator>()
                    .TryAddSingletonEnumerable<IModelCodeGenerator, CSharpModelGenerator>()
                    .TryAddSingleton<IModelCodeGeneratorSelector, ModelCodeGeneratorSelector>()
                    .TryAddSingleton<ICompiledModelCodeGenerator, CSharpRuntimeModelCodeGenerator>()
                    .TryAddSingleton<ICompiledModelCodeGeneratorSelector, CompiledModelCodeGeneratorSelector>()
                    .TryAddSingleton<ICompiledModelScaffolder, CompiledModelScaffolder>()
                    .TryAddSingleton<IDesignTimeConnectionStringResolver>(
                        new DesignTimeConnectionStringResolver(applicationServiceProviderAccessor))
                    .TryAddSingleton<IPluralizer, HumanizerPluralizer>()
                    .TryAddSingleton<IScaffoldingModelFactory, RelationalScaffoldingModelFactory>()
                    .TryAddSingleton<IScaffoldingTypeMapper, ScaffoldingTypeMapper>()
                    .TryAddSingleton<MigrationsCodeGeneratorDependencies, MigrationsCodeGeneratorDependencies>()
                    .TryAddSingleton<ModelCodeGeneratorDependencies, ModelCodeGeneratorDependencies>()
                    .TryAddScoped<IReverseEngineerScaffolder, ReverseEngineerScaffolder>()
                    .TryAddScoped<MigrationsScaffolderDependencies, MigrationsScaffolderDependencies>()
                    .TryAddScoped<IMigrationsScaffolder, MigrationsScaffolder>()
                    .TryAddScoped<ISnapshotModelProcessor, SnapshotModelProcessor>());

        var loggerFactory = new LoggerFactory(
            new[] { new OperationLoggerProvider(reporter) }, new LoggerFilterOptions { MinLevel = LogLevel.Debug });
        services.AddScoped<ILoggerFactory>(_ => loggerFactory);

        return services;
    }

    /// <summary>
    ///     Adds services from the <see cref="DbContext" /> which are used at design time.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-design-time-services">EF Core design-time services</see> for more information and examples.
    /// </remarks>
    /// <param name="services">The <see cref="IServiceCollection" /> the services will be added to.</param>
    /// <param name="context">The <see cref="DbContext" /> the services will be added from.</param>
    /// <returns>The <paramref name="services" />. This enables chaining additional method calls.</returns>
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
