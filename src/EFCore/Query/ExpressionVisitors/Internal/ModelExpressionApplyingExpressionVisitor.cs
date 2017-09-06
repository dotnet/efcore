// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;
using Remotion.Linq.Parsing.ExpressionVisitors;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ModelExpressionApplyingExpressionVisitor : RelinqExpressionVisitor
    {
        private readonly QueryCompilationContext _queryCompilationContext;
        private readonly IQueryModelGenerator _queryModelGenerator;

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
        public ModelExpressionApplyingExpressionVisitor(
            [NotNull] QueryCompilationContext queryCompilationContext,
            [NotNull] IQueryModelGenerator queryModelGenerator)
        {
            Check.NotNull(queryCompilationContext, nameof(queryCompilationContext));
            Check.NotNull(queryModelGenerator, nameof(queryModelGenerator));

            _queryCompilationContext = queryCompilationContext;
            _queryModelGenerator = queryModelGenerator;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsViewTypeQuery { get; private set; }

        private static readonly MethodInfo _whereMethod
            = typeof(Queryable)
                .GetTypeInfo()
                .GetDeclaredMethods(nameof(Queryable.Where))
                .Single(
                    mi => mi.GetParameters().Length == 2
                          && mi.GetParameters()[1].ParameterType
                              .GetGenericArguments()[0]
                              .GetGenericArguments().Length == 2);

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

                if (entityType != null)
                {
                    Expression newExpression = constantExpression;

                    if (entityType.IsQueryType())
                    {
                        IsViewTypeQuery = true;

                        var annotation = entityType.FindAnnotation(CoreAnnotationNames.DefiningQuery);

                        if (annotation != null)
                        {
                            var query = (LambdaExpression)annotation.Value;

                            var parameterizedQuery
                                = _queryModelGenerator
                                    .ExtractParameters(
                                        _queryCompilationContext.Logger,
                                        query.Body,
                                        _parameters,
                                        parameterize: false,
                                        generateContextAccessors: true);

                            var subQueryModel = _queryModelGenerator.ParseQuery(parameterizedQuery);

                            newExpression = new SubQueryExpression(subQueryModel);
                        }
                    }

                    if (!_queryCompilationContext.IgnoreQueryFilters
                        && entityType.QueryFilter != null)
                    {
                        var parameterizedFilter
                            = (LambdaExpression)_queryModelGenerator
                                .ExtractParameters(
                                    _queryCompilationContext.Logger,
                                    entityType.QueryFilter,
                                    _parameters,
                                    parameterize: false,
                                    generateContextAccessors: true);

                        var oldParameterExpression = parameterizedFilter.Parameters[0];
                        var newParameterExpression = Expression.Parameter(type, oldParameterExpression.Name);

                        var predicateExpression
                            = ReplacingExpressionVisitor
                                .Replace(
                                    oldParameterExpression,
                                    newParameterExpression,
                                    parameterizedFilter.Body);

                        var whereExpression
                            = Expression.Call(
                                _whereMethod.MakeGenericMethod(type),
                                newExpression,
                                Expression.Lambda(
                                    predicateExpression,
                                    newParameterExpression));

                        var subQueryModel = _queryModelGenerator.ParseQuery(whereExpression);

                        newExpression = new SubQueryExpression(subQueryModel);
                    }

                    return newExpression;
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
