// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.Common;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

#pragma warning disable EF1001 // Internal EF Core API usage

public partial class RelationalMaterializerFactory
{
    /// <summary>
    ///     Builds a non-generated query executor for a split enumerable query.
    ///     The main query returns one row per entity; collection includes are loaded via separate SQL queries.
    /// </summary>
    private Func<QueryContext, IEnumerable<TElement>> CreateSplitQueryEnumerableMaterializer<TElement>(
        RelationalQueryCompilationContext queryCompilationContext,
        SelectExpression select,
        Expression shaper)
    {
        var isTracking = queryCompilationContext.QueryTrackingBehavior is QueryTrackingBehavior.TrackAll;
        var relationalCommandCache = CreateCommandCache(queryCompilationContext, select);

        var splitShaper = BuildSplitQueryShaper<TElement>(
            queryCompilationContext,
            select,
            shaper,
            isTracking,
            out var relatedDataLoaders,
            out var relatedDataLoadersAsync);

        return qc => new SplitQueryingEnumerable<TElement>(
            (RelationalQueryContext)qc,
            relationalCommandResolver: parameters => relationalCommandCache.GetRelationalCommandTemplate(parameters),
            readerColumns: null,
            materializer: splitShaper!,
            relatedDataLoaders: relatedDataLoaders,
            relatedDataLoadersAsync: relatedDataLoadersAsync,
            contextType: queryCompilationContext.ContextType,
            standAloneStateManager: queryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution,
            detailedErrorsEnabled: _detailedErrorsEnabled,
            threadSafetyChecksEnabled: _threadSafetyChecksEnabled);
    }

    private Func<QueryContext, DbDataReader, ResultContext, SplitQueryResultCoordinator, TElement?> BuildSplitQueryShaper<TElement>(
        RelationalQueryCompilationContext queryCompilationContext,
        SelectExpression select,
        Expression shaper,
        bool isTracking,
        out Action<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator>? relatedDataLoaders,
        out Func<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator, Task>? relatedDataLoadersAsync)
    {
        if (shaper is UnaryExpression { NodeType: ExpressionType.Convert } convert)
        {
            shaper = convert.Operand;
        }

        // Build the entity materializer. For split queries, the entity materializer handles the
        // main query result (entity + reference includes) but NOT collection includes — those are
        // handled by separate relatedDataLoaders.
        var splitCollectionInfos = new List<SplitCollectionIncludeInfo>();
        var nextCollectionId = 0;
        var entityMaterializer = BuildSplitEntityMaterializer(
            shaper, select, isTracking, queryCompilationContext.QueryTrackingBehavior,
            splitCollectionInfos, ref nextCollectionId);

        // Build the shaper delegate that wraps the entity materializer.
        // When there are collections (relatedDataLoaders non-null), the shaper is called twice per row:
        // first call materializes + caches + returns default, second call returns cached entity.
        // When there are no collections, the shaper is called once and returns the entity directly.
        Func<QueryContext, DbDataReader, ResultContext, SplitQueryResultCoordinator, TElement?> splitShaper;

        if (splitCollectionInfos.Count > 0)
        {
            splitShaper = (queryCtx, reader, rc, coord) =>
            {
                if (rc.Values is null)
                {
                    var entity = entityMaterializer(queryCtx, reader);
                    rc.Values = [entity!];

                    for (var i = 0; i < splitCollectionInfos.Count; i++)
                    {
                        var ci = splitCollectionInfos[i];
                        var parentEntity = ci.ParentEntityProvider is not null
                            ? ci.ParentEntityProvider()
                            : entity;

                        InitializeSplitIncludeCollection(
                            queryCtx, reader, coord, isTracking, parentEntity, ci);
                    }

                    return default!;
                }

                return (TElement)rc.Values[0];
            };
        }
        else
        {
            splitShaper = (queryCtx, reader, rc, coord) => (TElement?)entityMaterializer(queryCtx, reader);
        }

        // Build relatedDataLoaders: for each split collection, execute a separate query and populate.
        relatedDataLoaders = null;
        relatedDataLoadersAsync = null;

        if (splitCollectionInfos.Count > 0)
        {
            BuildSplitCommandCaches(splitCollectionInfos, queryCompilationContext);

            relatedDataLoaders = (queryCtx, executionStrategy, coord) =>
            {
                for (var i = 0; i < splitCollectionInfos.Count; i++)
                {
                    PopulateSplitIncludeCollection(
                        queryCtx, executionStrategy, coord, isTracking, splitCollectionInfos[i]);
                }
            };

            relatedDataLoadersAsync = async (queryCtx, executionStrategy, coord) =>
            {
                for (var i = 0; i < splitCollectionInfos.Count; i++)
                {
                    await PopulateSplitIncludeCollectionAsync(
                        queryCtx, executionStrategy, coord, isTracking, splitCollectionInfos[i])
                        .ConfigureAwait(false);
                }
            };
        }

        return splitShaper;
    }

