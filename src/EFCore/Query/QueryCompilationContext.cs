// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

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
                    .Select(
                        kv =>
                            Expression.Call(
                                QueryContextParameter,
                                QueryContextAddParameterMethodInfo,
                                Expression.Constant(kv.Key),
                                Expression.Convert(Expression.Invoke(kv.Value, QueryContextParameter), typeof(object))))
                    .Append(query));

    private static readonly MethodInfo QueryContextAddParameterMethodInfo
        = typeof(QueryContext).GetTypeInfo().GetDeclaredMethod(nameof(QueryContext.AddParameter))!;

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
            typeof(List<IComplexProperty>).GetMethod(nameof(List<IComplexProperty>.Add))!;

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
