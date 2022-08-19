// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

/// <inheritdoc />
public partial class RelationalShapedQueryCompilingExpressionVisitor : ShapedQueryCompilingExpressionVisitor
{
    private readonly Type _contextType;
    private readonly ISet<string> _tags;
    private readonly bool _threadSafetyChecksEnabled;
    private readonly bool _detailedErrorsEnabled;
    private readonly bool _useRelationalNulls;

    /// <summary>
    ///     Creates a new instance of the <see cref="ShapedQueryCompilingExpressionVisitor" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this class.</param>
    /// <param name="relationalDependencies">Parameter object containing relational dependencies for this class.</param>
    /// <param name="queryCompilationContext">The query compilation context object to use.</param>
    public RelationalShapedQueryCompilingExpressionVisitor(
        ShapedQueryCompilingExpressionVisitorDependencies dependencies,
        RelationalShapedQueryCompilingExpressionVisitorDependencies relationalDependencies,
        QueryCompilationContext queryCompilationContext)
        : base(dependencies, queryCompilationContext)
    {
        RelationalDependencies = relationalDependencies;

        _contextType = queryCompilationContext.ContextType;
        _tags = queryCompilationContext.Tags;
        _threadSafetyChecksEnabled = dependencies.CoreSingletonOptions.AreThreadSafetyChecksEnabled;
        _detailedErrorsEnabled = dependencies.CoreSingletonOptions.AreDetailedErrorsEnabled;
        _useRelationalNulls = RelationalOptionsExtension.Extract(queryCompilationContext.ContextOptions).UseRelationalNulls;
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalShapedQueryCompilingExpressionVisitorDependencies RelationalDependencies { get; }

    /// <inheritdoc />
    protected override Expression VisitExtension(Expression extensionExpression)
        => extensionExpression is NonQueryExpression nonQueryExpression
            ? VisitNonQuery(nonQueryExpression)
            : base.VisitExtension(extensionExpression);

    /// <summary>
    ///     Visits the given <paramref name="nonQueryExpression" />, returning an expression that when compiled, can execute the non-
    ///     query operation against the database.
    /// </summary>
    /// <param name="nonQueryExpression">The expression to be compiled.</param>
    /// <returns>An expression which executes a non-query operation.</returns>
    protected virtual Expression VisitNonQuery(NonQueryExpression nonQueryExpression)
    {
        // Apply tags
        var innerExpression = nonQueryExpression.Expression;
        switch (innerExpression)
        {
            case UpdateExpression updateExpression:
                innerExpression = updateExpression.ApplyTags(_tags);
                break;

            case DeleteExpression deleteExpression:
                innerExpression = deleteExpression.ApplyTags(_tags);
                break;
        }

        var relationalCommandCache = new RelationalCommandCache(
            Dependencies.MemoryCache,
            RelationalDependencies.QuerySqlGeneratorFactory,
            RelationalDependencies.RelationalParameterBasedSqlProcessorFactory,
            innerExpression,
            _useRelationalNulls);

        return Expression.Call(
            QueryCompilationContext.IsAsync ? NonQueryAsyncMethodInfo : NonQueryMethodInfo,
            Expression.Convert(QueryCompilationContext.QueryContextParameter, typeof(RelationalQueryContext)),
            Expression.Constant(relationalCommandCache),
            Expression.Constant(_contextType),
            Expression.Constant(nonQueryExpression.CommandSource),
            Expression.Constant(_threadSafetyChecksEnabled));
    }

    private static readonly MethodInfo NonQueryMethodInfo
        = typeof(RelationalShapedQueryCompilingExpressionVisitor).GetTypeInfo()
            .GetDeclaredMethods(nameof(NonQueryResult))
            .Single(mi => mi.GetParameters().Length == 5);

    private static readonly MethodInfo NonQueryAsyncMethodInfo
        = typeof(RelationalShapedQueryCompilingExpressionVisitor).GetTypeInfo()
            .GetDeclaredMethods(nameof(NonQueryResultAsync))
            .Single(mi => mi.GetParameters().Length == 5);

    private static int NonQueryResult(
        RelationalQueryContext relationalQueryContext,
        RelationalCommandCache relationalCommandCache,
        Type contextType,
        CommandSource commandSource,
        bool threadSafetyChecksEnabled)
    {
        try
        {
            if (threadSafetyChecksEnabled)
            {
                relationalQueryContext.ConcurrencyDetector.EnterCriticalSection();
            }

            try
            {
                return relationalQueryContext.ExecutionStrategy.Execute(
                    (relationalQueryContext, relationalCommandCache, commandSource),
                    static (_, state) =>
                    {
                        EntityFrameworkEventSource.Log.QueryExecuting();

                        var relationalCommand = state.relationalCommandCache.RentAndPopulateRelationalCommand(state.relationalQueryContext);

                        return relationalCommand.ExecuteNonQuery(new RelationalCommandParameterObject(
                            state.relationalQueryContext.Connection,
                            state.relationalQueryContext.ParameterValues,
                            null,
                            state.relationalQueryContext.Context,
                            state.relationalQueryContext.CommandLogger,
                            state.commandSource));
                    },
                    null);
            }
            finally
            {
                if (threadSafetyChecksEnabled)
                {
                    relationalQueryContext.ConcurrencyDetector.ExitCriticalSection();
                }
            }
        }
        catch (Exception exception)
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

            throw;
        }
    }

