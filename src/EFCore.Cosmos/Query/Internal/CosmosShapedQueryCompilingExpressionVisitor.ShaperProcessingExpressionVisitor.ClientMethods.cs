// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

public partial class CosmosShapedQueryCompilingExpressionVisitor
{
    private sealed partial class ShaperProcessingExpressionVisitor
    {
        private static readonly MethodInfo AnyMethodInfo
            = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(Any))!;

        private static readonly MethodInfo MaterializeJsonStructuralTypeMethodInfo
            = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(MaterializeJsonStructuralType))!;

        private static readonly MethodInfo MaterializeJsonNullableValueStructuralTypeMethodInfo
            = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(MaterializeJsonNullableValueStructuralType))!;

        private static readonly MethodInfo MaterializeJsonEntityCollectionMethodInfo
            = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(MaterializeJsonEntityCollection))!;

        private static readonly MethodInfo InverseCollectionFixupMethod
            = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(InverseCollectionFixup))!;

        private static readonly MethodInfo TryMaterializeNextJsonCollectionItemMethod
            = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(TryMaterializeNextJsonCollectionItem))!;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public static bool Any(IEnumerable source)
        {
            foreach (var _ in source)
            {
                return true;
            }

            return false;
        }

        // Almost 1-1 copy from relational, but no key values
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public static TStructural? MaterializeJsonStructuralType<TStructural>(
            QueryContext queryContext,
            JsonReaderData jsonReaderData,
            bool nullable,
            Func<QueryContext, JsonReaderData, TStructural> shaper)
        {
            var manager = new Utf8JsonReaderManager(jsonReaderData, queryContext.QueryLogger);
            var tokenType = manager.CurrentReader.TokenType;

            switch (tokenType)
            {
                case JsonTokenType.Null:
                    return nullable
                        ? default
                        : throw new InvalidOperationException("Nullable object must have a value"); // @TODO ?

                case not JsonTokenType.StartObject:
                    throw new InvalidOperationException(
                        CoreStrings.JsonReaderInvalidTokenType(tokenType.ToString()));
            }

            var result = shaper(queryContext, jsonReaderData);

            return result;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public static TStructural? MaterializeJsonNullableValueStructuralType<TStructural>(
            QueryContext queryContext,
            JsonReaderData jsonReaderData,
            bool nullable,
            Func<QueryContext, JsonReaderData, TStructural> shaper)
            where TStructural : struct
        {
            var manager = new Utf8JsonReaderManager(jsonReaderData, queryContext.QueryLogger);
            var tokenType = manager.CurrentReader.TokenType;

            switch (tokenType)
            {
                case JsonTokenType.Null:
                    return nullable
                        ? null
                        : throw new InvalidOperationException("Nullable object must have a value"); // @TODO ?

                case not JsonTokenType.StartObject:
                    throw new InvalidOperationException(
                        CoreStrings.JsonReaderInvalidTokenType(tokenType.ToString()));
            }

            var result = shaper(queryContext, jsonReaderData);

            return result;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public static TResult? MaterializeJsonEntityCollection<TEntity, TResult>(
            QueryContext queryContext,
            JsonReaderData jsonReaderData,
            IPropertyBase structuralProperty,
            Func<QueryContext, JsonReaderData, TEntity> innerShaper)
            where TEntity : class
        {
            var manager = new Utf8JsonReaderManager(jsonReaderData, queryContext.QueryLogger);
            var tokenType = manager.CurrentReader.TokenType;

            switch (tokenType)
            {
                case JsonTokenType.Null:
                    return default; // throw new InvalidOperationException("Nullable object must have a value"); // @TODO ?

                case not JsonTokenType.StartArray:
                    throw new InvalidOperationException(CoreStrings.JsonReaderInvalidTokenType(tokenType.ToString()));
            }

            var collectionAccessor = structuralProperty.GetCollectionAccessor();
            var result = (TResult)collectionAccessor!.Create();

            tokenType = manager.MoveNext();

            while (tokenType != JsonTokenType.EndArray)
            {
                if (tokenType == JsonTokenType.StartObject)
                {
                    manager.CaptureState();
                    var entity = innerShaper(queryContext, jsonReaderData);
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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public static void InverseCollectionFixup<TCollectionElement, TEntity>(
            ICollection<TCollectionElement> collection,
            TEntity entity,
            Action<TCollectionElement, TEntity> elementFixup)
        {
            foreach (var collectionElement in collection)
            {
                elementFixup(collectionElement, entity);
            }
        }

        private static readonly byte[] WhiteSpaceBytes = Encoding.UTF8.GetBytes(" \t");
        private static readonly byte EndArrayByte = Encoding.UTF8.GetBytes("]")[0];
        private static readonly byte NextArrayItemByte = Encoding.UTF8.GetBytes(",")[0];

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public static bool TryMaterializeNextJsonCollectionItem<T>(
            CosmosQueryContext queryContext,
            ReadOnlyMemory<byte> data,
            JsonReaderState jsonReaderState,
            Func<CosmosQueryContext, ReadOnlyMemory<byte>, T> shaper,
            out int bytesConsumed,
            out T? result)
        {
            var startLength = data.Length;

            data = data.TrimStart(WhiteSpaceBytes);
            if (data.Span[0] == EndArrayByte)
            {
                bytesConsumed = startLength - data.Length + 1;
                result = default;
                return false;
            }

            bytesConsumed = startLength - data.Length;

            result = shaper(queryContext, data);

            // @TODO: Get BytesConsumed from shaper?
            var reader = new Utf8JsonReader(data.Span, true, jsonReaderState);
            reader.Read();
            // This could be a scalar or a structural type, if it is an object, we need to move past it.
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                reader.Read();
                while (reader.TokenType != JsonTokenType.EndObject)
                {
                    reader.Skip();
                    reader.Read();
                }
            }

            startLength = data.Length;
            data = data.Slice((int)reader.BytesConsumed).TrimStart(WhiteSpaceBytes);
            bytesConsumed += startLength - data.Length;

            if (data.Span[0] == NextArrayItemByte)
            {
                bytesConsumed += 1;
            }

            return true;
        }
    }
}
