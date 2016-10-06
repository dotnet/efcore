// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Query.Sql.Internal;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class SqliteDatabaseProviderServices : RelationalDatabaseProviderServices
    {
        public SqliteDatabaseProviderServices([NotNull] IServiceProvider services)
            : base(services)
        {
        }

        public override string InvariantName => GetType().GetTypeInfo().Assembly.GetName().Name;
        public override IHistoryRepository HistoryRepository => GetService<SqliteHistoryRepository>();
        public override ISqlGenerationHelper SqlGenerationHelper => GetService<SqliteSqlGenerationHelper>();
        public override IMigrationsSqlGenerator MigrationsSqlGenerator => GetService<SqliteMigrationsSqlGenerator>();
        public override IModelSource ModelSource => GetService<SqliteModelSource>();
        public override IRelationalConnection RelationalConnection => GetService<SqliteRelationalConnection>();
        public override IUpdateSqlGenerator UpdateSqlGenerator => GetService<SqliteUpdateSqlGenerator>();
        public override IValueGeneratorCache ValueGeneratorCache => GetService<SqliteValueGeneratorCache>();
        public override IRelationalTypeMapper TypeMapper => GetService<SqliteTypeMapper>();
        public override IModificationCommandBatchFactory ModificationCommandBatchFactory => GetService<SqliteModificationCommandBatchFactory>();
        public override IRelationalDatabaseCreator RelationalDatabaseCreator => GetService<SqliteDatabaseCreator>();
        public override IConventionSetBuilder ConventionSetBuilder => GetService<SqliteConventionSetBuilder>();
        public override IRelationalAnnotationProvider AnnotationProvider => GetService<SqliteAnnotationProvider>();
        public override IMethodCallTranslator CompositeMethodCallTranslator => GetService<SqliteCompositeMethodCallTranslator>();
        public override IMemberTranslator CompositeMemberTranslator => GetService<SqliteCompositeMemberTranslator>();
        public override IMigrationsAnnotationProvider MigrationsAnnotationProvider => GetService<SqliteMigrationsAnnotationProvider>();
        public override IQuerySqlGeneratorFactory QuerySqlGeneratorFactory => GetService<SqliteQuerySqlGeneratorFactory>();
    }
}
