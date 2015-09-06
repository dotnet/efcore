// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public class InMemoryEntityQueryableExpressionVisitorFactory : IEntityQueryableExpressionVisitorFactory
    {
        private readonly IModel _model;
        private readonly IEntityKeyFactorySource _entityKeyFactorySource;
        private readonly IMaterializerFactory _materializerFactory;

        public InMemoryEntityQueryableExpressionVisitorFactory(
            [NotNull] IModel model,
            [NotNull] IEntityKeyFactorySource entityKeyFactorySource,
            [NotNull] IMaterializerFactory materializerFactory)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(entityKeyFactorySource, nameof(entityKeyFactorySource));
            Check.NotNull(materializerFactory, nameof(materializerFactory));

            _model = model;
            _entityKeyFactorySource = entityKeyFactorySource;
            _materializerFactory = materializerFactory;
        }

        public virtual ExpressionVisitor Create(
            [NotNull] EntityQueryModelVisitor queryModelVisitor,
            [NotNull] IQuerySource querySource)
            => new InMemoryEntityQueryableExpressionVisitor(
                _model,
                _entityKeyFactorySource,
                _materializerFactory,
                Check.NotNull(queryModelVisitor, nameof(queryModelVisitor)),
                Check.NotNull(querySource, nameof(querySource)));
    }
}
