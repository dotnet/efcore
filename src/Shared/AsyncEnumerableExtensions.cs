// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq
{
    [DebuggerStepThrough]
    internal static class AsyncEnumerableExtensions
    {
        public static IAsyncEnumerable<TResult> Select<TSource, TResult>(
            this IAsyncEnumerable<TSource> source,
            Func<TSource, CancellationToken, Task<TResult>> selector)
            => new AsyncSelectEnumerable<TSource, TResult>(source, selector);

        private class AsyncSelectEnumerable<TSource, TResult> : IAsyncEnumerable<TResult>
        {
            private readonly IAsyncEnumerable<TSource> _source;
            private readonly Func<TSource, CancellationToken, Task<TResult>> _selector;

            public AsyncSelectEnumerable(
                IAsyncEnumerable<TSource> source,
                Func<TSource, CancellationToken, Task<TResult>> selector)
            {
                _source = source;
                _selector = selector;
            }

            public IAsyncEnumerator<TResult> GetEnumerator() => new AsyncSelectEnumerator(this);

            private class AsyncSelectEnumerator : IAsyncEnumerator<TResult>
            {
                private readonly IAsyncEnumerator<TSource> _enumerator;
                private readonly Func<TSource, CancellationToken, Task<TResult>> _selector;

                public AsyncSelectEnumerator(AsyncSelectEnumerable<TSource, TResult> enumerable)
                {
                    _enumerator = enumerable._source.GetEnumerator();
                    _selector = enumerable._selector;
                }

                public async Task<bool> MoveNext(CancellationToken cancellationToken)
                {
                    if (!await _enumerator.MoveNext(cancellationToken))
                    {
                        return false;
                    }

                    Current = await _selector(_enumerator.Current, cancellationToken);

                    return true;
                }

                public TResult Current { get; private set; }

                public void Dispose() => _enumerator.Dispose();
            }
        }
    }
}
