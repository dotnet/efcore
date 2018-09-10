// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Sql.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Sql.Query.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Query.ExpressionVisitors.Internal
{
    public class CosmosSqlEntityQueryableExpressionVisitor : EntityQueryableExpressionVisitor
    {
        private readonly IModel _model;
        private readonly IQuerySource _querySource;
        private readonly IEntityMaterializerSource _entityMaterializerSource;

        public CosmosSqlEntityQueryableExpressionVisitor(
            IModel model,
            IEntityMaterializerSource entityMaterializerSource,
            CosmosSqlQueryModelVisitor cosmosSqlQueryModelVisitor,
            IQuerySource querySource)
            : base(cosmosSqlQueryModelVisitor)
        {
            _model = model;
            _querySource = querySource;
            _entityMaterializerSource = entityMaterializerSource;
        }

        public new CosmosSqlQueryModelVisitor QueryModelVisitor => (CosmosSqlQueryModelVisitor)base.QueryModelVisitor;

        protected override Expression VisitEntityQueryable([NotNull] Type elementType)
        {
            var entityType = _model.FindEntityType(elementType);

            return new QueryShaperExpression(
                QueryModelVisitor.QueryCompilationContext.IsAsyncQuery,
                new DocumentQueryExpression(
                    QueryModelVisitor.QueryCompilationContext.IsAsyncQuery,
                    entityType.CosmosSql().CollectionName,
                    new SelectExpression(entityType, _querySource)),
                new EntityShaper(entityType,
                    trackingQuery: QueryModelVisitor.QueryCompilationContext.IsTrackingQuery
                        && !entityType.IsQueryType,
                    useQueryBuffer: QueryModelVisitor.QueryCompilationContext.IsQueryBufferRequired
                        && !entityType.IsQueryType,
                    _entityMaterializerSource));
        }
    }
}
