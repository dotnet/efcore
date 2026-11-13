public class SplitQueryResultCoordinator
{
    public bool? HasNext { get; set; }
    public bool CallerBuffersResults { get; set; }

    public void CompleteConsumption()
    {
        if (!CallerBuffersResults)
        {
            VerifyNoOrphanedChildRows();
        }
    }
}

public class SplitQueryingEnumerable<T>
{
    private sealed class Enumerator
    {
        public bool MoveNext()
        {
            var hasNext = _dataReader!.Read();
            if (hasNext)
            {
                Current = _shaper(_relationalQueryContext, _dbDataReader!, _resultCoordinator);
                _relatedDataLoaders?.Invoke(
                    _relationalQueryContext,
                    _relationalQueryContext.ExecutionStrategy,
                    _resultCoordinator!);
            }
            else
            {
                _resultCoordinator!.CompleteConsumption();
                Current = default!;
            }

            return hasNext;
        }

        public void Dispose()
        {
            _resultCoordinator?.CompleteConsumption();
            _dataReader?.Dispose();
        }
    }

    private sealed class AsyncEnumerator
    {
        public async ValueTask<bool> MoveNextAsync()
        {
            var hasNext = await _dataReader!.ReadAsync(_cancellationToken).ConfigureAwait(false);
            if (hasNext)
            {
                Current = _shaper(_relationalQueryContext, _dbDataReader!, _resultCoordinator);
                if (_relatedDataLoaders != null)
                {
                    await _relatedDataLoaders(
                            _relationalQueryContext,
                            _relationalQueryContext.ExecutionStrategy,
                            _resultCoordinator!)
                        .ConfigureAwait(false);
                }
            }
            else
            {
                _resultCoordinator!.CompleteConsumption();
                Current = default!;
            }

            return hasNext;
        }

        public async ValueTask DisposeAsync()
        {
            _resultCoordinator?.CompleteConsumption();
            if (_dataReader != null)
            {
                await _dataReader.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}

public class GroupBySplitQueryingEnumerable<TKey, TElement>
{
    private sealed class Enumerator
    {
        public bool MoveNext()
        {
            var hasNext = _resultCoordinator!.HasNext ?? _dataReader!.Read();
            if (hasNext)
            {
                var group = new InternalGrouping(_keySelector(_relationalQueryContext, _dbDataReader!));
                var keyIdentifier = _keyIdentifier(_relationalQueryContext, _dbDataReader!);
                do
                {
                    _resultCoordinator.HasNext = null;
                    _resultCoordinator.ResultContext.Values = null;
                    var element = _elementSelector(
                        _relationalQueryContext,
                        _dbDataReader!,
                        _resultCoordinator.ResultContext,
                        _resultCoordinator);
                    _relatedDataLoaders?.Invoke(
                        _relationalQueryContext,
                        _relationalQueryContext.ExecutionStrategy,
                        _resultCoordinator);
                    group.Add(element);

                    if (_resultCoordinator.HasNext ?? _dbDataReader!.Read())
                    {
                        if (!CompareIdentifiers(
                                _keyIdentifierValueComparers,
                                keyIdentifier,
                                _keyIdentifier(_relationalQueryContext, _dbDataReader!)))
                        {
                            _resultCoordinator.HasNext = true;
                            _resultCoordinator.CompleteConsumption();
                            Current = group;
                            break;
                        }
                    }
                    else
                    {
                        Current = group;
                        break;
                    }
                }
                while (true);
            }
            else
            {
                Current = default!;
            }

            return hasNext;
        }

        public void Dispose()
        {
            _resultCoordinator?.CompleteConsumption();
            _dataReader?.Dispose();
        }
    }

    private sealed class AsyncEnumerator
    {
        public async ValueTask<bool> MoveNextAsync()
        {
            var hasNext = _resultCoordinator!.HasNext
                ?? await _dataReader!.ReadAsync(_cancellationToken).ConfigureAwait(false);
            if (hasNext)
            {
                var group = new InternalGrouping(_keySelector(_relationalQueryContext, _dbDataReader!));
                var keyIdentifier = _keyIdentifier(_relationalQueryContext, _dbDataReader!);
                do
                {
                    _resultCoordinator.HasNext = null;
                    _resultCoordinator.ResultContext.Values = null;
                    var element = _elementSelector(
                        _relationalQueryContext,
                        _dbDataReader!,
                        _resultCoordinator.ResultContext,
                        _resultCoordinator);
                    if (_relatedDataLoaders != null)
                    {
                        await _relatedDataLoaders(
                                _relationalQueryContext,
                                _relationalQueryContext.ExecutionStrategy,
                                _resultCoordinator)
                            .ConfigureAwait(false);
                    }
                    group.Add(element);

                    if (_resultCoordinator.HasNext
                        ?? await _dbDataReader!.ReadAsync(_cancellationToken).ConfigureAwait(false))
                    {
                        if (!CompareIdentifiers(
                                _keyIdentifierValueComparers,
                                keyIdentifier,
                                _keyIdentifier(_relationalQueryContext, _dbDataReader!)))
                        {
                            _resultCoordinator.HasNext = true;
                            _resultCoordinator.CompleteConsumption();
                            Current = group;
                            break;
                        }
                    }
                    else
                    {
                        Current = group;
                        break;
                    }
                }
                while (true);
            }
            else
            {
                Current = default!;
            }

            return hasNext;
        }

        public async ValueTask DisposeAsync()
        {
            _resultCoordinator?.CompleteConsumption();
            if (_dataReader != null)
            {
                await _dataReader.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}