// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public partial class CosmosShapedQueryCompilingExpressionVisitor
{
    private sealed class QueryingEnumerable<T> : IEnumerable<T>, IAsyncEnumerable<T>, IQueryingEnumerable
    {
        private readonly CosmosQueryContext _cosmosQueryContext;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private readonly SelectExpression _selectExpression;
        private readonly Shaper<T> _shaper;
        private readonly IQuerySqlGeneratorFactory _querySqlGeneratorFactory;
        private readonly Type _contextType;
        private readonly string _cosmosContainer;
        private readonly PartitionKey _cosmosPartitionKey;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _queryLogger;
        private readonly bool _standAloneStateManager;
        private readonly bool _threadSafetyChecksEnabled;

        private readonly string? _extremumKeyProjection;
        private readonly string? _valueProjection;
        private readonly (string Alias, bool Ascending)[]? _tieBreakers;

        public QueryingEnumerable(
            CosmosQueryContext cosmosQueryContext,
            ISqlExpressionFactory sqlExpressionFactory,
            IQuerySqlGeneratorFactory querySqlGeneratorFactory,
            SelectExpression selectExpression,
            Shaper<T> shaper,
            Type contextType,
            IEntityType rootEntityType,
            List<Expression> partitionKeyPropertyValues,
            bool standAloneStateManager,
            bool threadSafetyChecksEnabled,
            string? extremumKeyProjection,
            string? valueProjection,
            (string Alias, bool Ascending)[]? tieBreakers)
        {
            _cosmosQueryContext = cosmosQueryContext;
            _sqlExpressionFactory = sqlExpressionFactory;
            _querySqlGeneratorFactory = querySqlGeneratorFactory;
            _selectExpression = selectExpression;
            _shaper = shaper;
            _extremumKeyProjection = extremumKeyProjection;
            _valueProjection = valueProjection;
            _tieBreakers = tieBreakers;
            _contextType = contextType;
            _queryLogger = cosmosQueryContext.QueryLogger;
            _standAloneStateManager = standAloneStateManager;
            _threadSafetyChecksEnabled = threadSafetyChecksEnabled;

            _cosmosContainer = rootEntityType.GetContainer()
                ?? throw new UnreachableException("Root entity type without a Cosmos container.");
            _cosmosPartitionKey = GeneratePartitionKey(
                rootEntityType, partitionKeyPropertyValues, _cosmosQueryContext.Parameters);
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new AsyncEnumerator(this, cancellationToken);

        public IEnumerator<T> GetEnumerator()
            => throw new InvalidOperationException(CosmosStrings.SyncNotSupported);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        private CosmosSqlQuery GenerateQuery()
            => _querySqlGeneratorFactory.Create(_cosmosQueryContext.Context.Model).GetSqlQuery(
                (SelectExpression)new ParameterInliner(
                        _sqlExpressionFactory,
                        _cosmosQueryContext.Parameters)
                    .Visit(_selectExpression),
                _cosmosQueryContext.Parameters!);

        public string ToQueryString()
        {
            var sqlQuery = GenerateQuery();
            if (sqlQuery.Parameters.Count == 0)
            {
                return sqlQuery.Query;
            }

            var builder = new StringBuilder();
            foreach (var parameter in sqlQuery.Parameters)
            {
                var paramJsonString = parameter.ToJsonString();
                builder
                    .Append("-- ")
                    .Append(parameter.Name)
                    .Append('=');

                if (paramJsonString == "null")
                {
                    builder.AppendLine(paramJsonString);
                }
                else
                {
                    builder.Append('\'')
                        .Append(paramJsonString.Trim('"'))
                        .AppendLine("'");
                }
            }

            return builder.Append(sqlQuery.Query).ToString();
        }

        private sealed class AsyncEnumerator : IAsyncEnumerator<T>
        {
            private readonly QueryingEnumerable<T> _queryingEnumerable;
            private readonly CosmosQueryContext _cosmosQueryContext;
            private readonly Shaper<T> _shaper;
            private readonly Type _contextType;
            private readonly string _cosmosContainer;
            private readonly PartitionKey _cosmosPartitionKey;
            private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _queryLogger;
            private readonly bool _standAloneStateManager;
            private readonly CancellationToken _cancellationToken;
            private readonly IConcurrencyDetector? _concurrencyDetector;
            private readonly IExceptionDetector _exceptionDetector;

            private int _ordinal;
            private T? _current;
            private ReadOnlyMemory<byte>? _data;
            private IAsyncEnumerator<ReadOnlyMemory<byte>>? _enumerator;

            public AsyncEnumerator(QueryingEnumerable<T> queryingEnumerable, CancellationToken cancellationToken)
            {
                _queryingEnumerable = queryingEnumerable;
                _cosmosQueryContext = queryingEnumerable._cosmosQueryContext;
                _shaper = queryingEnumerable._shaper;
                _contextType = queryingEnumerable._contextType;
                _cosmosContainer = queryingEnumerable._cosmosContainer;
                _cosmosPartitionKey = queryingEnumerable._cosmosPartitionKey;
                _queryLogger = queryingEnumerable._queryLogger;
                _standAloneStateManager = queryingEnumerable._standAloneStateManager;
                _exceptionDetector = _cosmosQueryContext.ExceptionDetector;
                _cancellationToken = cancellationToken;

                _concurrencyDetector = queryingEnumerable._threadSafetyChecksEnabled
                    ? _cosmosQueryContext.ConcurrencyDetector
                    : null;
            }

            public T Current
                => _current!;

            public async ValueTask<bool> MoveNextAsync()
            {
                try
                {
                    using var criticalSection = _concurrencyDetector?.EnterCriticalSection();

                    if (_enumerator == null)
                    {
                        var sqlQuery = _queryingEnumerable.GenerateQuery();

                        EntityFrameworkMetricsData.ReportQueryExecuting();

                        _enumerator = _cosmosQueryContext.CosmosClient
                            .ExecuteSqlQueryAsync(_cosmosContainer, _cosmosPartitionKey, sqlQuery, _cosmosQueryContext.SessionTokenStorage)
                            .GetAsyncEnumerator(_cancellationToken);
                        _cosmosQueryContext.InitializeStateManager(_standAloneStateManager);
                    }

                    if (_queryingEnumerable._extremumKeyProjection is { } projection)
                    {
                        if (_ordinal != 0)
                        {
                            return false;
                        }

                        var winner = await ReadExtremumAsync(projection).ConfigureAwait(false);
                        if (winner.IsEmpty)
                        {
                            return false;
                        }

                        _current = _shaper(_cosmosQueryContext, winner, _ordinal++, out _);
                        return true;
                    }

                    ReadNext:
                    if (!_data.HasValue)
                    {
                        var hasNext = await _enumerator.MoveNextAsync().ConfigureAwait(false);
                        if (!hasNext)
                        {
                            return false;
                        }

                        _data = _enumerator.Current;

                        _data = ShaperProcessingExpressionVisitor.ExtractDocuments(_data.Value);
                    }

                    if (!ShaperProcessingExpressionVisitor.TryMaterializeNextJsonCollectionItem(
                            _cosmosQueryContext, _data.Value,
                            _shaper, _ordinal,
                            out var bytesConsumed, out _current))
                    {
                        _data = null;
                        goto ReadNext;
                    }

                    _ordinal++;
                    _data = _data.Value[bytesConsumed..];

                    return true;
                }
                catch (Exception exception)
                {
                    if (_exceptionDetector.IsCancellation(exception, _cancellationToken))
                    {
                        _queryLogger.QueryCanceled(_contextType);
                    }
                    else
                    {
                        _queryLogger.QueryIterationFailed(_contextType, exception);
                    }

                    throw;
                }
            }

            // The server orders by key. MinBy skips leading nulls unless every key is null.
            // For ordered sources, resolve ties before shaping so only the winner is tracked.
            private async ValueTask<ReadOnlyMemory<byte>> ReadExtremumAsync(string projection)
            {
                var ties = _queryingEnumerable._tieBreakers;
                Shaper<Candidate> readCandidate = ReadCandidate;
                Candidate winner = default;
                while (await _enumerator!.MoveNextAsync().ConfigureAwait(false))
                {
                    var winnerChanged = false;
                    var data = ShaperProcessingExpressionVisitor.ExtractDocuments(_enumerator.Current);
                    while (ShaperProcessingExpressionVisitor.TryMaterializeNextJsonCollectionItem(
                               _cosmosQueryContext, data, readCandidate, 0, out var consumed, out Candidate candidate))
                    {
                        data = data[consumed..];
                        if (candidate.Data.IsEmpty)
                        {
                            continue;
                        }

                        if (ties == null && !candidate.Key.IsEmpty)
                        {
                            return candidate.Data;
                        }

                        if (!winner.Key.IsEmpty && !SameKey(candidate.Key, winner.Key))
                        {
                            return winner.Data;
                        }

                        if (winner.Data.IsEmpty || (winner.Key.IsEmpty && !candidate.Key.IsEmpty)
                            || (ties != null && ComesFirst(candidate.SortValues!, winner.SortValues!, ties)))
                        {
                            winner = candidate;
                            winnerChanged = true;
                        }
                    }

                    if (winnerChanged)
                    {
                        // Preserve the selected row when advancing the response feed to the next page.
                        winner = new Candidate(
                            winner.Data.ToArray(), winner.Key.ToArray(),
                            winner.SortValues?.Select(v => (ReadOnlyMemory<byte>)v.ToArray()).ToArray());
                    }
                }

                return winner.Data;

                Candidate ReadCandidate(QueryContext context, ReadOnlyMemory<byte> data, int ordinal, out int bytesConsumed)
                {
                    var reader = new Utf8JsonReader(data.Span);
                    reader.Read();
                    ReadOnlyMemory<byte> key = default;
                    var sortValues = ties == null ? null : new ReadOnlyMemory<byte>[ties.Length];
                    ReadOnlyMemory<byte> value = default;
                    if (projection.Length == 0)
                    {
                        var hasKey = reader.TokenType != JsonTokenType.Null;
                        reader.Skip();
                        if (hasKey)
                        {
                            key = data[..(int)reader.BytesConsumed];
                        }
                    }
                    else
                    {
                        while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
                        {
                            var isKey = reader.ValueTextEquals(projection);
                            var isValue = _queryingEnumerable._valueProjection is { } alias && reader.ValueTextEquals(alias);
                            var sortIndex = -1;
                            if (ties != null)
                            {
                                for (var i = 0; i < ties.Length; i++)
                                {
                                    if (reader.ValueTextEquals(ties[i].Alias))
                                    {
                                        sortIndex = i;
                                        break;
                                    }
                                }
                            }

                            reader.Read();
                            var start = (int)reader.TokenStartIndex;
                            var hasKey = isKey && reader.TokenType != JsonTokenType.Null;
                            reader.Skip();
                            if (sortIndex >= 0)
                            {
                                sortValues![sortIndex] = data[start..(int)reader.BytesConsumed];
                            }

                            if (hasKey)
                            {
                                key = data[start..(int)reader.BytesConsumed];
                            }

                            if (isValue)
                            {
                                value = data[start..(int)reader.BytesConsumed];
                            }
                        }
                    }

                    bytesConsumed = (int)reader.BytesConsumed;
                    var row = data[..bytesConsumed];
                    return new Candidate(_queryingEnumerable._valueProjection is null ? row : value, key, sortValues);
                }
            }

            private static bool SameKey(ReadOnlyMemory<byte> left, ReadOnlyMemory<byte> right)
                => left.Span.SequenceEqual(right.Span)
                    || (!left.IsEmpty && JsonElement.DeepEquals(JsonElement.Parse(left.Span), JsonElement.Parse(right.Span)));

            private static bool ComesFirst(
                ReadOnlyMemory<byte>[] current,
                ReadOnlyMemory<byte>[] previous,
                (string Alias, bool Ascending)[] ties)
            {
                for (var i = 0; i < ties.Length; i++)
                {
                    var comparison = CompareScalar(current[i].Span, previous[i].Span);
                    if (comparison != 0)
                    {
                        return ties[i].Ascending ? comparison < 0 : comparison > 0;
                    }
                }

                return false;
            }

            private static int CompareScalar(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
            {
                if (left.SequenceEqual(right))
                {
                    return 0;
                }

                // A missing path sorts before an explicit null.
                if (left.IsEmpty || right.IsEmpty)
                {
                    return left.IsEmpty ? -1 : 1;
                }

                var l = new Utf8JsonReader(left);
                var r = new Utf8JsonReader(right);
                l.Read();
                r.Read();
                if (l.TokenType == JsonTokenType.Null || r.TokenType == JsonTokenType.Null)
                {
                    return l.TokenType == JsonTokenType.Null ? -1 : 1;
                }

                if (l.TokenType == JsonTokenType.String)
                {
                    return l.ValueIsEscaped || r.ValueIsEscaped
                        ? Encoding.UTF8.GetBytes(l.GetString()!).AsSpan().SequenceCompareTo(Encoding.UTF8.GetBytes(r.GetString()!))
                        : l.ValueSpan.SequenceCompareTo(r.ValueSpan);
                }

                if (l.TokenType is JsonTokenType.True or JsonTokenType.False)
                {
                    return l.GetBoolean().CompareTo(r.GetBoolean());
                }

                var leftInteger = l.TryGetInt64(out var li);
                var rightInteger = r.TryGetInt64(out var ri);
                if (leftInteger && rightInteger)
                {
                    return li.CompareTo(ri);
                }

                var x = l.GetDouble();
                var y = r.GetDouble();
                var result = x.CompareTo(y);
                // If an Int64 rounded to its floating-point neighbor, compare the integral values exactly.
                if (result == 0 && leftInteger != rightInteger)
                {
                    return leftInteger ? ((Int128)li).CompareTo((Int128)y) : ((Int128)x).CompareTo((Int128)ri);
                }

                return result;
            }

            private readonly record struct Candidate(ReadOnlyMemory<byte> Data, ReadOnlyMemory<byte> Key, ReadOnlyMemory<byte>[]? SortValues);

            public ValueTask DisposeAsync()
            {
                var enumerator = _enumerator;
                if (enumerator != null)
                {
                    _enumerator = null;
                    return enumerator.DisposeAsync();
                }

                return default;
            }
        }
    }
}
