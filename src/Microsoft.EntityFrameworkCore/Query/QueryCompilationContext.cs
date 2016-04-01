// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.ResultOperators;
using Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class QueryCompilationContext
    {
        private readonly IRequiresMaterializationExpressionVisitorFactory _requiresMaterializationExpressionVisitorFactory;
        private readonly IEntityQueryModelVisitorFactory _entityQueryModelVisitorFactory;

        private IReadOnlyCollection<IQueryAnnotation> _queryAnnotations;
        private IDictionary<IQuerySource, List<IReadOnlyList<INavigation>>> _trackableIncludes;
        private ISet<IQuerySource> _querySourcesRequiringMaterialization;

        public QueryCompilationContext(
            [NotNull] IModel model,
            [NotNull] ILogger logger,
            [NotNull] IEntityQueryModelVisitorFactory entityQueryModelVisitorFactory,
            [NotNull] IRequiresMaterializationExpressionVisitorFactory requiresMaterializationExpressionVisitorFactory,
            [NotNull] ILinqOperatorProvider linqOperatorProvider,
            [NotNull] Type contextType,
            bool trackQueryResults)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(entityQueryModelVisitorFactory, nameof(entityQueryModelVisitorFactory));
            Check.NotNull(requiresMaterializationExpressionVisitorFactory, nameof(requiresMaterializationExpressionVisitorFactory));
            Check.NotNull(linqOperatorProvider, nameof(linqOperatorProvider));
            Check.NotNull(contextType, nameof(contextType));

            Model = model;
            Logger = logger;

            _entityQueryModelVisitorFactory = entityQueryModelVisitorFactory;
            _requiresMaterializationExpressionVisitorFactory = requiresMaterializationExpressionVisitorFactory;

            LinqOperatorProvider = linqOperatorProvider;
            ContextType = contextType;
            TrackQueryResults = trackQueryResults;
        }

        public virtual IModel Model { get; }
        public virtual ILogger Logger { get; }
        public virtual ILinqOperatorProvider LinqOperatorProvider { get; }

        public virtual Type ContextType { get; }
        public virtual bool TrackQueryResults { get; }

        public virtual QuerySourceMapping QuerySourceMapping { get; } = new QuerySourceMapping();

        public virtual IReadOnlyCollection<IQueryAnnotation> QueryAnnotations
        {
            get { return _queryAnnotations; }
            [param: NotNull]
            set
            {
                Check.NotNull(value, nameof(value));

                _queryAnnotations = value;
            }
        }

        public virtual bool IsTrackingQuery
        {
            get
            {
                var lastTrackingModifier
                    = QueryAnnotations
                        .OfType<TrackingResultOperator>()
                        .LastOrDefault();

                return lastTrackingModifier?.IsTracking ?? TrackQueryResults;
            }
        }

        public virtual bool IsIncludeQuery => QueryAnnotations.OfType<IncludeResultOperator>().Any();

        public virtual bool IsQueryBufferRequired { get; private set; }

        public virtual void DetermineQueryBufferRequirement([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            IsQueryBufferRequired
                = QueryAnnotations.OfType<IncludeResultOperator>().Any()
                  || new RequiresBufferingExpressionVisitor(Model).RequiresBuffering(queryModel);
        }

        private class RequiresBufferingExpressionVisitor : ExpressionVisitorBase
        {
            private readonly IModel _model;

            private int _referencedEntityTypes;
            private bool _requiresBuffering;

            public RequiresBufferingExpressionVisitor(IModel model)
            {
                _model = model;
            }

            public bool RequiresBuffering(QueryModel queryModel)
            {
                queryModel.TransformExpressions(Visit);

                return _requiresBuffering;
            }

            public override Expression Visit(Expression expression)
                => _requiresBuffering ? expression : base.Visit(expression);

            protected override Expression VisitConstant(ConstantExpression constantExpression)
            {
                if (constantExpression.Type.GetTypeInfo().IsGenericType
                    && constantExpression.Type.GetGenericTypeDefinition() == typeof(EntityQueryable<>))
                {
                    var entityType = _model.FindEntityType(((IQueryable)constantExpression.Value).ElementType);

                    if (entityType != null)
                    {
                        if (_referencedEntityTypes > 0
                            || entityType.ShadowPropertyCount() > 0)
                        {
                            _requiresBuffering = true;

                            return constantExpression;
                        }

                        _referencedEntityTypes++;
                    }
                }

                return base.VisitConstant(constantExpression);
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                if (EntityQueryModelVisitor.IsPropertyMethod(methodCallExpression.Method))
                {
                    _requiresBuffering = true;

                    return methodCallExpression;
                }

                return base.VisitMethodCall(methodCallExpression);
            }

            protected override Expression VisitSubQuery(SubQueryExpression expression)
            {
                expression.QueryModel.TransformExpressions(Visit);

                return expression;
            }
        }

        public virtual EntityQueryModelVisitor CreateQueryModelVisitor()
            => CreateQueryModelVisitor(parentEntityQueryModelVisitor: null);

        public virtual EntityQueryModelVisitor CreateQueryModelVisitor(
            [CanBeNull] EntityQueryModelVisitor parentEntityQueryModelVisitor)
            => _entityQueryModelVisitorFactory.Create(this, parentEntityQueryModelVisitor);

        public virtual void AddTrackableInclude(
            [NotNull] IQuerySource querySource, [NotNull] IReadOnlyList<INavigation> navigationPath)
        {
            Check.NotNull(querySource, nameof(querySource));
            Check.NotNull(navigationPath, nameof(navigationPath));

            if (_trackableIncludes == null)
            {
                _trackableIncludes = new Dictionary<IQuerySource, List<IReadOnlyList<INavigation>>>();
            }

            List<IReadOnlyList<INavigation>> includes;
            if (!_trackableIncludes.TryGetValue(querySource, out includes))
            {
                _trackableIncludes.Add(querySource, includes = new List<IReadOnlyList<INavigation>>());
            }

            includes.Add(navigationPath);
        }

        public virtual IReadOnlyList<IReadOnlyList<INavigation>> GetTrackableIncludes([NotNull] IQuerySource querySource)
        {
            Check.NotNull(querySource, nameof(querySource));

            if (_trackableIncludes == null)
            {
                return null;
            }

            List<IReadOnlyList<INavigation>> includes;

            return _trackableIncludes.TryGetValue(querySource, out includes) ? includes : null;
        }

        public virtual void FindQuerySourcesRequiringMaterialization(
            [NotNull] EntityQueryModelVisitor queryModelVisitor, [NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModelVisitor, nameof(queryModelVisitor));
            Check.NotNull(queryModel, nameof(queryModel));

            _querySourcesRequiringMaterialization
                = _requiresMaterializationExpressionVisitorFactory
                    .Create(queryModelVisitor)
                    .FindQuerySourcesRequiringMaterialization(queryModel);

            foreach (var groupJoinClause in queryModel.BodyClauses.OfType<GroupJoinClause>())
            {
                _querySourcesRequiringMaterialization.Add(groupJoinClause.JoinClause);
            }
        }

        public virtual bool QuerySourceRequiresMaterialization([NotNull] IQuerySource querySource)
        {
            Check.NotNull(querySource, nameof(querySource));

            return _querySourcesRequiringMaterialization.Contains(querySource);
        }
    }
}
