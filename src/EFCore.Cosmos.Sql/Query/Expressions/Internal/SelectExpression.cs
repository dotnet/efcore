// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Cosmos.Sql.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Query.Expressions.Internal
{
    public class SelectExpression : Expression
    {
        private const string _rootAlias = "c";
        private readonly IQuerySource _querySource;

        public EntityProjectionExpression Projection { get; }
        public Expression FromExpression { get; }
        public Expression FilterExpression { get; private set; }

        public SelectExpression(IEntityType entityType, IQuerySource querySource)
        {
            Projection = new EntityProjectionExpression(entityType, _rootAlias);

            FromExpression = new RootReferenceExpression(entityType, _rootAlias);

            // Add discriminator predicate
            var discriminatorProperty = entityType.CosmosSql().DiscriminatorProperty;

            FilterExpression = MakeBinary(
                ExpressionType.Equal,
                new KeyAccessExpression(discriminatorProperty, FromExpression),
                Constant(entityType.CosmosSql().DiscriminatorValue, discriminatorProperty.ClrType));

            EntityType = entityType;
            _querySource = querySource;
        }

        public Expression BindPropertyPath(
            QuerySourceReferenceExpression querySourceReferenceExpression, List<IPropertyBase> properties)
        {
            if (querySourceReferenceExpression.ReferencedQuerySource != _querySource)
            {
                return null;
            }

            var currentExpression = FromExpression;

            foreach (var property in properties)
            {
                currentExpression = new KeyAccessExpression(property, currentExpression);
            }

            return currentExpression;
        }

        public void AddToPredicate(Expression predicate)
        {
            FilterExpression = AndAlso(FilterExpression, predicate);
        }

        public override Type Type => typeof(JObject);
        public override ExpressionType NodeType => ExpressionType.Extension;

        public IEntityType EntityType { get; }

        public override string ToString()
        {
            return new CosmosSqlGenerator().GenerateSql(this);
        }
    }
}
