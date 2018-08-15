// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Cosmos.Sql.Query.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Sql.Storage.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Query.ExpressionVisitors.Internal
{
    public class CosmosSqlEntityQueryableExpressionVisitorFactory : IEntityQueryableExpressionVisitorFactory
    {
        private readonly IModel _model;
        private readonly CosmosClient _cosmosClient;
        private readonly IEntityMaterializerSource _entityMaterializerSource;

        public CosmosSqlEntityQueryableExpressionVisitorFactory(
            IModel model,
            CosmosClient cosmosClient,
            IEntityMaterializerSource entityMaterializerSource)
        {
            _model = model;
            _cosmosClient = cosmosClient;
            _entityMaterializerSource = entityMaterializerSource;
        }

        public ExpressionVisitor Create(EntityQueryModelVisitor entityQueryModelVisitor, IQuerySource querySource)
        {
            return new CosmosSqlEntityQueryableExpressionVisitor(
                _model,
                _cosmosClient,
                _entityMaterializerSource,
                (CosmosSqlQueryModelVisitor)entityQueryModelVisitor,
                querySource);
        }
    }
}
