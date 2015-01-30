// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Relational.Query;
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
            [NotNull] EntityMaterializerSource entityMaterializerSource,
            [NotNull] EntityKeyFactorySource entityKeyFactorySource,
            [NotNull] IQueryMethodProvider queryMethodProvider,
            [NotNull] IMethodCallTranslator methodCallTranslator)
            : base(
                Check.NotNull(model, "model"),
                Check.NotNull(logger, "logger"),
                Check.NotNull(linqOperatorProvider, "linqOperatorProvider"),
                Check.NotNull(resultOperatorHandler, "resultOperatorHandler"),
                Check.NotNull(entityMaterializerSource, "entityMaterializerSource"),
                Check.NotNull(entityKeyFactorySource, "entityKeyFactorySource"),
                Check.NotNull(queryMethodProvider, "queryMethodProvider"),
                Check.NotNull(methodCallTranslator, "methodCallTranslator"))
        {
        }

        public override ISqlQueryGenerator CreateSqlQueryGenerator()
        {
            return new SqlServerQueryGenerator();
        }

        public override string GetTableName(IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            return entityType.SqlServer().Table;
        }

        public override string GetSchema(IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            return entityType.SqlServer().Schema;
        }
        
        public override string GetColumnName(IProperty property)
        {
            Check.NotNull(property, "property");

            return property.SqlServer().Column;
        }
    }
}
