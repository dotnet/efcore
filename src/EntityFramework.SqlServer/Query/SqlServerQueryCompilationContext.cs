// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Relational.Query;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using Microsoft.Data.Entity.Relational.Query.Methods;
using Microsoft.Data.Entity.Relational.Query.Sql;
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
            [NotNull] IQueryMethodProvider queryMethodProvider,
            [NotNull] IMethodCallTranslator methodCallTranslator,
            [NotNull] ISqlServerValueReaderFactoryFactory valueReaderFactoryFactory)
            : base(
                Check.NotNull(model, nameof(model)),
                Check.NotNull(logger, nameof(logger)),
                Check.NotNull(linqOperatorProvider, nameof(linqOperatorProvider)),
                Check.NotNull(resultOperatorHandler, nameof(resultOperatorHandler)),
                Check.NotNull(entityMaterializerSource, nameof(entityMaterializerSource)),
                Check.NotNull(entityKeyFactorySource, nameof(entityKeyFactorySource)),
                Check.NotNull(queryMethodProvider, nameof(queryMethodProvider)),
                Check.NotNull(methodCallTranslator, nameof(methodCallTranslator)),
                Check.NotNull(valueReaderFactoryFactory, nameof(valueReaderFactoryFactory)))
        {
        }

        public override ISqlQueryGenerator CreateSqlQueryGenerator(SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            return new SqlServerQueryGenerator(selectExpression);
        }

        public override string GetTableName(IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.SqlServer().Table;
        }

        public override string GetSchema(IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.SqlServer().Schema;
        }

        public override string GetColumnName(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return property.SqlServer().Column;
        }
    }
}
