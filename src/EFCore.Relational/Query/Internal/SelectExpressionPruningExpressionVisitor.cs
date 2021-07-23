﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SelectExpressionPruningExpressionVisitor : ExpressionVisitor
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
        {
            switch (expression)
            {
                case ShapedQueryExpression shapedQueryExpression:
                    return shapedQueryExpression.Update(
                        ((SelectExpression)shapedQueryExpression.QueryExpression).Prune(),
                        Visit(shapedQueryExpression.ShaperExpression));

                case RelationalSplitCollectionShaperExpression relationalSplitCollectionShaperExpression:
                    return relationalSplitCollectionShaperExpression.Update(
                        relationalSplitCollectionShaperExpression.ParentIdentifier,
                        relationalSplitCollectionShaperExpression.ChildIdentifier,
                        relationalSplitCollectionShaperExpression.SelectExpression.Prune(),
                        Visit(relationalSplitCollectionShaperExpression.InnerShaper));

                default:
                    return base.Visit(expression);
            }
        }
    }
}
