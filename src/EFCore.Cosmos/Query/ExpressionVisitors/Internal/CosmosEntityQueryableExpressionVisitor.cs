// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Query.Sql;
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
        private readonly ISqlGeneratorFactory _sqlGeneratorFactory;

        public CosmosEntityQueryableExpressionVisitor(
            IModel model,
            IEntityMaterializerSource entityMaterializerSource,
            CosmosQueryModelVisitor cosmosQueryModelVisitor,
            IQuerySource querySource,
            ISqlGeneratorFactory sqlGeneratorFactory)
            : base(cosmosQueryModelVisitor)
        {
            _model = model;
            _querySource = querySource;
            _entityMaterializerSource = entityMaterializerSource;
            _sqlGeneratorFactory = sqlGeneratorFactory;
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
                    new SelectExpression(entityType, _querySource, _sqlGeneratorFactory)),
                new EntityShaper(
                    entityType,
                    trackingQuery: QueryModelVisitor.QueryCompilationContext.IsTrackingQuery
                                   && entityType.FindPrimaryKey() != null,
                    useQueryBuffer: QueryModelVisitor.QueryCompilationContext.IsQueryBufferRequired
                                    && entityType.FindPrimaryKey() != null,
                    _entityMaterializerSource));
        }
    }
}
