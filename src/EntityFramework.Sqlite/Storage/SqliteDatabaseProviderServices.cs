// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Infrastructure.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Internal;
using Microsoft.Data.Entity.Query.ExpressionTranslators;
using Microsoft.Data.Entity.Query.ExpressionTranslators.Internal;
using Microsoft.Data.Entity.Query.Sql;
using Microsoft.Data.Entity.Storage.Internal;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.Update.Internal;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Data.Entity.ValueGeneration.Internal;

namespace Microsoft.Data.Entity.Storage
{
    public class SqliteDatabaseProviderServices : RelationalDatabaseProviderServices
    {
        public SqliteDatabaseProviderServices([NotNull] IServiceProvider services)
            : base(services)
        {
        }

        public override string InvariantName => GetType().GetTypeInfo().Assembly.GetName().Name;
        public override IDatabaseCreator Creator => GetService<SqliteDatabaseCreator>();
        public override IHistoryRepository HistoryRepository => GetService<SqliteHistoryRepository>();
        public override IMigrationsSqlGenerator MigrationsSqlGenerator => GetService<SqliteMigrationsSqlGenerator>();
        public override IModelSource ModelSource => GetService<SqliteModelSource>();
        public override IRelationalConnection RelationalConnection => GetService<SqliteRelationalConnection>();
        public override IUpdateSqlGenerator UpdateSqlGenerator => GetService<SqliteUpdateSqlGenerator>();
        public override IValueGeneratorCache ValueGeneratorCache => GetService<SqliteValueGeneratorCache>();
        public override IRelationalTypeMapper TypeMapper => GetService<SqliteTypeMapper>();
        public override IModificationCommandBatchFactory ModificationCommandBatchFactory => GetService<SqliteModificationCommandBatchFactory>();
        public override IRelationalDatabaseCreator RelationalDatabaseCreator => GetService<SqliteDatabaseCreator>();
        public override IConventionSetBuilder ConventionSetBuilder => GetService<SqliteConventionSetBuilder>();
        public override IRelationalMetadataExtensionProvider MetadataExtensionProvider => GetService<SqliteAnnotationProvider>();
        public override IMethodCallTranslator CompositeMethodCallTranslator => GetService<SqliteCompositeMethodCallTranslator>();
        public override IMemberTranslator CompositeMemberTranslator => GetService<SqliteCompositeMemberTranslator>();
        public override IMigrationsAnnotationProvider MigrationsAnnotationProvider => GetService<SqliteMigrationsAnnotationProvider>();
        public override ISqlQueryGeneratorFactory SqlQueryGeneratorFactory => GetService<SqliteQuerySqlGeneratorFactory>();
    }
}
