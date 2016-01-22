// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Query.Sql.Internal;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
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
        public override ISqlGenerationHelper SqlGenerationHelper => GetService<SqlServerSqlGenerationHelper>();
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
        public override IRelationalAnnotationProvider AnnotationProvider => GetService<SqlServerAnnotationProvider>();
        public override IMethodCallTranslator CompositeMethodCallTranslator => GetService<SqlServerCompositeMethodCallTranslator>();
        public override IMemberTranslator CompositeMemberTranslator => GetService<SqlServerCompositeMemberTranslator>();
        public override IQueryCompilationContextFactory QueryCompilationContextFactory => GetService<SqlServerQueryCompilationContextFactory>();
        public override IQuerySqlGeneratorFactory QuerySqlGeneratorFactory => GetService<SqlServerQuerySqlGeneratorFactory>();
        public override IEntityQueryModelVisitorFactory EntityQueryModelVisitorFactory => GetService<SqlServerQueryModelVisitorFactory>();
        public override ICompiledQueryCacheKeyGenerator CompiledQueryCacheKeyGenerator => GetService<SqlServerCompiledQueryCacheKeyGenerator>();
        public override IModelValidator ModelValidator => GetService<SqlServerModelValidator>();
    }
}
