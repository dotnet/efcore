// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors
{
    /// <summary>
    ///     A factory for creating instances of <see cref="RelationalProjectionExpressionVisitor" />.
    /// </summary>
    public class RelationalProjectionExpressionVisitorFactory : IProjectionExpressionVisitorFactory
    {
        private readonly ISqlTranslatingExpressionVisitorFactory _sqlTranslatingExpressionVisitorFactory;
        private readonly IEntityMaterializerSource _entityMaterializerSource;

        /// <summary>
        ///     Creates a new instance of <see cref="RelationalProjectionExpressionVisitorFactory" />.
        /// </summary>
        /// <param name="sqlTranslatingExpressionVisitorFactory"> The SQL translating expression visitor factory. </param>
        /// <param name="entityMaterializerSource"> The entity materializer source. </param>
        public RelationalProjectionExpressionVisitorFactory(
            [NotNull] ISqlTranslatingExpressionVisitorFactory sqlTranslatingExpressionVisitorFactory,
            [NotNull] IEntityMaterializerSource entityMaterializerSource)
        {
            Check.NotNull(sqlTranslatingExpressionVisitorFactory, nameof(sqlTranslatingExpressionVisitorFactory));
            Check.NotNull(entityMaterializerSource, nameof(entityMaterializerSource));

            _sqlTranslatingExpressionVisitorFactory = sqlTranslatingExpressionVisitorFactory;
            _entityMaterializerSource = entityMaterializerSource;
        }

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
                _sqlTranslatingExpressionVisitorFactory,
                _entityMaterializerSource,
                (RelationalQueryModelVisitor)Check.NotNull(entityQueryModelVisitor, nameof(entityQueryModelVisitor)),
                Check.NotNull(querySource, nameof(querySource)));
    }
}