    /// <summary>
    ///     Builds a materializer delegate for the main query of a split query. Returns a simple
    ///     <c>Func&lt;QueryContext, DbDataReader, TElement&gt;</c> that materializes the entity
    ///     and reference includes from a single row. Collection includes are collected into
    ///     <paramref name="splitCollectionInfos" /> for separate loading.
    /// </summary>
    private Func<QueryContext, DbDataReader, object?> BuildSplitEntityMaterializer(
        Expression shaperExpression,
        SelectExpression select,
        bool isTracking,
        QueryTrackingBehavior? queryTrackingBehavior,
        List<SplitCollectionIncludeInfo> splitCollectionInfos,
        ref int nextCollectionId)
    {
        if (shaperExpression is UnaryExpression { NodeType: ExpressionType.Convert } convert)
        {
            shaperExpression = convert.Operand;
        }

        // Entity/include path (handles split collections via splitCollectionInfos).
        if (shaperExpression is RelationalStructuralTypeShaperExpression or IncludeExpression)
        {
            var entityMaterializer = BuildMaterializerFromShaper(
                shaperExpression, select, isTracking, queryTrackingBehavior, splitCollectionInfos, ref nextCollectionId);

            var resultContext = new ResultContext();
            var dummyCoordinator = new SingleQueryResultCoordinator();

            return (queryCtx, reader) =>
            {
                resultContext.Values = null;
                return entityMaterializer.Materialize(queryCtx, reader, resultContext, dummyCoordinator);
            };
        }

        // For NewExpression, build sub-materializers that each handle their own split collections.
        // Track the number of split collections before each sub-materializer to set ParentEntityProvider.
        if (shaperExpression is NewExpression newExpression)
        {
            var invoker = ConstructorInvoker.Create(newExpression.Constructor!);
            var argMaterializers = new Func<QueryContext, DbDataReader, object?>[newExpression.Arguments.Count];
            var argSplitStartIndices = new int[newExpression.Arguments.Count];

            for (var i = 0; i < newExpression.Arguments.Count; i++)
            {
                argSplitStartIndices[i] = splitCollectionInfos.Count;
                argMaterializers[i] = BuildSplitEntityMaterializer(
                    newExpression.Arguments[i], select, isTracking, queryTrackingBehavior,
                    splitCollectionInfos, ref nextCollectionId);
            }

            // Set ParentEntityProvider on each split collection to read from its arg materializer's result.
            var argResults = new object?[newExpression.Arguments.Count];

            for (var i = 0; i < newExpression.Arguments.Count; i++)
            {
                var argIndex = i;
                var startIndex = argSplitStartIndices[i];
                var endIndex = i + 1 < newExpression.Arguments.Count ? argSplitStartIndices[i + 1] : splitCollectionInfos.Count;

                for (var j = startIndex; j < endIndex; j++)
                {
                    splitCollectionInfos[j].ParentEntityProvider = () => argResults[argIndex];
                }
            }

            var invokerArgs = new object?[newExpression.Arguments.Count];

            return (queryCtx, reader) =>
            {
                for (var i = 0; i < argMaterializers.Length; i++)
                {
                    argResults[i] = argMaterializers[i](queryCtx, reader);
                    invokerArgs[i] = argResults[i];
                }

                return invoker.Invoke(invokerArgs.AsSpan());
            };
        }

        // For other shaper types (scalars, method calls, etc.)
        var materializer = BuildMaterializer<object>(
            shaperExpression, select, isTracking, queryTrackingBehavior, ref nextCollectionId, splitCollectionInfos);
        var rc = new ResultContext();
        var coord = new SingleQueryResultCoordinator();

        return (queryCtx, reader) =>
        {
            rc.Values = null;
            coord.ResultReady = true;
            return materializer(queryCtx, reader, rc, coord);
        };
    }

