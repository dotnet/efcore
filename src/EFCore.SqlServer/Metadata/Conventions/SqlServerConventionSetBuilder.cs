// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A builder for building conventions for SQL Server.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" /> and multiple registrations
///         are allowed. This means that each <see cref="DbContext" /> instance will use its own
///         set of instances of this service.
///         The implementations may depend on other services registered with any lifetime.
///         The implementations do not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see>, and
///         <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
///         for more information and examples.
///     </para>
/// </remarks>
public class SqlServerConventionSetBuilder : RelationalConventionSetBuilder
{
    private readonly ISqlGenerationHelper _sqlGenerationHelper;

    /// <summary>
    ///     Creates a new <see cref="SqlServerConventionSetBuilder" /> instance.
    /// </summary>
    /// <param name="dependencies">The core dependencies for this service.</param>
    /// <param name="relationalDependencies">The relational dependencies for this service.</param>
    /// <param name="sqlGenerationHelper">The SQL generation helper to use.</param>
    public SqlServerConventionSetBuilder(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies,
        ISqlGenerationHelper sqlGenerationHelper)
        : base(dependencies, relationalDependencies)
    {
        _sqlGenerationHelper = sqlGenerationHelper;
    }

    /// <summary>
    ///     Builds and returns the convention set for the current database provider.
    /// </summary>
    /// <returns>The convention set for the current database provider.</returns>
    public override ConventionSet CreateConventionSet()
    {
        var conventionSet = base.CreateConventionSet();

        conventionSet.Add(new SqlServerValueGenerationStrategyConvention(Dependencies, RelationalDependencies));
        conventionSet.Add(new RelationalMaxIdentifierLengthConvention(128, Dependencies, RelationalDependencies));
        conventionSet.Add(new SqlServerIndexConvention(Dependencies, RelationalDependencies, _sqlGenerationHelper));
        conventionSet.Add(new SqlServerMemoryOptimizedTablesConvention(Dependencies, RelationalDependencies));
        conventionSet.Add(new SqlServerDbFunctionConvention(Dependencies, RelationalDependencies));
        conventionSet.Add(new SqlServerOutputClauseConvention(Dependencies, RelationalDependencies));

        conventionSet.Replace<CascadeDeleteConvention>(
            new SqlServerOnDeleteConvention(Dependencies, RelationalDependencies));
        conventionSet.Replace<StoreGenerationConvention>(
            new SqlServerStoreGenerationConvention(Dependencies, RelationalDependencies));
        conventionSet.Replace<ValueGenerationConvention>(
            new SqlServerValueGenerationConvention(Dependencies, RelationalDependencies));
        conventionSet.Replace<RuntimeModelConvention>(new SqlServerRuntimeModelConvention(Dependencies, RelationalDependencies));
        conventionSet.Replace<SharedTableConvention>(
            new SqlServerSharedTableConvention(Dependencies, RelationalDependencies));

        var sqlServerTemporalConvention = new SqlServerTemporalConvention(Dependencies, RelationalDependencies);
        ConventionSet.AddBefore(
            conventionSet.EntityTypeAnnotationChangedConventions,
            sqlServerTemporalConvention,
            typeof(SqlServerValueGenerationConvention));
        conventionSet.SkipNavigationForeignKeyChangedConventions.Add(sqlServerTemporalConvention);
        conventionSet.ModelFinalizingConventions.Add(sqlServerTemporalConvention);

        return conventionSet;
    }

    /// <summary>
    ///     Call this method to build a <see cref="ConventionSet" /> for SQL Server when using
    ///     the <see cref="ModelBuilder" /> outside of <see cref="DbContext.OnModelCreating" />.
    /// </summary>
    /// <remarks>
    ///     Note that it is unusual to use this method. Consider using <see cref="DbContext" /> in the normal way instead.
    /// </remarks>
    /// <returns>The convention set.</returns>
    public static ConventionSet Build()
    {
        using var serviceScope = CreateServiceScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<DbContext>();
        return ConventionSet.CreateConventionSet(context);
    }

    /// <summary>
    ///     Call this method to build a <see cref="ModelBuilder" /> for SQL Server outside of <see cref="DbContext.OnModelCreating" />.
    /// </summary>
    /// <remarks>
    ///     Note that it is unusual to use this method. Consider using <see cref="DbContext" /> in the normal way instead.
    /// </remarks>
    /// <returns>The convention set.</returns>
    public static ModelBuilder CreateModelBuilder()
    {
        using var serviceScope = CreateServiceScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<DbContext>();
        return new ModelBuilder(ConventionSet.CreateConventionSet(context), context.GetService<ModelDependencies>());
    }

    private static IServiceScope CreateServiceScope()
    {
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkSqlServer()
            .AddDbContext<DbContext>(
                (p, o) =>
                    o.UseSqlServer("Server=.")
                        .UseInternalServiceProvider(p))
            .BuildServiceProvider();

        return serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
    }
}
