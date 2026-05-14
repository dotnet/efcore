// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         The primary data structure representing the state/components used during relational query compilation.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class RelationalQueryCompilationContext : QueryCompilationContext
{
    private readonly ExpressionPrinter _expressionPrinter = new();
    private readonly ITypeMappingSource _typeMappingSource;

    /// <summary>
    ///     Creates a new instance of the <see cref="RelationalQueryCompilationContext" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this class.</param>
    /// <param name="relationalDependencies">Parameter object containing relational dependencies for this class.</param>
    /// <param name="async">A bool value indicating whether it is for async query.</param>
    public RelationalQueryCompilationContext(
        QueryCompilationContextDependencies dependencies,
        RelationalQueryCompilationContextDependencies relationalDependencies,
        bool async)
        : this(dependencies, relationalDependencies, async, precompiling: false)
    {
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="RelationalQueryCompilationContext" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this class.</param>
    /// <param name="relationalDependencies">Parameter object containing relational dependencies for this class.</param>
    /// <param name="async">A bool value indicating whether it is for async query.</param>
    /// <param name="precompiling">Indicates whether the query is being precompiled.</param>
    [Experimental(EFDiagnostics.PrecompiledQueryExperimental)]
    public RelationalQueryCompilationContext(
        QueryCompilationContextDependencies dependencies,
        RelationalQueryCompilationContextDependencies relationalDependencies,
        bool async,
        bool precompiling)
        : base(dependencies, async, precompiling)
    {
        RelationalDependencies = relationalDependencies;
        QuerySplittingBehavior = RelationalOptionsExtension.Extract(ContextOptions).QuerySplittingBehavior;
        SqlAliasManager = relationalDependencies.SqlAliasManagerFactory.Create();
        _typeMappingSource = dependencies.Context.GetService<ITypeMappingSource>();
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    public virtual RelationalQueryCompilationContextDependencies RelationalDependencies { get; }

    /// <summary>
    ///     A value indicating the <see cref="EntityFrameworkCore.QuerySplittingBehavior" /> configured for the query.
    ///     If no value has been configured then <see cref="QuerySplittingBehavior.SingleQuery" />
    ///     will be used.
    /// </summary>
    public virtual QuerySplittingBehavior? QuerySplittingBehavior { get; internal set; }

    /// <summary>
    ///     A manager for SQL aliases, capable of generate uniquified table aliases.
    /// </summary>
    public virtual SqlAliasManager SqlAliasManager { get; }

    /// <inheritdoc />
    public override Func<QueryContext, IEnumerable<TElement>> CreateEnumerableQueryExecutor<TElement>(Expression query)
    {
        var postprocessedQuery = TranslateQuery(query);
        Logger.QueryExecutionPlanned(Dependencies.Context, _expressionPrinter, postprocessedQuery);

        var runtimeParameterPopulators = GetRuntimeParameterPopulators();

        if (postprocessedQuery is ShapedQueryExpression { ResultCardinality: ResultCardinality.Enumerable } shapedQuery)
        {
            ValidateShaper(shapedQuery.ShaperExpression);

            var inner = RelationalDependencies.RelationalMaterializerFactory
                .CreateEnumerableMaterializer<TElement>(this, shapedQuery);

            return runtimeParameterPopulators is null
                ? inner
                : qc => { runtimeParameterPopulators(qc); return inner(qc); };
        }

        throw new NotImplementedException(
            $"The non-generated materializer does not yet support this query shape (TElement={typeof(TElement).Name}, "
            + $"postprocessed expression type: {postprocessedQuery.GetType().Name}).");
    }

    /// <inheritdoc />
    public override Func<QueryContext, TResult> CreateSingleValueQueryExecutor<TResult>(Expression query)
    {
        var postprocessedQuery = TranslateQuery(query);
        Logger.QueryExecutionPlanned(Dependencies.Context, _expressionPrinter, postprocessedQuery);

        var runtimeParameterPopulators = GetRuntimeParameterPopulators();

        switch (postprocessedQuery)
        {
            case ShapedQueryExpression { ResultCardinality: ResultCardinality.Single or ResultCardinality.SingleOrDefault } shapedQuery:
            {
                ValidateShaper(shapedQuery.ShaperExpression);

                var enumerableFactory = RelationalDependencies.RelationalMaterializerFactory
                    .CreateEnumerableMaterializer<TResult>(this, shapedQuery);

                Func<QueryContext, TResult> inner = shapedQuery.ResultCardinality switch
                {
                    ResultCardinality.Single => qc => enumerableFactory(qc).Single()!,
                    ResultCardinality.SingleOrDefault => qc => enumerableFactory(qc).SingleOrDefault()!,

                    _ => throw new UnreachableException()
                };

                return runtimeParameterPopulators is null
                    ? inner
                    : qc => { runtimeParameterPopulators(qc); return inner(qc); };
            }

            // Enumerable cardinality can appear here when ToQueryString() calls
            // CreateSingleValueQueryExecutor<IEnumerable>. Return the enumerable directly.
            case ShapedQueryExpression { ResultCardinality: ResultCardinality.Enumerable } shapedQuery:
            {
                ValidateShaper(shapedQuery.ShaperExpression);

                var enumerableFactory = RelationalDependencies.RelationalMaterializerFactory
                    .CreateEnumerableMaterializer<TResult>(this, shapedQuery);

                Func<QueryContext, TResult> inner = qc => (TResult)(object)enumerableFactory(qc)!;

                return runtimeParameterPopulators is null
                    ? inner
                    : qc => { runtimeParameterPopulators(qc); return inner(qc); };
            }

            case DeleteExpression or UpdateExpression:
            {
                var inner = RelationalDependencies.RelationalMaterializerFactory
                    .CreateNonQueryExecutor(this, postprocessedQuery);

                Func<QueryContext, TResult> typed = qc => (TResult)(object)inner(qc);

                return runtimeParameterPopulators is null
                    ? typed
                    : qc => { runtimeParameterPopulators(qc); return typed(qc); };
            }

            default:
                throw new NotImplementedException(
                    $"The non-generated materializer does not yet support this query shape (TResult={typeof(TResult).Name}, "
                    + $"postprocessed expression type: {postprocessedQuery.GetType().Name}).");
        }
    }

    /// <inheritdoc />
    public override Func<QueryContext, Task<TResult>> CreateSingleValueAsyncQueryExecutor<TResult>(Expression query)
    {
        var postprocessedQuery = TranslateQuery(query);
        Logger.QueryExecutionPlanned(Dependencies.Context, _expressionPrinter, postprocessedQuery);

        var runtimeParameterPopulators = GetRuntimeParameterPopulators();

        switch (postprocessedQuery)
        {
            case ShapedQueryExpression { ResultCardinality: ResultCardinality.Single or ResultCardinality.SingleOrDefault } shapedQuery:
            {
                ValidateShaper(shapedQuery.ShaperExpression);

                var enumerableFactory = RelationalDependencies.RelationalMaterializerFactory
                    .CreateEnumerableMaterializer<TResult>(this, shapedQuery);

                Func<QueryContext, Task<TResult>> inner = shapedQuery.ResultCardinality switch
                {
                    ResultCardinality.Single =>
                        qc => ((IAsyncEnumerable<TResult>)enumerableFactory(qc))
                            .SingleAsync(((RelationalQueryContext)qc).CancellationToken).AsTask(),

                    ResultCardinality.SingleOrDefault =>
                        qc => ((IAsyncEnumerable<TResult>)enumerableFactory(qc))
                            .SingleOrDefaultAsync(((RelationalQueryContext)qc).CancellationToken).AsTask()!,

                    _ => throw new UnreachableException()
                };

                return runtimeParameterPopulators is null
                    ? inner
                    : qc => { runtimeParameterPopulators(qc); return inner(qc); };
            }

            case DeleteExpression or UpdateExpression:
            {
                var inner = RelationalDependencies.RelationalMaterializerFactory
                    .CreateNonQueryAsyncExecutor(this, postprocessedQuery);

                Func<QueryContext, Task<TResult>> typed = async qc => (TResult)(object)await inner(qc).ConfigureAwait(false);

                return runtimeParameterPopulators is null
                    ? typed
                    : qc => { runtimeParameterPopulators(qc); return typed(qc); };
            }

            default:
                throw new NotImplementedException(
                    $"The non-generated materializer does not yet support this query shape (TResult={typeof(TResult).Name}, "
                    + $"postprocessed expression type: {postprocessedQuery.GetType().Name}).");
        }
    }

    private Expression TranslateQuery(Expression query)
    {
        var queryAndEventData = Logger.QueryCompilationStarting(Dependencies.Context, _expressionPrinter, query);
        var preprocessedQuery = Dependencies.QueryTranslationPreprocessorFactory.Create(this).Process(queryAndEventData.Query);
        var translatedQuery = Dependencies.QueryableMethodTranslatingExpressionVisitorFactory.Create(this).Translate(preprocessedQuery);
        return Dependencies.QueryTranslationPostprocessorFactory.Create(this).Process(translatedQuery);
    }

    private void ValidateShaper(Expression shaperExpression)
        => new ShaperValidator(_typeMappingSource).Validate(shaperExpression, QueryTrackingBehavior);

    /// <summary>
    ///     Validates universal shaper invariants before building a non-generated materializer.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Client projections must not capture arbitrary object constants in cached query delegates. Constants
    ///         which can be mapped by the provider, such as scalar values, are allowed. Other constants may keep
    ///         application objects alive through the compiled query cache. For example, <c>Select(c => InstanceMethod(c))</c>
    ///         captures <c>this</c> as the method instance, <c>Select(c => StaticMethod(this, c))</c> captures <c>this</c>
    ///         as a method argument, and <c>Select(c => new { A = this })</c> captures <c>this</c> directly in the
    ///         projection tree.
    ///     </para>
    ///     <para>
    ///         Tracking projections must also contain the owners of any projected owned entities, since owned entities
    ///         cannot be tracked without their owner.
    ///     </para>
    /// </remarks>
    private sealed class ShaperValidator(ITypeMappingSource typeMappingSource) : ExpressionVisitor
    {
        private readonly HashSet<IEntityType> _visitedEntityTypes = [];

        private bool _validatingTrackAll;

        public void Validate(Expression shaperExpression, QueryTrackingBehavior queryTrackingBehavior)
        {
            _validatingTrackAll = queryTrackingBehavior == QueryTrackingBehavior.TrackAll;
            _visitedEntityTypes.Clear();

            Visit(shaperExpression);

            if (_validatingTrackAll)
            {
                foreach (var entityType in _visitedEntityTypes)
                {
                    if (entityType.FindOwnership() is { } ownership
                        && !ContainsOwner(ownership.PrincipalEntityType))
                    {
                        throw new InvalidOperationException(CoreStrings.OwnedEntitiesCannotBeTrackedWithoutTheirOwner);
                    }
                }
            }
        }

        private bool ValidConstant(ConstantExpression constantExpression)
            => constantExpression.Value is null or Array { Length: 0 }
                || typeMappingSource.FindMapping(constantExpression.Type) != null;

        protected override Expression VisitConstant(ConstantExpression constantExpression)
            => !ValidConstant(constantExpression)
                ? throw new InvalidOperationException(
                    CoreStrings.ClientProjectionCapturingConstantInTree(constantExpression.Type.DisplayName()))
                : constantExpression;

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (RemoveConvert(methodCallExpression.Object) is ConstantExpression constantInstance
                && !ValidConstant(constantInstance))
            {
                throw new InvalidOperationException(
                    CoreStrings.ClientProjectionCapturingConstantInMethodInstance(
                        constantInstance.Type.DisplayName(),
                        methodCallExpression.Method.Name));
            }

            foreach (var argument in methodCallExpression.Arguments)
            {
                if (RemoveConvert(argument) is ConstantExpression constantArgument
                    && !ValidConstant(constantArgument))
                {
                    throw new InvalidOperationException(
                        CoreStrings.ClientProjectionCapturingConstantInMethodArgument(
                            constantArgument.Type.DisplayName(),
                            methodCallExpression.Method.Name));
                }
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (_validatingTrackAll
                && extensionExpression is StructuralTypeShaperExpression { StructuralType: IEntityType entityType })
            {
                _visitedEntityTypes.Add(entityType);
            }

            return extensionExpression is StructuralTypeShaperExpression or ProjectionBindingExpression
                ? extensionExpression
                : base.VisitExtension(extensionExpression);
        }

        private bool ContainsOwner(IEntityType? owner)
            => owner is not null
                && (_visitedEntityTypes.Any(owner.IsAssignableFrom) || ContainsOwner(owner.BaseType));

        private static Expression? RemoveConvert(Expression? expression)
        {
            while (expression is { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked })
            {
                expression = RemoveConvert(((UnaryExpression)expression).Operand);
            }

            return expression;
        }
    }
}
