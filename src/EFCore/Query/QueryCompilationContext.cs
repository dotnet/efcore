// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
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
    /// <summary>
    ///     A query compilation context. The primary data structure representing the state/components
    ///     used during query compilation.
    /// </summary>
    public class QueryCompilationContext
    {
        private readonly IRequiresMaterializationExpressionVisitorFactory _requiresMaterializationExpressionVisitorFactory;
        private readonly IEntityQueryModelVisitorFactory _entityQueryModelVisitorFactory;

        private IReadOnlyCollection<IQueryAnnotation> _queryAnnotations;
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
        public virtual ILogger Logger { get; }

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
        ///     Gets the query annotations./
        /// </summary>
        /// <value>
        ///     The query annotations.
        /// </value>
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

            List<IReadOnlyList<INavigation>> includes;
            if (!_trackableIncludes.TryGetValue(querySource, out includes))
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

            List<IReadOnlyList<INavigation>> includes;

            return _trackableIncludes.TryGetValue(querySource, out includes) ? includes : null;
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

            var requiresMaterializationExpressionVisitor
                = _requiresMaterializationExpressionVisitorFactory
                    .Create(queryModelVisitor);

            var querySourcesRequiringMaterialization = requiresMaterializationExpressionVisitor
                .FindQuerySourcesRequiringMaterialization(queryModel);

            var groupJoinCompensatingVisitor = new GroupJoinMaterializationCompensatingVisitor();

            groupJoinCompensatingVisitor.VisitQueryModel(queryModel);

            var optionalCollectionNavigationCompensatingVisitor = new OptionalCollectionNavigationCompensatingVisitor();
            optionalCollectionNavigationCompensatingVisitor.VisitQueryModel(queryModel);

            QuerySourcesRequiringMaterialization.UnionWith(
                querySourcesRequiringMaterialization
                    .Concat(groupJoinCompensatingVisitor.QuerySources)
                    .Concat(optionalCollectionNavigationCompensatingVisitor.QuerySources));
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

        /// <summary>
        ///     Temporary measure for issue #7787
        ///     Problem is that for cases where collection navigation is chained after optional navigation we don't currently have robust null
        ///     protection logic in place
        ///     Since those cases don't need to be materialized, we will now try to bind to a value buffer, which may result in null reference for
        ///     InMemory scenarios
        ///     Workaround is to detect those cases and force materialization so that null protection is handled by GetValue() method based on entity
        /// </summary>
        private class OptionalCollectionNavigationCompensatingVisitor : QueryModelVisitorBase
        {
            public ISet<IQuerySource> QuerySources { get; } = new HashSet<IQuerySource>();

            public override void VisitQueryModel(QueryModel queryModel)
            {
                queryModel.TransformExpressions(new TransformingQueryModelExpressionVisitor<OptionalCollectionNavigationCompensatingVisitor>(this).Visit);

                base.VisitQueryModel(queryModel);
            }

            public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
            {
                if (whereClause.Predicate is BinaryExpression binaryExpression
                    && binaryExpression.NodeType == ExpressionType.Equal)
                {
                    var rightQsre = GetPropertyAccessQsre(binaryExpression.Right);
                    if (rightQsre?.ReferencedQuerySource is MainFromClause)
                    {
                        var leftQsre = GetPropertyAccessQsre(binaryExpression.Left);
                        MaterializeOptionalNavigationSource(leftQsre);
                    }
                }

                base.VisitWhereClause(whereClause, queryModel, index);
            }

            private static QuerySourceReferenceExpression GetPropertyAccessQsre(Expression expression)
            {
                if (expression.RemoveConvert() is MemberExpression member)
                {
                    return member.Expression as QuerySourceReferenceExpression;
                }

                if (expression.RemoveConvert() is MethodCallExpression method
                    && EntityQueryModelVisitor.IsPropertyMethod(method.Method))
                {
                    return method.Arguments[0] as QuerySourceReferenceExpression;
                }

                return null;
            }

            private void MaterializeOptionalNavigationSource(QuerySourceReferenceExpression sourceQsre)
            {
                if (sourceQsre?.ReferencedQuerySource is AdditionalFromClause additionalFromClause)
                {
                    var flattenedGroupJoin = additionalFromClause.TryGetFlattenedGroupJoinClause();
                    if (flattenedGroupJoin != null)
                    {
                        QuerySources.Add(flattenedGroupJoin.JoinClause);
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
