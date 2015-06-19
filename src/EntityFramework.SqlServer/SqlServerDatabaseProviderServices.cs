// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Migrations.History;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational.Migrations.Sql;
using Microsoft.Data.Entity.Relational.Query.Methods;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.SqlServer.Migrations;
using Microsoft.Data.Entity.SqlServer.Update;
using Microsoft.Data.Entity.SqlServer.ValueGeneration;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.ValueGeneration;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerDatabaseProviderServices : RelationalDatabaseProviderServices
    {
        public SqlServerDatabaseProviderServices([NotNull] IServiceProvider services)
            : base(services)
        {
        }

        public override IDatabase Database => GetService<SqlServerDatabase>();
        public override IDatabaseCreator Creator => GetService<SqlServerDatabaseCreator>();
        public override IDatabaseConnection Connection => GetService<ISqlServerConnection>();
        public override IRelationalConnection RelationalConnection => GetService<ISqlServerConnection>();
        public override IValueGeneratorSelector ValueGeneratorSelector => GetService<SqlServerValueGeneratorSelector>();
        public override IRelationalDatabaseCreator RelationalDatabaseCreator => GetService<SqlServerDatabaseCreator>();
        public override IConventionSetBuilder ConventionSetBuilder => GetService<SqlServerConventionSetBuilder>();
        public override IMigrationAnnotationProvider MigrationAnnotationProvider => GetService<SqlServerMigrationAnnotationProvider>();
        public override IHistoryRepository HistoryRepository => GetService<SqlServerHistoryRepository>();
        public override IMigrationSqlGenerator MigrationSqlGenerator => GetService<SqlServerMigrationSqlGenerator>();
        public override IModelSource ModelSource => GetService<SqlServerModelSource>();
        public override ISqlGenerator SqlGenerator => GetService<ISqlServerSqlGenerator>();
        public override IValueGeneratorCache ValueGeneratorCache => GetService<ISqlServerValueGeneratorCache>();
        public override IRelationalTypeMapper TypeMapper => GetService<SqlServerTypeMapper>();
        public override IModificationCommandBatchFactory ModificationCommandBatchFactory => GetService<SqlServerModificationCommandBatchFactory>();
        public override IRelationalValueBufferFactoryFactory ValueBufferFactoryFactory => GetService<UntypedValueBufferFactoryFactory>();
        public override IRelationalMetadataExtensionProvider MetadataExtensionProvider => GetService<SqlServerMetadataExtensionProvider>();
        public override IMethodCallTranslator CompositeMethodCallTranslator => GetService<SqlServerCompositeMethodCallTranslator>();
        public override IMemberTranslator CompositeMemberTranslator => GetService<SqlServerCompositeMemberTranslator>();
    }
}