    private static Task<int> NonQueryResultAsync(
        RelationalQueryContext relationalQueryContext,
        RelationalCommandCache relationalCommandCache,
        Type contextType,
        CommandSource commandSource,
        bool threadSafetyChecksEnabled)
    {
        try
        {
            if (threadSafetyChecksEnabled)
            {
                relationalQueryContext.ConcurrencyDetector.EnterCriticalSection();
            }

            try
            {
                return relationalQueryContext.ExecutionStrategy.ExecuteAsync(
                    (relationalQueryContext, relationalCommandCache, commandSource),
                    static (_, state, cancellationToken) =>
                    {
                        EntityFrameworkEventSource.Log.QueryExecuting();

                        var relationalCommand = state.relationalCommandCache.RentAndPopulateRelationalCommand(state.relationalQueryContext);

                        return relationalCommand.ExecuteNonQueryAsync(new RelationalCommandParameterObject(
                            state.relationalQueryContext.Connection,
                            state.relationalQueryContext.ParameterValues,
                            null,
                            state.relationalQueryContext.Context,
                            state.relationalQueryContext.CommandLogger,
                            state.commandSource),
                            cancellationToken);
                    },
                    null);
            }
            finally
            {
                if (threadSafetyChecksEnabled)
                {
                    relationalQueryContext.ConcurrencyDetector.ExitCriticalSection();
                }
            }
        }
        catch (Exception exception)
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

            throw;
        }
    }

    /// <inheritdoc />
    protected override Expression VisitShapedQuery(ShapedQueryExpression shapedQueryExpression)
    {
        var selectExpression = (SelectExpression)shapedQueryExpression.QueryExpression;

        VerifyNoClientConstant(shapedQueryExpression.ShaperExpression);
        var nonComposedFromSql = selectExpression.IsNonComposedFromSql();
        var querySplittingBehavior = ((RelationalQueryCompilationContext)QueryCompilationContext).QuerySplittingBehavior;
        var splitQuery = querySplittingBehavior == QuerySplittingBehavior.SplitQuery;
        var collectionCount = 0;

        var shaper = new ShaperProcessingExpressionVisitor(this, selectExpression, _tags, splitQuery, nonComposedFromSql).ProcessShaper(
            shapedQueryExpression.ShaperExpression,
            out var relationalCommandCache, out var readerColumns, out var relatedDataLoaders, ref collectionCount);

        if (querySplittingBehavior == null
            && collectionCount > 1)
        {
            QueryCompilationContext.Logger.MultipleCollectionIncludeWarning();
        }

        if (nonComposedFromSql)
        {
            return Expression.New(
                typeof(FromSqlQueryingEnumerable<>).MakeGenericType(shaper.ReturnType).GetConstructors()[0],
                Expression.Convert(QueryCompilationContext.QueryContextParameter, typeof(RelationalQueryContext)),
                Expression.Constant(relationalCommandCache),
                Expression.Constant(readerColumns, typeof(IReadOnlyList<ReaderColumn?>)),
                Expression.Constant(
                    selectExpression.Projection.Select(pe => ((ColumnExpression)pe.Expression).Name).ToList(),
                    typeof(IReadOnlyList<string>)),
                Expression.Constant(shaper.Compile()),
                Expression.Constant(_contextType),
                Expression.Constant(
                    QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution),
                Expression.Constant(_detailedErrorsEnabled),
                Expression.Constant(_threadSafetyChecksEnabled));
        }

        if (splitQuery)
        {
            var relatedDataLoadersParameter = Expression.Constant(
                QueryCompilationContext.IsAsync ? null : relatedDataLoaders?.Compile(),
                typeof(Action<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator>));

            var relatedDataLoadersAsyncParameter = Expression.Constant(
                QueryCompilationContext.IsAsync ? relatedDataLoaders?.Compile() : null,
                typeof(Func<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator, Task>));

            return Expression.New(
                typeof(SplitQueryingEnumerable<>).MakeGenericType(shaper.ReturnType).GetConstructors().Single(),
                Expression.Convert(QueryCompilationContext.QueryContextParameter, typeof(RelationalQueryContext)),
                Expression.Constant(relationalCommandCache),
                Expression.Constant(readerColumns, typeof(IReadOnlyList<ReaderColumn?>)),
                Expression.Constant(shaper.Compile()),
                relatedDataLoadersParameter,
                relatedDataLoadersAsyncParameter,
                Expression.Constant(_contextType),
                Expression.Constant(
                    QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution),
                Expression.Constant(_detailedErrorsEnabled),
                Expression.Constant(_threadSafetyChecksEnabled));
        }

        return Expression.New(
            typeof(SingleQueryingEnumerable<>).MakeGenericType(shaper.ReturnType).GetConstructors()[0],
            Expression.Convert(QueryCompilationContext.QueryContextParameter, typeof(RelationalQueryContext)),
            Expression.Constant(relationalCommandCache),
            Expression.Constant(readerColumns, typeof(IReadOnlyList<ReaderColumn?>)),
            Expression.Constant(shaper.Compile()),
            Expression.Constant(_contextType),
            Expression.Constant(
                QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution),
            Expression.Constant(_detailedErrorsEnabled),
            Expression.Constant(_threadSafetyChecksEnabled));
    }
}
