// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.EntityFrameworkCore.Relational.Tests.TestUtilities.FakeProvider
{
    public class FakeRelationalOptionsExtension : RelationalOptionsExtension
    {
        public FakeRelationalOptionsExtension()
        {
        }

        public FakeRelationalOptionsExtension(FakeRelationalOptionsExtension copyFrom)
            : base(copyFrom)
        {
        }

        public override bool ApplyServices(IServiceCollection services)
        {
            AddEntityFrameworkRelationalDatabase(services);

            return true;
        }
        public static IServiceCollection AddEntityFrameworkRelationalDatabase(IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddEnumerable(
                ServiceDescriptor.Singleton<IDatabaseProvider, DatabaseProvider<FakeRelationalOptionsExtension>>());

            serviceCollection.TryAdd(new ServiceCollection()
                .AddSingleton<ISqlGenerationHelper, RelationalSqlGenerationHelper>()
                .AddSingleton<IRelationalTypeMapper, TestRelationalTypeMapper>()
                .AddSingleton<IRelationalAnnotationProvider, TestAnnotationProvider>()
                .AddScoped<IMigrationsSqlGenerator, TestRelationalMigrationSqlGenerator>()
                .AddScoped<IConventionSetBuilder, TestRelationalConventionSetBuilder>()
                .AddScoped<IMemberTranslator, TestRelationalCompositeMemberTranslator>()
                .AddScoped<IMethodCallTranslator, TestRelationalCompositeMethodCallTranslator>()
                .AddScoped<IQuerySqlGeneratorFactory, TestQuerySqlGeneratorFactory>()
                .AddScoped<IRelationalConnection, FakeRelationalConnection>()
                .AddScoped<IHistoryRepository>(_ => null)
                .AddScoped<IUpdateSqlGenerator>(_ => null)
                .AddScoped<IModificationCommandBatchFactory>(_ => null)
                .AddScoped<IRelationalDatabaseCreator, FakeRelationalDatabaseCreator>());

            ServiceCollectionRelationalProviderInfrastructure.TryAddDefaultRelationalServices(new ServiceCollectionMap(serviceCollection));

            return serviceCollection;
        }
    }
}
