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
        private static readonly byte[] WhiteSpaceBytes = Encoding.UTF8.GetBytes(" \t");
        private static readonly byte EndArrayByte = Encoding.UTF8.GetBytes("]")[0];
        private static readonly byte NextItemByte = Encoding.UTF8.GetBytes(",")[0];

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

            data = data.Slice(bytesConsumedShaper);
            bytesConsumed += bytesConsumedShaper;

            SliceNextItemToken(data, out var bytesConsumedNextItem);
            bytesConsumed += bytesConsumedNextItem;

            return true;
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
