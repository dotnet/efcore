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
using Microsoft.Data.Entity.Query.Sql;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.SqlServer.Query.ExpressionTranslators;
using Microsoft.Data.Entity.SqlServer.Update;
using Microsoft.Data.Entity.SqlServer.ValueGeneration;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.ValueGeneration;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerDatabaseProviderServices : RelationalDatabaseProviderServices
    {
        public SqlServerDatabaseProviderServices([NotNull] IServiceProvider services)
            : base(services)
        {
        }

        public override string InvariantName => GetType().GetTypeInfo().Assembly.GetName().Name;
        public override IDatabaseCreator Creator => GetService<SqlServerDatabaseCreator>();
        public override IRelationalConnection RelationalConnection => GetService<ISqlServerConnection>();
        public override IValueGeneratorSelector ValueGeneratorSelector => GetService<SqlServerValueGeneratorSelector>();
        public override IRelationalDatabaseCreator RelationalDatabaseCreator => GetService<SqlServerDatabaseCreator>();
        public override IConventionSetBuilder ConventionSetBuilder => GetService<SqlServerConventionSetBuilder>();
        public override IMigrationsAnnotationProvider MigrationsAnnotationProvider => GetService<SqlServerMigrationsAnnotationProvider>();
        public override IHistoryRepository HistoryRepository => GetService<SqlServerHistoryRepository>();
        public override IMigrationsSqlGenerator MigrationsSqlGenerator => GetService<SqlServerMigrationsSqlGenerator>();
        public override IModelSource ModelSource => GetService<SqlServerModelSource>();
        public override IUpdateSqlGenerator UpdateSqlGenerator => GetService<ISqlServerUpdateSqlGenerator>();
        public override IValueGeneratorCache ValueGeneratorCache => GetService<ISqlServerValueGeneratorCache>();
        public override IRelationalTypeMapper TypeMapper => GetService<SqlServerTypeMapper>();
        public override IModificationCommandBatchFactory ModificationCommandBatchFactory => GetService<SqlServerModificationCommandBatchFactory>();
        public override IRelationalValueBufferFactoryFactory ValueBufferFactoryFactory => GetService<UntypedRelationalValueBufferFactoryFactory>();
        public override IRelationalMetadataExtensionProvider MetadataExtensionProvider => GetService<SqlServerMetadataExtensionProvider>();
        public override IMethodCallTranslator CompositeMethodCallTranslator => GetService<SqlServerCompositeMethodCallTranslator>();
        public override IMemberTranslator CompositeMemberTranslator => GetService<SqlServerCompositeMemberTranslator>();
        public override IExpressionFragmentTranslator CompositeExpressionFragmentTranslator => GetService<SqlServerCompositeExpressionFragmentTranslator>();
        public override IQueryCompilationContextFactory QueryCompilationContextFactory => GetService<SqlServerQueryCompilationContextFactory>();
        public override ISqlQueryGeneratorFactory SqlQueryGeneratorFactory => GetService<SqlServerQuerySqlGeneratorFactory>();
    }
}
