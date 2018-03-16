// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Oracle.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Oracle.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Oracle.Query.ExpressionTranslators.Internal;
using Microsoft.EntityFrameworkCore.Oracle.Query.Internal;
using Microsoft.EntityFrameworkCore.Oracle.Query.Sql.Internal;
using Microsoft.EntityFrameworkCore.Oracle.Storage.Internal;
using Microsoft.EntityFrameworkCore.Oracle.Update.Internal;
using Microsoft.EntityFrameworkCore.Oracle.ValueGeneration.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    ///     Oracle specific extension methods for <see cref="IServiceCollection" />.
    /// </summary>
    public static class OracleServiceCollectionExtensions
    {
        /// <summary>
        ///     <para>
        ///         Adds the services required by the Microsoft Oracle database provider for Entity Framework
        ///         to an <see cref="IServiceCollection" />. You use this method when using dependency injection
        ///         in your application, such as with ASP.NET. For more information on setting up dependency
        ///         injection, see http://go.microsoft.com/fwlink/?LinkId=526890.
        ///     </para>
        ///     <para>
        ///         You only need to use this functionality when you want Entity Framework to resolve the services it uses
        ///         from an external dependency injection container. If you are not using an external
        ///         dependency injection container, Entity Framework will take care of creating the services it requires.
        ///     </para>
        /// </summary>
        /// <example>
        ///     <code>
        ///           public void ConfigureServices(IServiceCollection services)
        ///           {
        ///               var connectionString = "connection string to database";
        ///
        ///               services
        ///                   .AddEntityFrameworkOracle()
        ///                   .AddDbContext&lt;MyContext&gt;((serviceProvider, options) =>
        ///                       options.UseOracle(connectionString)
        ///                              .UseInternalServiceProvider(serviceProvider));
        ///           }
        ///       </code>
        /// </example>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
        /// <returns>
        ///     The same service collection so that multiple calls can be chained.
        /// </returns>
        public static IServiceCollection AddEntityFrameworkOracle([NotNull] this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            var builder = new EntityFrameworkRelationalServicesBuilder(serviceCollection)
                .TryAdd<IDatabaseProvider, DatabaseProvider<OracleOptionsExtension>>()
                .TryAdd<IValueGeneratorCache>(p => p.GetService<IOracleValueGeneratorCache>())
                .TryAdd<IRelationalTypeMappingSource, OracleTypeMappingSource>()
                .TryAdd<ISqlGenerationHelper, OracleSqlGenerationHelper>()
                .TryAdd<IMigrationsAnnotationProvider, OracleMigrationsAnnotationProvider>()
                .TryAdd<IModelValidator, OracleModelValidator>()
                .TryAdd<IConventionSetBuilder, OracleConventionSetBuilder>()
                .TryAdd<IUpdateSqlGenerator, OracleUpdateSqlGenerator>()
                .TryAdd<ISingletonUpdateSqlGenerator, OracleUpdateSqlGenerator>()
                .TryAdd<IModificationCommandBatchFactory, OracleModificationCommandBatchFactory>()
                .TryAdd<IValueGeneratorSelector, OracleValueGeneratorSelector>()
                .TryAdd<IRelationalConnection>(p => p.GetService<IOracleConnection>())
                .TryAdd<IMigrationsSqlGenerator, OracleMigrationsSqlGenerator>()
                .TryAdd<IRelationalDatabaseCreator, OracleDatabaseCreator>()
                .TryAdd<IHistoryRepository, OracleHistoryRepository>()
                .TryAdd<ICompiledQueryCacheKeyGenerator, OracleCompiledQueryCacheKeyGenerator>()
                .TryAdd<IExecutionStrategyFactory, OracleExecutionStrategyFactory>()
                .TryAdd<IQueryCompilationContextFactory, OracleQueryCompilationContextFactory>()
                .TryAdd<IMemberTranslator, OracleCompositeMemberTranslator>()
                .TryAdd<ICompositeMethodCallTranslator, OracleCompositeMethodCallTranslator>()
                .TryAdd<IQuerySqlGeneratorFactory, OracleQuerySqlGeneratorFactory>()
                .TryAdd<IRelationalCommandBuilderFactory, OracleRelationalCommandBuilderFactory>()
                .TryAddProviderSpecificServices(
                    b => b
                        .TryAddSingleton<IOracleValueGeneratorCache, OracleValueGeneratorCache>()
                        .TryAddScoped<IOracleSequenceValueGeneratorFactory, OracleSequenceValueGeneratorFactory>()
                        .TryAddScoped<IOracleConnection, OracleRelationalConnection>());

            builder.TryAddCoreServices();

            return serviceCollection;
        }
    }
}
