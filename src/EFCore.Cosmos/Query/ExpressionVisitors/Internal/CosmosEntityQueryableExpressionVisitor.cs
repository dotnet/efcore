// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.ExpressionVisitors.Internal
{
    public class CosmosEntityQueryableExpressionVisitor : EntityQueryableExpressionVisitor
    {
        private readonly IModel _model;
        private readonly IQuerySource _querySource;
        private readonly IEntityMaterializerSource _entityMaterializerSource;

        public CosmosEntityQueryableExpressionVisitor(
            IModel model,
            IEntityMaterializerSource entityMaterializerSource,
            CosmosQueryModelVisitor cosmosQueryModelVisitor,
            IQuerySource querySource)
            : base(cosmosQueryModelVisitor)
        {
            _model = model;
            _querySource = querySource;
            _entityMaterializerSource = entityMaterializerSource;
        }

        public new CosmosQueryModelVisitor QueryModelVisitor => (CosmosQueryModelVisitor)base.QueryModelVisitor;

        protected override Expression VisitEntityQueryable([NotNull] Type elementType)
        {
            var entityType = _model.FindEntityType(elementType);
            if (!entityType.IsDocumentRoot())
            {
                throw new InvalidOperationException(
                    CosmosStrings.QueryRootNestedEntityType(entityType.DisplayName(), entityType.FindOwnership().PrincipalEntityType.DisplayName()));
            }

            return new QueryShaperExpression(
                QueryModelVisitor.QueryCompilationContext.IsAsyncQuery,
                new DocumentQueryExpression(
                    QueryModelVisitor.QueryCompilationContext.IsAsyncQuery,
                    entityType.Cosmos().ContainerName,
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
