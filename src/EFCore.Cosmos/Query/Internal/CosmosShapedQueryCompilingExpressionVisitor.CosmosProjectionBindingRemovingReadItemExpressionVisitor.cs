// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

#nullable disable

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal
{
    public partial class CosmosShapedQueryCompilingExpressionVisitor
    {
        private sealed class CosmosProjectionBindingRemovingReadItemExpressionVisitor : CosmosProjectionBindingRemovingExpressionVisitorBase
        {
            private readonly ReadItemExpression _readItemExpression;

            public CosmosProjectionBindingRemovingReadItemExpressionVisitor(
                ReadItemExpression readItemExpression,
                ParameterExpression jObjectParameter,
                bool trackQueryResults)
                : base(jObjectParameter, trackQueryResults)
            {
                _readItemExpression = readItemExpression;
            }

            protected override ProjectionExpression GetProjection(ProjectionBindingExpression _)
                => _readItemExpression.ProjectionExpression;
        }
    }
}
