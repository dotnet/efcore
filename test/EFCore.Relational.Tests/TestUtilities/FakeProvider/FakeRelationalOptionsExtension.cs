// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider
{
    public class FakeRelationalOptionsExtension : RelationalOptionsExtension
    {
        public FakeRelationalOptionsExtension()
        {
        }

        protected FakeRelationalOptionsExtension(FakeRelationalOptionsExtension copyFrom)
            : base(copyFrom)
        {
        }

        protected override RelationalOptionsExtension Clone()
            => new FakeRelationalOptionsExtension(this);

        public override bool ApplyServices(IServiceCollection services)
        {
            AddEntityFrameworkRelationalDatabase(services);

            return true;
        }

        public static IServiceCollection AddEntityFrameworkRelationalDatabase(IServiceCollection serviceCollection)
        {
            var builder = new EntityFrameworkRelationalServicesBuilder(serviceCollection)
                .TryAdd<IDatabaseProvider, DatabaseProvider<FakeRelationalOptionsExtension>>()
                .TryAdd<ISqlGenerationHelper, RelationalSqlGenerationHelper>()
                .TryAdd<IRelationalTypeMappingSource, TestRelationalTypeMappingSource>()
                .TryAdd<IMigrationsSqlGenerator, TestRelationalMigrationSqlGenerator>()
                .TryAdd<IConventionSetBuilder, TestRelationalConventionSetBuilder>()
                .TryAdd<IMemberTranslator, TestRelationalCompositeMemberTranslator>()
                .TryAdd<ICompositeMethodCallTranslator, TestRelationalCompositeMethodCallTranslator>()
                .TryAdd<IQuerySqlGeneratorFactory, TestQuerySqlGeneratorFactory>()
                .TryAdd<IRelationalConnection, FakeRelationalConnection>()
                .TryAdd<IHistoryRepository>(_ => null)
                .TryAdd<IUpdateSqlGenerator, FakeSqlGenerator>()
                .TryAdd<IModificationCommandBatchFactory, TestModificationCommandBatchFactory>()
                .TryAdd<IRelationalDatabaseCreator, FakeRelationalDatabaseCreator>();

            builder.TryAddCoreServices();

            return serviceCollection;
        }
    }
}
