// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors
{
    /// <summary>
    ///     A factory for creating projection expression visitors.
    /// </summary>
    public interface IProjectionExpressionVisitorFactory
    {
        /// <summary>
        ///     Creates a new ExpressionVisitor.
        /// </summary>
        /// <param name="entityQueryModelVisitor"> The entity query model visitor. </param>
        /// <param name="querySource"> The query source. </param>
        /// <returns>
        ///     An ExpressionVisitor.
        /// </returns>
        ExpressionVisitor Create(
            [NotNull] EntityQueryModelVisitor entityQueryModelVisitor,
            [NotNull] IQuerySource querySource);
    }
}
