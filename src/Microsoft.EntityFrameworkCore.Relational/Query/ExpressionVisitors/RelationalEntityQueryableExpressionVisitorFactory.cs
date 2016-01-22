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
    public class RelationalEntityQueryableExpressionVisitorFactory : IEntityQueryableExpressionVisitorFactory
    {
        private readonly IModel _model;
        private readonly ISelectExpressionFactory _selectExpressionFactory;
        private readonly IMaterializerFactory _materializerFactory;
        private readonly IShaperCommandContextFactory _shaperCommandContextFactory;
        private readonly IRelationalAnnotationProvider _relationalAnnotationProvider;

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
