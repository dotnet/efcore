// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query.Expressions;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public class RelationalEntityQueryableExpressionVisitorFactory : IEntityQueryableExpressionVisitorFactory
    {
        private readonly IModel _model;
        private readonly IKeyValueFactorySource _keyValueFactorySource;
        private readonly ISelectExpressionFactory _selectExpressionFactory;
        private readonly IMaterializerFactory _materializerFactory;
        private readonly ICommandBuilderFactory _commandBuilderFactory;
        private readonly IRelationalAnnotationProvider _relationalAnnotationProvider;

        public RelationalEntityQueryableExpressionVisitorFactory(
            [NotNull] IModel model,
            [NotNull] IKeyValueFactorySource keyValueFactorySource,
            [NotNull] ISelectExpressionFactory selectExpressionFactory,
            [NotNull] IMaterializerFactory materializerFactory,
            [NotNull] ICommandBuilderFactory commandBuilderFactory,
            [NotNull] IRelationalAnnotationProvider relationalAnnotationProvider)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(keyValueFactorySource, nameof(keyValueFactorySource));
            Check.NotNull(selectExpressionFactory, nameof(selectExpressionFactory));
            Check.NotNull(materializerFactory, nameof(materializerFactory));
            Check.NotNull(commandBuilderFactory, nameof(commandBuilderFactory));
            Check.NotNull(relationalAnnotationProvider, nameof(relationalAnnotationProvider));

            _model = model;
            _keyValueFactorySource = keyValueFactorySource;
            _selectExpressionFactory = selectExpressionFactory;
            _materializerFactory = materializerFactory;
            _commandBuilderFactory = commandBuilderFactory;
            _relationalAnnotationProvider = relationalAnnotationProvider;
        }

        public virtual ExpressionVisitor Create(
            [NotNull] EntityQueryModelVisitor queryModelVisitor,
            [NotNull] IQuerySource querySource)
            => new RelationalEntityQueryableExpressionVisitor(
                _model,
                _keyValueFactorySource,
                _selectExpressionFactory,
                _materializerFactory,
                _commandBuilderFactory,
                _relationalAnnotationProvider,
                (RelationalQueryModelVisitor)Check.NotNull(queryModelVisitor, nameof(queryModelVisitor)),
                Check.NotNull(querySource, nameof(querySource)));
    }
}
