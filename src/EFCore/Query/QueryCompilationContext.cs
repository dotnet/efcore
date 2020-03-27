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
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class QueryCompilationContext
    {
        /// <summary>
        ///     <para>
        ///         Prefix for all the query parameters generated during parameter extraction in query pipeline.
        ///     </para>
        ///     <para>
        ///         This property is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        public const string QueryParameterPrefix = "__";

        /// <summary>
        ///     <para>
        ///         ParameterExpression representing <see cref="QueryContext"/> parameter in query expression.
        ///     </para>
        ///     <para>
        ///         This property is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        public static readonly ParameterExpression QueryContextParameter = Expression.Parameter(typeof(QueryContext), "queryContext");

        private readonly IQueryTranslationPreprocessorFactory _queryTranslationPreprocessorFactory;
        private readonly IQueryableMethodTranslatingExpressionVisitorFactory _queryableMethodTranslatingExpressionVisitorFactory;
        private readonly IQueryTranslationPostprocessorFactory _queryTranslationPostprocessorFactory;
        private readonly IShapedQueryCompilingExpressionVisitorFactory _shapedQueryCompilingExpressionVisitorFactory;

        private Dictionary<string, LambdaExpression> _runtimeParameters;

        public QueryCompilationContext(
            [NotNull] QueryCompilationContextDependencies dependencies,
            bool async)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            IsAsync = async;
            IsTracking = dependencies.IsTracking;
            IsBuffering = dependencies.IsRetryingExecutionStrategy;
            Model = dependencies.Model;
            ContextOptions = dependencies.ContextOptions;
            ContextType = dependencies.ContextType;
            Logger = dependencies.Logger;

            _queryTranslationPreprocessorFactory = dependencies.QueryTranslationPreprocessorFactory;
            _queryableMethodTranslatingExpressionVisitorFactory = dependencies.QueryableMethodTranslatingExpressionVisitorFactory;
            _queryTranslationPostprocessorFactory = dependencies.QueryTranslationPostprocessorFactory;
            _shapedQueryCompilingExpressionVisitorFactory = dependencies.ShapedQueryCompilingExpressionVisitorFactory;
        }

        public virtual bool IsAsync { get; }
        public virtual IModel Model { get; }
        public virtual IDbContextOptions ContextOptions { get; }
        public virtual bool IsTracking { get; internal set; }
        public virtual bool IsBuffering { get; }
        public virtual bool IgnoreQueryFilters { get; internal set; }
        public virtual ISet<string> Tags { get; } = new HashSet<string>();
        public virtual IDiagnosticsLogger<DbLoggerCategory.Query> Logger { get; }
        public virtual Type ContextType { get; }

        public virtual void AddTag([NotNull] string tag)
        {
            Check.NotEmpty(tag, nameof(tag));

            Tags.Add(tag);
        }

        public virtual Func<QueryContext, TResult> CreateQueryExecutor<TResult>([NotNull] Expression query)
        {
            Check.NotNull(query, nameof(query));

            query = _queryTranslationPreprocessorFactory.Create(this).Process(query);
            // Convert EntityQueryable to ShapedQueryExpression
            query = _queryableMethodTranslatingExpressionVisitorFactory.Create(this).Visit(query);
            query = _queryTranslationPostprocessorFactory.Create(this).Process(query);

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
        public virtual ParameterExpression RegisterRuntimeParameter([NotNull] string name, [NotNull] LambdaExpression valueExtractor)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(valueExtractor, nameof(valueExtractor));

            if (valueExtractor.Parameters.Count != 1
                || valueExtractor.Parameters[0] != QueryContextParameter)
            {
                throw new ArgumentException(CoreStrings.RuntimeParameterMissingParameter, nameof(valueExtractor));
            }

            if (_runtimeParameters == null)
            {
                _runtimeParameters = new Dictionary<string, LambdaExpression>();
            }

            _runtimeParameters[name] = valueExtractor;
            return Expression.Parameter(valueExtractor.ReturnType, name);
        }

        private Expression InsertRuntimeParameters(Expression query)
            => _runtimeParameters == null
                ? query
                : Expression.Block(
                    _runtimeParameters
                        .Select(
                            kv =>
                                Expression.Call(
                                    QueryContextParameter,
                                    _queryContextAddParameterMethodInfo,
                                    Expression.Constant(kv.Key),
                                    Expression.Convert(Expression.Invoke(kv.Value, QueryContextParameter), typeof(object))))
                        .Append(query));

        private static readonly MethodInfo _queryContextAddParameterMethodInfo
            = typeof(QueryContext)
                .GetTypeInfo()
                .GetDeclaredMethod(nameof(QueryContext.AddParameter));
    }
}
