// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         The primary data structure representing the state/components used during query compilation.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     and <see href="https://aka.ms/efcore-docs-how-query-works">How EF Core queries work</see> for more information and examples.
/// </remarks>
public class QueryCompilationContext
{
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

    /// <summary>
    ///     <para>
    ///         Expression representing a not translated expression in query tree during translation phase.
    ///     </para>
    ///     <para>
    ///         This property is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public static readonly Expression NotTranslatedExpression = new NotTranslatedExpressionType();

    private readonly IQueryTranslationPreprocessorFactory _queryTranslationPreprocessorFactory;
    private readonly IQueryableMethodTranslatingExpressionVisitorFactory _queryableMethodTranslatingExpressionVisitorFactory;
    private readonly IQueryTranslationPostprocessorFactory _queryTranslationPostprocessorFactory;
    private readonly IShapedQueryCompilingExpressionVisitorFactory _shapedQueryCompilingExpressionVisitorFactory;

    private readonly ExpressionPrinter _expressionPrinter = new();
    private readonly RuntimeParameterConstantLifter _runtimeParameterConstantLifter;

    private Dictionary<string, LambdaExpression>? _runtimeParameters;

    /// <summary>
    ///     Creates a new instance of the <see cref="QueryCompilationContext" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this class.</param>
    /// <param name="async">A bool value indicating whether it is for async query.</param>
    public QueryCompilationContext(QueryCompilationContextDependencies dependencies, bool async)
        : this(dependencies, async, precompiling: false)
    {
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="QueryCompilationContext" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this class.</param>
    /// <param name="async">A bool value indicating whether it is for async query.</param>
    /// <param name="precompiling">Indicates whether the query is being precompiled.</param>
    [Experimental(EFDiagnostics.PrecompiledQueryExperimental)]
    public QueryCompilationContext(QueryCompilationContextDependencies dependencies, bool async, bool precompiling)
    {
        Dependencies = dependencies;
        IsAsync = async;
        QueryTrackingBehavior = dependencies.QueryTrackingBehavior;
        IsBuffering = ExecutionStrategy.Current?.RetriesOnFailure ?? dependencies.IsRetryingExecutionStrategy;
        IsPrecompiling = precompiling;
        Model = dependencies.Model;
        ContextOptions = dependencies.ContextOptions;
        ContextType = dependencies.ContextType;
        Logger = dependencies.Logger;

        _queryTranslationPreprocessorFactory = dependencies.QueryTranslationPreprocessorFactory;
        _queryableMethodTranslatingExpressionVisitorFactory = dependencies.QueryableMethodTranslatingExpressionVisitorFactory;
        _queryTranslationPostprocessorFactory = dependencies.QueryTranslationPostprocessorFactory;
        _shapedQueryCompilingExpressionVisitorFactory = dependencies.ShapedQueryCompilingExpressionVisitorFactory;
        _runtimeParameterConstantLifter = new RuntimeParameterConstantLifter(dependencies.LiftableConstantFactory);
    }

    /// <summary>
    ///     Dependencies for this service.
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
    ///     A value indicating whether the underlying server query needs to pre-buffer all data.
    /// </summary>
    public virtual bool IsBuffering { get; }

    /// <summary>
    ///     Indicates whether the query is being precompiled.
    /// </summary>
    public virtual bool IsPrecompiling { get; }

    /// <summary>
    ///     A value indicating whether query filters are ignored in this query.
    /// </summary>
    public virtual bool IgnoreQueryFilters { get; internal set; }

    /// <summary>
    ///     A collection of ignored query filters.
    /// </summary>
    public virtual HashSet<string>? IgnoredQueryFilters { get; internal set; }

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
    /// <param name="tag">The tag to add.</param>
    public virtual void AddTag(string tag)
        => Tags.Add(tag);

    /// <summary>
    ///     A value indicating whether the provider supports precompiled query. Default value is <see langword="false" />. Providers that do
    ///     support this feature should opt-in by setting this value to <see langword="true" />.
    /// </summary>
    [Experimental(EFDiagnostics.PrecompiledQueryExperimental)]
    public virtual bool SupportsPrecompiledQuery
        => false;

    /// <summary>
    ///     Creates the query executor func which gives results for this query.
    /// </summary>
    /// <typeparam name="TResult">The result type of this query.</typeparam>
    /// <param name="query">The query to generate executor for.</param>
    /// <returns>Returns <see cref="Func{QueryContext, TResult}" /> which can be invoked to get results of this query.</returns>
    public virtual Func<QueryContext, TResult> CreateQueryExecutor<TResult>(Expression query)
    {
        var queryExecutorExpression = CreateQueryExecutorExpression<TResult>(query);

        // The materializer expression tree has liftable constant nodes, pointing to various constants that should be the same instances
        // across invocations of the query.
        // In normal mode, these nodes should simply be evaluated, and a ConstantExpression to those instances embedded directly in the
        // tree (for precompiled queries we generate C# code for resolving those instances instead).
        var queryExecutorAfterLiftingExpression =
            (Expression<Func<QueryContext, TResult>>)Dependencies.LiftableConstantProcessor.InlineConstants(
                queryExecutorExpression, SupportsPrecompiledQuery);

        try
        {
            return queryExecutorAfterLiftingExpression.Compile();
        }
        finally
        {
            Logger.QueryExecutionPlanned(Dependencies.Context, _expressionPrinter, queryExecutorExpression);
        }
    }

    /// <summary>
    ///     Creates the query executor func for an enumerable query where <typeparamref name="TElement" /> is the
    ///     element type directly (not wrapped in <see cref="IEnumerable{T}" /> or <see cref="IAsyncEnumerable{T}" />).
    ///     The returned enumerable is expected to implement both <see cref="IEnumerable{T}" /> and
    ///     <see cref="IAsyncEnumerable{T}" />.
    /// </summary>
    /// <typeparam name="TElement">The element type of the query result.</typeparam>
    /// <param name="query">The query to generate executor for.</param>
    /// <returns>Returns <see cref="Func{QueryContext, IEnumerable}" /> which can be invoked to get results of this query.</returns>
    public virtual Func<QueryContext, IEnumerable<TElement>> CreateEnumerableQueryExecutor<TElement>(Expression query)
    {
        // Default implementation: delegate to the old expression-tree-based path.
        // The returned IEnumerable<TElement> is expected to also implement IAsyncEnumerable<TElement>
        // (SingleQueryingEnumerable<T> and InMemory's QueryingEnumerable<T> both do).
        if (IsAsync)
        {
            var asyncExecutor = CreateQueryExecutor<IAsyncEnumerable<TElement>>(query);
            return qc => (IEnumerable<TElement>)asyncExecutor(qc);
        }

        return CreateQueryExecutor<IEnumerable<TElement>>(query);
    }

    /// <summary>
    ///     Creates the query executor func for a non-enumerable sync query (e.g. Single, Count, Max) where
    ///     <typeparamref name="TResult" /> is the result type directly.
    /// </summary>
    /// <typeparam name="TResult">The result type of the query.</typeparam>
    /// <param name="query">The query to generate executor for.</param>
    /// <returns>Returns <see cref="Func{QueryContext, TResult}" /> which can be invoked to get the result of this query.</returns>
    public virtual Func<QueryContext, TResult> CreateSingleValueQueryExecutor<TResult>(Expression query)
        // Default implementation: delegate to the old expression-tree-based path.
        => CreateQueryExecutor<TResult>(query);

    /// <summary>
    ///     Creates the query executor func for a non-enumerable async query (e.g. SingleAsync, CountAsync, MaxAsync)
    ///     where <typeparamref name="TResult" /> is the result type directly (not wrapped in <see cref="Task{T}" />).
    /// </summary>
    /// <typeparam name="TResult">The result type of the query.</typeparam>
    /// <param name="query">The query to generate executor for.</param>
    /// <returns>Returns a func which can be invoked to get the async result of this query.</returns>
    public virtual Func<QueryContext, Task<TResult>> CreateSingleValueAsyncQueryExecutor<TResult>(Expression query)
        // Default implementation: delegate to the old expression-tree-based path.
        => CreateQueryExecutor<Task<TResult>>(query);

    /// <summary>
    ///     Creates the query executor func which gives results for this query.
    /// </summary>
    /// <typeparam name="TResult">The result type of this query.</typeparam>
    /// <param name="query">The query to generate executor for.</param>
    /// <returns>Returns <see cref="Func{QueryContext, TResult}" /> which can be invoked to get results of this query.</returns>
    [Experimental(EFDiagnostics.PrecompiledQueryExperimental)]
    public virtual Expression<Func<QueryContext, TResult>> CreateQueryExecutorExpression<TResult>(Expression query)
    {
        var queryAndEventData = Logger.QueryCompilationStarting(Dependencies.Context, _expressionPrinter, query);
        var interceptedQuery = queryAndEventData.Query;

        var preprocessedQuery = _queryTranslationPreprocessorFactory.Create(this).Process(interceptedQuery);
        var translatedQuery = _queryableMethodTranslatingExpressionVisitorFactory.Create(this).Translate(preprocessedQuery);
        var postprocessedQuery = _queryTranslationPostprocessorFactory.Create(this).Process(translatedQuery);

        var compiledQuery = _shapedQueryCompilingExpressionVisitorFactory.Create(this).Visit(postprocessedQuery);

        // If any additional parameters were added during the compilation phase (e.g. entity equality ID expression),
        // wrap the query with code adding those parameters to the query context
        var compiledQueryWithRuntimeParameters = InsertRuntimeParameters(compiledQuery);

        return Expression.Lambda<Func<QueryContext, TResult>>(
            compiledQueryWithRuntimeParameters,
            QueryContextParameter);
    }

    /// <summary>
    ///     Registers a runtime parameter that is being added at some point during the compilation phase.
    ///     A lambda must be provided, which will extract the parameter's value from the QueryContext every time
    ///     the query is executed.
    /// </summary>
    public virtual QueryParameterExpression RegisterRuntimeParameter(string name, LambdaExpression valueExtractor)
    {
        var valueExtractorBody = valueExtractor.Body;
        if (SupportsPrecompiledQuery)
        {
            valueExtractorBody = _runtimeParameterConstantLifter.Visit(valueExtractorBody);
        }

        valueExtractor = Expression.Lambda(valueExtractorBody, valueExtractor.Parameters);

        if (valueExtractor.Parameters.Count != 1
            || valueExtractor.Parameters[0] != QueryContextParameter)
        {
            throw new ArgumentException(CoreStrings.RuntimeParameterMissingParameter, nameof(valueExtractor));
        }

        _runtimeParameters ??= new Dictionary<string, LambdaExpression>();

        _runtimeParameters[name] = valueExtractor;
        return new QueryParameterExpression(name, valueExtractor.ReturnType);
    }

    private Expression InsertRuntimeParameters(Expression query)
        => _runtimeParameters == null
            ? query
            : Expression.Block(
                _runtimeParameters
                    .Select(kv =>
                        Expression.Call(
                            Expression.Property(
                                QueryContextParameter,
                                QueryContextParametersProperty),
                            ParameterDictionaryAddMethod,
                            Expression.Constant(kv.Key),
                            Expression.Convert(Expression.Invoke(kv.Value, QueryContextParameter), typeof(object))))
                    .Append(query));

    /// <summary>
    ///     Returns compiled runtime parameter populators that, when invoked with a <see cref="QueryContext" />,
    ///     add runtime parameters to <see cref="QueryContext.Parameters" />. Returns <see langword="null" />
    ///     if no runtime parameters were registered during compilation.
    /// </summary>
    /// <remarks>
    ///     This is the non-generated equivalent of <see cref="InsertRuntimeParameters" />.
    /// </remarks>
    protected Action<QueryContext>? GetRuntimeParameterPopulators()
    {
        if (_runtimeParameters is null)
        {
            return null;
        }

        var populators = new (string Name, Func<QueryContext, object?> ValueExtractor)[_runtimeParameters.Count];
        var i = 0;
        foreach (var (name, valueExtractor) in _runtimeParameters)
        {
            // The valueExtractor lambda may contain QueryParameterExpression nodes that reference
            // other parameters in the QueryContext. Resolve them to dictionary lookups before compiling.
            var resolvedBody = new QueryParameterReplacingVisitor(valueExtractor.Parameters[0]).Visit(valueExtractor.Body);

            populators[i++] = (name, Expression.Lambda<Func<QueryContext, object?>>(
                Expression.Convert(resolvedBody, typeof(object)),
                valueExtractor.Parameters).Compile());
        }

        return qc =>
        {
            for (var j = 0; j < populators.Length; j++)
            {
                qc.Parameters.Add(populators[j].Name, populators[j].ValueExtractor(qc));
            }
        };
    }

    private sealed class QueryParameterReplacingVisitor(ParameterExpression queryContextParameter) : ExpressionVisitor
    {
        private static readonly PropertyInfo ParametersProperty
            = typeof(QueryContext).GetProperty(nameof(QueryContext.Parameters))!;

        private static readonly PropertyInfo DictionaryIndexer
            = typeof(Dictionary<string, object?>).GetProperty("Item")!;

        protected override Expression VisitExtension(Expression node)
        {
            switch (node)
            {
                case QueryParameterExpression queryParameter:
                    return Expression.Convert(
                        Expression.Property(
                            Expression.Property(queryContextParameter, ParametersProperty),
                            DictionaryIndexer,
                            Expression.Constant(queryParameter.Name)),
                        queryParameter.Type);

                case LiftableConstantExpression liftableConstant:
                    return liftableConstant.OriginalExpression;

                default:
                    return base.VisitExtension(node);
            }
        }
    }

    private static readonly PropertyInfo QueryContextParametersProperty
        = typeof(QueryContext).GetProperty(nameof(QueryContext.Parameters))!;

    private static readonly MethodInfo ParameterDictionaryAddMethod
        = typeof(Dictionary<string, object?>).GetMethod(nameof(Dictionary<,>.Add))!;

    [DebuggerDisplay("{Microsoft.EntityFrameworkCore.Query.ExpressionPrinter.Print(this), nq}")]
    private sealed class NotTranslatedExpressionType : Expression, IPrintableExpression
    {
        public override Type Type
            => typeof(object);

        public override ExpressionType NodeType
            => ExpressionType.Extension;

        void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
            => expressionPrinter.Append("!!! NotTranslated !!!");
    }

    private sealed class RuntimeParameterConstantLifter(ILiftableConstantFactory liftableConstantFactory) : ExpressionVisitor
    {
        private static readonly MethodInfo ComplexPropertyListElementAddMethod =
            typeof(List<IComplexProperty>).GetMethod(nameof(List<>.Add))!;

        protected override Expression VisitConstant(ConstantExpression constantExpression)
        {
            switch (constantExpression.Value)
            {
                case IProperty property:
                {
                    return liftableConstantFactory.CreateLiftableConstant(
                        constantExpression.Value,
                        LiftableConstantExpressionHelpers.BuildMemberAccessLambdaForProperty(property),
                        property.Name + "Property",
                        typeof(IProperty));
                }

                case List<IComplexProperty> complexPropertyChain:
                {
                    var elementInitExpressions = new ElementInit[complexPropertyChain.Count];
                    var prm = Expression.Parameter(typeof(MaterializerLiftableConstantContext));

                    for (var i = 0; i < complexPropertyChain.Count; i++)
                    {
                        var complexType = complexPropertyChain[i].ComplexType;
                        var complexTypeExpression =
                            LiftableConstantExpressionHelpers.BuildMemberAccessForEntityOrComplexType(complexType, prm);
                        elementInitExpressions[i] = Expression.ElementInit(
                            ComplexPropertyListElementAddMethod,
                            Expression.Property(complexTypeExpression, nameof(IComplexType.ComplexProperty)));
                    }

                    return liftableConstantFactory.CreateLiftableConstant(
                        constantExpression.Value,
                        Expression.Lambda<Func<MaterializerLiftableConstantContext, object>>(
                            Expression.ListInit(Expression.New(typeof(List<IComplexProperty>)), elementInitExpressions),
                            prm),
                        "ComplexPropertyChain",
                        constantExpression.Type);
                }

                default:
                    return base.VisitConstant(constantExpression);
            }
        }
    }
}
