// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

        /// <summary>
        ///     A dictionary mapping parameter names to lambdas that, given a QueryContext, can extract that parameter's value.
        ///     This is needed for cases where we need to introduce a parameter during the compilation phase (e.g. entity equality rewrites
        ///     a parameter to an ID property on that parameter).
        /// </summary>
        private Dictionary<string, LambdaExpression> _runtimeParameters;

        public QueryCompilationContext(
            IModel model,
            IQueryOptimizerFactory queryOptimizerFactory,
            IQueryableMethodTranslatingExpressionVisitorFactory queryableMethodTranslatingExpressionVisitorFactory,
            IShapedQueryOptimizerFactory shapedQueryOptimizerFactory,
            IShapedQueryCompilingExpressionVisitorFactory shapedQueryCompilingExpressionVisitorFactory,
            ICurrentDbContext currentContext,
            IDbContextOptions contextOptions,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger,
            bool async)
        {
            Async = async;
            TrackQueryResults = currentContext.Context.ChangeTracker.QueryTrackingBehavior == QueryTrackingBehavior.TrackAll;
            Model = model;
            ContextOptions = contextOptions;
            ContextType = currentContext.Context.GetType();
            Logger = logger;

            _queryOptimizerFactory = queryOptimizerFactory;
            _queryableMethodTranslatingExpressionVisitorFactory = queryableMethodTranslatingExpressionVisitorFactory;
            _shapedQueryOptimizerFactory = shapedQueryOptimizerFactory;
            _shapedQueryCompilingExpressionVisitorFactory = shapedQueryCompilingExpressionVisitorFactory;
        }

        public bool Async { get; }
        public IModel Model { get; }
        public IDbContextOptions ContextOptions { get; }
        public bool TrackQueryResults { get; internal set; }
        public ISet<string> Tags { get; } = new HashSet<string>();
        public virtual IDiagnosticsLogger<DbLoggerCategory.Query> Logger { get; }
        public virtual Type ContextType { get; }

        public virtual void AddTag(string tag)
        {
            Tags.Add(tag);
        }

        public virtual Func<QueryContext, TResult> CreateQueryExecutor<TResult>(Expression query)
        {
            query = _queryOptimizerFactory.Create(this).Visit(query);
            // Convert EntityQueryable to ShapedQueryExpression
            query = _queryableMethodTranslatingExpressionVisitorFactory.Create(Model).Visit(query);
            query = _shapedQueryOptimizerFactory.Create(this).Visit(query);

            // Inject actual entity materializer
            // Inject tracking
            query = _shapedQueryCompilingExpressionVisitorFactory.Create(this).Visit(query);

            // If any additional parameters were added during the compilation phase (e.g. entity equality ID expression),
            // wrap the query with code adding those parameters to the query context
            query = InsertRuntimeParameters(query);

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

        /// <summary>
        ///     Registers a runtime parameter that is being added at some point during the compilation phase.
        ///     A lambda must be provided, which will extract the parameter's value from the QueryContext every time
        ///     the query is executed.
        /// </summary>
        public void RegisterRuntimeParameter(string name, LambdaExpression valueExtractor)
        {
            if (valueExtractor.Parameters.Count != 1
                || valueExtractor.Parameters[0] != QueryContextParameter
                || valueExtractor.ReturnType != typeof(object))
            {
                throw new ArgumentException("Runtime parameter extraction lambda must have one QueryContext parameter and return an object",
                    nameof(valueExtractor));
            }

            if (_runtimeParameters == null)
            {
                _runtimeParameters = new Dictionary<string, LambdaExpression>();
            }

            _runtimeParameters[name] = valueExtractor;
        }

        private Expression InsertRuntimeParameters(Expression query)
            => _runtimeParameters == null
                ? query
                : Expression.Block(_runtimeParameters
                    .Select(kv =>
                        Expression.Call(
                            QueryContextParameter,
                            _queryContextAddParameterMethodInfo,
                            Expression.Constant(kv.Key),
                            Expression.Invoke(kv.Value, QueryContextParameter)))
                    .Append(query));

        private static readonly MethodInfo _queryContextAddParameterMethodInfo
            = typeof(QueryContext)
                .GetTypeInfo()
                .GetDeclaredMethod(nameof(QueryContext.AddParameter));
    }
}
