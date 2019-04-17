// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class ModelExpressionApplyingExpressionVisitor : RelinqExpressionVisitor
    {
        private readonly QueryCompilationContext _queryCompilationContext;
        private readonly IQueryModelGenerator _queryModelGenerator;
        private readonly EntityQueryModelVisitor _entityQueryModelVisitor;
        private readonly ParameterExtractingExpressionVisitor _parameterExtractingExpressionVisitor;

        private readonly Parameters _parameters = new Parameters();

        private IQuerySource _querySource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IReadOnlyDictionary<string, object> ContextParameters => _parameters.ParameterValues;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public ModelExpressionApplyingExpressionVisitor(
            [NotNull] QueryCompilationContext queryCompilationContext,
            [NotNull] IQueryModelGenerator queryModelGenerator,
            [NotNull] EntityQueryModelVisitor entityQueryModelVisitor)
        {
            Check.NotNull(queryCompilationContext, nameof(queryCompilationContext));
            Check.NotNull(queryModelGenerator, nameof(queryModelGenerator));
            Check.NotNull(entityQueryModelVisitor, nameof(entityQueryModelVisitor));

            _queryCompilationContext = queryCompilationContext;
            _queryModelGenerator = queryModelGenerator;
            _entityQueryModelVisitor = entityQueryModelVisitor;

            _parameterExtractingExpressionVisitor = new ParameterExtractingExpressionVisitor(
                ((QueryModelGenerator)queryModelGenerator).EvaluatableExpressionFilter,
                _parameters,
                _queryCompilationContext.ContextType,
                _queryCompilationContext.Logger,
                parameterize: false,
                generateContextAccessors: true);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsKeylessQuery { get; private set; }

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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void ApplyModelExpressions([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            _querySource = queryModel.MainFromClause;

            queryModel.TransformExpressions(Visit);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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

                    if (entityType.FindPrimaryKey() == null)
                    {
                        IsKeylessQuery = true;

                        var query = entityType.GetDefiningQuery();

                        if (query != null
                            && _entityQueryModelVisitor.ShouldApplyDefiningQuery(entityType, _querySource))
                        {
                            var parameterizedQuery
                                = _parameterExtractingExpressionVisitor.ExtractParameters(query.Body);

                            var subQueryModel = _queryModelGenerator.ParseQuery(parameterizedQuery);

                            newExpression = new SubQueryExpression(subQueryModel);
                        }
                    }

                    if (!_queryCompilationContext.IgnoreQueryFilters
                        && entityType.GetQueryFilter() != null)
                    {
                        var parameterizedFilter
                            = (LambdaExpression)_parameterExtractingExpressionVisitor
                                .ExtractParameters(entityType.GetQueryFilter());

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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitSubQuery(SubQueryExpression subQueryExpression)
        {
            var querySource = _querySource;

            _querySource = subQueryExpression.QueryModel.MainFromClause;

            subQueryExpression.QueryModel.TransformExpressions(Visit);

            _querySource = querySource;

            return subQueryExpression;
        }
    }
}
