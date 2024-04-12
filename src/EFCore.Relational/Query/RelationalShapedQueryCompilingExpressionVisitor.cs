// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using static System.Linq.Expressions.Expression;

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

        var relationalCommandCache = CreateRelationalCommandCacheExpression(innerExpression);

        return Call(
            QueryCompilationContext.IsAsync ? NonQueryAsyncMethodInfo : NonQueryMethodInfo,
            Convert(QueryCompilationContext.QueryContextParameter, typeof(RelationalQueryContext)),
            relationalCommandCache,
            Constant(_contextType),
            Constant(nonQueryExpression.CommandSource),
            Constant(_threadSafetyChecksEnabled));
    }

    private static readonly MethodInfo NonQueryMethodInfo
        = typeof(RelationalShapedQueryCompilingExpressionVisitor).GetTypeInfo()
            .GetDeclaredMethods(nameof(NonQueryResult))
            .Single(mi => mi.GetParameters().Length == 5);

    private static readonly MethodInfo NonQueryAsyncMethodInfo
        = typeof(RelationalShapedQueryCompilingExpressionVisitor).GetTypeInfo()
            .GetDeclaredMethods(nameof(NonQueryResultAsync))
            .Single(mi => mi.GetParameters().Length == 5);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public static int NonQueryResult(
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

                        return relationalCommand.ExecuteNonQuery(
                            new RelationalCommandParameterObject(
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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public static Task<int> NonQueryResultAsync(
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

                        return relationalCommand.ExecuteNonQueryAsync(
                            new RelationalCommandParameterObject(
                                state.relationalQueryContext.Connection,
                                state.relationalQueryContext.ParameterValues,
                                null,
                                state.relationalQueryContext.Context,
                                state.relationalQueryContext.CommandLogger,
                                state.commandSource),
                            cancellationToken);
                    },
                    null,
                    relationalQueryContext.CancellationToken);
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
        var querySplittingBehavior = ((RelationalQueryCompilationContext)QueryCompilationContext).QuerySplittingBehavior;
        var splitQuery = querySplittingBehavior == QuerySplittingBehavior.SplitQuery;
        var collectionCount = 0;

        if (shapedQueryExpression.ShaperExpression is RelationalGroupByResultExpression relationalGroupByResultExpression)
        {
            var elementSelector = new ShaperProcessingExpressionVisitor(this, selectExpression, selectExpression.Tags, splitQuery, false)
                .ProcessRelationalGroupingResult(
                    relationalGroupByResultExpression,
                    out var relationalCommandCache,
                    out var readerColumns,
                    out var keySelector,
                    out var keyIdentifier,
                    out var relatedDataLoaders,
                    ref collectionCount);

            if (querySplittingBehavior == null
                && collectionCount > 1)
            {
                QueryCompilationContext.Logger.MultipleCollectionIncludeWarning();
            }

            var readerColumnsExpression = CreateReaderColumnsExpression(readerColumns, Dependencies.LiftableConstantFactory);
            if (splitQuery)
            {
                var relatedDataLoadersParameter = QueryCompilationContext.IsAsync || relatedDataLoaders == null
                    ? (Expression)Constant(null, typeof(Action<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator>))
                    : relatedDataLoaders;

                var relatedDataLoadersAsyncParameter = QueryCompilationContext.IsAsync && relatedDataLoaders != null
                    ? relatedDataLoaders!
                    : (Expression)Constant(null, typeof(Func<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator, Task>));

                return Call(
                    typeof(GroupBySplitQueryingEnumerable).GetMethods()
                        .Single(m => m.Name == nameof(GroupBySplitQueryingEnumerable.Create))
                        .MakeGenericMethod(keySelector.ReturnType, elementSelector.ReturnType),
                    Convert(QueryCompilationContext.QueryContextParameter, typeof(RelationalQueryContext)),
                    relationalCommandCache,
                    readerColumnsExpression,
                    keySelector,
                    keyIdentifier,
                    Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                        relationalGroupByResultExpression.KeyIdentifierValueComparers.Select(x => (Func<object, object, bool>)x.Equals).ToArray(),
                        Lambda<Func<MaterializerLiftableConstantContext, object>>(
                            NewArrayInit(
                                typeof(Func<object, object, bool>),
                                relationalGroupByResultExpression.KeyIdentifierValueComparers.Select(vc => vc.ObjectEqualsExpression)),
                            Parameter(typeof(MaterializerLiftableConstantContext), "_")),
                        "keyIdentifierValueComparers",
                        typeof(Func<object, object, bool>[])),
                    elementSelector,
                    relatedDataLoadersParameter,
                    relatedDataLoadersAsyncParameter,
                    Constant(_contextType),
                    Constant(QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution),
                    Constant(_detailedErrorsEnabled),
                    Constant(_threadSafetyChecksEnabled));
            }

            return Call(
                typeof(GroupBySingleQueryingEnumerable).GetMethods()
                    .Single(m => m.Name == nameof(GroupBySingleQueryingEnumerable.Create))
                    .MakeGenericMethod(keySelector.ReturnType, elementSelector.ReturnType),
                Convert(QueryCompilationContext.QueryContextParameter, typeof(RelationalQueryContext)),
                relationalCommandCache,
                readerColumnsExpression,
                keySelector,
                keyIdentifier,
                Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                    relationalGroupByResultExpression.KeyIdentifierValueComparers.Select(x => (Func<object, object, bool>)x.Equals).ToArray(),
                    Lambda<Func<MaterializerLiftableConstantContext, object>>(
                        NewArrayInit(
                            typeof(Func<object, object, bool>),
                            relationalGroupByResultExpression.KeyIdentifierValueComparers.Select(vc => vc.ObjectEqualsExpression)),
                        Parameter(typeof(MaterializerLiftableConstantContext), "_")),
                    "keyIdentifierValueComparers",
                    typeof(Func<object, object, bool>[])),
                elementSelector,
                Constant(_contextType),
                Constant(QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution),
                Constant(_detailedErrorsEnabled),
                Constant(_threadSafetyChecksEnabled));
        }
        else
        {
            var nonComposedFromSql = selectExpression.IsNonComposedFromSql();
            var shaper = new ShaperProcessingExpressionVisitor(this, selectExpression, _tags, splitQuery, nonComposedFromSql).ProcessShaper(
                shapedQueryExpression.ShaperExpression, out var relationalCommandCache, out var readerColumns,
                out var relatedDataLoaders, ref collectionCount);

            if (querySplittingBehavior == null
                && collectionCount > 1)
            {
                QueryCompilationContext.Logger.MultipleCollectionIncludeWarning();
            }

            var readerColumnsExpression = CreateReaderColumnsExpression(readerColumns, Dependencies.LiftableConstantFactory);
            if (nonComposedFromSql)
            {
                return Call(
                    typeof(FromSqlQueryingEnumerable).GetMethods()
                        .Single(m => m.Name == nameof(FromSqlQueryingEnumerable.Create))
                        .MakeGenericMethod(shaper.ReturnType),
                    Convert(QueryCompilationContext.QueryContextParameter, typeof(RelationalQueryContext)),
                    relationalCommandCache,
                    readerColumnsExpression,
                    Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                        selectExpression.Projection.Select(pe => ((ColumnExpression)pe.Expression).Name).ToArray(),
                        Lambda<Func<MaterializerLiftableConstantContext, object>>(
                            NewArrayInit(
                                typeof(string),
                                selectExpression.Projection.Select(pe => Constant(((ColumnExpression)pe.Expression).Name, typeof(string)))),
                            Parameter(typeof(MaterializerLiftableConstantContext), "_")),
                        "columnNames",
                        typeof(string[])),
                    shaper,
                    Constant(_contextType),
                    Constant(QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution),
                    Constant(_detailedErrorsEnabled),
                    Constant(_threadSafetyChecksEnabled));
            }

            if (splitQuery)
            {
                var relatedDataLoadersParameter =
                    QueryCompilationContext.IsAsync || relatedDataLoaders is null
                        ? Constant(null, typeof(Action<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator>))
                        : (Expression)relatedDataLoaders;

                var relatedDataLoadersAsyncParameter =
                    QueryCompilationContext.IsAsync && relatedDataLoaders is not null
                        ? (Expression)relatedDataLoaders
                        : Constant(null, typeof(Func<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator, Task>));

                return Call(
                    typeof(SplitQueryingEnumerable).GetMethods()
                        .Single(m => m.Name == nameof(FromSqlQueryingEnumerable.Create))
                        .MakeGenericMethod(shaper.ReturnType),
                    Convert(QueryCompilationContext.QueryContextParameter, typeof(RelationalQueryContext)),
                    relationalCommandCache,
                    readerColumnsExpression,
                    shaper,
                    relatedDataLoadersParameter,
                    relatedDataLoadersAsyncParameter,
                    Constant(_contextType),
                    Constant(QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution),
                    Constant(_detailedErrorsEnabled),
                    Constant(_threadSafetyChecksEnabled));
            }

            return Call(
                typeof(SingleQueryingEnumerable).GetMethods()
                    .Single(m => m.Name == nameof(SingleQueryingEnumerable.Create))
                    .MakeGenericMethod(shaper.ReturnType),
                Convert(QueryCompilationContext.QueryContextParameter, typeof(RelationalQueryContext)),
                relationalCommandCache,
                readerColumnsExpression,
                shaper,
                Constant(_contextType),
                Constant(QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution),
                Constant(_detailedErrorsEnabled),
                Constant(_threadSafetyChecksEnabled));
        }
    }

    private static Expression CreateReaderColumnsExpression(
        IReadOnlyList<ReaderColumn?>? readerColumns,
        ILiftableConstantFactory liftableConstantFactory)
    {
        if (readerColumns is null)
        {
            return Constant(readerColumns, typeof(ReaderColumn?[]));
        }

        var materializerLiftableConstantContextParameter = Parameter(typeof(MaterializerLiftableConstantContext));
        var initializers = new List<Expression>();

        foreach (var readerColumn in readerColumns)
        {
            var currentReaderColumn = readerColumn;
            if (currentReaderColumn is null)
            {
                initializers.Add(Constant(null, typeof(ReaderColumn)));
                continue;
            }

            var propertyExpression = LiftableConstantExpressionHelpers.BuildMemberAccessForProperty(
                currentReaderColumn.Property,
                materializerLiftableConstantContextParameter);

            initializers.Add(
                New(
                    ReaderColumn.GetConstructor(currentReaderColumn.Type),
                    Constant(currentReaderColumn.IsNullable),
                    Constant(currentReaderColumn.Name, typeof(string)),
                    propertyExpression,
                    currentReaderColumn.GetFieldValueExpression));
        }

        var result = liftableConstantFactory.CreateLiftableConstant(
            readerColumns,
            Lambda<Func<MaterializerLiftableConstantContext, object>>(
                NewArrayInit(
                    typeof(ReaderColumn),
                    initializers),
                materializerLiftableConstantContextParameter),
            "readerColumns",
            typeof(ReaderColumn[]));

        return result;
    }

    private Expression CreateRelationalCommandCacheExpression(Expression queryExpression)
    {
        var relationalCommandCache = new RelationalCommandCache(
            Dependencies.MemoryCache,
            RelationalDependencies.QuerySqlGeneratorFactory,
            RelationalDependencies.RelationalParameterBasedSqlProcessorFactory,
            queryExpression,
            _useRelationalNulls);

        return RelationalDependencies.RelationalLiftableConstantFactory.CreateLiftableConstant(
            relationalCommandCache,
            c => new RelationalCommandCache(
                c.Dependencies.MemoryCache,
                c.RelationalDependencies.QuerySqlGeneratorFactory,
                c.RelationalDependencies.RelationalParameterBasedSqlProcessorFactory,
                queryExpression,
                _useRelationalNulls),
            "relationalCommandCache",
            typeof(RelationalCommandCache));
    }
}
