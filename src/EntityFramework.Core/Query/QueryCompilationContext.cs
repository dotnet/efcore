// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query.Annotations;
using Microsoft.Data.Entity.Query.ExpressionVisitors;
using Microsoft.Data.Entity.Query.Internal;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Extensions.Logging;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.Data.Entity.Query
{
    public class QueryCompilationContext
    {
        private readonly IRequiresMaterializationExpressionVisitorFactory _requiresMaterializationExpressionVisitorFactory;
        private readonly IEntityQueryModelVisitorFactory _entityQueryModelVisitorFactory;

        private IReadOnlyCollection<QueryAnnotationBase> _queryAnnotations;
        private IDictionary<IQuerySource, List<IReadOnlyList<INavigation>>> _trackableIncludes;
        private ISet<IQuerySource> _querySourcesRequiringMaterialization;

        public QueryCompilationContext(
            [NotNull] ILogger logger,
            [NotNull] IEntityQueryModelVisitorFactory entityQueryModelVisitorFactory,
            [NotNull] IRequiresMaterializationExpressionVisitorFactory requiresMaterializationExpressionVisitorFactory,
            [NotNull] ILinqOperatorProvider linqOperatorProvider,
            [NotNull] Type contextType,
            bool trackQueryResults)
        {
            Check.NotNull(entityQueryModelVisitorFactory, nameof(entityQueryModelVisitorFactory));
            Check.NotNull(requiresMaterializationExpressionVisitorFactory, nameof(requiresMaterializationExpressionVisitorFactory));
            Check.NotNull(linqOperatorProvider, nameof(linqOperatorProvider));
            Check.NotNull(contextType, nameof(contextType));

            Logger = logger;

            _entityQueryModelVisitorFactory = entityQueryModelVisitorFactory;
            _requiresMaterializationExpressionVisitorFactory = requiresMaterializationExpressionVisitorFactory;

            LinqOperatorProvider = linqOperatorProvider;
            ContextType = contextType;
            TrackQueryResults = trackQueryResults;
        }

        public virtual ILogger Logger { get; }
        public virtual ILinqOperatorProvider LinqOperatorProvider { get; }

        public virtual Type ContextType { get; }
        public virtual bool TrackQueryResults { get; }

        public virtual QuerySourceMapping QuerySourceMapping { get; } = new QuerySourceMapping();

        public virtual IReadOnlyCollection<QueryAnnotationBase> QueryAnnotations
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
                        .OfType<QueryAnnotation>()
                        .LastOrDefault(
                            qa => qa.IsCallTo(EntityFrameworkQueryableExtensions.AsNoTrackingMethodInfo)
                                  || qa.IsCallTo(EntityFrameworkQueryableExtensions.AsTrackingMethodInfo));

                return lastTrackingModifier
                    ?.IsCallTo(EntityFrameworkQueryableExtensions.AsTrackingMethodInfo)
                       ?? TrackQueryResults;
            }
        }

        public virtual bool IsQueryBufferRequired { get; private set; }

        public virtual void DetermineQueryBufferRequirement([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            IsQueryBufferRequired
                = IsTrackingQuery
                  || QueryAnnotations.OfType<IncludeQueryAnnotation>().Any()
                  || new ShadowAccessFindingExpressionVisitor().AnyShadowAccess(queryModel);
        }

        private class ShadowAccessFindingExpressionVisitor : ExpressionVisitorBase
        {
            private bool _anyShadowAccess;

            public bool AnyShadowAccess(QueryModel queryModel)
            {
                queryModel.TransformExpressions(Visit);

                return _anyShadowAccess;
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Method.IsGenericMethod
                    && ReferenceEquals(
                        methodCallExpression.Method.GetGenericMethodDefinition(),
                        EntityQueryModelVisitor.PropertyMethodInfo))
                {
                    _anyShadowAccess = true;
                }

                return base.VisitMethodCall(methodCallExpression);
            }

            protected override Expression VisitSubQuery(SubQueryExpression expression)
            {
                expression.QueryModel.TransformExpressions(Visit);

                return expression;
            }
        }

        public virtual IEnumerable<QueryAnnotation> GetCustomQueryAnnotations([NotNull] MethodInfo methodInfo)
            => _queryAnnotations
                .OfType<QueryAnnotation>()
                .Where(qa => qa.IsCallTo(Check.NotNull(methodInfo, nameof(methodInfo))));

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
