// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Query.Expressions;
using Microsoft.Data.Entity.Query.Methods;
using Microsoft.Data.Entity.Query.Sql;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.SqlServer.Query
{
    public class SqlServerQueryCompilationContext : RelationalQueryCompilationContext
    {
        public SqlServerQueryCompilationContext(
            [NotNull] IModel model,
            [NotNull] ILogger logger,
            [NotNull] ILinqOperatorProvider linqOperatorProvider,
            [NotNull] IResultOperatorHandler resultOperatorHandler,
            [NotNull] IEntityMaterializerSource entityMaterializerSource,
            [NotNull] IEntityKeyFactorySource entityKeyFactorySource,
            [NotNull] IClrAccessorSource<IClrPropertyGetter> clrPropertyGetterSource,
            [NotNull] IQueryMethodProvider queryMethodProvider,
            [NotNull] IMethodCallTranslator compositeMethodCallTranslator,
            [NotNull] IMemberTranslator compositeMemberTranslator,
            [NotNull] IRelationalValueBufferFactoryFactory valueBufferFactoryFactory,
            [NotNull] IRelationalTypeMapper typeMapper)
            : base(
                model,
                logger,
                linqOperatorProvider,
                resultOperatorHandler,
                entityMaterializerSource,
                entityKeyFactorySource,
                clrPropertyGetterSource,
                queryMethodProvider,
                compositeMethodCallTranslator,
                compositeMemberTranslator,
                valueBufferFactoryFactory,
                typeMapper)
        {
        }

        public override ISqlQueryGenerator CreateSqlQueryGenerator(SelectExpression selectExpression)
            => new SqlServerQuerySqlGenerator(Check.NotNull(selectExpression, nameof(selectExpression)), TypeMapper);

        public override string GetTableName(IEntityType entityType)
            => Check.NotNull(entityType, nameof(entityType)).SqlServer().Table;

        public override string GetSchema(IEntityType entityType)
            => Check.NotNull(entityType, nameof(entityType)).SqlServer().Schema;

        public override string GetColumnName(IProperty property)
            => Check.NotNull(property, nameof(property)).SqlServer().Column;
    }
}
