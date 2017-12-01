// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;
using Remotion.Linq.Parsing.ExpressionVisitors;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class FilterApplyingExpressionVisitor : RelinqExpressionVisitor
    {
        private readonly QueryCompilationContext _queryCompilationContext;
        private readonly IQueryProcessor _queryProcessor;

        private readonly Parameters _parameters = new Parameters();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyDictionary<string, object> ContextParameters => _parameters.ParameterValues;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public FilterApplyingExpressionVisitor(
            [NotNull] QueryCompilationContext queryCompilationContext,
            [NotNull] IQueryProcessor queryProcessor)
        {
            Check.NotNull(queryCompilationContext, nameof(queryCompilationContext));
            Check.NotNull(queryProcessor, nameof(queryProcessor));

            _queryCompilationContext = queryCompilationContext;
            _queryProcessor = queryProcessor;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitConstant(ConstantExpression constantExpression)
        {
            if (constantExpression.IsEntityQueryable())
            {
                var type = ((IQueryable)constantExpression.Value).ElementType;
                var entityType = _queryCompilationContext.Model.FindEntityType(type)?.RootType();

                if (entityType?.QueryFilter != null)
                {
                    var parameterizedFilter
                        = (LambdaExpression)_queryProcessor
                            .ExtractParameters(
                                _queryCompilationContext.Logger,
                                entityType.QueryFilter,
                                _parameters,
                                parameterize: false,
                                generateContextAccessors: true);

                    var mainFromClause
                        = new MainFromClause(
                            type.Name.Substring(0, 1).ToLowerInvariant(),
                            type,
                            constantExpression);

                    var querySourceReferenceExpression = new QuerySourceReferenceExpression(mainFromClause);
                    var selectClause = new SelectClause(querySourceReferenceExpression);
                    var subQueryModel = new QueryModel(mainFromClause, selectClause);

                    var predicate
                        = ReplacingExpressionVisitor
                            .Replace(
                                parameterizedFilter.Parameters.Single(),
                                querySourceReferenceExpression,
                                parameterizedFilter.Body);

                    subQueryModel.BodyClauses.Add(new WhereClause(predicate));

                    return new SubQueryExpression(subQueryModel);
                }
            }

            return constantExpression;
        }

        private sealed class Parameters : IParameterValues
        {
            private readonly IDictionary<string, object> _parameterValues = new Dictionary<string, object>();

            public IReadOnlyDictionary<string, object> ParameterValues
                => (IReadOnlyDictionary<string, object>)_parameterValues;

            public void AddParameter(string name, object value)
            {
                _parameterValues.Add(name, value);
            }

            public object RemoveParameter(string name)
            {
                var value = _parameterValues[name];

                _parameterValues.Remove(name);

                return value;
            }

            public void SetParameter(string name, object value)
            {
                _parameterValues[name] = value;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitSubQuery(SubQueryExpression subQueryExpression)
        {
            subQueryExpression.QueryModel.TransformExpressions(Visit);

            return subQueryExpression;
        }
    }
}
