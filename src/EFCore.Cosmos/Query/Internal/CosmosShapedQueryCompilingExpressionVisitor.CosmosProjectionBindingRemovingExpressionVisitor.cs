// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

public partial class CosmosShapedQueryCompilingExpressionVisitor
{
    private sealed class CosmosProjectionBindingRemovingExpressionVisitor(
        SelectExpression selectExpression,
        ParameterExpression jTokenParameter,
        bool trackQueryResults)
        : CosmosProjectionBindingRemovingExpressionVisitorBase(jTokenParameter, trackQueryResults)
    {
        protected override ProjectionExpression GetProjection(ProjectionBindingExpression projectionBindingExpression)
            => selectExpression.Projection[GetProjectionIndex(projectionBindingExpression)];

        private int GetProjectionIndex(ProjectionBindingExpression projectionBindingExpression)
            => projectionBindingExpression.ProjectionMember != null
                ? selectExpression.GetMappedProjection(projectionBindingExpression.ProjectionMember).GetConstantValue<int>()
                : (projectionBindingExpression.Index
                    ?? throw new InvalidOperationException(CoreStrings.TranslationFailed(projectionBindingExpression.Print())));
    }
}
