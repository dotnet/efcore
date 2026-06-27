// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

public partial class CosmosShapedQueryCompilingExpressionVisitor
{
    private sealed partial class ShaperProcessingExpressionVisitor
    {
        private static readonly byte[] WhiteSpaceBytes = Encoding.UTF8.GetBytes(" \t");
        private static readonly byte EndArrayByte = Encoding.UTF8.GetBytes("]")[0];
        private static readonly byte NextItemByte = Encoding.UTF8.GetBytes(",")[0];

        public static bool TryMaterializeNextJsonCollectionItem<T>(QueryContext queryContext,
            ReadOnlyMemory<byte> data,
            Shaper<T> shaper,
            out int bytesConsumed,
            [NotNullWhen(true)] out T? result)
            => TryMaterializeNextJsonCollectionItem(queryContext, data, shaper, 0, out bytesConsumed, out result);

        public static bool TryMaterializeNextJsonCollectionItem<T>(
            QueryContext queryContext,
            ReadOnlyMemory<byte> data,
            Shaper<T> shaper,
            int ordinal,
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

            result = shaper(queryContext, data, ordinal, out var shaperBytesConsumed)!;

            // The shaper might be a constant expression, in which case it won't consume any bytes. In that case, we need to skip the next token.
            if (shaperBytesConsumed == 0)
            {
                var reader = new Utf8JsonReader(data.Span, isFinalBlock: true, default);
                reader.Read();
                reader.Skip();
                shaperBytesConsumed = (int)reader.BytesConsumed;
            }

            data = data.Slice(shaperBytesConsumed);
            bytesConsumed += shaperBytesConsumed;

            SliceNextItemToken(data, out var bytesConsumedNextItem);
            bytesConsumed += bytesConsumedNextItem;

            return true;
        }

        private static readonly MethodInfo ShapeCollectionMethodInfo
            = typeof(ShaperProcessingExpressionVisitor).GetMethod(nameof(ShapeCollection), BindingFlags.NonPublic | BindingFlags.Static) ?? throw new UnreachableException();

        private static TCollection ShapeCollection<TElement, TCollection>(
            QueryContext queryContext,
            ReadOnlyMemory<byte> data,
            Func<object> creator,
            Shaper<TElement> shaper,
            out int bytesConsumed)
        {
            // TODO: throw a better exception for non ICollection navigations
            var collection = (ICollection<TElement>)creator();

            var reader = new Utf8JsonReader(data.Span);
            reader.Read();
            data = data.Slice((int)reader.BytesConsumed);
            bytesConsumed = (int)reader.BytesConsumed;

            if (reader.TokenType == JsonTokenType.Null)
            {
                // #35916
                return (TCollection)collection;
            }

            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new InvalidOperationException(CoreStrings.JsonReaderInvalidTokenType(reader.TokenType));
            }

            int itemBytesConsumed;
            while (TryMaterializeNextJsonCollectionItem(queryContext, data, shaper, collection.Count, out itemBytesConsumed, out var item))
            {
                collection.Add(item);
                data = data.Slice(itemBytesConsumed);
                bytesConsumed += itemBytesConsumed;
            }
            bytesConsumed += itemBytesConsumed; // ']'

            return (TCollection)collection;
        }

        private static readonly MethodInfo SliceNextItemTokenMethodInfo
            = typeof(ShaperProcessingExpressionVisitor).GetMethod(nameof(SliceNextItemToken), BindingFlags.NonPublic | BindingFlags.Static) ?? throw new UnreachableException();

        private static ReadOnlyMemory<byte> SliceNextItemToken(ReadOnlyMemory<byte> data, out int bytesConsumed)
        {
            var startLength = data.Length;

            data = data.TrimStart(WhiteSpaceBytes);
            if (data.Span[0] == NextItemByte)
            {
                bytesConsumed = startLength - data.Length + 1;
                return data.Slice(1);
            }

            bytesConsumed = startLength - data.Length;
            return data;
        }

        private static readonly MethodInfo NewJsonReaderInvalidTokenTypeExceptionMethodInfo
            = typeof(ShaperProcessingExpressionVisitor).GetMethod(nameof(NewJsonReaderInvalidTokenTypeException), BindingFlags.NonPublic | BindingFlags.Static) ?? throw new UnreachableException();

        private static InvalidOperationException NewJsonReaderInvalidTokenTypeException(JsonTokenType jsonTokenType)
            => new(CoreStrings.JsonReaderInvalidTokenType(jsonTokenType));
    }
}