    #region Split query collection population

    private static void InitializeSplitIncludeCollection(
        QueryContext queryContext,
        DbDataReader dataReader,
        SplitQueryResultCoordinator resultCoordinator,
        bool isTracking,
        object? parentEntity,
        SplitCollectionIncludeInfo ci)
    {
        object? collection = null;

        if (ci.Navigation is not null
            && parentEntity is not null
            && ci.Navigation.DeclaringEntityType.ClrType.IsInstanceOfType(parentEntity))
        {
            // Include-based: set up navigation loading and get/create the collection on the entity
            if (ci.SetLoaded && isTracking)
            {
                queryContext.SetNavigationIsLoaded(parentEntity, ci.Navigation);
            }
            else if (ci.SetLoaded)
            {
                ci.Navigation.SetIsLoadedWhenNoTracking(parentEntity);
            }

            collection = ci.CollectionAccessor!.GetOrCreate(parentEntity, forMaterialization: true);
        }
        else if (ci.Navigation is null)
        {
            // Standalone split collection: parentEntity IS the collection itself
            collection = parentEntity;
        }

        var parentKey = ci.ParentIdentifier(queryContext, dataReader);

        resultCoordinator.SetSplitQueryCollectionContext(
            ci.CollectionId,
            new SplitQueryCollectionContext(parentEntity, collection, parentKey));
    }

