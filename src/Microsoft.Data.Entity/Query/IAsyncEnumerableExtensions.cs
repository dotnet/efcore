// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query
{
    public static class IAsyncEnumerableExtensions
    {
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
    }
}
