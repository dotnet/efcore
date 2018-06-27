// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Cosmos.Sql.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Query.Expressions.Internal
{
    public class SelectExpression : Expression
    {
        private const string _rootAlias = "c";
        public EntityProjectionExpression Projection { get; }
        public Expression FromExpression { get; }
        public Expression FilterExpression { get; }

        public SelectExpression(IEntityType entityType)
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
