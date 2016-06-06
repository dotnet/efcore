// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors
{
    /// <summary>
    ///     A factory for creating instances of <see cref="RelationalEntityQueryableExpressionVisitor" />.
    /// </summary>
    public class RelationalEntityQueryableExpressionVisitorFactory : IEntityQueryableExpressionVisitorFactory
    {
        private readonly IModel _model;
        private readonly ISelectExpressionFactory _selectExpressionFactory;
        private readonly IMaterializerFactory _materializerFactory;
        private readonly IShaperCommandContextFactory _shaperCommandContextFactory;
        private readonly IRelationalAnnotationProvider _relationalAnnotationProvider;

        /// <summary>
        ///     Creates a new instance of <see cref="RelationalEntityQueryableExpressionVisitorFactory" />.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="selectExpressionFactory"> The select expression factory. </param>
        /// <param name="materializerFactory"> The materializer factory. </param>
        /// <param name="shaperCommandContextFactory"> The shaper command context factory. </param>
        /// <param name="relationalAnnotationProvider"> The relational annotation provider. </param>
        public RelationalEntityQueryableExpressionVisitorFactory(
            [NotNull] IModel model,
            [NotNull] ISelectExpressionFactory selectExpressionFactory,
            [NotNull] IMaterializerFactory materializerFactory,
            [NotNull] IShaperCommandContextFactory shaperCommandContextFactory,
            [NotNull] IRelationalAnnotationProvider relationalAnnotationProvider)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(selectExpressionFactory, nameof(selectExpressionFactory));
            Check.NotNull(materializerFactory, nameof(materializerFactory));
            Check.NotNull(shaperCommandContextFactory, nameof(shaperCommandContextFactory));
            Check.NotNull(relationalAnnotationProvider, nameof(relationalAnnotationProvider));

            _model = model;
            _selectExpressionFactory = selectExpressionFactory;
            _materializerFactory = materializerFactory;
            _shaperCommandContextFactory = shaperCommandContextFactory;
            _relationalAnnotationProvider = relationalAnnotationProvider;
        }

        /// <summary>
        ///     Creates a new ExpressionVisitor.
        /// </summary>
        /// <param name="queryModelVisitor"> The query model visitor. </param>
        /// <param name="querySource"> The query source. </param>
        /// <returns>
        ///     An ExpressionVisitor.
        /// </returns>
        public virtual ExpressionVisitor Create(
            EntityQueryModelVisitor queryModelVisitor, IQuerySource querySource)
            => new RelationalEntityQueryableExpressionVisitor(
                _model,
                _selectExpressionFactory,
                _materializerFactory,
                _shaperCommandContextFactory,
                _relationalAnnotationProvider,
                (RelationalQueryModelVisitor)Check.NotNull(queryModelVisitor, nameof(queryModelVisitor)),
                querySource);
    }
}
