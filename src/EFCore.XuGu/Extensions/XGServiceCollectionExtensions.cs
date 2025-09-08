// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Migrations.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Update.Internal;
using Microsoft.EntityFrameworkCore.XuGu.ValueGeneration.Internal;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.XuGu.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure;
using Microsoft.EntityFrameworkCore.XuGu.Metadata.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Migrations;
using Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Query.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class XGServiceCollectionExtensions
    {
        /// <summary>
        ///     <para>
        ///         Registers the given Entity Framework context as a service in the <see cref="IServiceCollection" />
        ///         and configures it to connect to a MySQL compatible database.
        ///     </para>
        ///     <para>
        ///         Use this method when using dependency injection in your application, such as with ASP.NET Core.
        ///         For applications that don't use dependency injection, consider creating <see cref="DbContext" />
        ///         instances directly with its constructor. The <see cref="DbContext.OnConfiguring" /> method can then be
        ///         overridden to configure the Microsoft.EntityFrameworkCore.XuGu provider and connection string.
        ///     </para>
        ///     <para>
        ///         To configure the <see cref="DbContextOptions{TContext}" /> for the context, either override the
        ///         <see cref="DbContext.OnConfiguring" /> method in your derived context, or supply
        ///         an optional action to configure the <see cref="DbContextOptions" /> for the context.
        ///     </para>
        ///     <para>
        ///         For more information on how to use this method, see the Entity Framework Core documentation at https://aka.ms/efdocs.
        ///         For more information on using dependency injection, see https://go.microsoft.com/fwlink/?LinkId=526890.
        ///     </para>
        /// </summary>
        /// <typeparam name="TContext"> The type of context to be registered. </typeparam>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
        /// <param name="connectionString"> The connection string of the database to connect to. </param>
        /// <param name="serverVersion">
        ///     <para>
        ///         The version of the database server.
        ///     </para>
        ///     <para>
        ///         Create an object for this parameter by calling the static method
        ///         <see cref="ServerVersion.Create(System.Version,ServerType)"/>,
        ///         by calling the static method <see cref="ServerVersion.AutoDetect(string)"/> (which retrieves the server version directly
        ///         from the database server),
        ///         by parsing a version string using the static methods
        ///         <see cref="ServerVersion.Parse(string)"/> or <see cref="ServerVersion.TryParse(string,out ServerVersion)"/>,
        ///         or by directly instantiating an object from the <see cref="XGServerVersion"/> (for MySQL) classes.
        ///      </para>
        /// </param>
        /// <param name="xgOptionsAction"> An optional action to allow additional MySQL specific configuration. </param>
        /// <param name="optionsAction"> An optional action to configure the <see cref="DbContextOptions" /> for the context. </param>
        /// <returns> The same service collection so that multiple calls can be chained. </returns>
        public static IServiceCollection AddXG<TContext>(
            this IServiceCollection serviceCollection,
            string connectionString,
            ServerVersion serverVersion,
            Action<XGDbContextOptionsBuilder> xgOptionsAction = null,
            Action<DbContextOptionsBuilder> optionsAction = null)
            where TContext : DbContext
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            return serviceCollection.AddDbContext<TContext>((_, options) =>
            {
                optionsAction?.Invoke(options);
                options.UseXG(connectionString, serverVersion, xgOptionsAction);
            });
        }

        public static IServiceCollection AddEntityFrameworkXG([NotNull] this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            var builder = new EntityFrameworkRelationalServicesBuilder(serviceCollection)
                .TryAdd<LoggingDefinitions, XGLoggingDefinitions>()
                .TryAdd<IDatabaseProvider, DatabaseProvider<XGOptionsExtension>>()
                //.TryAdd<IValueGeneratorCache>(p => p.GetService<IXGValueGeneratorCache>())
                .TryAdd<IRelationalTypeMappingSource, XGTypeMappingSource>()
                .TryAdd<ISqlGenerationHelper, XGSqlGenerationHelper>()
                .TryAdd<IRelationalAnnotationProvider, XGAnnotationProvider>()
                .TryAdd<IModelValidator, XGModelValidator>()
                .TryAdd<IProviderConventionSetBuilder, XGConventionSetBuilder>()
                //.TryAdd<IRelationalValueBufferFactoryFactory, TypedRelationalValueBufferFactoryFactory>() // What is that?
                .TryAdd<IUpdateSqlGenerator, XGUpdateSqlGenerator>()
                .TryAdd<IModificationCommandFactory, XGModificationCommandFactory>()
                .TryAdd<IModificationCommandBatchFactory, XGModificationCommandBatchFactory>()
                .TryAdd<IValueGeneratorSelector, XGValueGeneratorSelector>()
                .TryAdd<IRelationalConnection>(p => p.GetRequiredService<IXGRelationalConnection>())
                .TryAdd<IMigrationsSqlGenerator, XGMigrationsSqlGenerator>()
                .TryAdd<IRelationalDatabaseCreator, XGDatabaseCreator>()
                .TryAdd<IHistoryRepository, XGHistoryRepository>()
                .TryAdd<ICompiledQueryCacheKeyGenerator, XGCompiledQueryCacheKeyGenerator>()
                .TryAdd<IExecutionStrategyFactory, XGExecutionStrategyFactory>()
                .TryAdd<IQueryableMethodTranslatingExpressionVisitorFactory, XGQueryableMethodTranslatingExpressionVisitorFactory>()
                .TryAdd<IRelationalQueryStringFactory, XGQueryStringFactory>()
                .TryAdd<IMethodCallTranslatorProvider, XGMethodCallTranslatorProvider>()
                .TryAdd<IMemberTranslatorProvider, XGMemberTranslatorProvider>()
                .TryAdd<IEvaluatableExpressionFilter, XGEvaluatableExpressionFilter>()
                .TryAdd<IQuerySqlGeneratorFactory, XGQuerySqlGeneratorFactory>()
                .TryAdd<IRelationalSqlTranslatingExpressionVisitorFactory, XGSqlTranslatingExpressionVisitorFactory>()
                .TryAdd<IRelationalParameterBasedSqlProcessorFactory, XGParametersBasedSqlProcessorFactory>()
                .TryAdd<ISqlExpressionFactory, XGSqlExpressionFactory>()
                .TryAdd<ISingletonOptions, IXGOptions>(p => p.GetRequiredService<IXGOptions>())
                //.TryAdd<IValueConverterSelector, XGValueConverterSelector>()
                .TryAdd<IQueryCompilationContextFactory, XGQueryCompilationContextFactory>()
                .TryAdd<IQueryTranslationPostprocessorFactory, XGQueryTranslationPostprocessorFactory>()

                // TODO: Injecting this service will make our original JSON implementations work, but interferes with EF Core 8's new
                //       primitive collections support.
                //       We will need to limit the preprocessor logic to only the relevant cases.
                .TryAdd<IQueryTranslationPreprocessorFactory, XGQueryTranslationPreprocessorFactory>()

                .TryAdd<IMigrationsModelDiffer, XGMigrationsModelDiffer>()
                .TryAdd<IMigrator, XGMigrator>()
                .TryAddProviderSpecificServices(m => m
                    //.TryAddSingleton<IXGValueGeneratorCache, XGValueGeneratorCache>()
                    .TryAddSingleton<IXGOptions, XGOptions>()
                    .TryAddSingleton<IXGConnectionStringOptionsValidator, XGConnectionStringOptionsValidator>()
                    //.TryAddScoped<IXGSequenceValueGeneratorFactory, XGSequenceValueGeneratorFactory>()
                    .TryAddScoped<IXGUpdateSqlGenerator, XGUpdateSqlGenerator>()
                    .TryAddScoped<IXGRelationalConnection, XGRelationalConnection>());

            builder.TryAddCoreServices();

            return serviceCollection;
        }
    }
}
