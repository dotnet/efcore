// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Query.ExpressionTranslators;
using Microsoft.Data.Entity.Sqlite.Metadata;
using Microsoft.Data.Entity.Sqlite.Query.ExpressionTranslators;
using Microsoft.Data.Entity.Sqlite.Update;
using Microsoft.Data.Entity.Sqlite.ValueGeneration;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.ValueGeneration;

namespace Microsoft.Data.Entity.Sqlite
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
        public override IRelationalConnection RelationalConnection => GetService<SqliteDatabaseConnection>();
        public override IUpdateSqlGenerator UpdateSqlGenerator => GetService<SqliteUpdateSqlGenerator>();
        public override IQueryCompilationContextFactory QueryCompilationContextFactory => GetService<SqliteQueryCompilationContextFactory>();
        public override IValueGeneratorCache ValueGeneratorCache => GetService<SqliteValueGeneratorCache>();
        public override IRelationalTypeMapper TypeMapper => GetService<SqliteTypeMapper>();
        public override IModificationCommandBatchFactory ModificationCommandBatchFactory => GetService<SqliteModificationCommandBatchFactory>();
        public override IRelationalDatabaseCreator RelationalDatabaseCreator => GetService<SqliteDatabaseCreator>();
        public override IConventionSetBuilder ConventionSetBuilder => GetService<SqliteConventionSetBuilder>();
        public override IRelationalMetadataExtensionProvider MetadataExtensionProvider => GetService<SqliteMetadataExtensionProvider>();
        public override IMethodCallTranslator CompositeMethodCallTranslator => GetService<SqliteCompositeMethodCallTranslator>();
        public override IMemberTranslator CompositeMemberTranslator => GetService<SqliteCompositeMemberTranslator>();
        public override IExpressionFragmentTranslator CompositeExpressionFragmentTranslator => GetService<SqliteCompositeExpressionFragmentTranslator>();
        public override IMigrationsAnnotationProvider MigrationsAnnotationProvider => GetService<SqliteMigrationsAnnotationProvider>();
    }
}
