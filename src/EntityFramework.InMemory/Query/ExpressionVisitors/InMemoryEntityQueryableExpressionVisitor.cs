// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public class InMemoryEntityQueryableExpressionVisitor : EntityQueryableExpressionVisitor
    {
        private readonly IModel _model;
        private readonly IEntityKeyFactorySource _entityKeyFactorySource;
        private readonly IMaterializerFactory _materializerFactory;

        private IQuerySource _querySource;

        public InMemoryEntityQueryableExpressionVisitor(
            [NotNull] IModel model,
            [NotNull] IEntityKeyFactorySource entityKeyFactorySource,
            [NotNull] IMaterializerFactory materializerFactory,
            [NotNull] EntityQueryModelVisitor entityQueryModelVisitor,
            [NotNull] IQuerySource querySource)
            : base(Check.NotNull(entityQueryModelVisitor, nameof(entityQueryModelVisitor)))
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(entityKeyFactorySource, nameof(entityKeyFactorySource));
            Check.NotNull(materializerFactory, nameof(materializerFactory));
            Check.NotNull(querySource, nameof(querySource));

            _model = model;
            _entityKeyFactorySource = entityKeyFactorySource;
            _materializerFactory = materializerFactory;
            _querySource = querySource;
        }

        private new InMemoryQueryModelVisitor QueryModelVisitor => (InMemoryQueryModelVisitor)base.QueryModelVisitor;

        protected override Expression VisitEntityQueryable(Type elementType)
        {
            Check.NotNull(elementType, nameof(elementType));

            var entityType = _model.GetEntityType(elementType);

            var keyProperties
                = entityType.GetPrimaryKey().Properties;

            var keyFactory = _entityKeyFactorySource.GetKeyFactory(entityType.GetPrimaryKey());

            Func<ValueBuffer, EntityKey> entityKeyFactory
                = vr => keyFactory.Create(keyProperties, vr);

            if (QueryModelVisitor.QueryCompilationContext
                .QuerySourceRequiresMaterialization(_querySource))
            {
                var materializer = _materializerFactory.CreateMaterializer(entityType);

                return Expression.Call(
                    InMemoryQueryModelVisitor.EntityQueryMethodInfo.MakeGenericMethod(elementType),
                    EntityQueryModelVisitor.QueryContextParameter,
                    Expression.Constant(entityType),
                    Expression.Constant(entityKeyFactory),
                    materializer,
                    Expression.Constant(QueryModelVisitor.QuerySourceRequiresTracking(_querySource)));
            }

            return Expression.Call(
                InMemoryQueryModelVisitor.ProjectionQueryMethodInfo,
                EntityQueryModelVisitor.QueryContextParameter,
                Expression.Constant(entityType));
        }
    }
}
