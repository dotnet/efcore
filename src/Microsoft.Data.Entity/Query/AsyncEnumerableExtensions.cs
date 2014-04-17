// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query
{
    public static class AsyncEnumerableExtensions
    {
        #region Any

        public static async Task<bool> AnyAsync<TSource>(
            [NotNull] this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(source, "source");

            cancellationToken.ThrowIfCancellationRequested();

            using (var e = source.GetAsyncEnumerator())
            {
                if (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    return true;
                }
            }

            return false;
        }

        public static async Task<bool> AnyAsync<TSource>(
            [NotNull] this IAsyncEnumerable<TSource> source,
            [NotNull] Func<TSource, bool> predicate,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            cancellationToken.ThrowIfCancellationRequested();

            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    if (predicate(e.Current))
                    {
                        return true;
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            return false;
        }

        #endregion

        #region First

        public static async Task<TSource> FirstAsync<TSource>(
            [NotNull] this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(source, "source");

            cancellationToken.ThrowIfCancellationRequested();

            using (var e = source.GetAsyncEnumerator())
            {
                if (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    return e.Current;
                }
            }

            throw new InvalidOperationException(Strings.EmptySequence);
        }

        public static async Task<TSource> FirstAsync<TSource>(
            [NotNull] this IAsyncEnumerable<TSource> source,
            [NotNull] Func<TSource, bool> predicate,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            cancellationToken.ThrowIfCancellationRequested();

            using (var e = source.GetAsyncEnumerator())
            {
                if (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    if (predicate(e.Current))
                    {
                        return e.Current;
                    }
                }
            }

            throw new InvalidOperationException(Strings.NoMatch);
        }

        #endregion

        #region Single

        public static async Task<TSource> SingleAsync<TSource>(
            [NotNull] this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(source, "source");

            cancellationToken.ThrowIfCancellationRequested();

            using (var e = source.GetAsyncEnumerator())
            {
                if (!await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    throw new InvalidOperationException(Strings.EmptySequence);
                }

                cancellationToken.ThrowIfCancellationRequested();

                var result = e.Current;

                if (!await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    return result;
                }
            }

            throw new InvalidOperationException(Strings.MoreThanOneElement);
        }

        public static async Task<TSource> SingleAsync<TSource>(
            [NotNull] this IAsyncEnumerable<TSource> source,
            [NotNull] Func<TSource, bool> predicate,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            cancellationToken.ThrowIfCancellationRequested();

            var result = default(TSource);
            long count = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (predicate(e.Current))
                    {
                        result = e.Current;
                        checked
                        {
                            count++;
                        }
                    }
                }
            }

            switch (count)
            {
                case 0:
                    throw new InvalidOperationException(Strings.NoMatch);
                case 1:
                    return result;
            }

            throw new InvalidOperationException(Strings.MoreThanOneMatch);
        }

        #endregion

        #region SingleOrDefault

        public static async Task<TSource> SingleOrDefaultAsync<TSource>(
            [NotNull] this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(source, "source");

            cancellationToken.ThrowIfCancellationRequested();

            using (var e = source.GetAsyncEnumerator())
            {
                if (!await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    return default(TSource);
                }

                cancellationToken.ThrowIfCancellationRequested();

                var result = e.Current;

                if (!await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    return result;
                }
            }

            throw new InvalidOperationException(Strings.MoreThanOneElement);
        }

        public static async Task<TSource> SingleOrDefaultAsync<TSource>(
            [NotNull] this IAsyncEnumerable<TSource> source,
            [NotNull] Func<TSource, bool> predicate,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            cancellationToken.ThrowIfCancellationRequested();

            var result = default(TSource);
            long count = 0;
            using (var e = source.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (predicate(e.Current))
                    {
                        result = e.Current;
                        checked
                        {
                            count++;
                        }
                    }
                }
            }

            if (count < 2)
            {
                return result;
            }

            throw new InvalidOperationException(Strings.MoreThanOneMatch);
        }

        #endregion

        #region Count

        public static async Task<int> CountAsync<TSource>(
            [NotNull] this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(source, "source");

            cancellationToken.ThrowIfCancellationRequested();

            var count = 0;

            using (var e = source.GetAsyncEnumerator())
            {
                checked
                {
                    while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        count++;
                    }
                }
            }

            return count;
        }

        public static async Task<int> CountAsync<TSource>(
            [NotNull] this IAsyncEnumerable<TSource> source,
            [NotNull] Func<TSource, bool> predicate,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var count = 0;

            using (var e = source.GetAsyncEnumerator())
            {
                checked
                {
                    while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (predicate(e.Current))
                        {
                            count++;
                        }
                    }
                }
            }

            return count;
        }

        #endregion

        #region ForEach

        public static async Task ForEachAsync<T>(
            [NotNull] this IAsyncEnumerable<T> source,
            [NotNull] Action<T> action,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(source, "source");
            Check.NotNull(action, "action");

            var enumerator = source.GetAsyncEnumerator();

            using (enumerator)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (await enumerator.MoveNextAsync(cancellationToken)
                    .ConfigureAwait(continueOnCapturedContext: false))
                {
                    Task<bool> moveNextTask;

                    do
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var current = enumerator.Current;

                        moveNextTask = enumerator.MoveNextAsync(cancellationToken);

                        action(current);
                    }
                    while (await moveNextTask.ConfigureAwait(continueOnCapturedContext: false));
                }
            }
        }

        #endregion

        #region ToArray

        public static async Task<T[]> ToArrayAsync<T>(
            [NotNull] this IAsyncEnumerable<T> source,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(source, "source");

            return (await source.ToListAsync(cancellationToken)
                .ConfigureAwait(continueOnCapturedContext: false))
                .ToArray();
        }

        #endregion

        #region ToList

        public static Task<List<T>> ToListAsync<T>(
            [NotNull] this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(source, "source");

            var taskCompletionSource = new TaskCompletionSource<List<T>>();
            var list = new List<T>();

            source.ForEachAsync(list.Add, cancellationToken).ContinueWith(
                t =>
                    {
                        if (t.IsFaulted)
                        {
                            if (t.Exception != null)
                            {
                                taskCompletionSource.TrySetException(t.Exception.InnerExceptions);
                            }
                        }
                        else if (t.IsCanceled)
                        {
                            taskCompletionSource.TrySetCanceled();
                        }
                        else
                        {
                            taskCompletionSource.TrySetResult(list);
                        }
                    },
                TaskContinuationOptions.ExecuteSynchronously);

            return taskCompletionSource.Task;
        }

        #endregion
    }
}