    private static void PopulateSplitIncludeCollection(
        QueryContext queryContext,
        IExecutionStrategy executionStrategy,
        SplitQueryResultCoordinator resultCoordinator,
        bool isTracking,
        SplitCollectionIncludeInfo ci)
    {
        if (resultCoordinator.DataReaders.Count <= ci.CollectionId
            || resultCoordinator.DataReaders[ci.CollectionId] == null)
        {
            var relationalQueryContext = (RelationalQueryContext)queryContext;
            RelationalCommandResolver commandResolver = ci.CommandCache!.GetRelationalCommandTemplate;

            var dataReader = executionStrategy.Execute(
                (relationalQueryContext, commandResolver),
                static (_, state) =>
                {
                    var relationalCommand = state.commandResolver.RentAndPopulateRelationalCommand(state.relationalQueryContext);

                    return relationalCommand.ExecuteReader(
                        new RelationalCommandParameterObject(
                            state.relationalQueryContext.Connection,
                            state.relationalQueryContext.Parameters,
                            null,
                            state.relationalQueryContext.Context,
                            state.relationalQueryContext.CommandLogger,
                            CommandSource.LinqQuery));
                },
                verifySucceeded: null);

            resultCoordinator.SetDataReader(ci.CollectionId, dataReader);
        }

        var collectionContext = resultCoordinator.Collections[ci.CollectionId]!;
        var dataReaderContext = resultCoordinator.DataReaders[ci.CollectionId]!;
        var dbDataReader = dataReaderContext.DataReader.DbDataReader;
        var parent = collectionContext.Parent;

        // Standalone split collection (non-include): always populate.
        // Include-based split collection: only populate if parent entity matches.
        var shouldPopulate = ci.Navigation is null
            || (parent is not null && ci.Navigation.DeclaringEntityType.ClrType.IsInstanceOfType(parent));

        if (shouldPopulate)
        {
            var childResultContext = collectionContext.ResultContext;

            while (dataReaderContext.HasBufferedNextRow || dataReaderContext.HasNext is null && dbDataReader.Read())
            {
                if (!CompareIdentifiers(
                        ci.IdentifierValueComparers,
                        collectionContext.ParentIdentifier,
                        ci.ChildIdentifier(queryContext, dbDataReader)))
                {
                    dataReaderContext.MarkRowForNextResult();
                    return;
                }

                dataReaderContext.MarkCurrentRowConsumed();
                childResultContext.Values = null;

                // Materialize child element (first call inits + caches)
                var dummyCoord = new SingleQueryResultCoordinator();
                ci.ElementMaterializer(queryContext, dbDataReader, childResultContext, dummyCoord);

                // Initialize + load nested split collections for the child
                for (var i = 0; i < ci.ChildSplitCollections.Count; i++)
                {
                    var childCi = ci.ChildSplitCollections[i];
                    var childEntity = childResultContext.Values?[0];

                    InitializeSplitIncludeCollection(
                        queryContext, dbDataReader, resultCoordinator, isTracking, childEntity, childCi);

                    PopulateSplitIncludeCollection(
                        queryContext, executionStrategy, resultCoordinator, isTracking, childCi);
                }

                // Second call returns the materialized child
                var relatedEntity = ci.ElementMaterializer(
                    queryContext, dbDataReader, childResultContext, dummyCoord);

                if (relatedEntity is not null)
                {
                    if (ci.Navigation is not null)
                    {
                        // Always add to the collection — for tracking queries, the change tracker
                        // also performs fixup, but explicit addition ensures the collection is correct
                        // even in complex reference-include chains where fixup may not fire for
                        // already-tracked entities.
                        ci.CollectionAccessor!.Add(parent!, relatedEntity, forMaterialization: isTracking ? false : true);
                        if (!isTracking)
                        {
                            switch (ci.InverseNavigation)
                            {
                                case { IsCollection: true }:
                                    ci.InverseNavigationCollectionAccessor?.Add(relatedEntity, parent!, forMaterialization: true);
                                    break;

                                case { IsCollection: false }:
                                    ci.InverseNavigationSetter?.SetClrValue(relatedEntity, parent);
                                    ci.InverseNavigation.SetIsLoadedWhenNoTracking(relatedEntity);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        ci.CollectionAdd!(collectionContext.Collection!, relatedEntity);
                    }
                }
            }

            dataReaderContext.MarkReaderExhausted();
        }
    }

    private static async Task PopulateSplitIncludeCollectionAsync(
        QueryContext queryContext,
        IExecutionStrategy executionStrategy,
        SplitQueryResultCoordinator resultCoordinator,
        bool isTracking,
        SplitCollectionIncludeInfo ci)
    {
        if (resultCoordinator.DataReaders.Count <= ci.CollectionId
            || resultCoordinator.DataReaders[ci.CollectionId] == null)
        {
            var relationalQueryContext = (RelationalQueryContext)queryContext;
            RelationalCommandResolver commandResolver = ci.CommandCache!.GetRelationalCommandTemplate;

            var dataReader = await executionStrategy.ExecuteAsync(
                (relationalQueryContext, commandResolver),
                static (_, state, ct) =>
                {
                    var relationalCommand = state.commandResolver.RentAndPopulateRelationalCommand(state.relationalQueryContext);

                    return relationalCommand.ExecuteReaderAsync(
                        new RelationalCommandParameterObject(
                            state.relationalQueryContext.Connection,
                            state.relationalQueryContext.Parameters,
                            null,
                            state.relationalQueryContext.Context,
                            state.relationalQueryContext.CommandLogger,
                            CommandSource.LinqQuery),
                        ct);
                },
                verifySucceeded: null,
                cancellationToken: relationalQueryContext.CancellationToken)
                .ConfigureAwait(false);

            resultCoordinator.SetDataReader(ci.CollectionId, dataReader);
        }

        var collectionContext = resultCoordinator.Collections[ci.CollectionId]!;
        var dataReaderContext = resultCoordinator.DataReaders[ci.CollectionId]!;
        var dbDataReader = dataReaderContext.DataReader.DbDataReader;
        var parent = collectionContext.Parent;

        var shouldPopulate = ci.Navigation is null
            || (parent is not null && ci.Navigation.DeclaringEntityType.ClrType.IsInstanceOfType(parent));

        if (shouldPopulate)
        {
            var childResultContext = collectionContext.ResultContext;

            while (dataReaderContext.HasBufferedNextRow || dataReaderContext.HasNext is null && await dbDataReader.ReadAsync(
                       ((RelationalQueryContext)queryContext).CancellationToken).ConfigureAwait(false))
            {
                if (!CompareIdentifiers(
                        ci.IdentifierValueComparers,
                        collectionContext.ParentIdentifier,
                        ci.ChildIdentifier(queryContext, dbDataReader)))
                {
                    dataReaderContext.MarkRowForNextResult();
                    return;
                }

                dataReaderContext.MarkCurrentRowConsumed();
                childResultContext.Values = null;

                var dummyCoord = new SingleQueryResultCoordinator();
                ci.ElementMaterializer(queryContext, dbDataReader, childResultContext, dummyCoord);

                for (var i = 0; i < ci.ChildSplitCollections.Count; i++)
                {
                    var childCi = ci.ChildSplitCollections[i];
                    var childEntity = childResultContext.Values?[0];

                    InitializeSplitIncludeCollection(
                        queryContext, dbDataReader, resultCoordinator, isTracking, childEntity, childCi);

                    await PopulateSplitIncludeCollectionAsync(
                        queryContext, executionStrategy, resultCoordinator, isTracking, childCi)
                        .ConfigureAwait(false);
                }

                var relatedEntity = ci.ElementMaterializer(
                    queryContext, dbDataReader, childResultContext, dummyCoord);

                if (relatedEntity is not null)
                {
                    if (ci.Navigation is not null)
                    {
                        ci.CollectionAccessor!.Add(parent!, relatedEntity, forMaterialization: isTracking ? false : true);
                        if (!isTracking)
                        {
                            switch (ci.InverseNavigation)
                            {
                                case { IsCollection: true }:
                                    ci.InverseNavigationCollectionAccessor?.Add(relatedEntity, parent!, forMaterialization: true);
                                    break;

                                case { IsCollection: false }:
                                    ci.InverseNavigationSetter?.SetClrValue(relatedEntity, parent);
                                    ci.InverseNavigation.SetIsLoadedWhenNoTracking(relatedEntity);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        ci.CollectionAdd!(collectionContext.Collection!, relatedEntity);
                    }
                }
            }

            dataReaderContext.MarkReaderExhausted();
        }
    }

    /// <summary>
    ///     Builds <see cref="RelationalCommandCache" /> instances for each split collection and assigns them.
    /// </summary>
    private void BuildSplitCommandCaches(
        List<SplitCollectionIncludeInfo> splitCollections,
        RelationalQueryCompilationContext queryCompilationContext)
    {
        for (var i = 0; i < splitCollections.Count; i++)
        {
            var ci = splitCollections[i];
            ci.CommandCache = CreateCommandCache(queryCompilationContext, ci.SelectExpression);

            if (ci.ChildSplitCollections.Count > 0)
            {
                BuildSplitCommandCaches(ci.ChildSplitCollections, queryCompilationContext);
            }
        }
    }

    #endregion Split query collection population
}
