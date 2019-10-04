// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class CorrelatedCollectionFindingExpressionVisitor : RelinqExpressionVisitor
    {
        private readonly EntityQueryModelVisitor _queryModelVisitor;

        private static readonly MethodInfo _toListMethodInfo
            = typeof(Enumerable).GetTypeInfo().GetDeclaredMethod(nameof(Enumerable.ToList));

        private static readonly MethodInfo _toArrayMethodInfo
            = typeof(Enumerable).GetTypeInfo().GetDeclaredMethod(nameof(Enumerable.ToArray));

        private readonly bool _trackingQuery;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public CorrelatedCollectionFindingExpressionVisitor(
            [NotNull] EntityQueryModelVisitor queryModelVisitor,
            bool trackingQuery)
        {
            _queryModelVisitor = queryModelVisitor;
            _trackingQuery = trackingQuery;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name.StartsWith(nameof(IQueryBuffer.IncludeCollection), StringComparison.Ordinal))
            {
                return node;
            }

            SubQueryExpression subQueryExpression = null;
            if ((node.Method.MethodIsClosedFormOf(_toListMethodInfo) || node.Method.MethodIsClosedFormOf(_toArrayMethodInfo))
                && node.Arguments[0] is SubQueryExpression)
            {
                subQueryExpression = (SubQueryExpression)node.Arguments[0];
            }

            if (node.Method.MethodIsClosedFormOf(CollectionNavigationSubqueryInjector.MaterializeCollectionNavigationMethodInfo)
                && node.Arguments[1] is SubQueryExpression)
            {
                subQueryExpression = (SubQueryExpression)node.Arguments[1];
            }

            if (subQueryExpression != null)
            {
                TryMarkSubQuery(subQueryExpression);

                return node;
            }

            return base.VisitMethodCall(node);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitSubQuery(SubQueryExpression expression)
            // prune subqueries (and potential subqueries inside them) that are not wrapped around ToList/ToArray
            // we can't optimize correlated collection if it's parent is streaming
            => expression;

        private void TryMarkSubQuery(SubQueryExpression expression)
        {
            var subQueryModel = expression.QueryModel;

            subQueryModel.SelectClause.TransformExpressions(Visit);

            if (CorrelatedSubqueryOptimizationValidator.CanTryOptimizeCorrelatedSubquery(subQueryModel))
            {
                // if the query passes validation it becomes a candidate for future optimization
                // optimization can't always be performed, e.g. when client-eval is needed
                // but we need to collect metadata (i.e. INavigations) before nav rewrite converts them into joins
                _queryModelVisitor.BindNavigationPathPropertyExpression(
                    subQueryModel.MainFromClause.FromExpression,
                    (properties, querySource) =>
                    {
                        var collectionNavigation = properties.OfType<INavigation>().SingleOrDefault(n => n.IsCollection());

                        if (collectionNavigation != null && querySource != null)
                        {
                            _queryModelVisitor.QueryCompilationContext.RegisterCorrelatedSubqueryMetadata(
                                subQueryModel.MainFromClause,
                                _trackingQuery,
                                properties.OfType<INavigation>().First(),
                                collectionNavigation,
                                querySource);

                            return expression;
                        }

                        return default;
                    });
            }
        }

        private static class CorrelatedSubqueryOptimizationValidator
        {
            public static bool CanTryOptimizeCorrelatedSubquery(QueryModel queryModel)
            {
                if (queryModel.ResultOperators.Count > 0)
                {
                    return false;
                }

                // first pass finds all the query sources defined in this scope (i.e. from clauses)
                var definedQuerySourcesFinder = new DefinedQuerySourcesFindingVisitor();
                definedQuerySourcesFinder.VisitQueryModel(queryModel);

                // second pass makes sure that all qsres reference only query sources that were discovered in the first step, i.e. nothing from the outside
                var qsreScopeValidator = new ReferencedQuerySourcesScopeValidatingVisitor(
                    queryModel.MainFromClause, definedQuerySourcesFinder.QuerySources);

                qsreScopeValidator.VisitQueryModel(queryModel);

                return qsreScopeValidator.AllQuerySourceReferencesInScope;
            }

            private class DefinedQuerySourcesFindingVisitor : QueryModelVisitorBase
            {
                public ISet<IQuerySource> QuerySources { get; } = new HashSet<IQuerySource>();

                public override void VisitQueryModel(QueryModel queryModel)
                {
                    queryModel.TransformExpressions(new TransformingQueryModelExpressionVisitor<DefinedQuerySourcesFindingVisitor>(this).Visit);

                    base.VisitQueryModel(queryModel);
                }

                public override void VisitMainFromClause(MainFromClause fromClause, QueryModel queryModel)
                {
                    QuerySources.Add(fromClause);

                    base.VisitMainFromClause(fromClause, queryModel);
                }

                public override void VisitAdditionalFromClause(AdditionalFromClause fromClause, QueryModel queryModel, int index)
                {
                    QuerySources.Add(fromClause);

                    base.VisitAdditionalFromClause(fromClause, queryModel, index);
                }
            }

            private sealed class ReferencedQuerySourcesScopeValidatingVisitor : QueryModelVisitorBase
            {
                private class InnerVisitor : TransformingQueryModelExpressionVisitor<ReferencedQuerySourcesScopeValidatingVisitor>
                {
                    private readonly ISet<IQuerySource> _querySourcesInScope;

                    public InnerVisitor(ISet<IQuerySource> querySourcesInScope, ReferencedQuerySourcesScopeValidatingVisitor transformingQueryModelVisitor)
                        : base(transformingQueryModelVisitor)
                    {
                        _querySourcesInScope = querySourcesInScope;
                    }

                    public bool AllQuerySourceReferencesInScope { get; private set; } = true;

                    protected override Expression VisitQuerySourceReference(QuerySourceReferenceExpression expression)
                    {
                        if (!_querySourcesInScope.Contains(expression.ReferencedQuerySource))
                        {
                            AllQuerySourceReferencesInScope = false;
                        }

                        return base.VisitQuerySourceReference(expression);
                    }
                }

                // query source that can reference something outside the scope, e.g. main from clause that contains the correlated navigation
                private readonly IQuerySource _exemptQuerySource;
                private readonly InnerVisitor _innerVisitor;

                public ReferencedQuerySourcesScopeValidatingVisitor(IQuerySource exemptQuerySource, ISet<IQuerySource> querySourcesInScope)
                {
                    _exemptQuerySource = exemptQuerySource;
                    _innerVisitor = new InnerVisitor(querySourcesInScope, this);
                }

                public bool AllQuerySourceReferencesInScope => _innerVisitor.AllQuerySourceReferencesInScope;

                public override void VisitMainFromClause(MainFromClause fromClause, QueryModel queryModel)
                {
                    if (fromClause != _exemptQuerySource)
                    {
                        fromClause.TransformExpressions(_innerVisitor.Visit);
                    }
                }

                protected override void VisitBodyClauses(ObservableCollection<IBodyClause> bodyClauses, QueryModel queryModel)
                {
                    foreach (var bodyClause in bodyClauses)
                    {
                        if (bodyClause != _exemptQuerySource)
                        {
                            bodyClause.TransformExpressions(_innerVisitor.Visit);
                        }
                    }
                }

                public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel)
                {
                    selectClause.TransformExpressions(_innerVisitor.Visit);
                }

                public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
                {
                    // it is not necessary to visit result ops at the moment, since we don't optimize subqueries that contain any result ops
                    // however, we might support some result ops in the future
                    resultOperator.TransformExpressions(_innerVisitor.Visit);
                }
            }
        }
    }
}
