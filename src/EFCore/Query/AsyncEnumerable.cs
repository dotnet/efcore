// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     Represents an asynchronous sequence produced by executing a compiled query.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    public readonly struct AsyncEnumerable<TResult> : IAsyncEnumerableAccessor<TResult>
    {
        private readonly IAsyncEnumerable<TResult> _asyncEnumerable;

        /// <summary>
        ///     Creates a new instance of <see cref="AsyncEnumerable{TResult}" />
        /// </summary>
        /// <param name="asyncEnumerable">The underlying <see cref="IAsyncEnumerable{TResult}" /> instance.</param>
        public AsyncEnumerable([NotNull] IAsyncEnumerable<TResult> asyncEnumerable)
        {
            Check.NotNull(asyncEnumerable, nameof(asyncEnumerable));

            _asyncEnumerable = asyncEnumerable;
        }

        IAsyncEnumerable<TResult> IAsyncEnumerableAccessor<TResult>.AsyncEnumerable => _asyncEnumerable;

        /// <summary>
        ///     Asynchronously creates a <see cref="List{T}" /> from this <see cref="AsyncEnumerable{T}" />
        ///     by enumerating it asynchronously.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains a <see cref="List{T}" /> that contains elements from the input sequence.
        /// </returns>
        public async Task<List<TResult>> ToListAsync(
            CancellationToken cancellationToken = default)
        {
            var list = new List<TResult>();

            using (var asyncEnumerator = _asyncEnumerable.GetEnumerator())
            {
                while (await asyncEnumerator.MoveNext(cancellationToken))
                {
                    list.Add(asyncEnumerator.Current);
                }
            }

            return list;
        }

        /// <summary>
        ///     Asynchronously creates an array from this <see cref="AsyncEnumerable{TResult}" />.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains an array that contains elements from the input sequence.
        /// </returns>
        public async Task<TResult[]> ToArrayAsync(
            CancellationToken cancellationToken = default)
            => (await ToListAsync(cancellationToken)).ToArray();

        /// <summary>
        ///     Asynchronously enumerates the query. When using Entity Framework, this causes the results of the query to
        ///     be loaded into the associated context. This is equivalent to calling ToList
        ///     and then throwing away the list (without the overhead of actually creating the list).
        /// </summary>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns> A task that represents the asynchronous operation. </returns>
        public async Task LoadAsync(
            CancellationToken cancellationToken = default)
        {
            using (var enumerator = _asyncEnumerable.GetEnumerator())
            {
                while (await enumerator.MoveNext(cancellationToken))
                {
                }
            }
        }

        /// <summary>
        ///     Creates a <see cref="Dictionary{TKey, TValue}" /> from this <see cref="AsyncEnumerable{TResult}" />
        ///     by enumerating it asynchronously according to a specified key selector function.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TKey">
        ///     The type of the key returned by <paramref name="keySelector" /> .
        /// </typeparam>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains a <see cref="Dictionary{TKey, TResult}" /> that contains selected keys and values.
        /// </returns>
        public Task<Dictionary<TKey, TResult>> ToDictionaryAsync<TKey>(
            [NotNull] Func<TResult, TKey> keySelector,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(keySelector, nameof(keySelector));

            return _asyncEnumerable.ToDictionary(keySelector, cancellationToken);
        }

        /// <summary>
        ///     Creates a <see cref="Dictionary{TKey, TValue}" /> from this <see cref="AsyncEnumerable{TResult}" />
        ///     by enumerating it
        ///     asynchronously
        ///     according to a specified key selector function and a comparer.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TKey">
        ///     The type of the key returned by <paramref name="keySelector" /> .
        /// </typeparam>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="comparer">
        ///     An <see cref="IEqualityComparer{TKey}" /> to compare keys.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains a <see cref="Dictionary{TKey, TResult}" /> that contains selected keys and values.
        /// </returns>
        public Task<Dictionary<TKey, TResult>> ToDictionaryAsync<TKey>(
            [NotNull] Func<TResult, TKey> keySelector,
            [NotNull] IEqualityComparer<TKey> comparer,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(keySelector, nameof(keySelector));
            Check.NotNull(comparer, nameof(comparer));

            return _asyncEnumerable.ToDictionary(keySelector, comparer, cancellationToken);
        }

        /// <summary>
        ///     Creates a <see cref="Dictionary{TKey, TValue}" /> from this <see cref="AsyncEnumerable{TResult}" />
        ///     by enumerating it asynchronously according to a specified key selector and an element selector function.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TKey">
        ///     The type of the key returned by <paramref name="keySelector" /> .
        /// </typeparam>
        /// <typeparam name="TElement">
        ///     The type of the value returned by <paramref name="elementSelector" />.
        /// </typeparam>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="elementSelector"> A transform function to produce a result element value from each element. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains a <see cref="Dictionary{TKey, TElement}" /> that contains values of type
        ///     <typeparamref name="TElement" /> selected from the input sequence.
        /// </returns>
        public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey, TElement>(
            [NotNull] Func<TResult, TKey> keySelector,
            [NotNull] Func<TResult, TElement> elementSelector,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(keySelector, nameof(keySelector));
            Check.NotNull(elementSelector, nameof(elementSelector));

            return _asyncEnumerable.ToDictionary(keySelector, elementSelector, cancellationToken);
        }

        /// <summary>
        ///     Creates a <see cref="Dictionary{TKey, TValue}" /> from this <see cref="AsyncEnumerable{TResult}" />
        ///     by enumerating it asynchronously according to a specified key selector function, a comparer, and an element selector function.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TKey">
        ///     The type of the key returned by <paramref name="keySelector" /> .
        /// </typeparam>
        /// <typeparam name="TElement">
        ///     The type of the value returned by <paramref name="elementSelector" />.
        /// </typeparam>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="elementSelector"> A transform function to produce a result element value from each element. </param>
        /// <param name="comparer">
        ///     An <see cref="IEqualityComparer{TKey}" /> to compare keys.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains a <see cref="Dictionary{TKey, TElement}" /> that contains values of type
        ///     <typeparamref name="TElement" /> selected from the input sequence.
        /// </returns>
        public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey, TElement>(
            [NotNull] Func<TResult, TKey> keySelector,
            [NotNull] Func<TResult, TElement> elementSelector,
            [NotNull] IEqualityComparer<TKey> comparer,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(keySelector, nameof(keySelector));
            Check.NotNull(elementSelector, nameof(elementSelector));
            Check.NotNull(comparer, nameof(comparer));

            return _asyncEnumerable.ToDictionary(keySelector, elementSelector, comparer, cancellationToken);
        }

        /// <summary>
        ///     Asynchronously enumerates the query results and performs the specified action on each element.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="action"> The action to perform on each element. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns> A task that represents the asynchronous operation. </returns>
        public async Task ForEachAsync(
            [NotNull] Action<TResult> action,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(action, nameof(action));

            using (var asyncEnumerator = _asyncEnumerable.GetEnumerator())
            {
                while (await asyncEnumerator.MoveNext(cancellationToken))
                {
                    action(asyncEnumerator.Current);
                }
            }
        }
    }
}
