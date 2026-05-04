// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;

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
        var queryAndEventData = Logger.QueryCompilationStarting(Dependencies.Context, new ExpressionPrinter(), query);
        var interceptedQuery = queryAndEventData.Query;

        var preprocessedQuery = Dependencies.QueryTranslationPreprocessorFactory.Create(this).Process(interceptedQuery);
        var translatedQuery = Dependencies.QueryableMethodTranslatingExpressionVisitorFactory.Create(this).Translate(preprocessedQuery);
        var postprocessedQuery = Dependencies.QueryTranslationPostprocessorFactory.Create(this).Process(translatedQuery);

        return postprocessedQuery switch
        {
            ShapedQueryExpression { ResultCardinality: ResultCardinality.Enumerable } shapedQuery
                => RelationalDependencies.RelationalMaterializerFactory
                    .CreateEnumerableMaterializer<TElement>(this, shapedQuery),

                _ => throw new NotImplementedException(
                    $"The non-generated materializer does not yet support this query shape (TElement={typeof(TElement).Name}, "
                    + $"postprocessed expression type: {postprocessedQuery.GetType().Name})."),
        };
    }

    /// <inheritdoc />
    public override Func<QueryContext, TResult> CreateSingleValueQueryExecutor<TResult>(Expression query)
    {
        var postprocessedQuery = TranslateQuery(query);

        switch (postprocessedQuery)
        {
            case ShapedQueryExpression { ResultCardinality: ResultCardinality.Single or ResultCardinality.SingleOrDefault } shapedQuery:
            {
                var enumerableFactory = RelationalDependencies.RelationalMaterializerFactory
                    .CreateEnumerableMaterializer<TResult>(this, shapedQuery);

                return shapedQuery.ResultCardinality switch
                {
                    ResultCardinality.Single => qc => enumerableFactory(qc).Single()!,
                    ResultCardinality.SingleOrDefault => qc => enumerableFactory(qc).SingleOrDefault()!,

                    _ => throw new UnreachableException()
                };
            }

            // Non-query operations (ExecuteDelete / ExecuteUpdate) produce a DeleteExpression / UpdateExpression
            // instead of a ShapedQueryExpression. These don't involve entity materialization — they just execute
            // a SQL command and return the affected row count. This mirrors the generated shaper's NonQueryResult.
            case DeleteExpression or UpdateExpression:
            {
                var commandCache = CreateNonQueryCommandCache(postprocessedQuery);
                var contextType = ContextType;
                var threadSafetyChecksEnabled = RelationalDependencies.RelationalMaterializerFactory.ThreadSafetyChecksEnabled;

                return qc => (TResult)(object)ExecuteNonQuery(
                    (RelationalQueryContext)qc, commandCache.GetRelationalCommandTemplate, contextType,
                    CommandSource.ExecuteUpdate, threadSafetyChecksEnabled);
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

        switch (postprocessedQuery)
        {
            case ShapedQueryExpression { ResultCardinality: ResultCardinality.Single or ResultCardinality.SingleOrDefault } shapedQuery:
            {
                var enumerableFactory = RelationalDependencies.RelationalMaterializerFactory
                    .CreateEnumerableMaterializer<TResult>(this, shapedQuery);

                return shapedQuery.ResultCardinality switch
                {
                    ResultCardinality.Single =>
                        qc => ((IAsyncEnumerable<TResult>)enumerableFactory(qc))
                            .SingleAsync(((RelationalQueryContext)qc).CancellationToken).AsTask(),

                    ResultCardinality.SingleOrDefault =>
                        qc => ((IAsyncEnumerable<TResult>)enumerableFactory(qc))
                            .SingleOrDefaultAsync(((RelationalQueryContext)qc).CancellationToken).AsTask()!,

                    _ => throw new UnreachableException()
                };
            }

            case DeleteExpression or UpdateExpression:
            {
                var commandCache = CreateNonQueryCommandCache(postprocessedQuery);
                var contextType = ContextType;
                var threadSafetyChecksEnabled = RelationalDependencies.RelationalMaterializerFactory.ThreadSafetyChecksEnabled;

                return async qc => (TResult)(object)await ExecuteNonQueryAsync(
                    (RelationalQueryContext)qc, commandCache.GetRelationalCommandTemplate, contextType,
                    CommandSource.ExecuteUpdate, threadSafetyChecksEnabled).ConfigureAwait(false);
            }

            default:
                throw new NotImplementedException(
                    $"The non-generated materializer does not yet support this query shape (TResult={typeof(TResult).Name}, "
                    + $"postprocessed expression type: {postprocessedQuery.GetType().Name}).");
        }
    }

    private Expression TranslateQuery(Expression query)
    {
        var queryAndEventData = Logger.QueryCompilationStarting(Dependencies.Context, new ExpressionPrinter(), query);
        var interceptedQuery = queryAndEventData.Query;

        var preprocessedQuery = Dependencies.QueryTranslationPreprocessorFactory.Create(this).Process(interceptedQuery);
        var translatedQuery = Dependencies.QueryableMethodTranslatingExpressionVisitorFactory.Create(this).Translate(preprocessedQuery);
        return Dependencies.QueryTranslationPostprocessorFactory.Create(this).Process(translatedQuery);
    }

    private RelationalCommandCache CreateNonQueryCommandCache(Expression nonQueryExpression)
    {
        var useRelationalNulls = RelationalOptionsExtension.Extract(ContextOptions).UseRelationalNulls;
        var collectionParameterTranslationMode
            = RelationalOptionsExtension.Extract(ContextOptions).ParameterizedCollectionMode;

        // Apply tags, matching the generated shaper
        if (nonQueryExpression is DeleteExpression deleteExpression)
        {
            nonQueryExpression = deleteExpression.ApplyTags(Tags);
        }
        else if (nonQueryExpression is UpdateExpression updateExpression)
        {
            nonQueryExpression = updateExpression.ApplyTags(Tags);
        }

        return new RelationalCommandCache(
            RelationalDependencies.MemoryCache,
            RelationalDependencies.QuerySqlGeneratorFactory,
            RelationalDependencies.RelationalParameterBasedSqlProcessorFactory,
            nonQueryExpression,
            useRelationalNulls,
            collectionParameterTranslationMode);
    }

    private static int ExecuteNonQuery(
        RelationalQueryContext relationalQueryContext,
        RelationalCommandResolver relationalCommandResolver,
        Type contextType,
        CommandSource commandSource,
        bool threadSafetyChecksEnabled)
    {
        try
        {
            using var _ = threadSafetyChecksEnabled
                ? relationalQueryContext.ConcurrencyDetector.EnterCriticalSection()
                : default(ConcurrencyDetectorCriticalSectionDisposer?);

            return relationalQueryContext.ExecutionStrategy.Execute(
                (relationalQueryContext, relationalCommandResolver, commandSource),
                static (_, state) =>
                {
                    EntityFrameworkMetricsData.ReportQueryExecuting();

                    var relationalCommand = state.relationalCommandResolver.RentAndPopulateRelationalCommand(state.relationalQueryContext);

                    return relationalCommand.ExecuteNonQuery(
                        new RelationalCommandParameterObject(
                            state.relationalQueryContext.Connection,
                            state.relationalQueryContext.Parameters,
                            null,
                            state.relationalQueryContext.Context,
                            state.relationalQueryContext.CommandLogger,
                            state.commandSource));
                },
                null);
        }
        catch (Exception exception)
        {
            HandleNonQueryException(relationalQueryContext, contextType, commandSource, exception);
            throw;
        }
    }

    private static Task<int> ExecuteNonQueryAsync(
        RelationalQueryContext relationalQueryContext,
        RelationalCommandResolver relationalCommandResolver,
        Type contextType,
        CommandSource commandSource,
        bool threadSafetyChecksEnabled)
    {
        try
        {
            using var _ = threadSafetyChecksEnabled
                ? relationalQueryContext.ConcurrencyDetector.EnterCriticalSection()
                : default(ConcurrencyDetectorCriticalSectionDisposer?);

            return relationalQueryContext.ExecutionStrategy.ExecuteAsync(
                (relationalQueryContext, relationalCommandResolver, commandSource),
                static (_, state, cancellationToken) =>
                {
                    EntityFrameworkMetricsData.ReportQueryExecuting();

                    var relationalCommand = state.relationalCommandResolver.RentAndPopulateRelationalCommand(state.relationalQueryContext);

                    return relationalCommand.ExecuteNonQueryAsync(
                        new RelationalCommandParameterObject(
                            state.relationalQueryContext.Connection,
                            state.relationalQueryContext.Parameters,
                            null,
                            state.relationalQueryContext.Context,
                            state.relationalQueryContext.CommandLogger,
                            state.commandSource),
                        cancellationToken);
                },
                null,
                relationalQueryContext.CancellationToken);
        }
        catch (Exception exception)
        {
            HandleNonQueryException(relationalQueryContext, contextType, commandSource, exception);
            throw;
        }
    }

    private static void HandleNonQueryException(
        RelationalQueryContext relationalQueryContext,
        Type contextType,
        CommandSource commandSource,
        Exception exception)
    {
        if (relationalQueryContext.ExceptionDetector.IsCancellation(exception))
        {
            relationalQueryContext.QueryLogger.QueryCanceled(contextType);
        }
        else
        {
            switch (commandSource)
            {
                case CommandSource.ExecuteDelete:
                    relationalQueryContext.QueryLogger.ExecuteDeleteFailed(contextType, exception);
                    break;

                case CommandSource.ExecuteUpdate:
                    relationalQueryContext.QueryLogger.ExecuteUpdateFailed(contextType, exception);
                    break;

                default:
                    relationalQueryContext.QueryLogger.NonQueryOperationFailed(contextType, exception);
                    break;
            }
        }
    }
}
