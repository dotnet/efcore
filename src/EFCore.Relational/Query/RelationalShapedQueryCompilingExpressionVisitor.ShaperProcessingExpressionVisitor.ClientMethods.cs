// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Query;

public partial class RelationalShapedQueryCompilingExpressionVisitor
{
    private sealed partial class ShaperProcessingExpressionVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo ThrowReadValueExceptionMethod =
            typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(ThrowReadValueException))!;

        private static readonly MethodInfo ThrowExtractJsonPropertyExceptionMethod =
            typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(ThrowExtractJsonPropertyException))!;

        // Performing collection materialization
        private static readonly MethodInfo IncludeReferenceMethodInfo
            = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(IncludeReference))!;

        private static readonly MethodInfo InitializeIncludeCollectionMethodInfo
            = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(InitializeIncludeCollection))!;

        private static readonly MethodInfo PopulateIncludeCollectionMethodInfo
            = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(PopulateIncludeCollection))!;

        private static readonly MethodInfo InitializeSplitIncludeCollectionMethodInfo
            = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(InitializeSplitIncludeCollection))!;

        private static readonly MethodInfo PopulateSplitIncludeCollectionMethodInfo
            = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(PopulateSplitIncludeCollection))!;

        private static readonly MethodInfo PopulateSplitIncludeCollectionAsyncMethodInfo
            = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(PopulateSplitIncludeCollectionAsync))!;

        private static readonly MethodInfo InitializeCollectionMethodInfo
            = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(InitializeCollection))!;

        private static readonly MethodInfo PopulateCollectionMethodInfo
            = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(PopulateCollection))!;

        private static readonly MethodInfo InitializeSplitCollectionMethodInfo
            = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(InitializeSplitCollection))!;

        private static readonly MethodInfo PopulateSplitCollectionMethodInfo
            = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(PopulateSplitCollection))!;

        private static readonly MethodInfo PopulateSplitCollectionAsyncMethodInfo
            = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(PopulateSplitCollectionAsync))!;

        private static readonly MethodInfo TaskAwaiterMethodInfo
            = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(TaskAwaiter))!;

        private static readonly MethodInfo IncludeJsonEntityReferenceMethodInfo
            = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(IncludeJsonEntityReference))!;

        private static readonly MethodInfo IncludeJsonEntityCollectionMethodInfo
            = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(IncludeJsonEntityCollection))!;

        private static readonly MethodInfo MaterializeJsonEntityMethodInfo
            = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(MaterializeJsonEntity))!;

        private static readonly MethodInfo MaterializeJsonEntityCollectionMethodInfo
            = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(MaterializeJsonEntityCollection))!;

        private static readonly MethodInfo InverseCollectionFixupMethod
            = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(InverseCollectionFixup))!;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TValue ThrowReadValueException<TValue>(
            Exception exception,
            object? value,
            Type expectedType,
            IPropertyBase? property = null)
        {
            var actualType = value?.GetType();

            string message;

            if (property != null)
            {
                var entityType = property.DeclaringType.DisplayName();
                var propertyName = property.Name;
                if (expectedType == typeof(object))
                {
                    expectedType = property.ClrType;
                }

                message = exception is NullReferenceException
                    || Equals(value, DBNull.Value)
                        ? RelationalStrings.ErrorMaterializingPropertyNullReference(entityType, propertyName, expectedType)
                        : exception is InvalidCastException
                            ? CoreStrings.ErrorMaterializingPropertyInvalidCast(entityType, propertyName, expectedType, actualType)
                            : RelationalStrings.ErrorMaterializingProperty(entityType, propertyName);
            }
            else
            {
                message = exception is NullReferenceException
                    || Equals(value, DBNull.Value)
                        ? RelationalStrings.ErrorMaterializingValueNullReference(expectedType)
                        : exception is InvalidCastException
                            ? RelationalStrings.ErrorMaterializingValueInvalidCast(expectedType, actualType)
                            : RelationalStrings.ErrorMaterializingValue;
            }

            throw new InvalidOperationException(message, exception);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TValue ThrowExtractJsonPropertyException<TValue>(Exception exception, IProperty property)
        {
            var entityType = property.DeclaringType.DisplayName();
            var propertyName = property.Name;

            throw new InvalidOperationException(
                RelationalStrings.JsonErrorExtractingJsonProperty(entityType, propertyName),
                exception);
        }

        private static void IncludeReference<TEntity, TIncludingEntity, TIncludedEntity>(
            QueryContext queryContext,
            TEntity entity,
            TIncludedEntity? relatedEntity,
            INavigationBase navigation,
            INavigationBase? inverseNavigation,
            Action<TIncludingEntity, TIncludedEntity> fixup,
            bool trackingQuery)
            where TEntity : class
            where TIncludingEntity : class, TEntity
            where TIncludedEntity : class
        {
            if (entity is TIncludingEntity includingEntity)
            {
                if (trackingQuery
                    && navigation.DeclaringEntityType.FindPrimaryKey() != null)
                {
                    // For non-null relatedEntity StateManager will set the flag
                    if (relatedEntity == null)
                    {
                        queryContext.SetNavigationIsLoaded(includingEntity, navigation);
                    }
                }
                else
                {
                    navigation.SetIsLoadedWhenNoTracking(includingEntity);
                    if (relatedEntity != null)
                    {
                        fixup(includingEntity, relatedEntity);
                        if (inverseNavigation is { IsCollection: false })
                        {
                            inverseNavigation.SetIsLoadedWhenNoTracking(relatedEntity);
                        }
                    }
                }
            }
        }

        private static void InitializeIncludeCollection<TParent, TNavigationEntity>(
            int collectionId,
            QueryContext queryContext,
            DbDataReader dbDataReader,
            SingleQueryResultCoordinator resultCoordinator,
            TParent entity,
            Func<QueryContext, DbDataReader, object[]> parentIdentifier,
            Func<QueryContext, DbDataReader, object[]> outerIdentifier,
            INavigationBase navigation,
            IClrCollectionAccessor? clrCollectionAccessor,
            bool trackingQuery,
            bool setLoaded)
            where TParent : class
            where TNavigationEntity : class, TParent
        {
            object? collection = null;
            if (entity is TNavigationEntity)
            {
                if (setLoaded)
                {
                    if (trackingQuery)
                    {
                        queryContext.SetNavigationIsLoaded(entity, navigation);
                    }
                    else
                    {
                        navigation.SetIsLoadedWhenNoTracking(entity);
                    }
                }

                collection = clrCollectionAccessor?.GetOrCreate(entity, forMaterialization: true);
            }

            var parentKey = parentIdentifier(queryContext, dbDataReader);
            var outerKey = outerIdentifier(queryContext, dbDataReader);

            var collectionMaterializationContext = new SingleQueryCollectionContext(entity, collection, parentKey, outerKey);

            resultCoordinator.SetSingleQueryCollectionContext(collectionId, collectionMaterializationContext);
        }

        private static void PopulateIncludeCollection<TIncludingEntity, TIncludedEntity>(
            int collectionId,
            QueryContext queryContext,
            DbDataReader dbDataReader,
            SingleQueryResultCoordinator resultCoordinator,
            Func<QueryContext, DbDataReader, object[]> parentIdentifier,
            Func<QueryContext, DbDataReader, object[]> outerIdentifier,
            Func<QueryContext, DbDataReader, object[]> selfIdentifier,
            IReadOnlyList<ValueComparer> parentIdentifierValueComparers,
            IReadOnlyList<ValueComparer> outerIdentifierValueComparers,
            IReadOnlyList<ValueComparer> selfIdentifierValueComparers,
            Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, TIncludedEntity> innerShaper,
            INavigationBase? inverseNavigation,
            Action<TIncludingEntity, TIncludedEntity> fixup,
            bool trackingQuery)
            where TIncludingEntity : class
            where TIncludedEntity : class
        {
            var collectionMaterializationContext = resultCoordinator.Collections[collectionId]!;
            if (collectionMaterializationContext.Parent is TIncludingEntity entity)
            {
                if (resultCoordinator.HasNext == false)
                {
                    // Outer Enumerator has ended
                    GenerateCurrentElementIfPending();
                    return;
                }

                if (!CompareIdentifiers(
                        outerIdentifierValueComparers,
                        outerIdentifier(queryContext, dbDataReader), collectionMaterializationContext.OuterIdentifier))
                {
                    // Outer changed so collection has ended. Materialize last element.
                    GenerateCurrentElementIfPending();
                    // If parent also changed then this row is now pointing to element of next collection
                    if (!CompareIdentifiers(
                            parentIdentifierValueComparers,
                            parentIdentifier(queryContext, dbDataReader), collectionMaterializationContext.ParentIdentifier))
                    {
                        resultCoordinator.HasNext = true;
                    }

                    return;
                }

                var innerKey = selfIdentifier(queryContext, dbDataReader);
                if (innerKey.All(e => e == null))
                {
                    // No correlated element
                    return;
                }

                if (collectionMaterializationContext.SelfIdentifier != null)
                {
                    if (CompareIdentifiers(selfIdentifierValueComparers, innerKey, collectionMaterializationContext.SelfIdentifier))
                    {
                        // repeated row for current element
                        // If it is pending materialization then it may have nested elements
                        if (collectionMaterializationContext.ResultContext.Values != null)
                        {
                            ProcessCurrentElementRow();
                        }

                        resultCoordinator.ResultReady = false;
                        return;
                    }

                    // Row for new element which is not first element
                    // So materialize the element
                    GenerateCurrentElementIfPending();
                    resultCoordinator.HasNext = null;
                    collectionMaterializationContext.UpdateSelfIdentifier(innerKey);
                }
                else
                {
                    // First row for current element
                    collectionMaterializationContext.UpdateSelfIdentifier(innerKey);
                }

                ProcessCurrentElementRow();
                resultCoordinator.ResultReady = false;
            }

            void ProcessCurrentElementRow()
            {
                var previousResultReady = resultCoordinator.ResultReady;
                resultCoordinator.ResultReady = true;
                var relatedEntity = innerShaper(
                    queryContext, dbDataReader, collectionMaterializationContext.ResultContext, resultCoordinator);
                if (resultCoordinator.ResultReady)
                {
                    // related entity is materialized
                    collectionMaterializationContext.ResultContext.Values = null;
                    if (!trackingQuery)
                    {
                        fixup(entity, relatedEntity);
                        if (inverseNavigation is { IsCollection: false })
                        {
                            inverseNavigation.SetIsLoadedWhenNoTracking(relatedEntity);
                        }
                    }
                }

                resultCoordinator.ResultReady &= previousResultReady;
            }

            void GenerateCurrentElementIfPending()
            {
                if (collectionMaterializationContext.ResultContext.Values != null)
                {
                    resultCoordinator.HasNext = false;
                    ProcessCurrentElementRow();
                }

                collectionMaterializationContext.UpdateSelfIdentifier(null);
            }
        }

        private static void InitializeSplitIncludeCollection<TParent, TNavigationEntity>(
            int collectionId,
            QueryContext queryContext,
            DbDataReader parentDataReader,
            SplitQueryResultCoordinator resultCoordinator,
            TParent entity,
            Func<QueryContext, DbDataReader, object[]> parentIdentifier,
            INavigationBase navigation,
            IClrCollectionAccessor? clrCollectionAccessor,
            bool trackingQuery,
            bool setLoaded)
            where TParent : class
            where TNavigationEntity : class, TParent
        {
            object? collection = null;
            if (entity is TNavigationEntity)
            {
                if (setLoaded)
                {
                    if (trackingQuery)
                    {
                        queryContext.SetNavigationIsLoaded(entity, navigation);
                    }
                    else
                    {
                        navigation.SetIsLoadedWhenNoTracking(entity);
                    }
                }

                collection = clrCollectionAccessor?.GetOrCreate(entity, forMaterialization: true);
            }

            var parentKey = parentIdentifier(queryContext, parentDataReader);

            var splitQueryCollectionContext = new SplitQueryCollectionContext(entity, collection, parentKey);

            resultCoordinator.SetSplitQueryCollectionContext(collectionId, splitQueryCollectionContext);
        }

        private static void PopulateSplitIncludeCollection<TIncludingEntity, TIncludedEntity>(
            int collectionId,
            RelationalQueryContext queryContext,
            IExecutionStrategy executionStrategy,
            RelationalCommandCache relationalCommandCache,
            IReadOnlyList<ReaderColumn?>? readerColumns,
            bool detailedErrorsEnabled,
            SplitQueryResultCoordinator resultCoordinator,
            Func<QueryContext, DbDataReader, object[]> childIdentifier,
            IReadOnlyList<ValueComparer> identifierValueComparers,
            Func<QueryContext, DbDataReader, ResultContext, SplitQueryResultCoordinator, TIncludedEntity> innerShaper,
            Action<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator>? relatedDataLoaders,
            INavigationBase? inverseNavigation,
            Action<TIncludingEntity, TIncludedEntity> fixup,
            bool trackingQuery)
            where TIncludingEntity : class
            where TIncludedEntity : class
        {
            if (resultCoordinator.DataReaders.Count <= collectionId
                || resultCoordinator.DataReaders[collectionId] == null)
            {
                // Execute and fetch data reader
                var dataReader = executionStrategy.Execute(
                    (queryContext, relationalCommandCache, readerColumns, detailedErrorsEnabled),
                    ((RelationalQueryContext, RelationalCommandCache, IReadOnlyList<ReaderColumn?>?, bool) tup)
                        => InitializeReader(tup.Item1, tup.Item2, tup.Item3, tup.Item4),
                    verifySucceeded: null);

                static RelationalDataReader InitializeReader(
                    RelationalQueryContext queryContext,
                    RelationalCommandCache relationalCommandCache,
                    IReadOnlyList<ReaderColumn?>? readerColumns,
                    bool detailedErrorsEnabled)
                {
                    var relationalCommand = relationalCommandCache.RentAndPopulateRelationalCommand(queryContext);

                    return relationalCommand.ExecuteReader(
                        new RelationalCommandParameterObject(
                            queryContext.Connection,
                            queryContext.ParameterValues,
                            readerColumns,
                            queryContext.Context,
                            queryContext.CommandLogger,
                            detailedErrorsEnabled, CommandSource.LinqQuery));
                }

                resultCoordinator.SetDataReader(collectionId, dataReader);
            }

            var splitQueryCollectionContext = resultCoordinator.Collections[collectionId]!;
            var dataReaderContext = resultCoordinator.DataReaders[collectionId]!;
            var dbDataReader = dataReaderContext.DataReader.DbDataReader;
            if (splitQueryCollectionContext.Parent is TIncludingEntity entity)
            {
                while (dataReaderContext.HasNext ?? dbDataReader.Read())
                {
                    if (!CompareIdentifiers(
                            identifierValueComparers,
                            splitQueryCollectionContext.ParentIdentifier, childIdentifier(queryContext, dbDataReader)))
                    {
                        dataReaderContext.HasNext = true;

                        return;
                    }

                    dataReaderContext.HasNext = null;
                    splitQueryCollectionContext.ResultContext.Values = null;

                    innerShaper(queryContext, dbDataReader, splitQueryCollectionContext.ResultContext, resultCoordinator);
                    relatedDataLoaders?.Invoke(queryContext, executionStrategy, resultCoordinator);
                    var relatedEntity = innerShaper(
                        queryContext, dbDataReader, splitQueryCollectionContext.ResultContext, resultCoordinator);

                    if (!trackingQuery)
                    {
                        fixup(entity, relatedEntity);
                        inverseNavigation?.SetIsLoadedWhenNoTracking(relatedEntity);
                    }
                }

                dataReaderContext.HasNext = false;
            }
        }

        private static async Task PopulateSplitIncludeCollectionAsync<TIncludingEntity, TIncludedEntity>(
            int collectionId,
            RelationalQueryContext queryContext,
            IExecutionStrategy executionStrategy,
            RelationalCommandCache relationalCommandCache,
            IReadOnlyList<ReaderColumn?>? readerColumns,
            bool detailedErrorsEnabled,
            SplitQueryResultCoordinator resultCoordinator,
            Func<QueryContext, DbDataReader, object[]> childIdentifier,
            IReadOnlyList<ValueComparer> identifierValueComparers,
            Func<QueryContext, DbDataReader, ResultContext, SplitQueryResultCoordinator, TIncludedEntity> innerShaper,
            Func<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator, Task>? relatedDataLoaders,
            INavigationBase? inverseNavigation,
            Action<TIncludingEntity, TIncludedEntity> fixup,
            bool trackingQuery)
            where TIncludingEntity : class
            where TIncludedEntity : class
        {
            if (resultCoordinator.DataReaders.Count <= collectionId
                || resultCoordinator.DataReaders[collectionId] == null)
            {
                // Execute and fetch data reader
                var dataReader = await executionStrategy.ExecuteAsync(
                        (queryContext, relationalCommandCache, readerColumns, detailedErrorsEnabled),
                        (
                                (RelationalQueryContext, RelationalCommandCache, IReadOnlyList<ReaderColumn?>?, bool) tup,
                                CancellationToken cancellationToken)
                            => InitializeReaderAsync(tup.Item1, tup.Item2, tup.Item3, tup.Item4, cancellationToken),
                        verifySucceeded: null,
                        queryContext.CancellationToken)
                    .ConfigureAwait(false);

                static async Task<RelationalDataReader> InitializeReaderAsync(
                    RelationalQueryContext queryContext,
                    RelationalCommandCache relationalCommandCache,
                    IReadOnlyList<ReaderColumn?>? readerColumns,
                    bool detailedErrorsEnabled,
                    CancellationToken cancellationToken)
                {
                    var relationalCommand = relationalCommandCache.RentAndPopulateRelationalCommand(queryContext);

                    return await relationalCommand.ExecuteReaderAsync(
                            new RelationalCommandParameterObject(
                                queryContext.Connection,
                                queryContext.ParameterValues,
                                readerColumns,
                                queryContext.Context,
                                queryContext.CommandLogger,
                                detailedErrorsEnabled,
                                CommandSource.LinqQuery),
                            cancellationToken)
                        .ConfigureAwait(false);
                }

                resultCoordinator.SetDataReader(collectionId, dataReader);
            }

            var splitQueryCollectionContext = resultCoordinator.Collections[collectionId]!;
            var dataReaderContext = resultCoordinator.DataReaders[collectionId]!;
            var dbDataReader = dataReaderContext.DataReader.DbDataReader;
            if (splitQueryCollectionContext.Parent is TIncludingEntity entity)
            {
                while (dataReaderContext.HasNext ?? await dbDataReader.ReadAsync(queryContext.CancellationToken).ConfigureAwait(false))
                {
                    if (!CompareIdentifiers(
                            identifierValueComparers,
                            splitQueryCollectionContext.ParentIdentifier, childIdentifier(queryContext, dbDataReader)))
                    {
                        dataReaderContext.HasNext = true;

                        return;
                    }

                    dataReaderContext.HasNext = null;
                    splitQueryCollectionContext.ResultContext.Values = null;

                    innerShaper(queryContext, dbDataReader, splitQueryCollectionContext.ResultContext, resultCoordinator);
                    if (relatedDataLoaders != null)
                    {
                        await relatedDataLoaders(queryContext, executionStrategy, resultCoordinator).ConfigureAwait(false);
                    }

                    var relatedEntity = innerShaper(
                        queryContext, dbDataReader, splitQueryCollectionContext.ResultContext, resultCoordinator);

                    if (!trackingQuery)
                    {
                        fixup(entity, relatedEntity);
                        inverseNavigation?.SetIsLoadedWhenNoTracking(relatedEntity);
                    }
                }

                dataReaderContext.HasNext = false;
            }
        }

        private static TCollection InitializeCollection<TElement, TCollection>(
            int collectionId,
            QueryContext queryContext,
            DbDataReader dbDataReader,
            SingleQueryResultCoordinator resultCoordinator,
            Func<QueryContext, DbDataReader, object[]> parentIdentifier,
            Func<QueryContext, DbDataReader, object[]> outerIdentifier,
            IClrCollectionAccessor? clrCollectionAccessor)
            where TCollection : class, ICollection<TElement>
        {
            var collection = clrCollectionAccessor?.Create() ?? new List<TElement>();

            var parentKey = parentIdentifier(queryContext, dbDataReader);
            var outerKey = outerIdentifier(queryContext, dbDataReader);

            var collectionMaterializationContext = new SingleQueryCollectionContext(null, collection, parentKey, outerKey);

            resultCoordinator.SetSingleQueryCollectionContext(collectionId, collectionMaterializationContext);

            return (TCollection)collection;
        }

        private static void PopulateCollection<TCollection, TElement, TRelatedEntity>(
            int collectionId,
            QueryContext queryContext,
            DbDataReader dbDataReader,
            SingleQueryResultCoordinator resultCoordinator,
            Func<QueryContext, DbDataReader, object[]> parentIdentifier,
            Func<QueryContext, DbDataReader, object[]> outerIdentifier,
            Func<QueryContext, DbDataReader, object[]> selfIdentifier,
            IReadOnlyList<ValueComparer> parentIdentifierValueComparers,
            IReadOnlyList<ValueComparer> outerIdentifierValueComparers,
            IReadOnlyList<ValueComparer> selfIdentifierValueComparers,
            Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, TRelatedEntity> innerShaper)
            where TRelatedEntity : TElement
            where TCollection : class, ICollection<TElement>
        {
            var collectionMaterializationContext = resultCoordinator.Collections[collectionId]!;
            if (collectionMaterializationContext.Collection is null)
            {
                // nothing to materialize since no collection created
                return;
            }

            if (resultCoordinator.HasNext == false)
            {
                // Outer Enumerator has ended
                GenerateCurrentElementIfPending();
                return;
            }

            if (!CompareIdentifiers(
                    outerIdentifierValueComparers,
                    outerIdentifier(queryContext, dbDataReader), collectionMaterializationContext.OuterIdentifier))
            {
                // Outer changed so collection has ended. Materialize last element.
                GenerateCurrentElementIfPending();
                // If parent also changed then this row is now pointing to element of next collection
                if (!CompareIdentifiers(
                        parentIdentifierValueComparers,
                        parentIdentifier(queryContext, dbDataReader), collectionMaterializationContext.ParentIdentifier))
                {
                    resultCoordinator.HasNext = true;
                }

                return;
            }

            var innerKey = selfIdentifier(queryContext, dbDataReader);
            if (innerKey.Length > 0 && innerKey.All(e => e == null))
            {
                // No correlated element
                return;
            }

            if (collectionMaterializationContext.SelfIdentifier != null)
            {
                if (CompareIdentifiers(
                        selfIdentifierValueComparers,
                        innerKey, collectionMaterializationContext.SelfIdentifier))
                {
                    // repeated row for current element
                    // If it is pending materialization then it may have nested elements
                    if (collectionMaterializationContext.ResultContext.Values != null)
                    {
                        ProcessCurrentElementRow();
                    }

                    resultCoordinator.ResultReady = false;
                    return;
                }

                // Row for new element which is not first element
                // So materialize the element
                GenerateCurrentElementIfPending();
                resultCoordinator.HasNext = null;
                collectionMaterializationContext.UpdateSelfIdentifier(innerKey);
            }
            else
            {
                // First row for current element
                collectionMaterializationContext.UpdateSelfIdentifier(innerKey);
            }

            ProcessCurrentElementRow();
            resultCoordinator.ResultReady = false;

            void ProcessCurrentElementRow()
            {
                var previousResultReady = resultCoordinator.ResultReady;
                resultCoordinator.ResultReady = true;
                var element = innerShaper(
                    queryContext, dbDataReader, collectionMaterializationContext.ResultContext, resultCoordinator);
                if (resultCoordinator.ResultReady)
                {
                    // related element is materialized
                    collectionMaterializationContext.ResultContext.Values = null;
                    ((TCollection)collectionMaterializationContext.Collection).Add(element);
                }

                resultCoordinator.ResultReady &= previousResultReady;
            }

            void GenerateCurrentElementIfPending()
            {
                if (collectionMaterializationContext.ResultContext.Values != null)
                {
                    resultCoordinator.HasNext = false;
                    ProcessCurrentElementRow();
                }

                collectionMaterializationContext.UpdateSelfIdentifier(null);
            }
        }

        private static TCollection InitializeSplitCollection<TElement, TCollection>(
            int collectionId,
            QueryContext queryContext,
            DbDataReader parentDataReader,
            SplitQueryResultCoordinator resultCoordinator,
            Func<QueryContext, DbDataReader, object[]> parentIdentifier,
            IClrCollectionAccessor? clrCollectionAccessor)
            where TCollection : class, ICollection<TElement>
        {
            var collection = clrCollectionAccessor?.Create() ?? new List<TElement>();
            var parentKey = parentIdentifier(queryContext, parentDataReader);
            var splitQueryCollectionContext = new SplitQueryCollectionContext(null, collection, parentKey);

            resultCoordinator.SetSplitQueryCollectionContext(collectionId, splitQueryCollectionContext);

            return (TCollection)collection;
        }

        private static void PopulateSplitCollection<TCollection, TElement, TRelatedEntity>(
            int collectionId,
            RelationalQueryContext queryContext,
            IExecutionStrategy executionStrategy,
            RelationalCommandCache relationalCommandCache,
            IReadOnlyList<ReaderColumn?>? readerColumns,
            bool detailedErrorsEnabled,
            SplitQueryResultCoordinator resultCoordinator,
            Func<QueryContext, DbDataReader, object[]> childIdentifier,
            IReadOnlyList<ValueComparer> identifierValueComparers,
            Func<QueryContext, DbDataReader, ResultContext, SplitQueryResultCoordinator, TRelatedEntity> innerShaper,
            Action<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator>? relatedDataLoaders)
            where TRelatedEntity : TElement
            where TCollection : class, ICollection<TElement>
        {
            if (resultCoordinator.DataReaders.Count <= collectionId
                || resultCoordinator.DataReaders[collectionId] == null)
            {
                // Execute and fetch data reader
                var dataReader = executionStrategy.Execute(
                    (queryContext, relationalCommandCache, readerColumns, detailedErrorsEnabled),
                    ((RelationalQueryContext, RelationalCommandCache, IReadOnlyList<ReaderColumn?>?, bool) tup)
                        => InitializeReader(tup.Item1, tup.Item2, tup.Item3, tup.Item4),
                    verifySucceeded: null);

                static RelationalDataReader InitializeReader(
                    RelationalQueryContext queryContext,
                    RelationalCommandCache relationalCommandCache,
                    IReadOnlyList<ReaderColumn?>? readerColumns,
                    bool detailedErrorsEnabled)
                {
                    var relationalCommand = relationalCommandCache.RentAndPopulateRelationalCommand(queryContext);

                    return relationalCommand.ExecuteReader(
                        new RelationalCommandParameterObject(
                            queryContext.Connection,
                            queryContext.ParameterValues,
                            readerColumns,
                            queryContext.Context,
                            queryContext.CommandLogger,
                            detailedErrorsEnabled, CommandSource.LinqQuery));
                }

                resultCoordinator.SetDataReader(collectionId, dataReader);
            }

            var splitQueryCollectionContext = resultCoordinator.Collections[collectionId]!;
            var dataReaderContext = resultCoordinator.DataReaders[collectionId]!;
            var dbDataReader = dataReaderContext.DataReader.DbDataReader;
            if (splitQueryCollectionContext.Collection is null)
            {
                // nothing to materialize since no collection created
                return;
            }

            while (dataReaderContext.HasNext ?? dbDataReader.Read())
            {
                if (!CompareIdentifiers(
                        identifierValueComparers,
                        splitQueryCollectionContext.ParentIdentifier, childIdentifier(queryContext, dbDataReader)))
                {
                    dataReaderContext.HasNext = true;

                    return;
                }

                dataReaderContext.HasNext = null;
                splitQueryCollectionContext.ResultContext.Values = null;

                innerShaper(queryContext, dbDataReader, splitQueryCollectionContext.ResultContext, resultCoordinator);
                relatedDataLoaders?.Invoke(queryContext, executionStrategy, resultCoordinator);
                var relatedElement = innerShaper(
                    queryContext, dbDataReader, splitQueryCollectionContext.ResultContext, resultCoordinator);
                ((TCollection)splitQueryCollectionContext.Collection).Add(relatedElement);
            }

            dataReaderContext.HasNext = false;
        }

        private static async Task PopulateSplitCollectionAsync<TCollection, TElement, TRelatedEntity>(
            int collectionId,
            RelationalQueryContext queryContext,
            IExecutionStrategy executionStrategy,
            RelationalCommandCache relationalCommandCache,
            IReadOnlyList<ReaderColumn?>? readerColumns,
            bool detailedErrorsEnabled,
            SplitQueryResultCoordinator resultCoordinator,
            Func<QueryContext, DbDataReader, object[]> childIdentifier,
            IReadOnlyList<ValueComparer> identifierValueComparers,
            Func<QueryContext, DbDataReader, ResultContext, SplitQueryResultCoordinator, TRelatedEntity> innerShaper,
            Func<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator, Task>? relatedDataLoaders)
            where TRelatedEntity : TElement
            where TCollection : class, ICollection<TElement>
        {
            if (resultCoordinator.DataReaders.Count <= collectionId
                || resultCoordinator.DataReaders[collectionId] == null)
            {
                // Execute and fetch data reader
                var dataReader = await executionStrategy.ExecuteAsync(
                        (queryContext, relationalCommandCache, readerColumns, detailedErrorsEnabled),
                        (
                                (RelationalQueryContext, RelationalCommandCache, IReadOnlyList<ReaderColumn?>?, bool) tup,
                                CancellationToken cancellationToken)
                            => InitializeReaderAsync(tup.Item1, tup.Item2, tup.Item3, tup.Item4, cancellationToken),
                        verifySucceeded: null,
                        queryContext.CancellationToken)
                    .ConfigureAwait(false);

                static async Task<RelationalDataReader> InitializeReaderAsync(
                    RelationalQueryContext queryContext,
                    RelationalCommandCache relationalCommandCache,
                    IReadOnlyList<ReaderColumn?>? readerColumns,
                    bool detailedErrorsEnabled,
                    CancellationToken cancellationToken)
                {
                    var relationalCommand = relationalCommandCache.RentAndPopulateRelationalCommand(queryContext);

                    return await relationalCommand.ExecuteReaderAsync(
                            new RelationalCommandParameterObject(
                                queryContext.Connection,
                                queryContext.ParameterValues,
                                readerColumns,
                                queryContext.Context,
                                queryContext.CommandLogger,
                                detailedErrorsEnabled,
                                CommandSource.LinqQuery),
                            cancellationToken)
                        .ConfigureAwait(false);
                }

                resultCoordinator.SetDataReader(collectionId, dataReader);
            }

            var splitQueryCollectionContext = resultCoordinator.Collections[collectionId]!;
            var dataReaderContext = resultCoordinator.DataReaders[collectionId]!;
            var dbDataReader = dataReaderContext.DataReader.DbDataReader;
            if (splitQueryCollectionContext.Collection is null)
            {
                // nothing to materialize since no collection created
                return;
            }

            while (dataReaderContext.HasNext ?? await dbDataReader.ReadAsync(queryContext.CancellationToken).ConfigureAwait(false))
            {
                if (!CompareIdentifiers(
                        identifierValueComparers,
                        splitQueryCollectionContext.ParentIdentifier, childIdentifier(queryContext, dbDataReader)))
                {
                    dataReaderContext.HasNext = true;

                    return;
                }

                dataReaderContext.HasNext = null;
                splitQueryCollectionContext.ResultContext.Values = null;

                innerShaper(queryContext, dbDataReader, splitQueryCollectionContext.ResultContext, resultCoordinator);
                if (relatedDataLoaders != null)
                {
                    await relatedDataLoaders(queryContext, executionStrategy, resultCoordinator).ConfigureAwait(false);
                }

                var relatedElement = innerShaper(
                    queryContext, dbDataReader, splitQueryCollectionContext.ResultContext, resultCoordinator);
                ((TCollection)splitQueryCollectionContext.Collection).Add(relatedElement);
            }

            dataReaderContext.HasNext = false;
        }

        private static TEntity? MaterializeJsonEntity<TEntity>(
            QueryContext queryContext,
            object[] keyPropertyValues,
            JsonReaderData? jsonReaderData,
            bool nullable,
            Func<QueryContext, object[], JsonReaderData, TEntity> shaper)
            where TEntity : class
        {
            if (jsonReaderData == null)
            {
                return nullable
                    ? null
                    : throw new InvalidOperationException(
                        RelationalStrings.JsonRequiredEntityWithNullJson(typeof(TEntity).Name));
            }

            var manager = new Utf8JsonReaderManager(jsonReaderData, queryContext.QueryLogger);
            var tokenType = manager.CurrentReader.TokenType;

            if (tokenType == JsonTokenType.Null)
            {
                return nullable
                    ? null
                    : throw new InvalidOperationException(
                        RelationalStrings.JsonRequiredEntityWithNullJson(typeof(TEntity).Name));
            }

            if (tokenType != JsonTokenType.StartObject)
            {
                throw new InvalidOperationException(
                    CoreStrings.JsonReaderInvalidTokenType(tokenType.ToString()));
            }

            manager.CaptureState();
            var result = shaper(queryContext, keyPropertyValues, jsonReaderData);

            return result;
        }

        private static TResult? MaterializeJsonEntityCollection<TEntity, TResult>(
            QueryContext queryContext,
            object[] keyPropertyValues,
            JsonReaderData? jsonReaderData,
            INavigationBase navigation,
            Func<QueryContext, object[], JsonReaderData, TEntity> innerShaper)
            where TEntity : class
        {
            if (jsonReaderData == null)
            {
                return default;
            }

            var manager = new Utf8JsonReaderManager(jsonReaderData, queryContext.QueryLogger);
            var tokenType = manager.CurrentReader.TokenType;

            if (tokenType == JsonTokenType.Null)
            {
                return default;
            }

            if (tokenType != JsonTokenType.StartArray)
            {
                throw new InvalidOperationException(
                    CoreStrings.JsonReaderInvalidTokenType(tokenType.ToString()));
            }

            var collectionAccessor = navigation.GetCollectionAccessor();
            var result = (TResult)collectionAccessor!.Create();

            var newKeyPropertyValues = new object[keyPropertyValues.Length + 1];
            Array.Copy(keyPropertyValues, newKeyPropertyValues, keyPropertyValues.Length);

            tokenType = manager.MoveNext();

            var i = 0;
            while (tokenType != JsonTokenType.EndArray)
            {
                newKeyPropertyValues[^1] = ++i;

                if (tokenType == JsonTokenType.StartObject)
                {
                    manager.CaptureState();
                    var entity = innerShaper(queryContext, newKeyPropertyValues, jsonReaderData);
                    collectionAccessor.AddStandalone(result, entity);
                    manager = new Utf8JsonReaderManager(manager.Data, queryContext.QueryLogger);

                    if (manager.CurrentReader.TokenType != JsonTokenType.EndObject)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.JsonReaderInvalidTokenType(tokenType.ToString()));
                    }

                    tokenType = manager.MoveNext();
                }
                else
                {
                    throw new InvalidOperationException(
                        CoreStrings.JsonReaderInvalidTokenType(tokenType.ToString()));
                }
            }

            manager.CaptureState();

            return result;
        }

        private static void IncludeJsonEntityReference<TIncludingEntity, TIncludedEntity>(
            QueryContext queryContext,
            object[] keyPropertyValues,
            JsonReaderData? jsonReaderData,
            TIncludingEntity entity,
            Func<QueryContext, object[], JsonReaderData, TIncludedEntity> innerShaper,
            Action<TIncludingEntity, TIncludedEntity> fixup,
            bool trackingQuery)
            where TIncludingEntity : class
            where TIncludedEntity : class
        {
            if (jsonReaderData == null)
            {
                return;
            }

            var included = innerShaper(queryContext, keyPropertyValues, jsonReaderData);

            if (!trackingQuery)
            {
                fixup(entity, included);
            }
        }

        private static void IncludeJsonEntityCollection<TIncludingEntity, TIncludedCollectionElement>(
            QueryContext queryContext,
            object[] keyPropertyValues,
            JsonReaderData? jsonReaderData,
            TIncludingEntity entity,
            Func<QueryContext, object[], JsonReaderData, TIncludedCollectionElement> innerShaper,
            Action<TIncludingEntity> getOrCreateCollectionObject,
            Action<TIncludingEntity, TIncludedCollectionElement> fixup,
            bool trackingQuery)
            where TIncludingEntity : class
            where TIncludedCollectionElement : class
        {
            if (jsonReaderData == null)
            {
                return;
            }

            var manager = new Utf8JsonReaderManager(jsonReaderData, queryContext.QueryLogger);
            var tokenType = manager.CurrentReader.TokenType;

            if (tokenType != JsonTokenType.StartArray)
            {
                throw new InvalidOperationException(
                    CoreStrings.JsonReaderInvalidTokenType(tokenType.ToString()));
            }

            getOrCreateCollectionObject(entity);

            var newKeyPropertyValues = new object[keyPropertyValues.Length + 1];
            Array.Copy(keyPropertyValues, newKeyPropertyValues, keyPropertyValues.Length);

            tokenType = manager.MoveNext();

            var i = 0;
            while (tokenType != JsonTokenType.EndArray)
            {
                newKeyPropertyValues[^1] = ++i;

                if (tokenType == JsonTokenType.StartObject)
                {
                    manager.CaptureState();
                    var resultElement = innerShaper(queryContext, newKeyPropertyValues, jsonReaderData);

                    if (!trackingQuery)
                    {
                        fixup(entity, resultElement);
                    }

                    manager = new Utf8JsonReaderManager(manager.Data, queryContext.QueryLogger);
                    if (manager.CurrentReader.TokenType != JsonTokenType.EndObject)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.JsonReaderInvalidTokenType(tokenType.ToString()));
                    }

                    tokenType = manager.MoveNext();
                }
                else
                {
                    throw new InvalidOperationException(
                        CoreStrings.JsonReaderInvalidTokenType(tokenType.ToString()));
                }
            }

            manager.CaptureState();
        }

        private static async Task TaskAwaiter(Func<Task>[] taskFactories)
        {
            for (var i = 0; i < taskFactories.Length; i++)
            {
                await taskFactories[i]().ConfigureAwait(false);
            }
        }

        private static bool CompareIdentifiers(IReadOnlyList<ValueComparer> valueComparers, object[] left, object[] right)
        {
            // Ignoring size check on all for perf as they should be same unless bug in code.
            for (var i = 0; i < left.Length; i++)
            {
                if (!valueComparers[i].Equals(left[i], right[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
