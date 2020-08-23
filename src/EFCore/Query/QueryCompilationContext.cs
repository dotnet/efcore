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
    /// <summary>
    ///     <para>
    ///         The primary data structure representing the state/components used during query compilation.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
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
        ///         ParameterExpression representing <see cref="QueryContext" /> parameter in query expression.
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

        private readonly ExpressionPrinter _expressionPrinter;

        private Dictionary<string, LambdaExpression> _runtimeParameters;

        /// <summary>
        ///     Creates a new instance of the <see cref="QueryCompilationContext" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this class. </param>
        /// <param name="async"> A bool value indicating whether it is for async query. </param>
        public QueryCompilationContext(
            [NotNull] QueryCompilationContextDependencies dependencies,
            bool async)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
            IsAsync = async;
            QueryTrackingBehavior = dependencies.QueryTrackingBehavior;
            IsBuffering = dependencies.IsRetryingExecutionStrategy;
            Model = dependencies.Model;
            ContextOptions = dependencies.ContextOptions;
            ContextType = dependencies.ContextType;
            Logger = dependencies.Logger;

            _queryTranslationPreprocessorFactory = dependencies.QueryTranslationPreprocessorFactory;
            _queryableMethodTranslatingExpressionVisitorFactory = dependencies.QueryableMethodTranslatingExpressionVisitorFactory;
            _queryTranslationPostprocessorFactory = dependencies.QueryTranslationPostprocessorFactory;
            _shapedQueryCompilingExpressionVisitorFactory = dependencies.ShapedQueryCompilingExpressionVisitorFactory;

            _expressionPrinter = new ExpressionPrinter();
        }

        /// <summary>
        ///     Parameter object containing dependencies for this service.
        /// </summary>
        protected virtual QueryCompilationContextDependencies Dependencies { get; }

        /// <summary>
        ///     A value indicating whether it is async query.
        /// </summary>
        public virtual bool IsAsync { get; }

        /// <summary>
        ///     The model to use during query compilation.
        /// </summary>
        public virtual IModel Model { get; }

        /// <summary>
        ///     The ContextOptions to use during query compilation.
        /// </summary>
        public virtual IDbContextOptions ContextOptions { get; }

        /// <summary>
        ///     A value indicating <see cref="EntityFrameworkCore.QueryTrackingBehavior" /> of the query.
        /// </summary>
        public virtual QueryTrackingBehavior QueryTrackingBehavior { get; internal set; }

        /// <summary>
        ///     A value indicating whether it is tracking query.
        /// </summary>
        [Obsolete("Use " + nameof(QueryTrackingBehavior) + " instead.")]
        public virtual bool IsTracking
            => QueryTrackingBehavior == QueryTrackingBehavior.TrackAll;

        /// <summary>
        ///     A value indicating whether the underlying server query needs to pre-buffer all data.
        /// </summary>
        public virtual bool IsBuffering { get; }

        /// <summary>
        ///     A value indicating whether query filters are ignored in this query.
        /// </summary>
        public virtual bool IgnoreQueryFilters { get; internal set; }

        /// <summary>
        ///     A value indicating whether eager loaded navigations are ignored in this query.
        /// </summary>
        public virtual bool IgnoreAutoIncludes { get; internal set; }

        /// <summary>
        ///     The set of tags applied to this query.
        /// </summary>
        public virtual ISet<string> Tags { get; } = new HashSet<string>();

        /// <summary>
        ///     The query logger to use during query compilation.
        /// </summary>
        public virtual IDiagnosticsLogger<DbLoggerCategory.Query> Logger { get; }

        /// <summary>
        ///     The CLR type of derived DbContext to use during query compilation.
        /// </summary>
        public virtual Type ContextType { get; }

        /// <summary>
        ///     Adds a tag to <see cref="Tags" />.
        /// </summary>
        /// <param name="tag"> The tag to add. </param>
        public virtual void AddTag([NotNull] string tag)
        {
            Check.NotEmpty(tag, nameof(tag));

            Tags.Add(tag);
        }

        /// <summary>
        ///     Creates the query executor func which gives results for this query.
        /// </summary>
        /// <typeparam name="TResult"> The result type of this query. </typeparam>
        /// <param name="query"> The query to generate executor for. </param>
        /// <returns> Returns <see cref="Func{QueryContext, TResult}" /> which can be invoked to get results of this query. </returns>
        public virtual Func<QueryContext, TResult> CreateQueryExecutor<TResult>([NotNull] Expression query)
        {
            Check.NotNull(query, nameof(query));

            Logger.QueryCompilationStarting(_expressionPrinter, query);

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
                Logger.QueryExecutionPlanned(_expressionPrinter, queryExecutorExpression);
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
