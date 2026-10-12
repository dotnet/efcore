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
                _resultCoordinator!.VerifyNoOrphanedChildRows();
                Current = default!;
            }

            return hasNext;
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
                _resultCoordinator!.VerifyNoOrphanedChildRows();
                Current = default!;
            }

            return hasNext;
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
                do
                {
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

                    if (!(_resultCoordinator.HasNext ?? _dbDataReader!.Read()))
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
                do
                {
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

                    if (!(_resultCoordinator.HasNext
                        ?? await _dbDataReader!.ReadAsync(_cancellationToken).ConfigureAwait(false)))
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
    }
}