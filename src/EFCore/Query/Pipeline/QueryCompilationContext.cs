// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Pipeline
{
    public class QueryCompilationContext
    {
        public static readonly ParameterExpression QueryContextParameter = Expression.Parameter(typeof(QueryContext), "queryContext");

        private readonly IQueryOptimizerFactory _queryOptimizerFactory;
        private readonly IQueryableMethodTranslatingExpressionVisitorFactory _queryableMethodTranslatingExpressionVisitorFactory;
        private readonly IShapedQueryOptimizerFactory _shapedQueryOptimizerFactory;
        private readonly IShapedQueryCompilingExpressionVisitorFactory _shapedQueryCompilingExpressionVisitorFactory;

        private readonly Parameters _parameters;

        public QueryCompilationContext(
            IModel model,
            IQueryOptimizerFactory queryOptimizerFactory,
            IQueryableMethodTranslatingExpressionVisitorFactory queryableMethodTranslatingExpressionVisitorFactory,
            IShapedQueryOptimizerFactory shapedQueryOptimizerFactory,
            IShapedQueryCompilingExpressionVisitorFactory shapedQueryCompilingExpressionVisitorFactory,
            ICurrentDbContext currentDbContext,
            IDbContextOptions contextOptions,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger,
            IEvaluatableExpressionFilter evaluatableExpressionFilter,
            bool async)
        {
            Async = async;
            TrackQueryResults = currentDbContext.Context.ChangeTracker.QueryTrackingBehavior == QueryTrackingBehavior.TrackAll;
            Model = model;
            ContextOptions = contextOptions;
            ContextType = currentDbContext.Context.GetType();
            Logger = logger;
            EvaluatableExpressionFilter = evaluatableExpressionFilter;

            _queryOptimizerFactory = queryOptimizerFactory;
            _queryableMethodTranslatingExpressionVisitorFactory = queryableMethodTranslatingExpressionVisitorFactory;
            _shapedQueryOptimizerFactory = shapedQueryOptimizerFactory;
            _shapedQueryCompilingExpressionVisitorFactory = shapedQueryCompilingExpressionVisitorFactory;

            _parameters = new Parameters();
        }

        public bool Async { get; }
        public IModel Model { get; }
        public IDbContextOptions ContextOptions { get; }
        public bool TrackQueryResults { get; internal set; }
        public bool IgnoreQueryFilters { get; internal set; }
        public virtual IDiagnosticsLogger<DbLoggerCategory.Query> Logger { get; }
        public virtual Type ContextType { get; }

        public virtual IEvaluatableExpressionFilter EvaluatableExpressionFilter { get; set; }

        internal virtual IParameterValues ParameterValues => _parameters;

        public virtual Func<QueryContext, TResult> CreateQueryExecutor<TResult>(Expression query)
        {
            query = _queryOptimizerFactory.Create(this).Visit(query);
            // Convert EntityQueryable to ShapedQueryExpression
            query = _queryableMethodTranslatingExpressionVisitorFactory.Create(Model).Visit(query);
            query = _shapedQueryOptimizerFactory.Create(this).Visit(query);

            // Inject actual entity materializer
            // Inject tracking
            query = _shapedQueryCompilingExpressionVisitorFactory.Create(this).Visit(query);

            var setFilterParameterExpressions
                = CreateSetFilterParametersExpressions(out var contextVariableExpression);

            if (setFilterParameterExpressions != null)
            {
                query = Expression.Block(
                    new[] { contextVariableExpression },
                    setFilterParameterExpressions.Concat(new[] { query }));
            }

            var queryExecutorExpression = Expression.Lambda<Func<QueryContext, TResult>>(
                query,
                QueryContextParameter);

            try
            {
                return queryExecutorExpression.Compile();
            }
            finally
            {
                Logger.QueryExecutionPlanned(new ExpressionPrinter(), queryExecutorExpression);
            }
        }

        private static readonly MethodInfo _queryContextAddParameterMethodInfo
            = typeof(QueryContext)
                .GetTypeInfo()
                .GetDeclaredMethod(nameof(QueryContext.AddParameter));

        private static readonly PropertyInfo _queryContextContextPropertyInfo
            = typeof(QueryContext)
                .GetTypeInfo()
                .GetDeclaredProperty(nameof(QueryContext.Context));

        private IEnumerable<Expression> CreateSetFilterParametersExpressions(out ParameterExpression contextVariableExpression)
        {
            contextVariableExpression = null;

            if (_parameters.ParameterValues.Count == 0)
            {
                return null;
            }

            contextVariableExpression = Expression.Variable(ContextType, "context");

            var blockExpressions
                = new List<Expression>
                {
                    Expression.Assign(
                        contextVariableExpression,
                        Expression.Convert(
                            Expression.Property(
                                QueryContextParameter,
                                _queryContextContextPropertyInfo),
                            ContextType))
                };

            foreach (var keyValuePair in _parameters.ParameterValues)
            {
                blockExpressions.Add(
                    Expression.Call(
                        QueryContextParameter,
                        _queryContextAddParameterMethodInfo,
                        Expression.Constant(keyValuePair.Key),
                        Expression.Convert(
                            Expression.Invoke(
                                (LambdaExpression)keyValuePair.Value,
                                contextVariableExpression),
                            typeof(object))));
            }

            return blockExpressions;
        }

        private class Parameters : IParameterValues
        {
            private readonly IDictionary<string, object> _parameterValues = new Dictionary<string, object>();

            public IReadOnlyDictionary<string, object> ParameterValues => (IReadOnlyDictionary<string, object>)_parameterValues;

            public virtual void AddParameter(string name, object value)
            {
                _parameterValues.Add(name, value);
            }

            public virtual void SetParameter(string name, object value)
            {
                _parameterValues[name] = value;
            }

            public virtual object RemoveParameter(string name)
            {
                var value = _parameterValues[name];
                _parameterValues.Remove(name);

                return value;
            }
        }
    }
}
