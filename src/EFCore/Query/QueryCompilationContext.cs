// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.ResultOperators;
using Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     A query compilation context. The primary data structure representing the state/components
    ///     used during query compilation.
    /// </summary>
    public class QueryCompilationContext
    {
        private readonly IRequiresMaterializationExpressionVisitorFactory _requiresMaterializationExpressionVisitorFactory;
        private readonly IEntityQueryModelVisitorFactory _entityQueryModelVisitorFactory;

        private readonly Dictionary<IQuerySource, IEntityType> _querySourceEntityTypeMapping = new Dictionary<IQuerySource, IEntityType>();
        private readonly List<IQueryAnnotation> _queryAnnotations = new List<IQueryAnnotation>();

        private IDictionary<IQuerySource, List<IReadOnlyList<INavigation>>> _trackableIncludes;
        private ISet<IQuerySource> _querySourcesRequiringMaterialization;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public QueryCompilationContext(
            [NotNull] QueryCompilationContextDependencies dependencies,
            [NotNull] ILinqOperatorProvider linqOperatorProvider,
            bool trackQueryResults)
        {
            Check.NotNull(dependencies, nameof(dependencies));
            Check.NotNull(linqOperatorProvider, nameof(linqOperatorProvider));

            Model = dependencies.Model;
            Logger = dependencies.Logger;

            _entityQueryModelVisitorFactory = dependencies.EntityQueryModelVisitorFactory;
            _requiresMaterializationExpressionVisitorFactory = dependencies.RequiresMaterializationExpressionVisitorFactory;

            LinqOperatorProvider = linqOperatorProvider;
            ContextType = dependencies.CurrentContext.Context.GetType();
            TrackQueryResults = trackQueryResults;
        }

        /// <summary>
        ///     Mapping between correlated collection query modles and metadata needed to process them
        /// </summary>
        public virtual Dictionary<MainFromClause, CorrelatedSubqueryMetadata> CorrelatedSubqueryMetadataMap { get; } = new Dictionary<MainFromClause, CorrelatedSubqueryMetadata>();

        /// <summary>
        ///     Gets the model.
        /// </summary>
        /// <value>
        ///     The model.
        /// </value>
        public virtual IModel Model { get; }

        /// <summary>
        ///     Gets the logger.
        /// </summary>
        /// <value>
        ///     The logger.
        /// </value>
        public virtual IDiagnosticsLogger<DbLoggerCategory.Query> Logger { get; }

        /// <summary>
        ///     Gets the linq operator provider.
        /// </summary>
        /// <value>
        ///     The linq operator provider.
        /// </value>
        public virtual ILinqOperatorProvider LinqOperatorProvider { get; }

        /// <summary>
        ///     Gets the type of the context./
        /// </summary>
        /// <value>
        ///     The type of the context.
        /// </value>
        public virtual Type ContextType { get; }

        /// <summary>
        ///     Gets a value indicating the default configured tracking behavior.
        /// </summary>
        /// <value>
        ///     true if the default is to track query results, false if not.
        /// </value>
        public virtual bool TrackQueryResults { get; }

        /// <summary>
        ///     Gets the query source mapping.
        /// </summary>
        /// <value>
        ///     The query source mapping.
        /// </value>
        public virtual QuerySourceMapping QuerySourceMapping { get; } = new QuerySourceMapping();

        /// <summary>
        ///     Gets the entity type mapped to the given query source
        /// </summary>
        public virtual IEntityType FindEntityType([NotNull] IQuerySource querySource)
        {
            Check.NotNull(querySource, nameof(querySource));

            _querySourceEntityTypeMapping.TryGetValue(querySource, out var entityType);
            return entityType;
        }

        /// <summary>
        ///     Gets the entity type mapped to the given query source
        /// </summary>
        public virtual void AddOrUpdateMapping([NotNull] IQuerySource querySource, [NotNull] IEntityType entityType)
            => _querySourceEntityTypeMapping[Check.NotNull(querySource, nameof(querySource))] = entityType;

        /// <summary>
        ///     Updates the query source mappings to the new query sources
        /// </summary>
        /// <param name="querySourceMapping"> The new query source mapping </param>
        public virtual void UpdateMapping([NotNull] QuerySourceMapping querySourceMapping)
        {
            Check.NotNull(querySourceMapping, nameof(querySourceMapping));

            foreach (var entityTypeMapping in _querySourceEntityTypeMapping.ToList())
            {
                if (querySourceMapping.ContainsMapping(entityTypeMapping.Key))
                {
                    var newQuerySource = (querySourceMapping.GetExpression(entityTypeMapping.Key) as QuerySourceReferenceExpression)
                        ?.ReferencedQuerySource;
                    if (newQuerySource != null)
                    {
                        _querySourceEntityTypeMapping[newQuerySource] = entityTypeMapping.Value;
                    }
                }
            }
        }

        /// <summary>
        ///     Adds or updates the expression mapped to a query source.
        /// </summary>
        /// <param name="querySource"> The query source. </param>
        /// <param name="expression"> The expression mapped to the query source. </param>
        public virtual void AddOrUpdateMapping(
            [NotNull] IQuerySource querySource, [NotNull] Expression expression)
        {
            Check.NotNull(querySource, nameof(querySource));
            Check.NotNull(expression, nameof(expression));

            if (!QuerySourceMapping.ContainsMapping(querySource))
            {
                QuerySourceMapping.AddMapping(querySource, expression);
            }
            else
            {
                QuerySourceMapping.ReplaceMapping(querySource, expression);
            }
        }

        /// <summary>
        ///     Gets the query annotations.
        /// </summary>
        /// <value>
        ///     The query annotations.
        /// </value>
        public virtual IReadOnlyCollection<IQueryAnnotation> QueryAnnotations => _queryAnnotations;

        /// <summary>
        ///     Adds query annotations to the existing list.
        /// </summary>
        /// <param name="annotations">The query annotations.</param>
        public virtual void AddAnnotations([NotNull] IEnumerable<IQueryAnnotation> annotations)
        {
            Check.NotNull(annotations, nameof(annotations));

            _queryAnnotations.AddRange(annotations);
        }

        /// <summary>
        ///     Creates cloned annotations targeting a new QueryModel.
        /// </summary>
        /// <param name="querySourceMapping">A query source mapping.</param>
        /// <param name="queryModel">A query model.</param>
        public virtual void CloneAnnotations(
            [NotNull] QuerySourceMapping querySourceMapping,
            [NotNull] QueryModel queryModel)
        {
            Check.NotNull(querySourceMapping, nameof(querySourceMapping));
            Check.NotNull(queryModel, nameof(queryModel));

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var annotation in QueryAnnotations.OfType<ICloneableQueryAnnotation>().ToList())
            {
                if (querySourceMapping.ContainsMapping(annotation.QuerySource)
                    && querySourceMapping.GetExpression(annotation.QuerySource)
                        is QuerySourceReferenceExpression querySourceReferenceExpression)
                {
                    _queryAnnotations.Add(
                        annotation.Clone(
                            querySourceReferenceExpression.ReferencedQuerySource, queryModel));
                }
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this is a tracking query.
        /// </summary>
        /// <value>
        ///     true if this object is a tracking query, false if not.
        /// </value>
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

        /// <summary>
        ///     Gets a value indicating whether this query should have model-level query filters applied.
        /// </summary>
        /// <value>
        ///     true if query filters should be applied, false if not.
        /// </value>
        public virtual bool IgnoreQueryFilters
            => QueryAnnotations
                .OfType<IgnoreQueryFiltersResultOperator>()
                .Any();

        /// <summary>
        ///     The query has at least one Include operation.
        /// </summary>
        public virtual bool IsIncludeQuery => QueryAnnotations.OfType<IncludeResultOperator>().Any();

        /// <summary>
        ///     Gets a value indicating whether this query requires a query buffer.
        /// </summary>
        /// <value>
        ///     true if this query requires a query buffer, false if not.
        /// </value>
        public virtual bool IsQueryBufferRequired { get; private set; }

        private ISet<IQuerySource> QuerySourcesRequiringMaterialization
            => _querySourcesRequiringMaterialization
               ?? (_querySourcesRequiringMaterialization = new HashSet<IQuerySource>());

        /// <summary>
        ///     Determine if the query requires a query buffer.
        /// </summary>
        /// <param name="queryModel"> The query model. </param>
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
                if (constantExpression.IsEntityQueryable())
                {
                    var entityQueryable = (IQueryable)constantExpression.Value;
                    var entityType = _model.FindEntityType(entityQueryable.ElementType);

                    if (entityType != null
                        && !entityType.IsQueryType
                        && (_referencedEntityTypes > 0
                            || entityType.ShadowPropertyCount() > 0))
                    {
                        _requiresBuffering = true;

                        return constantExpression;
                    }

                    if (entityType != null
                        || _model.HasEntityTypeWithDefiningNavigation(entityQueryable.ElementType))
                    {
                        _referencedEntityTypes++;
                    }
                }

                return base.VisitConstant(constantExpression);
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Method.IsEFPropertyMethod())
                {
                    _requiresBuffering = true;

                    return methodCallExpression;
                }

                return base.VisitMethodCall(methodCallExpression);
            }
        }

        /// <summary>
        ///     Creates query model visitor.
        /// </summary>
        /// <returns>
        ///     The new query model visitor.
        /// </returns>
        public virtual EntityQueryModelVisitor CreateQueryModelVisitor()
            => CreateQueryModelVisitor(parentEntityQueryModelVisitor: null);

        /// <summary>
        ///     Creates query model visitor.
        /// </summary>
        /// <param name="parentEntityQueryModelVisitor"> The parent entity query model visitor. </param>
        /// <returns>
        ///     The new query model visitor.
        /// </returns>
        public virtual EntityQueryModelVisitor CreateQueryModelVisitor(
            [CanBeNull] EntityQueryModelVisitor parentEntityQueryModelVisitor)
            => _entityQueryModelVisitorFactory.Create(this, parentEntityQueryModelVisitor);

        /// <summary>
        ///     Adds a trackable include.
        /// </summary>
        /// <param name="querySource"> The query source. </param>
        /// <param name="navigationPath"> The included navigation path. </param>
        public virtual void AddTrackableInclude(
            [NotNull] IQuerySource querySource, [NotNull] IReadOnlyList<INavigation> navigationPath)
        {
            Check.NotNull(querySource, nameof(querySource));
            Check.NotNull(navigationPath, nameof(navigationPath));

            if (_trackableIncludes == null)
            {
                _trackableIncludes = new Dictionary<IQuerySource, List<IReadOnlyList<INavigation>>>();
            }

            if (!_trackableIncludes.TryGetValue(querySource, out var includes))
            {
                _trackableIncludes.Add(querySource, includes = new List<IReadOnlyList<INavigation>>());
            }

            includes.Add(navigationPath);
        }

        /// <summary>
        ///     Gets all trackable includes for a given query source.
        /// </summary>
        /// <param name="querySource"> The query source. </param>
        /// <returns>
        ///     The trackable includes.
        /// </returns>
        public virtual IReadOnlyList<IReadOnlyList<INavigation>> GetTrackableIncludes([NotNull] IQuerySource querySource)
        {
            Check.NotNull(querySource, nameof(querySource));

            if (_trackableIncludes == null)
            {
                return null;
            }

            return _trackableIncludes.TryGetValue(querySource, out var includes) ? includes : null;
        }

        /// <summary>
        ///     Determines all query sources that require materialization.
        /// </summary>
        /// <param name="queryModelVisitor"> The query model visitor. </param>
        /// <param name="queryModel"> The query model. </param>
        public virtual void FindQuerySourcesRequiringMaterialization(
            [NotNull] EntityQueryModelVisitor queryModelVisitor, [NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModelVisitor, nameof(queryModelVisitor));
            Check.NotNull(queryModel, nameof(queryModel));

            var querySourcesRequiringMaterializationFinder = new QuerySourcesRequiringMaterializationFinder(
                _requiresMaterializationExpressionVisitorFactory,
                queryModelVisitor,
                QuerySourcesRequiringMaterialization);

            querySourcesRequiringMaterializationFinder.AddQuerySourcesRequiringMaterialization(queryModel);
        }

        private class QuerySourcesRequiringMaterializationFinder
        {
            private readonly IRequiresMaterializationExpressionVisitorFactory _requiresMaterializationExpressionVisitorFactory;
            private readonly EntityQueryModelVisitor _queryModelVisitor;
            private readonly ISet<IQuerySource> _querySourcesRequiringMaterialization;

            public QuerySourcesRequiringMaterializationFinder(
                IRequiresMaterializationExpressionVisitorFactory requiresMaterializationExpressionVisitorFactory,
                EntityQueryModelVisitor queryModelVisitor,
                ISet<IQuerySource> querySourcesRequiringMaterialization)
            {
                _requiresMaterializationExpressionVisitorFactory = requiresMaterializationExpressionVisitorFactory;
                _queryModelVisitor = queryModelVisitor;
                _querySourcesRequiringMaterialization = querySourcesRequiringMaterialization;
            }

            public void AddQuerySourcesRequiringMaterialization(QueryModel queryModel)
            {
                var requiresMaterializationExpressionVisitor
                    = _requiresMaterializationExpressionVisitorFactory
                        .Create(_queryModelVisitor);

                var querySourcesRequiringMaterialization = requiresMaterializationExpressionVisitor
                    .FindQuerySourcesRequiringMaterialization(queryModel);

                var groupJoinCompensatingVisitor = new GroupJoinMaterializationCompensatingVisitor();
                groupJoinCompensatingVisitor.VisitQueryModel(queryModel);

                var blockedMemberPushdownCompensatingVisitor = new BlockedMemberPushdownCompensatingVisitor();
                queryModel.TransformExpressions(blockedMemberPushdownCompensatingVisitor.Visit);

                var setResultOperatorsCompensatingVisitor = new SetResultOperatorsCompensatingVisitor(this);
                setResultOperatorsCompensatingVisitor.VisitQueryModel(queryModel);

                _querySourcesRequiringMaterialization.UnionWith(
                    querySourcesRequiringMaterialization
                        .Concat(groupJoinCompensatingVisitor.QuerySources)
                        .Concat(blockedMemberPushdownCompensatingVisitor.QuerySources));
            }
        }

        private class GroupJoinMaterializationCompensatingVisitor : QueryModelVisitorBase
        {
            public ISet<IQuerySource> QuerySources { get; } = new HashSet<IQuerySource>();

            public override void VisitQueryModel(QueryModel queryModel)
            {
                queryModel.TransformExpressions(new TransformingQueryModelExpressionVisitor<GroupJoinMaterializationCompensatingVisitor>(this).Visit);

                base.VisitQueryModel(queryModel);
            }

            public override void VisitGroupJoinClause(GroupJoinClause groupJoinClause, QueryModel queryModel, int index)
            {
                if (!IsLeftJoin(groupJoinClause, queryModel, index))
                {
                    MarkForMaterialization(queryModel.MainFromClause);
                    MarkForMaterialization(groupJoinClause);
                }

                base.VisitGroupJoinClause(groupJoinClause, queryModel, index);
            }

            // Left join (which we don't need to materialize) is when there is a SelectMany clause right after the GroupJoin clause
            // and that the grouping is not referenced anywhere else in the query
            private static bool IsLeftJoin(GroupJoinClause groupJoinClause, QueryModel queryModel, int index)
                => queryModel.CountQuerySourceReferences(groupJoinClause) == 1
                   && queryModel.BodyClauses.ElementAtOrDefault(index + 1) is AdditionalFromClause additionalFromClause
                   && additionalFromClause.TryGetFlattenedGroupJoinClause() == groupJoinClause;

            private void MarkForMaterialization(IQuerySource querySource)
            {
                RequiresMaterializationExpressionVisitor.HandleUnderlyingQuerySources(querySource, MarkForMaterialization);
                QuerySources.Add(querySource);
            }
        }

        private class BlockedMemberPushdownCompensatingVisitor : ExpressionVisitorBase
        {
            public ISet<IQuerySource> QuerySources { get; } = new HashSet<IQuerySource>();

            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Expression is SubQueryExpression subQuery
                    && subQuery.QueryModel.ResultOperators.Any(
                        ro =>
                            ro is DistinctResultOperator
                            || ro is ConcatResultOperator
                            || ro is UnionResultOperator
                            || ro is IntersectResultOperator
                            || ro is ExceptResultOperator))
                {
                    MarkForMaterialization(subQuery.QueryModel.MainFromClause);
                }

                return base.VisitMember(node);
            }

            private void MarkForMaterialization(IQuerySource querySource)
            {
                RequiresMaterializationExpressionVisitor.HandleUnderlyingQuerySources(querySource, MarkForMaterialization);
                QuerySources.Add(querySource);
            }
        }

        private class SetResultOperatorsCompensatingVisitor : QueryModelVisitorBase
        {
            private readonly QuerySourcesRequiringMaterializationFinder _querySourcesRequiringMaterializationFinder;

            public SetResultOperatorsCompensatingVisitor(
                QuerySourcesRequiringMaterializationFinder querySourcesRequiringMaterializationFinder)
            {
                _querySourcesRequiringMaterializationFinder = querySourcesRequiringMaterializationFinder;
            }

            public override void VisitQueryModel(QueryModel queryModel)
            {
                queryModel.TransformExpressions(new TransformingQueryModelExpressionVisitor<SetResultOperatorsCompensatingVisitor>(this).Visit);

                base.VisitQueryModel(queryModel);
            }

            protected override void VisitResultOperators(ObservableCollection<ResultOperatorBase> resultOperators, QueryModel queryModel)
            {
                var resultOperatorSources = RequiresMaterializationExpressionVisitor.GetSetResultOperatorSourceExpressions(resultOperators);
                if (resultOperatorSources.Any())
                {
                    // in case of set1.Concat(set2) we also need to add set1 qsre to materialization
                    // reusing existing infrastructure for cases where the projection is not trivial
                    var queryModelCopy = new QueryModel(queryModel.MainFromClause, queryModel.SelectClause);
                    foreach (var bodyClause in queryModel.BodyClauses)
                    {
                        queryModelCopy.BodyClauses.Add(bodyClause);
                    }

                    _querySourcesRequiringMaterializationFinder.AddQuerySourcesRequiringMaterialization(queryModelCopy);

                    foreach (var resultOperatorSource in RequiresMaterializationExpressionVisitor.GetSetResultOperatorSourceExpressions(resultOperators))
                    {
                        if (resultOperatorSource is SubQueryExpression subQuery)
                        {
                            _querySourcesRequiringMaterializationFinder.AddQuerySourcesRequiringMaterialization(subQuery.QueryModel);
                        }
                        else if (resultOperatorSource is MethodCallExpression methodCall
                                 && methodCall.Method.MethodIsClosedFormOf(CollectionNavigationSubqueryInjector.MaterializeCollectionNavigationMethodInfo))
                        {
                            _querySourcesRequiringMaterializationFinder.AddQuerySourcesRequiringMaterialization(((SubQueryExpression)methodCall.Arguments[1]).QueryModel);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Determine whether or not a query source requires materialization.
        /// </summary>
        /// <param name="querySource"> The query source. </param>
        /// <returns>
        ///     true if it requires materialization, false if not.
        /// </returns>
        public virtual bool QuerySourceRequiresMaterialization([NotNull] IQuerySource querySource)
        {
            Check.NotNull(querySource, nameof(querySource));

            return QuerySourcesRequiringMaterialization.Contains(querySource);
        }

        /// <summary>
        ///     Add a query source to the set of query sources requiring materialization.
        /// </summary>
        /// <param name="querySource"> The query source. </param>
        public virtual void AddQuerySourceRequiringMaterialization([NotNull] IQuerySource querySource)
        {
            QuerySourcesRequiringMaterialization.Add(querySource);
        }
    }
}
