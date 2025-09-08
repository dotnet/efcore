// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Internal
{
    public interface IXGEvaluatableExpressionFilter
    {
        /// <summary>
        ///     Checks whether the given expression can be evaluated.
        /// </summary>
        /// <param name="expression"> The expression. </param>
        /// <param name="model"> The model. </param>
        /// <returns>
        /// <see langword="true" /> if the expression can be evaluated, <see langword="false" /> if it can't and <see langword="null" /> if
        /// it doesn't handle the given expression.
        /// </returns>
        bool? IsEvaluatableExpression([NotNull] Expression expression, [NotNull] IModel model);
    }
}
