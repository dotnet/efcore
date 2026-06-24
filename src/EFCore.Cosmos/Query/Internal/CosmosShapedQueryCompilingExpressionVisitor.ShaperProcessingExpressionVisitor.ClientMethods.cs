// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
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

        private static readonly MethodInfo ReadShapedCollectionMethod
            = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(MaterializedShapedCollection))!;

        private static readonly MethodInfo TryMaterializeNextJsonCollectionItemMethod // @TODO: Unused?
            = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(TryMaterializeNextJsonCollectionItem))!;

        private static readonly MethodInfo ReadPathMethod
           = typeof(ShaperProcessingExpressionVisitor).GetMethod(nameof(ReadPath))!;

        public static bool Any(IEnumerable source)
        {
            foreach (var _ in source)
            {
                return true;
            }

            return false;
        }

        // Almost 1-1 copy from relational, but different key values
        public static TStructural? MaterializeJsonStructuralType<TStructural>(
            QueryContext queryContext,
            ISnapshot? keyValues,
            JsonReaderData jsonReaderData,
            bool nullable,
            Func<QueryContext, ISnapshot?, JsonReaderData, TStructural> shaper)
        {
            var manager = new Utf8JsonReaderManager(jsonReaderData, queryContext.QueryLogger);
            var tokenType = manager.CurrentReader.TokenType;

            switch (tokenType)
            {
                case JsonTokenType.Null:
                    return default; // See: #21006?

                case not JsonTokenType.StartObject:
                    throw new InvalidOperationException(
                        CoreStrings.JsonReaderInvalidTokenType(tokenType.ToString()));
            }

            var result = shaper(queryContext, keyValues, jsonReaderData);

            return result;
        }

        public static TStructural? MaterializeJsonNullableValueStructuralType<TStructural>(
            QueryContext queryContext,
            ISnapshot? keyValues,
            JsonReaderData jsonReaderData,
            bool nullable,
            Func<QueryContext, ISnapshot?, JsonReaderData, TStructural> shaper)
            where TStructural : struct
        {
            var manager = new Utf8JsonReaderManager(jsonReaderData, queryContext.QueryLogger);
            var tokenType = manager.CurrentReader.TokenType;

            switch (tokenType)
            {
                case JsonTokenType.Null:
                    return default; // See: #21006

                case not JsonTokenType.StartObject:
                    throw new InvalidOperationException(
                        CoreStrings.JsonReaderInvalidTokenType(tokenType.ToString()));
            }

            var result = shaper(queryContext, keyValues, jsonReaderData);

            return result;
        }

        public static TResult? MaterializeJsonEntityCollection<TEntity, TResult>(
            QueryContext queryContext,
            ISnapshot? keyValues,
            JsonReaderData jsonReaderData,
            IPropertyBase structuralProperty,
            Func<QueryContext, ISnapshot?, JsonReaderData, TEntity> innerShaper,
            Func<ISnapshot?, int, ISnapshot?> snapshotFactory)
            where TEntity : class
        {
            var manager = new Utf8JsonReaderManager(jsonReaderData, queryContext.QueryLogger);
            var tokenType = manager.CurrentReader.TokenType;

            var collectionAccessor = structuralProperty.GetCollectionAccessor();
            var result = (TResult)collectionAccessor!.Create();

            switch (tokenType)
            {
                case JsonTokenType.Null:
                    return result; // See: #21006

                case not JsonTokenType.StartArray:
                    throw new InvalidOperationException(CoreStrings.JsonReaderInvalidTokenType(tokenType.ToString()));
            }

            tokenType = manager.MoveNext();
            var count = 0;
            while (tokenType != JsonTokenType.EndArray)
            {
                if (tokenType == JsonTokenType.StartObject)
                {
                    manager.CaptureState();
                    var newKeyValues = snapshotFactory(keyValues, count++);
                    var entity = innerShaper(queryContext, newKeyValues, jsonReaderData);
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

        public static TCollection MaterializedShapedCollection<TElement, TCollection>(
            QueryContext queryContext,
            ReadOnlyMemory<byte> data,
            Func<object> creator,
            Shaper<TElement> shaper)
        {
            // TODO: throw a better exception for non ICollection navigations
            var collection = (ICollection<TElement>)creator();

            var reader = new Utf8JsonReader(data.Span);
            reader.Read();
            data = data.Slice((int)reader.BytesConsumed);

            if (reader.TokenType == JsonTokenType.Null)
            {
                // #35916
                return (TCollection)collection;
            }

            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new InvalidOperationException(CoreStrings.JsonReaderInvalidTokenType(reader.TokenType));
            }

            while (TryMaterializeNextJsonCollectionItem(queryContext, data, shaper, out var bytesConsumed, out var item))
            {
                collection.Add(item);
                data = data.Slice(bytesConsumed);
            }

            return (TCollection)collection;
        }

        private static readonly byte[] WhiteSpaceBytes = Encoding.UTF8.GetBytes(" \t");
        private static readonly byte EndArrayByte = Encoding.UTF8.GetBytes("]")[0];
        private static readonly byte NextArrayItemByte = Encoding.UTF8.GetBytes(",")[0];

        public static bool TryMaterializeNextJsonCollectionItem<T>(
            QueryContext queryContext,
            ReadOnlyMemory<byte> data,
            Shaper<T> shaper,
            out int bytesConsumed,
            [NotNullWhen(true)] out T? result)
        {
            var startLength = data.Length;

            // Don't use Utf8JsonReader, because it will read the whole next token, which could be a big string.
            data = data.TrimStart(WhiteSpaceBytes);
            if (data.Span[0] == EndArrayByte)
            {
                bytesConsumed = startLength - data.Length + 1;
                result = default;
                return false;
            }

            bytesConsumed = startLength - data.Length;

            result = shaper(queryContext, data, out var bytesConsumedShaper)!;

            data = data.Slice(bytesConsumedShaper).TrimStart(WhiteSpaceBytes);
            bytesConsumed += bytesConsumedShaper;

            if (data.Span[0] == NextArrayItemByte)
            {
                bytesConsumed += 1;
            }

            return true;
        }

        private static readonly byte[] NullBytes = Encoding.UTF8.GetBytes("null");

        public static ReadOnlyMemory<byte> ReadPath(ReadOnlyMemory<byte> data, LinkedList<byte[]> jsonPropertyPath)
        {
            var jsonReader = new Utf8JsonReader(data.Span);
            jsonReader.Read();

            var prop = jsonPropertyPath.First;
            if (jsonReader.TokenType != JsonTokenType.StartObject && jsonReader.TokenType != JsonTokenType.StartArray)
            {
                throw new InvalidOperationException(
                    CoreStrings.JsonReaderInvalidTokenType(jsonReader.TokenType));
            }

            while (prop != null)
            {
                jsonReader.Read();
                switch (jsonReader.TokenType)
                {
                    case JsonTokenType.StartObject:
                        break;
                    case JsonTokenType.PropertyName:
                        if (jsonReader.ValueTextEquals(prop.Value))
                        {
                            prop = prop.Next;
                        }
                        else
                        {
                            jsonReader.Skip();
                        }
                        break;
                    case JsonTokenType.EndObject:
                        return NullBytes; // @TODO: Improve? ReadPath is going to be thrown out anyway, so this won't need to be improved here, but in the new implementation.
                    case JsonTokenType.Null:
                        throw new NullReferenceException(); // This is what 10.0 threw... Should we just continue instead?
                    default:
                        throw new InvalidOperationException(CoreStrings.JsonReaderInvalidTokenType(jsonReader.TokenType));
                }
            }

            return data.Slice((int)jsonReader.BytesConsumed);
        }
    }
}
