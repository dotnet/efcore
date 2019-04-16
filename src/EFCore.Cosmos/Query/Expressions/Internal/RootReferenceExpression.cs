// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Expressions.Internal
{
    public class RootReferenceExpression : Expression
    {
        private readonly IEntityType _entityType;
        private readonly string _alias;

        public override ExpressionType NodeType => ExpressionType.Extension;
        public override Type Type => _entityType.ClrType;

        public RootReferenceExpression(IEntityType entityType, string alias)
        {
            _entityType = entityType;
            _alias = alias;
        }

        public override string ToString() => _alias;
    }
}
