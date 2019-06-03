// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public static class EntityFrameworkEnumerableExtensions
    {
        #region ToList/Array

        /// <summary>
        ///     Asynchronously creates a <see cref="List{T}" /> from an <see cref="IAsyncEnumerable{T}" /> by enumerating it
        ///     asynchronously.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IAsyncEnumerable{T}" /> to create a list from.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains a <see cref="List{T}" /> that contains elements from the input sequence.
        /// </returns>
        public static async Task<List<TSource>> ToListAsync<TSource>(
            [NotNull] this IAsyncEnumerable<TSource> source,
            CancellationToken cancellationToken = default)
        {
            var list = new List<TSource>();
            await foreach (var element in source.WithCancellation(cancellationToken))
            {
                list.Add(element);
            }

            return list;
        }

        /// <summary>
        ///     Asynchronously creates an array from an <see cref="IAsyncEnumerable{T}" /> by enumerating it asynchronously.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IAsyncEnumerable{T}" /> to create an array from.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains an array that contains elements from the input sequence.
        /// </returns>
        public static async Task<TSource[]> ToArrayAsync<TSource>(
            [NotNull] this IAsyncEnumerable<TSource> source,
            CancellationToken cancellationToken = default)
            => (await source.ToListAsync(cancellationToken)).ToArray();

        #endregion

        #region ToDictionary

        /// <summary>
        ///     Creates a <see cref="Dictionary{TKey, TValue}" /> from an <see cref="IAsyncEnumerable{T}" /> by enumerating it
        ///     asynchronously
        ///     according to a specified key selector function.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <typeparam name="TKey">
        ///     The type of the key returned by <paramref name="keySelector" /> .
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IAsyncEnumerable{T}" /> to create a <see cref="Dictionary{TKey, TValue}" /> from.
        /// </param>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains a <see cref="Dictionary{TKey, TSource}" /> that contains selected keys and values.
        /// </returns>
        public static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            [NotNull] this IAsyncEnumerable<TSource> source,
            [NotNull] Func<TSource, TKey> keySelector,
            CancellationToken cancellationToken = default)
            => ToDictionaryAsync(source, keySelector, e => e, comparer: null, cancellationToken);

        /// <summary>
        ///     Creates a <see cref="Dictionary{TKey, TValue}" /> from an <see cref="IAsyncEnumerable{T}" /> by enumerating it
        ///     asynchronously
        ///     according to a specified key selector function and a comparer.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <typeparam name="TKey">
        ///     The type of the key returned by <paramref name="keySelector" /> .
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IAsyncEnumerable{T}" /> to create a <see cref="Dictionary{TKey, TValue}" /> from.
        /// </param>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="comparer">
        ///     An <see cref="IEqualityComparer{TKey}" /> to compare keys.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains a <see cref="Dictionary{TKey, TSource}" /> that contains selected keys and values.
        /// </returns>
        public static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            [NotNull] this IAsyncEnumerable<TSource> source,
            [NotNull] Func<TSource, TKey> keySelector,
            [NotNull] IEqualityComparer<TKey> comparer,
            CancellationToken cancellationToken = default)
            => ToDictionaryAsync(source, keySelector, e => e, comparer, cancellationToken);

        /// <summary>
        ///     Creates a <see cref="Dictionary{TKey, TValue}" /> from an <see cref="IAsyncEnumerable{T}" /> by enumerating it
        ///     asynchronously
        ///     according to a specified key selector and an element selector function.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <typeparam name="TKey">
        ///     The type of the key returned by <paramref name="keySelector" /> .
        /// </typeparam>
        /// <typeparam name="TElement">
        ///     The type of the value returned by <paramref name="elementSelector" />.
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IAsyncEnumerable{T}" /> to create a <see cref="Dictionary{TKey, TValue}" /> from.
        /// </param>
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
        public static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            [NotNull] this IAsyncEnumerable<TSource> source,
            [NotNull] Func<TSource, TKey> keySelector,
            [NotNull] Func<TSource, TElement> elementSelector,
            CancellationToken cancellationToken = default)
            => ToDictionaryAsync(source, keySelector, elementSelector, comparer: null, cancellationToken);

        /// <summary>
        ///     Creates a <see cref="Dictionary{TKey, TValue}" /> from an <see cref="IAsyncEnumerable{T}" /> by enumerating it
        ///     asynchronously
        ///     according to a specified key selector function, a comparer, and an element selector function.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <typeparam name="TKey">
        ///     The type of the key returned by <paramref name="keySelector" /> .
        /// </typeparam>
        /// <typeparam name="TElement">
        ///     The type of the value returned by <paramref name="elementSelector" />.
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IAsyncEnumerable{T}" /> to create a <see cref="Dictionary{TKey, TValue}" /> from.
        /// </param>
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
        public static async Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            [NotNull] this IAsyncEnumerable<TSource> source,
            [NotNull] Func<TSource, TKey> keySelector,
            [NotNull] Func<TSource, TElement> elementSelector,
            [NotNull] IEqualityComparer<TKey> comparer,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(keySelector, nameof(keySelector));
            Check.NotNull(elementSelector, nameof(elementSelector));
            Check.NotNull(comparer, nameof(comparer));

            var d = new Dictionary<TKey, TElement>(comparer);
            await foreach (var element in source.WithCancellation(cancellationToken))
            {
                d.Add(keySelector(element), elementSelector(element));
            }

            return d;
        }

        #endregion

        #region ToLookup

        /// <summary>
        ///     Creates a <see cref="ILookup{TKey, TValue}" /> from an <see cref="IAsyncEnumerable{T}" /> by enumerating it
        ///     asynchronously
        ///     according to a specified key selector function.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <typeparam name="TKey">
        ///     The type of the key returned by <paramref name="keySelector" /> .
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IAsyncEnumerable{T}" /> to create a <see cref="ILookup{TKey, TValue}" /> from.
        /// </param>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains a <see cref="ILookup{TKey, TSource}" /> that contains selected keys and values.
        /// </returns>
        public static Task<ILookup<TKey, TSource>> ToLookupAsync<TSource, TKey>(
            [NotNull] this IAsyncEnumerable<TSource> source,
            [NotNull] Func<TSource, TKey> keySelector,
            CancellationToken cancellationToken = default)
            => ToLookupAsync(source, keySelector, e => e, comparer: null, cancellationToken);

        /// <summary>
        ///     Creates a <see cref="ILookup{TKey, TValue}" /> from an <see cref="IAsyncEnumerable{T}" /> by enumerating it
        ///     asynchronously
        ///     according to a specified key selector function and a comparer.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <typeparam name="TKey">
        ///     The type of the key returned by <paramref name="keySelector" /> .
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IAsyncEnumerable{T}" /> to create a <see cref="ILookup{TKey, TValue}" /> from.
        /// </param>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="comparer">
        ///     An <see cref="IEqualityComparer{TKey}" /> to compare keys.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains a <see cref="ILookup{TKey, TSource}" /> that contains selected keys and values.
        /// </returns>
        public static Task<ILookup<TKey, TSource>> ToLookupAsync<TSource, TKey>(
            [NotNull] this IAsyncEnumerable<TSource> source,
            [NotNull] Func<TSource, TKey> keySelector,
            [NotNull] IEqualityComparer<TKey> comparer,
            CancellationToken cancellationToken = default)
            => ToLookupAsync(source, keySelector, e => e, comparer, cancellationToken);

        /// <summary>
        ///     Creates a <see cref="ILookup{TKey, TValue}" /> from an <see cref="IAsyncEnumerable{T}" /> by enumerating it
        ///     asynchronously
        ///     according to a specified key selector and an element selector function.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <typeparam name="TKey">
        ///     The type of the key returned by <paramref name="keySelector" /> .
        /// </typeparam>
        /// <typeparam name="TElement">
        ///     The type of the value returned by <paramref name="elementSelector" />.
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IAsyncEnumerable{T}" /> to create a <see cref="ILookup{TKey, TValue}" /> from.
        /// </param>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="elementSelector"> A transform function to produce a result element value from each element. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains a <see cref="ILookup{TKey, TElement}" /> that contains values of type
        ///     <typeparamref name="TElement" /> selected from the input sequence.
        /// </returns>
        public static Task<ILookup<TKey, TElement>> ToLookupAsync<TSource, TKey, TElement>(
            [NotNull] this IAsyncEnumerable<TSource> source,
            [NotNull] Func<TSource, TKey> keySelector,
            [NotNull] Func<TSource, TElement> elementSelector,
            CancellationToken cancellationToken = default)
            => ToLookupAsync(source, keySelector, elementSelector, comparer: null, cancellationToken);

        /// <summary>
        ///     Creates a <see cref="ILookup{TKey, TValue}" /> from an <see cref="IAsyncEnumerable{T}" /> by enumerating it
        ///     asynchronously
        ///     according to a specified key selector function, a comparer, and an element selector function.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <typeparam name="TKey">
        ///     The type of the key returned by <paramref name="keySelector" /> .
        /// </typeparam>
        /// <typeparam name="TElement">
        ///     The type of the value returned by <paramref name="elementSelector" />.
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IAsyncEnumerable{T}" /> to create a <see cref="ILookup{TKey, TValue}" /> from.
        /// </param>
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
        ///     The task result contains a <see cref="ILookup{TKey, TElement}" /> that contains values of type
        ///     <typeparamref name="TElement" /> selected from the input sequence.
        /// </returns>
        public static Task<ILookup<TKey, TElement>> ToLookupAsync<TSource, TKey, TElement>(
            [NotNull] this IAsyncEnumerable<TSource> source,
            [NotNull] Func<TSource, TKey> keySelector,
            [NotNull] Func<TSource, TElement> elementSelector,
            [NotNull] IEqualityComparer<TKey> comparer,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(keySelector, nameof(keySelector));
            Check.NotNull(elementSelector, nameof(elementSelector));
            Check.NotNull(comparer, nameof(comparer));

            throw new NotImplementedException("ToLookupAsync");
        }

        #endregion

        #region ForEach

        /// <summary>
        ///     Asynchronously enumerates the query results and performs the specified action on each element.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="T">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IAsyncEnumerable{T}" /> to enumerate.
        /// </param>
        /// <param name="action"> The action to perform on each element. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns> A task that represents the asynchronous operation. </returns>
        public static async Task ForEachAsync<T>(
            [NotNull] this IAsyncEnumerable<T> source,
            [NotNull] Action<T> action,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(action, nameof(action));

            await foreach (var element in source.WithCancellation(cancellationToken))
            {
                action(element);
            }
        }

        #endregion
    }
}
