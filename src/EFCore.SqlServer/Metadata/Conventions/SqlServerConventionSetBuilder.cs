// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
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
    ///         <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and SQL Azure databases with EF Core</see>
    ///         for more information.
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

            var valueGenerationStrategyConvention = new SqlServerValueGenerationStrategyConvention(Dependencies, RelationalDependencies);
            conventionSet.ModelInitializedConventions.Add(valueGenerationStrategyConvention);
            conventionSet.ModelInitializedConventions.Add(
                new RelationalMaxIdentifierLengthConvention(128, Dependencies, RelationalDependencies));

            ValueGenerationConvention valueGenerationConvention =
                new SqlServerValueGenerationConvention(Dependencies, RelationalDependencies);
            var sqlServerIndexConvention = new SqlServerIndexConvention(Dependencies, RelationalDependencies, _sqlGenerationHelper);
            ReplaceConvention(conventionSet.EntityTypeBaseTypeChangedConventions, valueGenerationConvention);
            conventionSet.EntityTypeBaseTypeChangedConventions.Add(sqlServerIndexConvention);

            var sqlServerInMemoryTablesConvention = new SqlServerMemoryOptimizedTablesConvention(Dependencies, RelationalDependencies);
            conventionSet.EntityTypeAnnotationChangedConventions.Add(sqlServerInMemoryTablesConvention);
            ReplaceConvention(
                conventionSet.EntityTypeAnnotationChangedConventions, (RelationalValueGenerationConvention)valueGenerationConvention);

            var sqlServerTemporalConvention = new SqlServerTemporalConvention(Dependencies, RelationalDependencies);
            ConventionSet.AddBefore(
                conventionSet.EntityTypeAnnotationChangedConventions,
                sqlServerTemporalConvention,
                typeof(SqlServerValueGenerationConvention));

            ReplaceConvention(conventionSet.EntityTypePrimaryKeyChangedConventions, valueGenerationConvention);

            conventionSet.KeyAddedConventions.Add(sqlServerInMemoryTablesConvention);

            var sqlServerOnDeleteConvention = new SqlServerOnDeleteConvention(Dependencies, RelationalDependencies);
            ReplaceConvention(conventionSet.ForeignKeyAddedConventions, valueGenerationConvention);
            ReplaceConvention(conventionSet.ForeignKeyAddedConventions, (CascadeDeleteConvention)sqlServerOnDeleteConvention);

            ReplaceConvention(conventionSet.ForeignKeyRemovedConventions, valueGenerationConvention);

            ReplaceConvention(conventionSet.ForeignKeyRequirednessChangedConventions, (CascadeDeleteConvention)sqlServerOnDeleteConvention);

            conventionSet.SkipNavigationForeignKeyChangedConventions.Add(sqlServerOnDeleteConvention);

            conventionSet.IndexAddedConventions.Add(sqlServerInMemoryTablesConvention);
            conventionSet.IndexAddedConventions.Add(sqlServerIndexConvention);

            conventionSet.IndexUniquenessChangedConventions.Add(sqlServerIndexConvention);

            conventionSet.IndexAnnotationChangedConventions.Add(sqlServerIndexConvention);

            conventionSet.PropertyNullabilityChangedConventions.Add(sqlServerIndexConvention);

            StoreGenerationConvention storeGenerationConvention =
                new SqlServerStoreGenerationConvention(Dependencies, RelationalDependencies);
            conventionSet.PropertyAnnotationChangedConventions.Add(sqlServerIndexConvention);
            ReplaceConvention(conventionSet.PropertyAnnotationChangedConventions, storeGenerationConvention);
            ReplaceConvention(
                conventionSet.PropertyAnnotationChangedConventions, (RelationalValueGenerationConvention)valueGenerationConvention);

            conventionSet.ModelFinalizingConventions.Add(valueGenerationStrategyConvention);
            ReplaceConvention(conventionSet.ModelFinalizingConventions, storeGenerationConvention);
            ReplaceConvention(
                conventionSet.ModelFinalizingConventions,
                (SharedTableConvention)new SqlServerSharedTableConvention(Dependencies, RelationalDependencies));
            conventionSet.ModelFinalizingConventions.Add(new SqlServerDbFunctionConvention(Dependencies, RelationalDependencies));

            ReplaceConvention(
                conventionSet.ModelFinalizedConventions,
                (RuntimeModelConvention)new SqlServerRuntimeModelConvention(Dependencies, RelationalDependencies));

            conventionSet.SkipNavigationForeignKeyChangedConventions.Add(sqlServerTemporalConvention);

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
}
