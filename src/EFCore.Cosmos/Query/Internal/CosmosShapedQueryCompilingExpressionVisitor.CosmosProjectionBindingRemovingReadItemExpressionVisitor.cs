// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

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
