// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors
{
    /// <summary>
    ///     A factory for creating instances of <see cref="RelationalProjectionExpressionVisitor" />.
    /// </summary>
    public class RelationalProjectionExpressionVisitorFactory : IProjectionExpressionVisitorFactory
    {
        /// <summary>
        ///     Creates a new instance of <see cref="RelationalProjectionExpressionVisitorFactory" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public RelationalProjectionExpressionVisitorFactory(
            [NotNull] RelationalProjectionExpressionVisitorDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Dependencies used to create a <see cref="RelationalProjectionExpressionVisitorFactory" />
        /// </summary>
        protected virtual RelationalProjectionExpressionVisitorDependencies Dependencies { get; }

        /// <summary>
        ///     Creates a new ExpressionVisitor.
        /// </summary>
        /// <param name="entityQueryModelVisitor"> The query model visitor. </param>
        /// <param name="querySource"> The query source. </param>
        /// <returns>
        ///     An ExpressionVisitor.
        /// </returns>
        public virtual ExpressionVisitor Create(
            EntityQueryModelVisitor entityQueryModelVisitor, IQuerySource querySource)
            => new RelationalProjectionExpressionVisitor(
                Dependencies,
                (RelationalQueryModelVisitor)Check.NotNull(entityQueryModelVisitor, nameof(entityQueryModelVisitor)),
                Check.NotNull(querySource, nameof(querySource)));
    }
}
