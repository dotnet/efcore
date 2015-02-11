// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace System.Linq
{
    [DebuggerStepThrough]
    internal static class AsyncEnumerableExtensions
    {
        public static IAsyncEnumerable<TResult> Select<TSource, TResult>(
            [NotNull] this IAsyncEnumerable<TSource> source,
            [NotNull] Func<TSource, CancellationToken, Task<TResult>> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            return new AsyncSelectEnumerable<TSource, TResult>(source, selector);
        }

        private class AsyncSelectEnumerable<TSource, TResult> : IAsyncEnumerable<TResult>
        {
            private readonly IAsyncEnumerable<TSource> _source;
            private readonly Func<TSource, CancellationToken, Task<TResult>> _selector;

            public AsyncSelectEnumerable(
                [NotNull] IAsyncEnumerable<TSource> source,
                [NotNull] Func<TSource, CancellationToken, Task<TResult>> selector)
            {
                Check.NotNull(source, "source");
                Check.NotNull(selector, "selector");

                _source = source;
                _selector = selector;
            }

            public IAsyncEnumerator<TResult> GetEnumerator()
            {
                return new AsyncSelectEnumerator(this);
            }

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
                    if (!await _enumerator.MoveNext(cancellationToken).WithCurrentCulture())
                    {
                        return false;
                    }

                    Current = await _selector(_enumerator.Current, cancellationToken).WithCurrentCulture();

                    return true;
                }

                public TResult Current { get; private set; }

                public void Dispose()
                {
                    _enumerator.Dispose();
                }
            }
        }
    }
}
