// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using Microsoft.Data.Entity.Relational.Query.Methods;
using Microsoft.Data.Entity.Relational.Query.Sql;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class RelationalQueryCompilationContext : QueryCompilationContext
    {
        private readonly List<RelationalQueryModelVisitor> _relationalQueryModelVisitors
            = new List<RelationalQueryModelVisitor>();

        public RelationalQueryCompilationContext(
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
                Check.NotNull(entityKeyFactorySource, "entityKeyFactorySource"))
        {
            Check.NotNull(queryMethodProvider, "queryMethodProvider");
            Check.NotNull(methodCallTranslator, "methodCallTranslator");

            QueryMethodProvider = queryMethodProvider;
            MethodCallTranslator = methodCallTranslator;
        }

        public override EntityQueryModelVisitor CreateQueryModelVisitor(
            EntityQueryModelVisitor parentEntityQueryModelVisitor)
        {
            var relationalQueryModelVisitor
                = new RelationalQueryModelVisitor(
                    this, (RelationalQueryModelVisitor)parentEntityQueryModelVisitor);

            _relationalQueryModelVisitors.Add(relationalQueryModelVisitor);

            return relationalQueryModelVisitor;
        }

        public virtual SelectExpression FindSelectExpression([NotNull] IQuerySource querySource)
        {
            Check.NotNull(querySource, "querySource");

            return
                (from v in _relationalQueryModelVisitors
                    let selectExpression = v.TryGetQuery(querySource)
                    where selectExpression != null
                    select selectExpression)
                    .Single();
        }

        public virtual IQueryMethodProvider QueryMethodProvider { get; }

        public virtual IMethodCallTranslator MethodCallTranslator { get; }

        public virtual ISqlQueryGenerator CreateSqlQueryGenerator()
        {
            return new DefaultSqlQueryGenerator();
        }

        public virtual string GetTableName([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            return entityType.Relational().Table;
        }

        public virtual string GetSchema([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            return entityType.Relational().Schema;
        }

        public virtual string GetColumnName([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            return property.Relational().Column;
        }
    }
}
