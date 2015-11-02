// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Internal;
using Microsoft.Data.Entity.Query.ExpressionTranslators.Internal;
using Microsoft.Data.Entity.Query.Internal;
using Microsoft.Data.Entity.Query.Sql.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Storage.Internal;
using Microsoft.Data.Entity.Update.Internal;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Entity.ValueGeneration.Internal;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SqlServerEntityFrameworkServicesBuilderExtensions
    {
        /// <summary>
        ///     <para>
        ///         Adds the services required by the Microsoft SQL Server database provider for Entity Framework 
        ///         to an <see cref="IServiceCollection" />. You use this method when using dependency injection 
        ///         in your application, such as with ASP.NET. For more information on setting up dependency 
        ///         injection, see http://go.microsoft.com/fwlink/?LinkId=526890.
        ///     </para>
        ///     <para>
        ///         You only need to use this functionality when you want Entity Framework to resolve the services it uses
        ///         from an external <see cref="IServiceCollection" />. If you are not using an external
        ///         <see cref="IServiceCollection" /> Entity Framework will take care of creating the services it requires.
        ///     </para>
        /// </summary>
        /// <example>
        ///     <code>
        ///         public void ConfigureServices(IServiceCollection services) 
        ///         {
        ///             var connectionString = "connection string to database";
        /// 
        ///             services.AddEntityFramework() 
        ///                 .AddSqlServer()
        ///                 .AddDbContext&lt;MyContext&gt;(options => options.UseSqlServer(connectionString)); 
        ///         }
        ///     </code>
        /// </example>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
        /// <returns>
        ///     A builder that allows further Entity Framework specific setup of the <see cref="IServiceCollection" />.
        /// </returns>
        public static EntityFrameworkServicesBuilder AddSqlServer([NotNull] this EntityFrameworkServicesBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            var service = builder.AddRelational().GetInfrastructure();

            service.TryAddEnumerable(ServiceDescriptor
                .Singleton<IDatabaseProvider, DatabaseProvider<SqlServerDatabaseProviderServices, SqlServerOptionsExtension>>());

            service.TryAdd(new ServiceCollection()
                .AddSingleton<ISqlServerValueGeneratorCache, SqlServerValueGeneratorCache>()
                .AddSingleton<SqlServerTypeMapper>()
                .AddSingleton<SqlServerSqlGenerator>()
                .AddSingleton<SqlServerModelSource>()
                .AddSingleton<SqlServerAnnotationProvider>()
                .AddSingleton<SqlServerMigrationsAnnotationProvider>()
                .AddScoped<SqlServerModelValidator>()
                .AddScoped<SqlServerConventionSetBuilder>()
                .AddScoped<ISqlServerUpdateSqlGenerator, SqlServerUpdateSqlGenerator>()
                .AddScoped<ISqlServerSequenceValueGeneratorFactory, SqlServerSequenceValueGeneratorFactory>()
                .AddScoped<SqlServerModificationCommandBatchFactory>()
                .AddScoped<SqlServerValueGeneratorSelector>()
                .AddScoped<SqlServerDatabaseProviderServices>()
                .AddScoped<ISqlServerConnection, SqlServerConnection>()
                .AddScoped<SqlServerMigrationsSqlGenerator>()
                .AddScoped<SqlServerDatabaseCreator>()
                .AddScoped<SqlServerHistoryRepository>()
                .AddScoped<SqlServerQueryModelVisitorFactory>()
                .AddScoped<SqlServerCompiledQueryCacheKeyGenerator>()
                .AddQuery());

            return builder;
        }

        private static IServiceCollection AddQuery(this IServiceCollection serviceCollection)
            => serviceCollection
                .AddScoped<SqlServerQueryCompilationContextFactory>()
                .AddScoped<SqlServerCompositeMemberTranslator>()
                .AddScoped<SqlServerCompositeMethodCallTranslator>()
                .AddScoped<SqlServerQuerySqlGeneratorFactory>();
    }
}
