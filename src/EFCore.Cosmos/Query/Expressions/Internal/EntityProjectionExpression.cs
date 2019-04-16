// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Expressions.Internal
{
    public class EntityProjectionExpression : Expression
    {
        private readonly IEntityType _entityType;
        private readonly string _alias;

        public EntityProjectionExpression(IEntityType entityType, string alias)
        {
            _entityType = entityType;
            IsEntityProjection = true;
            _alias = alias;
        }

        public bool IsEntityProjection { get; set; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override string ToString()
        {
            return IsEntityProjection ? _alias : "";
        }
    }
}
