// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Entity Framework LINQ related extension methods.
    /// </summary>
    public static class EntityFrameworkQueryableExtensions
    {
        #region Any/All

        private static readonly MethodInfo _any = GetMethod(nameof(Queryable.Any));

        /// <summary>
        ///     Asynchronously determines whether a sequence contains any elements.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IQueryable{T}" /> to check for being empty.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains <c>true</c> if the source sequence contains any elements; otherwise, <c>false</c>.
        /// </returns>
        public static Task<bool> AnyAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));

            return ExecuteAsync<TSource, Task<bool>>(_any, source, cancellationToken);
        }

        private static readonly MethodInfo _anyPredicate = GetMethod(nameof(Queryable.Any), parameterCount: 1);

        /// <summary>
        ///     Asynchronously determines whether any element of a sequence satisfies a condition.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IQueryable{T}" /> whose elements to test for a condition.
        /// </param>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains <c>true</c> if any elements in the source sequence pass the test in the specified
        ///     predicate; otherwise, <c>false</c>.
        /// </returns>
        public static Task<bool> AnyAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(predicate, nameof(predicate));

            return ExecuteAsync<TSource, Task<bool>>(_anyPredicate, source, predicate, cancellationToken);
        }

        private static readonly MethodInfo _allPredicate = GetMethod(nameof(Queryable.All), parameterCount: 1);

        /// <summary>
        ///     Asynchronously determines whether all the elements of a sequence satisfy a condition.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IQueryable{T}" /> whose elements to test for a condition.
        /// </param>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains <c>true</c> if every element of the source sequence passes the test in the specified
        ///     predicate; otherwise, <c>false</c>.
        /// </returns>
        public static Task<bool> AllAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(predicate, nameof(predicate));

            return ExecuteAsync<TSource, Task<bool>>(_allPredicate, source, predicate, cancellationToken);
        }

        #endregion

        #region Count/LongCount

        private static readonly MethodInfo _count = GetMethod(nameof(Queryable.Count));

        /// <summary>
        ///     Asynchronously returns the number of elements in a sequence.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IQueryable{T}" /> that contains the elements to be counted.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the number of elements in the input sequence.
        /// </returns>
        public static Task<int> CountAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));

            return ExecuteAsync<TSource, Task<int>>(_count, source, cancellationToken);
        }

        private static readonly MethodInfo _countPredicate = GetMethod(nameof(Queryable.Count), parameterCount: 1);

        /// <summary>
        ///     Asynchronously returns the number of elements in a sequence that satisfy a condition.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IQueryable{T}" /> that contains the elements to be counted.
        /// </param>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the number of elements in the sequence that satisfy the condition in the predicate
        ///     function.
        /// </returns>
        public static Task<int> CountAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(predicate, nameof(predicate));

            return ExecuteAsync<TSource, Task<int>>(_countPredicate, source, predicate, cancellationToken);
        }

        private static readonly MethodInfo _longCount = GetMethod(nameof(Queryable.LongCount));

        /// <summary>
        ///     Asynchronously returns an <see cref="long" /> that represents the total number of elements in a sequence.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IQueryable{T}" /> that contains the elements to be counted.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the number of elements in the input sequence.
        /// </returns>
        public static Task<long> LongCountAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));

            return ExecuteAsync<TSource, Task<long>>(_longCount, source, cancellationToken);
        }

        private static readonly MethodInfo _longCountPredicate = GetMethod(nameof(Queryable.LongCount), parameterCount: 1);

        /// <summary>
        ///     Asynchronously returns an <see cref="long" /> that represents the number of elements in a sequence
        ///     that satisfy a condition.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IQueryable{T}" /> that contains the elements to be counted.
        /// </param>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the number of elements in the sequence that satisfy the condition in the predicate
        ///     function.
        /// </returns>
        public static Task<long> LongCountAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(predicate, nameof(predicate));

            return ExecuteAsync<TSource, Task<long>>(_longCountPredicate, source, predicate, cancellationToken);
        }

        #endregion

        #region First/FirstOrDefault

        private static readonly MethodInfo _first = GetMethod(nameof(Queryable.First));

        /// <summary>
        ///     Asynchronously returns the first element of a sequence.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IQueryable{T}" /> to return the first element of.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the first element in <paramref name="source" />.
        /// </returns>
        public static Task<TSource> FirstAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));

            return ExecuteAsync<TSource, Task<TSource>>(_first, source, cancellationToken);
        }

        private static readonly MethodInfo _firstPredicate = GetMethod(nameof(Queryable.First), parameterCount: 1);

        /// <summary>
        ///     Asynchronously returns the first element of a sequence that satisfies a specified condition.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IQueryable{T}" /> to return the first element of.
        /// </param>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the first element in <paramref name="source" /> that passes the test in
        ///     <paramref name="predicate" />.
        /// </returns>
        public static Task<TSource> FirstAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(predicate, nameof(predicate));

            return ExecuteAsync<TSource, Task<TSource>>(_firstPredicate, source, predicate, cancellationToken);
        }

        private static readonly MethodInfo _firstOrDefault = GetMethod(nameof(Queryable.FirstOrDefault));

        /// <summary>
        ///     Asynchronously returns the first element of a sequence, or a default value if the sequence contains no elements.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IQueryable{T}" /> to return the first element of.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains <c>default</c> ( <typeparamref name="TSource" /> ) if
        ///     <paramref name="source" /> is empty; otherwise, the first element in <paramref name="source" />.
        /// </returns>
        public static Task<TSource> FirstOrDefaultAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));

            return ExecuteAsync<TSource, Task<TSource>>(_firstOrDefault, source, cancellationToken);
        }

        private static readonly MethodInfo _firstOrDefaultPredicate = GetMethod(nameof(Queryable.FirstOrDefault), parameterCount: 1);

        /// <summary>
        ///     Asynchronously returns the first element of a sequence that satisfies a specified condition
        ///     or a default value if no such element is found.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IQueryable{T}" /> to return the first element of.
        /// </param>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains <c>default</c> ( <typeparamref name="TSource" /> ) if <paramref name="source" />
        ///     is empty or if no element passes the test specified by <paramref name="predicate" /> ; otherwise, the first
        ///     element in <paramref name="source" /> that passes the test specified by <paramref name="predicate" />.
        /// </returns>
        public static Task<TSource> FirstOrDefaultAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(predicate, nameof(predicate));

            return ExecuteAsync<TSource, Task<TSource>>(_firstOrDefaultPredicate, source, predicate, cancellationToken);
        }

        #endregion

        #region Last/LastOrDefault

        private static readonly MethodInfo _last = GetMethod(nameof(Queryable.Last));

        /// <summary>
        ///     Asynchronously returns the last element of a sequence.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IQueryable{T}" /> to return the last element of.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the last element in <paramref name="source" />.
        /// </returns>
        public static Task<TSource> LastAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));

            return ExecuteAsync<TSource, Task<TSource>>(_last, source, cancellationToken);
        }

        private static readonly MethodInfo _lastPredicate = GetMethod(nameof(Queryable.Last), parameterCount: 1);

        /// <summary>
        ///     Asynchronously returns the last element of a sequence that satisfies a specified condition.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IQueryable{T}" /> to return the last element of.
        /// </param>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the last element in <paramref name="source" /> that passes the test in
        ///     <paramref name="predicate" />.
        /// </returns>
        public static Task<TSource> LastAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(predicate, nameof(predicate));

            return ExecuteAsync<TSource, Task<TSource>>(_lastPredicate, source, predicate, cancellationToken);
        }

        private static readonly MethodInfo _lastOrDefault = GetMethod(nameof(Queryable.LastOrDefault));

        /// <summary>
        ///     Asynchronously returns the last element of a sequence, or a default value if the sequence contains no elements.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IQueryable{T}" /> to return the last element of.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains <c>default</c> ( <typeparamref name="TSource" /> ) if
        ///     <paramref name="source" /> is empty; otherwise, the last element in <paramref name="source" />.
        /// </returns>
        public static Task<TSource> LastOrDefaultAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));

            return ExecuteAsync<TSource, Task<TSource>>(_lastOrDefault, source, cancellationToken);
        }

        private static readonly MethodInfo _lastOrDefaultPredicate = GetMethod(nameof(Queryable.LastOrDefault), parameterCount: 1);

        /// <summary>
        ///     Asynchronously returns the last element of a sequence that satisfies a specified condition
        ///     or a default value if no such element is found.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IQueryable{T}" /> to return the last element of.
        /// </param>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains <c>default</c> ( <typeparamref name="TSource" /> ) if <paramref name="source" />
        ///     is empty or if no element passes the test specified by <paramref name="predicate" /> ; otherwise, the last
        ///     element in <paramref name="source" /> that passes the test specified by <paramref name="predicate" />.
        /// </returns>
        public static Task<TSource> LastOrDefaultAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(predicate, nameof(predicate));

            return ExecuteAsync<TSource, Task<TSource>>(_lastOrDefaultPredicate, source, predicate, cancellationToken);
        }

        #endregion

        #region Single/SingleOrDefault

        private static readonly MethodInfo _single = GetMethod(nameof(Queryable.Single));

        /// <summary>
        ///     Asynchronously returns the only element of a sequence, and throws an exception
        ///     if there is not exactly one element in the sequence.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IQueryable{T}" /> to return the single element of.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the single element of the input sequence.
        /// </returns>
        public static Task<TSource> SingleAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));

            return ExecuteAsync<TSource, Task<TSource>>(_single, source, cancellationToken);
        }

        private static readonly MethodInfo _singlePredicate = GetMethod(nameof(Queryable.Single), parameterCount: 1);

        /// <summary>
        ///     Asynchronously returns the only element of a sequence that satisfies a specified condition,
        ///     and throws an exception if more than one such element exists.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IQueryable{T}" /> to return the single element of.
        /// </param>
        /// <param name="predicate"> A function to test an element for a condition. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the single element of the input sequence that satisfies the condition in
        ///     <paramref name="predicate" />.
        /// </returns>
        public static Task<TSource> SingleAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(predicate, nameof(predicate));

            return ExecuteAsync<TSource, Task<TSource>>(_singlePredicate, source, predicate, cancellationToken);
        }

        private static readonly MethodInfo _singleOrDefault = GetMethod(nameof(Queryable.SingleOrDefault));

        /// <summary>
        ///     Asynchronously returns the only element of a sequence, or a default value if the sequence is empty;
        ///     this method throws an exception if there is more than one element in the sequence.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IQueryable{T}" /> to return the single element of.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the single element of the input sequence, or <c>default</c> (
        ///     <typeparamref name="TSource" />)
        ///     if the sequence contains no elements.
        /// </returns>
        public static Task<TSource> SingleOrDefaultAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));

            return ExecuteAsync<TSource, Task<TSource>>(_singleOrDefault, source, cancellationToken);
        }

        private static readonly MethodInfo _singleOrDefaultPredicate = GetMethod(nameof(Queryable.SingleOrDefault), parameterCount: 1);

        /// <summary>
        ///     Asynchronously returns the only element of a sequence that satisfies a specified condition or
        ///     a default value if no such element exists; this method throws an exception if more than one element
        ///     satisfies the condition.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IQueryable{T}" /> to return the single element of.
        /// </param>
        /// <param name="predicate"> A function to test an element for a condition. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the single element of the input sequence that satisfies the condition in
        ///     <paramref name="predicate" />, or <c>default</c> ( <typeparamref name="TSource" /> ) if no such element is found.
        /// </returns>
        public static Task<TSource> SingleOrDefaultAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(predicate, nameof(predicate));

            return ExecuteAsync<TSource, Task<TSource>>(_singleOrDefaultPredicate, source, predicate, cancellationToken);
        }

        #endregion

        #region Min

        private static readonly MethodInfo _min = GetMethod(nameof(Queryable.Min), predicate: mi => mi.IsGenericMethod);

        /// <summary>
        ///     Asynchronously returns the minimum value of a sequence.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IQueryable{T}" /> that contains the elements to determine the minimum of.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the minimum value in the sequence.
        /// </returns>
        public static Task<TSource> MinAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));

            return ExecuteAsync<TSource, Task<TSource>>(_min, source, cancellationToken);
        }

        private static readonly MethodInfo _minSelector = GetMethod(nameof(Queryable.Min), parameterCount: 1, predicate: mi => mi.IsGenericMethod);

        /// <summary>
        ///     Asynchronously invokes a projection function on each element of a sequence and returns the minimum resulting value.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <typeparam name="TResult">
        ///     The type of the value returned by the function represented by <paramref name="selector" /> .
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IQueryable{T}" /> that contains the elements to determine the minimum of.
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the minimum value in the sequence.
        /// </returns>
        public static Task<TResult> MinAsync<TSource, TResult>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, TResult>> selector,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(selector, nameof(selector));

            return ExecuteAsync<TSource, Task<TResult>>(_minSelector, source, selector, cancellationToken);
        }

        #endregion

        #region Max

        private static readonly MethodInfo _max = GetMethod(nameof(Queryable.Max), predicate: mi => mi.IsGenericMethod);

        /// <summary>
        ///     Asynchronously returns the maximum value of a sequence.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IQueryable{T}" /> that contains the elements to determine the maximum of.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the maximum value in the sequence.
        /// </returns>
        public static Task<TSource> MaxAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));

            return ExecuteAsync<TSource, Task<TSource>>(_max, source, cancellationToken);
        }

        private static readonly MethodInfo _maxSelector = GetMethod(nameof(Queryable.Max), parameterCount: 1, predicate: mi => mi.IsGenericMethod);

        /// <summary>
        ///     Asynchronously invokes a projection function on each element of a sequence and returns the maximum resulting value.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <typeparam name="TResult">
        ///     The type of the value returned by the function represented by <paramref name="selector" /> .
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IQueryable{T}" /> that contains the elements to determine the maximum of.
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the maximum value in the sequence.
        /// </returns>
        public static Task<TResult> MaxAsync<TSource, TResult>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, TResult>> selector,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(selector, nameof(selector));

            return ExecuteAsync<TSource, Task<TResult>>(_maxSelector, source, selector, cancellationToken);
        }

        #endregion

        #region Sum

        private static readonly MethodInfo _sumDecimal = GetMethod<decimal>(nameof(Queryable.Sum));

        /// <summary>
        ///     Asynchronously computes the sum of a sequence of values.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        ///     A sequence of values to calculate the sum of.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the sum of the values in the sequence.
        /// </returns>
        public static Task<decimal> SumAsync(
            [NotNull] this IQueryable<decimal> source,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));

            return ExecuteAsync<decimal, Task<decimal>>(_sumDecimal, source, cancellationToken);
        }

        private static readonly MethodInfo _sumNullableDecimal = GetMethod<decimal?>(nameof(Queryable.Sum));

        /// <summary>
        ///     Asynchronously computes the sum of a sequence of values.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        ///     A sequence of values to calculate the sum of.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the sum of the values in the sequence.
        /// </returns>
        public static Task<decimal?> SumAsync(
            [NotNull] this IQueryable<decimal?> source,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));

            return ExecuteAsync<decimal?, Task<decimal?>>(_sumNullableDecimal, source, cancellationToken);
        }

        private static readonly MethodInfo _sumDecimalSelector = GetMethod<decimal>(nameof(Queryable.Sum), parameterCount: 1);

        /// <summary>
        ///     Asynchronously computes the sum of the sequence of values that is obtained by invoking a projection function on
        ///     each element of the input sequence.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        ///     A sequence of values of type <typeparamref name="TSource" />.
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the sum of the projected values..
        /// </returns>
        public static Task<decimal> SumAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, decimal>> selector,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(selector, nameof(selector));

            return ExecuteAsync<TSource, Task<decimal>>(_sumDecimalSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _sumNullableDecimalSelector = GetMethod<decimal?>(nameof(Queryable.Sum), parameterCount: 1);

        /// <summary>
        ///     Asynchronously computes the sum of the sequence of values that is obtained by invoking a projection function on
        ///     each element of the input sequence.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        ///     A sequence of values of type <typeparamref name="TSource" />.
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the sum of the projected values..
        /// </returns>
        public static Task<decimal?> SumAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, decimal?>> selector,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(selector, nameof(selector));

            return ExecuteAsync<TSource, Task<decimal?>>(_sumNullableDecimalSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _sumInt = GetMethod<int>(nameof(Queryable.Sum));

        /// <summary>
        ///     Asynchronously computes the sum of a sequence of values.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        ///     A sequence of values to calculate the sum of.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the sum of the values in the sequence.
        /// </returns>
        public static Task<int> SumAsync(
            [NotNull] this IQueryable<int> source,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));

            return ExecuteAsync<int, Task<int>>(_sumInt, source, cancellationToken);
        }

        private static readonly MethodInfo _sumNullableInt = GetMethod<int?>(nameof(Queryable.Sum));

        /// <summary>
        ///     Asynchronously computes the sum of a sequence of values.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        ///     A sequence of values to calculate the sum of.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the sum of the values in the sequence.
        /// </returns>
        public static Task<int?> SumAsync(
            [NotNull] this IQueryable<int?> source,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));

            return ExecuteAsync<int?, Task<int?>>(_sumNullableInt, source, cancellationToken);
        }

        private static readonly MethodInfo _sumIntSelector = GetMethod<int>(nameof(Queryable.Sum), parameterCount: 1);

        /// <summary>
        ///     Asynchronously computes the sum of the sequence of values that is obtained by invoking a projection function on
        ///     each element of the input sequence.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        ///     A sequence of values of type <typeparamref name="TSource" />.
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the sum of the projected values..
        /// </returns>
        public static Task<int> SumAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, int>> selector,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(selector, nameof(selector));

            return ExecuteAsync<TSource, Task<int>>(_sumIntSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _sumNullableIntSelector = GetMethod<int?>(nameof(Queryable.Sum), parameterCount: 1);

        /// <summary>
        ///     Asynchronously computes the sum of the sequence of values that is obtained by invoking a projection function on
        ///     each element of the input sequence.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        ///     A sequence of values of type <typeparamref name="TSource" />.
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the sum of the projected values..
        /// </returns>
        public static Task<int?> SumAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, int?>> selector,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(selector, nameof(selector));

            return ExecuteAsync<TSource, Task<int?>>(_sumNullableIntSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _sumLong = GetMethod<long>(nameof(Queryable.Sum));

        /// <summary>
        ///     Asynchronously computes the sum of a sequence of values.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        ///     A sequence of values to calculate the sum of.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the sum of the values in the sequence.
        /// </returns>
        public static Task<long> SumAsync(
            [NotNull] this IQueryable<long> source,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));

            return ExecuteAsync<long, Task<long>>(_sumLong, source, cancellationToken);
        }

        private static readonly MethodInfo _sumNullableLong = GetMethod<long?>(nameof(Queryable.Sum));

        /// <summary>
        ///     Asynchronously computes the sum of a sequence of values.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        ///     A sequence of values to calculate the sum of.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the sum of the values in the sequence.
        /// </returns>
        public static Task<long?> SumAsync(
            [NotNull] this IQueryable<long?> source,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));

            return ExecuteAsync<long?, Task<long?>>(_sumNullableLong, source, cancellationToken);
        }

        private static readonly MethodInfo _sumLongSelector = GetMethod<long>(nameof(Queryable.Sum), parameterCount: 1);

        /// <summary>
        ///     Asynchronously computes the sum of the sequence of values that is obtained by invoking a projection function on
        ///     each element of the input sequence.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        ///     A sequence of values of type <typeparamref name="TSource" />.
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the sum of the projected values..
        /// </returns>
        public static Task<long> SumAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, long>> selector,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(selector, nameof(selector));

            return ExecuteAsync<TSource, Task<long>>(_sumLongSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _sumNullableLongSelector = GetMethod<long?>(nameof(Queryable.Sum), parameterCount: 1);

        /// <summary>
        ///     Asynchronously computes the sum of the sequence of values that is obtained by invoking a projection function on
        ///     each element of the input sequence.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        ///     A sequence of values of type <typeparamref name="TSource" />.
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the sum of the projected values..
        /// </returns>
        public static Task<long?> SumAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, long?>> selector,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(selector, nameof(selector));

            return ExecuteAsync<TSource, Task<long?>>(_sumNullableLongSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _sumDouble = GetMethod<double>(nameof(Queryable.Sum));

        /// <summary>
        ///     Asynchronously computes the sum of a sequence of values.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        ///     A sequence of values to calculate the sum of.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the sum of the values in the sequence.
        /// </returns>
        public static Task<double> SumAsync(
            [NotNull] this IQueryable<double> source,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));

            return ExecuteAsync<double, Task<double>>(_sumDouble, source, cancellationToken);
        }

        private static readonly MethodInfo _sumNullableDouble = GetMethod<double?>(nameof(Queryable.Sum));

        /// <summary>
        ///     Asynchronously computes the sum of a sequence of values.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        ///     A sequence of values to calculate the sum of.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the sum of the values in the sequence.
        /// </returns>
        public static Task<double?> SumAsync(
            [NotNull] this IQueryable<double?> source,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));

            return ExecuteAsync<double?, Task<double?>>(_sumNullableDouble, source, cancellationToken);
        }

        private static readonly MethodInfo _sumDoubleSelector = GetMethod<double>(nameof(Queryable.Sum), parameterCount: 1);

        /// <summary>
        ///     Asynchronously computes the sum of the sequence of values that is obtained by invoking a projection function on
        ///     each element of the input sequence.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        ///     A sequence of values of type <typeparamref name="TSource" />.
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the sum of the projected values..
        /// </returns>
        public static Task<double> SumAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, double>> selector,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(selector, nameof(selector));

            return ExecuteAsync<TSource, Task<double>>(_sumDoubleSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _sumNullableDoubleSelector = GetMethod<double?>(nameof(Queryable.Sum), parameterCount: 1);

        /// <summary>
        ///     Asynchronously computes the sum of the sequence of values that is obtained by invoking a projection function on
        ///     each element of the input sequence.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        ///     A sequence of values of type <typeparamref name="TSource" />.
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the sum of the projected values..
        /// </returns>
        public static Task<double?> SumAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, double?>> selector,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(selector, nameof(selector));

            return ExecuteAsync<TSource, Task<double?>>(_sumNullableDoubleSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _sumFloat = GetMethod<float>(nameof(Queryable.Sum));

        /// <summary>
        ///     Asynchronously computes the sum of a sequence of values.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        ///     A sequence of values to calculate the sum of.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the sum of the values in the sequence.
        /// </returns>
        public static Task<float> SumAsync(
            [NotNull] this IQueryable<float> source,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));

            return ExecuteAsync<float, Task<float>>(_sumFloat, source, cancellationToken);
        }

        private static readonly MethodInfo _sumNullableFloat = GetMethod<float?>(nameof(Queryable.Sum));

        /// <summary>
        ///     Asynchronously computes the sum of a sequence of values.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        ///     A sequence of values to calculate the sum of.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the sum of the values in the sequence.
        /// </returns>
        public static Task<float?> SumAsync(
            [NotNull] this IQueryable<float?> source,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));

            return ExecuteAsync<float?, Task<float?>>(_sumNullableFloat, source, cancellationToken);
        }

        private static readonly MethodInfo _sumFloatSelector = GetMethod<float>(nameof(Queryable.Sum), parameterCount: 1);

        /// <summary>
        ///     Asynchronously computes the sum of the sequence of values that is obtained by invoking a projection function on
        ///     each element of the input sequence.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        ///     A sequence of values of type <typeparamref name="TSource" />.
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the sum of the projected values..
        /// </returns>
        public static Task<float> SumAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, float>> selector,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(selector, nameof(selector));

            return ExecuteAsync<TSource, Task<float>>(_sumFloatSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _sumNullableFloatSelector = GetMethod<float?>(nameof(Queryable.Sum), parameterCount: 1);

        /// <summary>
        ///     Asynchronously computes the sum of the sequence of values that is obtained by invoking a projection function on
        ///     each element of the input sequence.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        ///     A sequence of values of type <typeparamref name="TSource" />.
        /// </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the sum of the projected values..
        /// </returns>
        public static Task<float?> SumAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, float?>> selector,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(selector, nameof(selector));

            return ExecuteAsync<TSource, Task<float?>>(_sumNullableFloatSelector, source, selector, cancellationToken);
        }

        #endregion

        #region Average

        private static MethodInfo GetAverageMethod<TOperand, TResult>(int parameterCount = 0)
            => GetMethod<TResult>(
                nameof(Queryable.Average),
                parameterCount,
                mi => parameterCount == 0
                      && mi.GetParameters()[0].ParameterType == typeof(IQueryable<TOperand>)
                      || mi.GetParameters().Length == 2
                      && mi.GetParameters()[1]
                          .ParameterType.GenericTypeArguments[0]
                          .GenericTypeArguments[1] == typeof(TOperand));

        private static readonly MethodInfo _averageDecimal = GetAverageMethod<decimal, decimal>();

        /// <summary>
        ///     Asynchronously computes the average of a sequence of values.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        ///     A sequence of values to calculate the average of.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the average of the sequence of values.
        /// </returns>
        public static Task<decimal> AverageAsync(
            [NotNull] this IQueryable<decimal> source,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));

            return ExecuteAsync<decimal, Task<decimal>>(_averageDecimal, source, cancellationToken);
        }

        private static readonly MethodInfo _averageNullableDecimal = GetAverageMethod<decimal?, decimal?>();

        /// <summary>
        ///     Asynchronously computes the average of a sequence of values.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        ///     A sequence of values to calculate the average of.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the average of the sequence of values.
        /// </returns>
        public static Task<decimal?> AverageAsync(
            [NotNull] this IQueryable<decimal?> source,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));

            return ExecuteAsync<decimal?, Task<decimal?>>(_averageNullableDecimal, source, cancellationToken);
        }

        private static readonly MethodInfo _averageDecimalSelector = GetAverageMethod<decimal, decimal>(parameterCount: 1);

        /// <summary>
        ///     Asynchronously computes the average of a sequence of values that is obtained
        ///     by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source"> A sequence of values of type <typeparamref name="TSource" />. </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the average of the projected values.
        /// </returns>
        public static Task<decimal> AverageAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, decimal>> selector,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(selector, nameof(selector));

            return ExecuteAsync<TSource, Task<decimal>>(_averageDecimalSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _averageNullableDecimalSelector = GetAverageMethod<decimal?, decimal?>(parameterCount: 1);

        /// <summary>
        ///     Asynchronously computes the average of a sequence of values that is obtained
        ///     by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source"> A sequence of values of type <typeparamref name="TSource" />. </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the average of the projected values.
        /// </returns>
        public static Task<decimal?> AverageAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, decimal?>> selector,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(selector, nameof(selector));

            return ExecuteAsync<TSource, Task<decimal?>>(_averageNullableDecimalSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _averageInt = GetAverageMethod<int, double>();

        /// <summary>
        ///     Asynchronously computes the average of a sequence of values.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        ///     A sequence of values to calculate the average of.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the average of the sequence of values.
        /// </returns>
        public static Task<double> AverageAsync(
            [NotNull] this IQueryable<int> source,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));

            return ExecuteAsync<int, Task<double>>(_averageInt, source, cancellationToken);
        }

        private static readonly MethodInfo _averageNullableInt = GetAverageMethod<int?, double?>();

        /// <summary>
        ///     Asynchronously computes the average of a sequence of values.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        ///     A sequence of values to calculate the average of.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the average of the sequence of values.
        /// </returns>
        public static Task<double?> AverageAsync(
            [NotNull] this IQueryable<int?> source,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));

            return ExecuteAsync<int?, Task<double?>>(_averageNullableInt, source, cancellationToken);
        }

        private static readonly MethodInfo _averageIntSelector = GetAverageMethod<int, double>(parameterCount: 1);

        /// <summary>
        ///     Asynchronously computes the average of a sequence of values that is obtained
        ///     by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source"> A sequence of values of type <typeparamref name="TSource" />. </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the average of the projected values.
        /// </returns>
        public static Task<double> AverageAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, int>> selector,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(selector, nameof(selector));

            return ExecuteAsync<TSource, Task<double>>(_averageIntSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _averageNullableIntSelector = GetAverageMethod<int?, double?>(parameterCount: 1);

        /// <summary>
        ///     Asynchronously computes the average of a sequence of values that is obtained
        ///     by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source"> A sequence of values of type <typeparamref name="TSource" />. </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the average of the projected values.
        /// </returns>
        public static Task<double?> AverageAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, int?>> selector,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(selector, nameof(selector));

            return ExecuteAsync<TSource, Task<double?>>(_averageNullableIntSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _averageLong = GetAverageMethod<long, double>();

        /// <summary>
        ///     Asynchronously computes the average of a sequence of values.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        ///     A sequence of values to calculate the average of.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the average of the sequence of values.
        /// </returns>
        public static Task<double> AverageAsync(
            [NotNull] this IQueryable<long> source,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));

            return ExecuteAsync<long, Task<double>>(_averageLong, source, cancellationToken);
        }

        private static readonly MethodInfo _averageNullableLong = GetAverageMethod<long?, double?>();

        /// <summary>
        ///     Asynchronously computes the average of a sequence of values.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        ///     A sequence of values to calculate the average of.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the average of the sequence of values.
        /// </returns>
        public static Task<double?> AverageAsync(
            [NotNull] this IQueryable<long?> source,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));

            return ExecuteAsync<long?, Task<double?>>(_averageNullableLong, source, cancellationToken);
        }

        private static readonly MethodInfo _averageLongSelector = GetAverageMethod<long, double>(parameterCount: 1);

        /// <summary>
        ///     Asynchronously computes the average of a sequence of values that is obtained
        ///     by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source"> A sequence of values of type <typeparamref name="TSource" />. </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the average of the projected values.
        /// </returns>
        public static Task<double> AverageAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, long>> selector,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(selector, nameof(selector));

            return ExecuteAsync<TSource, Task<double>>(_averageLongSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _averageNullableLongSelector = GetAverageMethod<long?, double?>(parameterCount: 1);

        /// <summary>
        ///     Asynchronously computes the average of a sequence of values that is obtained
        ///     by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source"> A sequence of values of type <typeparamref name="TSource" />. </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the average of the projected values.
        /// </returns>
        public static Task<double?> AverageAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, long?>> selector,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(selector, nameof(selector));

            return ExecuteAsync<TSource, Task<double?>>(_averageNullableLongSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _averageDouble = GetAverageMethod<double, double>();

        /// <summary>
        ///     Asynchronously computes the average of a sequence of values.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        ///     A sequence of values to calculate the average of.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the average of the sequence of values.
        /// </returns>
        public static Task<double> AverageAsync(
            [NotNull] this IQueryable<double> source,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));

            return ExecuteAsync<double, Task<double>>(_averageDouble, source, cancellationToken);
        }

        private static readonly MethodInfo _averageNullableDouble = GetAverageMethod<double?, double?>();

        /// <summary>
        ///     Asynchronously computes the average of a sequence of values.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        ///     A sequence of values to calculate the average of.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the average of the sequence of values.
        /// </returns>
        public static Task<double?> AverageAsync(
            [NotNull] this IQueryable<double?> source,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));

            return ExecuteAsync<double?, Task<double?>>(_averageNullableDouble, source, cancellationToken);
        }

        private static readonly MethodInfo _averageDoubleSelector = GetAverageMethod<double, double>(parameterCount: 1);

        /// <summary>
        ///     Asynchronously computes the average of a sequence of values that is obtained
        ///     by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source"> A sequence of values of type <typeparamref name="TSource" />. </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the average of the projected values.
        /// </returns>
        public static Task<double> AverageAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, double>> selector,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(selector, nameof(selector));

            return ExecuteAsync<TSource, Task<double>>(_averageDoubleSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _averageNullableDoubleSelector = GetAverageMethod<double?, double?>(parameterCount: 1);

        /// <summary>
        ///     Asynchronously computes the average of a sequence of values that is obtained
        ///     by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source"> A sequence of values of type <typeparamref name="TSource" />. </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the average of the projected values.
        /// </returns>
        public static Task<double?> AverageAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, double?>> selector,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(selector, nameof(selector));

            return ExecuteAsync<TSource, Task<double?>>(_averageNullableDoubleSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _averageFloat = GetAverageMethod<float, float>();

        /// <summary>
        ///     Asynchronously computes the average of a sequence of values.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        ///     A sequence of values to calculate the average of.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the average of the sequence of values.
        /// </returns>
        public static Task<float> AverageAsync(
            [NotNull] this IQueryable<float> source,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));

            return ExecuteAsync<float, Task<float>>(_averageFloat, source, cancellationToken);
        }

        private static readonly MethodInfo _averageNullableFloat = GetAverageMethod<float?, float?>();

        /// <summary>
        ///     Asynchronously computes the average of a sequence of values.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="source">
        ///     A sequence of values to calculate the average of.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the average of the sequence of values.
        /// </returns>
        public static Task<float?> AverageAsync(
            [NotNull] this IQueryable<float?> source,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));

            return ExecuteAsync<float?, Task<float?>>(_averageNullableFloat, source, cancellationToken);
        }

        private static readonly MethodInfo _averageFloatSelector = GetAverageMethod<float, float>(parameterCount: 1);

        /// <summary>
        ///     Asynchronously computes the average of a sequence of values that is obtained
        ///     by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source"> A sequence of values of type <typeparamref name="TSource" />. </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the average of the projected values.
        /// </returns>
        public static Task<float> AverageAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, float>> selector,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(selector, nameof(selector));

            return ExecuteAsync<TSource, Task<float>>(_averageFloatSelector, source, selector, cancellationToken);
        }

        private static readonly MethodInfo _averageNullableFloatSelector = GetAverageMethod<float?, float?>(parameterCount: 1);

        /// <summary>
        ///     Asynchronously computes the average of a sequence of values that is obtained
        ///     by invoking a projection function on each element of the input sequence.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" /> .
        /// </typeparam>
        /// <param name="source"> A sequence of values of type <typeparamref name="TSource" />. </param>
        /// <param name="selector"> A projection function to apply to each element. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains the average of the projected values.
        /// </returns>
        public static Task<float?> AverageAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, float?>> selector,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(selector, nameof(selector));

            return ExecuteAsync<TSource, Task<float?>>(_averageNullableFloatSelector, source, selector, cancellationToken);
        }

        #endregion

        #region Contains

        private static readonly MethodInfo _contains = GetMethod(nameof(Queryable.Contains), parameterCount: 1);

        /// <summary>
        ///     Asynchronously determines whether a sequence contains a specified element by using the default equality comparer.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IQueryable{T}" /> to return the single element of.
        /// </param>
        /// <param name="item"> The object to locate in the sequence. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains <c>true</c> if the input sequence contains the specified value; otherwise, <c>false</c>.
        /// </returns>
        public static Task<bool> ContainsAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] TSource item,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));

            return ExecuteAsync<TSource, Task<bool>>(
                _contains,
                source,
                Expression.Constant(item, typeof(TSource)),
                cancellationToken);
        }

        #endregion

        #region ToList/Array

        /// <summary>
        ///     Asynchronously creates a <see cref="List{T}" /> from an <see cref="IQueryable{T}" /> by enumerating it
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
        ///     An <see cref="IQueryable{T}" /> to create a list from.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains a <see cref="List{T}" /> that contains elements from the input sequence.
        /// </returns>
        public static async Task<List<TSource>> ToListAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            CancellationToken cancellationToken = default)
        {
            var list = new List<TSource>();
            await foreach (var element in source.AsAsyncEnumerable().WithCancellation(cancellationToken))
            {
                list.Add(element);
            }

            return list;
        }

        /// <summary>
        ///     Asynchronously creates an array from an <see cref="IQueryable{T}" /> by enumerating it asynchronously.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IQueryable{T}" /> to create an array from.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains an array that contains elements from the input sequence.
        /// </returns>
        public static async Task<TSource[]> ToArrayAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            CancellationToken cancellationToken = default)
            => (await source.ToListAsync(cancellationToken)).ToArray();

        #endregion

        #region Include

        internal static readonly MethodInfo IncludeMethodInfo
            = typeof(EntityFrameworkQueryableExtensions)
                .GetTypeInfo().GetDeclaredMethods(nameof(Include))
                .Single(
                    mi =>
                        mi.GetGenericArguments().Count() == 2
                        && mi.GetParameters().Any(
                            pi => pi.Name == "navigationPropertyPath" && pi.ParameterType != typeof(string)));

        /// <summary>
        ///     Specifies related entities to include in the query results. The navigation property to be included is specified starting with the
        ///     type of entity being queried (<typeparamref name="TEntity" />). If you wish to include additional types based on the navigation
        ///     properties of the type being included, then chain a call to
        ///     <see
        ///         cref="ThenInclude{TEntity, TPreviousProperty, TProperty}(IIncludableQueryable{TEntity, IEnumerable{TPreviousProperty}}, Expression{Func{TPreviousProperty, TProperty}})" />
        ///     after this call.
        /// </summary>
        /// <example>
        ///     <para>
        ///         The following query shows including a single level of related entities.
        ///         <code>
        ///             context.Blogs.Include(blog => blog.Posts);
        ///         </code>
        ///     </para>
        ///     <para>
        ///         The following query shows including two levels of entities on the same branch.
        ///         <code>
        ///             context.Blogs
        ///                 .Include(blog => blog.Posts).ThenInclude(post => post.Tags);
        ///         </code>
        ///     </para>
        ///     <para>
        ///         The following query shows including multiple levels and branches of related data.
        ///         <code>
        ///             context.Blogs
        ///                 .Include(blog => blog.Posts).ThenInclude(post => post.Tags).ThenInclude(tag => tag.TagInfo)
        ///                 .Include(blog => blog.Contributors);
        ///         </code>
        ///     </para>
        ///     <para>
        ///         The following query shows including a single level of related entities on a derived type using casting.
        ///         <code>
        ///             context.Blogs.Include(blog => ((SpecialBlog)blog).SpecialPosts);
        ///         </code>
        ///     </para>
        ///     <para>
        ///         The following query shows including a single level of related entities on a derived type using 'as' operator.
        ///         <code>
        ///             context.Blogs.Include(blog => (blog as SpecialBlog).SpecialPosts);
        ///         </code>
        ///     </para>
        /// </example>
        /// <typeparam name="TEntity"> The type of entity being queried. </typeparam>
        /// <typeparam name="TProperty"> The type of the related entity to be included. </typeparam>
        /// <param name="source"> The source query. </param>
        /// <param name="navigationPropertyPath">
        ///     A lambda expression representing the navigation property to be included (<c>t => t.Property1</c>).
        /// </param>
        /// <returns>
        ///     A new query with the related data included.
        /// </returns>
        public static IIncludableQueryable<TEntity, TProperty> Include<TEntity, TProperty>(
            [NotNull] this IQueryable<TEntity> source,
            [NotNull] Expression<Func<TEntity, TProperty>> navigationPropertyPath)
            where TEntity : class
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(navigationPropertyPath, nameof(navigationPropertyPath));

            return new IncludableQueryable<TEntity, TProperty>(
                source.Provider is EntityQueryProvider
                    ? source.Provider.CreateQuery<TEntity>(
                        Expression.Call(
                            instance: null,
                            method: IncludeMethodInfo.MakeGenericMethod(typeof(TEntity), typeof(TProperty)),
                            arguments: new[] { source.Expression, Expression.Quote(navigationPropertyPath) }))
                    : source);
        }

        internal static readonly MethodInfo ThenIncludeAfterEnumerableMethodInfo
            = GetThenIncludeMethodInfo(typeof(IEnumerable<>));

        private static MethodInfo GetThenIncludeMethodInfo(Type navType)
            => typeof(EntityFrameworkQueryableExtensions)
                .GetTypeInfo().GetDeclaredMethods(nameof(ThenInclude))
                .Where(mi => mi.GetGenericArguments().Count() == 3)
                .Single(
                    mi =>
                    {
                        var typeInfo = mi.GetParameters()[0].ParameterType.GenericTypeArguments[1].GetTypeInfo();
                        return typeInfo.IsGenericType
                               && typeInfo.GetGenericTypeDefinition() == navType;
                    });

        internal static readonly MethodInfo ThenIncludeAfterReferenceMethodInfo
            = typeof(EntityFrameworkQueryableExtensions)
                .GetTypeInfo().GetDeclaredMethods(nameof(ThenInclude))
                .Single(
                    mi => mi.GetGenericArguments().Count() == 3
                          && mi.GetParameters()[0].ParameterType.GenericTypeArguments[1].IsGenericParameter);

        /// <summary>
        ///     Specifies additional related data to be further included based on a related type that was just included.
        /// </summary>
        /// <example>
        ///     <para>
        ///         The following query shows including a single level of related entities.
        ///         <code>
        ///             context.Blogs.Include(blog => blog.Posts);
        ///         </code>
        ///     </para>
        ///     <para>
        ///         The following query shows including two levels of entities on the same branch.
        ///         <code>
        ///             context.Blogs
        ///                 .Include(blog => blog.Posts).ThenInclude(post => post.Tags);
        ///         </code>
        ///     </para>
        ///     <para>
        ///         The following query shows including multiple levels and branches of related data.
        ///         <code>
        ///             context.Blogs
        ///                 .Include(blog => blog.Posts).ThenInclude(post => post.Tags).ThenInclude(tag => tag.TagInfo)
        ///                 .Include(blog => blog.Contributors);
        ///         </code>
        ///     </para>
        ///     <para>
        ///         The following query shows including two levels of entities on the same branch, second one being on derived type using casting.
        ///         <code>
        ///             context.Blogs
        ///                 .Include(blog => blog.Posts).ThenInclude(post => ((SpecialPost)post).SpecialTags);
        ///         </code>
        ///     </para>
        ///     <para>
        ///         The following query shows including two levels of entities on the same branch, second one being on derived type using 'as'
        ///         operator.
        ///         <code>
        ///             context.Blogs
        ///                 .Include(blog => blog.Posts).ThenInclude(post => (post as SpecialPost).SpecialTags);
        ///         </code>
        ///     </para>
        /// </example>
        /// <typeparam name="TEntity"> The type of entity being queried. </typeparam>
        /// <typeparam name="TPreviousProperty"> The type of the entity that was just included. </typeparam>
        /// <typeparam name="TProperty"> The type of the related entity to be included. </typeparam>
        /// <param name="source"> The source query. </param>
        /// <param name="navigationPropertyPath">
        ///     A lambda expression representing the navigation property to be included (<c>t => t.Property1</c>).
        /// </param>
        /// <returns>
        ///     A new query with the related data included.
        /// </returns>
        public static IIncludableQueryable<TEntity, TProperty> ThenInclude<TEntity, TPreviousProperty, TProperty>(
            [NotNull] this IIncludableQueryable<TEntity, IEnumerable<TPreviousProperty>> source,
            [NotNull] Expression<Func<TPreviousProperty, TProperty>> navigationPropertyPath)
            where TEntity : class
            => new IncludableQueryable<TEntity, TProperty>(
                source.Provider is EntityQueryProvider
                    ? source.Provider.CreateQuery<TEntity>(
                        Expression.Call(
                            instance: null,
                            method: ThenIncludeAfterEnumerableMethodInfo.MakeGenericMethod(typeof(TEntity), typeof(TPreviousProperty), typeof(TProperty)),
                            arguments: new[] { source.Expression, Expression.Quote(navigationPropertyPath) }))
                    : source);

        /// <summary>
        ///     Specifies additional related data to be further included based on a related type that was just included.
        /// </summary>
        /// <example>
        ///     <para>
        ///         The following query shows including a single level of related entities.
        ///         <code>
        ///             context.Blogs.Include(blog => blog.Posts);
        ///         </code>
        ///     </para>
        ///     <para>
        ///         The following query shows including two levels of entities on the same branch.
        ///         <code>
        ///             context.Blogs
        ///                 .Include(blog => blog.Posts).ThenInclude(post => post.Tags);
        ///         </code>
        ///     </para>
        ///     <para>
        ///         The following query shows including multiple levels and branches of related data.
        ///         <code>
        ///             context.Blogs
        ///                 .Include(blog => blog.Posts).ThenInclude(post => post.Tags).ThenInclude(tag => tag.TagInfo)
        ///                 .Include(blog => blog.Contributors);
        ///         </code>
        ///     </para>
        ///     <para>
        ///         The following query shows including two levels of entities on the same branch, second one being on derived type.
        ///         <code>
        ///             context.Blogs
        ///                 .Include(blog => blog.Posts).ThenInclude(post => ((SpecialPost)post).SpecialTags);
        ///         </code>
        ///     </para>
        ///     <para>
        ///         The following query shows including two levels of entities on the same branch, second one being on derived type using alternative
        ///         method.
        ///         <code>
        ///             context.Blogs
        ///                 .Include(blog => blog.Posts).ThenInclude(post => (post as SpecialPost).SpecialTags);
        ///         </code>
        ///     </para>
        /// </example>
        /// <typeparam name="TEntity"> The type of entity being queried. </typeparam>
        /// <typeparam name="TPreviousProperty"> The type of the entity that was just included. </typeparam>
        /// <typeparam name="TProperty"> The type of the related entity to be included. </typeparam>
        /// <param name="source"> The source query. </param>
        /// <param name="navigationPropertyPath">
        ///     A lambda expression representing the navigation property to be included (<c>t => t.Property1</c>).
        /// </param>
        /// <returns>
        ///     A new query with the related data included.
        /// </returns>
        public static IIncludableQueryable<TEntity, TProperty> ThenInclude<TEntity, TPreviousProperty, TProperty>(
            [NotNull] this IIncludableQueryable<TEntity, TPreviousProperty> source,
            [NotNull] Expression<Func<TPreviousProperty, TProperty>> navigationPropertyPath)
            where TEntity : class
            => new IncludableQueryable<TEntity, TProperty>(
                source.Provider is EntityQueryProvider
                    ? source.Provider.CreateQuery<TEntity>(
                        Expression.Call(
                            instance: null,
                            method: ThenIncludeAfterReferenceMethodInfo.MakeGenericMethod(typeof(TEntity), typeof(TPreviousProperty), typeof(TProperty)),
                            arguments: new[] { source.Expression, Expression.Quote(navigationPropertyPath) }))
                    : source);

        private class IncludableQueryable<TEntity, TProperty> : IIncludableQueryable<TEntity, TProperty>, IAsyncEnumerable<TEntity>
        {
            private readonly IQueryable<TEntity> _queryable;

            public IncludableQueryable(IQueryable<TEntity> queryable)
            {
                _queryable = queryable;
            }

            public Expression Expression => _queryable.Expression;
            public Type ElementType => _queryable.ElementType;
            public IQueryProvider Provider => _queryable.Provider;

            public IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken cancellationToken = default)
                => ((IAsyncEnumerable<TEntity>)_queryable).GetAsyncEnumerator(cancellationToken);

            public IEnumerator<TEntity> GetEnumerator() => _queryable.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        internal static readonly MethodInfo StringIncludeMethodInfo
            = typeof(EntityFrameworkQueryableExtensions)
                .GetTypeInfo().GetDeclaredMethods(nameof(Include))
                .Single(
                    mi => mi.GetParameters().Any(
                        pi => pi.Name == "navigationPropertyPath" && pi.ParameterType == typeof(string)));

        /// <summary>
        ///     Specifies related entities to include in the query results. The navigation property to be included is
        ///     specified starting with the type of entity being queried (<typeparamref name="TEntity" />). Further
        ///     navigation properties to be included can be appended, separated by the '.' character.
        /// </summary>
        /// <example>
        ///     <para>
        ///         The following query shows including a single level of related entities.
        ///         <code>
        ///             context.Blogs.Include("Posts");
        ///         </code>
        ///     </para>
        ///     <para>
        ///         The following query shows including two levels of entities on the same branch.
        ///         <code>
        ///             context.Blogs.Include("Posts.Tags");
        ///         </code>
        ///     </para>
        ///     <para>
        ///         The following query shows including multiple levels and branches of related data.
        ///         <code>
        ///             context.Blogs
        ///                 .Include("Posts.Tags.TagInfo')
        ///                 .Include("Contributors");
        ///         </code>
        ///     </para>
        /// </example>
        /// <typeparam name="TEntity"> The type of entity being queried. </typeparam>
        /// <param name="source"> The source query. </param>
        /// <param name="navigationPropertyPath"> A string of '.' separated navigation property names to be included.  </param>
        /// <returns> A new query with the related data included. </returns>
        public static IQueryable<TEntity> Include<TEntity>(
            [NotNull] this IQueryable<TEntity> source,
            [NotNull] [NotParameterized] string navigationPropertyPath)
            where TEntity : class
        {
            Check.NotNull(source, nameof(source));
            Check.NotEmpty(navigationPropertyPath, nameof(navigationPropertyPath));

            return
                source.Provider is EntityQueryProvider
                    ? source.Provider.CreateQuery<TEntity>(
                        Expression.Call(
                            instance: null,
                            method: StringIncludeMethodInfo.MakeGenericMethod(typeof(TEntity)),
                            arg0: source.Expression,
                            arg1: Expression.Constant(navigationPropertyPath)))
                    : source;
        }

        #endregion

        #region Query Filters

        internal static readonly MethodInfo IgnoreQueryFiltersMethodInfo
            = typeof(EntityFrameworkQueryableExtensions)
                .GetTypeInfo().GetDeclaredMethod(nameof(IgnoreQueryFilters));

        /// <summary>
        ///     Specifies that the current Entity Framework LINQ query should not have any
        ///     model-level entity query filters applied.
        /// </summary>
        /// <typeparam name="TEntity"> The type of entity being queried. </typeparam>
        /// <param name="source"> The source query. </param>
        /// <returns>
        ///     A new query that will not apply any model-level entity query filters.
        /// </returns>
        public static IQueryable<TEntity> IgnoreQueryFilters<TEntity>(
            [NotNull] this IQueryable<TEntity> source)
            where TEntity : class
        {
            Check.NotNull(source, nameof(source));

            return
                source.Provider is EntityQueryProvider
                    ? source.Provider.CreateQuery<TEntity>(
                        Expression.Call(
                            instance: null,
                            method: IgnoreQueryFiltersMethodInfo.MakeGenericMethod(typeof(TEntity)),
                            arguments: source.Expression))
                    : source;
        }

        #endregion

        #region Tracking

        internal static readonly MethodInfo AsNoTrackingMethodInfo
            = typeof(EntityFrameworkQueryableExtensions)
                .GetTypeInfo().GetDeclaredMethod(nameof(AsNoTracking));

        /// <summary>
        ///     <para>
        ///         Returns a new query where the change tracker will not track any of the entities that are returned.
        ///         If the entity instances are modified, this will not be detected by the change tracker and
        ///         <see cref="DbContext.SaveChanges()" /> will not persist those changes to the database.
        ///     </para>
        ///     <para>
        ///         Disabling change tracking is useful for read-only scenarios because it avoids the overhead of setting
        ///         up change tracking for each entity instance. You should not disable change tracking if you want to
        ///         manipulate entity instances and persist those changes to the database using
        ///         <see cref="DbContext.SaveChanges()" />.
        ///     </para>
        ///     <para>
        ///         The default tracking behavior for queries can be controlled by <see cref="ChangeTracker.QueryTrackingBehavior" />.
        ///     </para>
        /// </summary>
        /// <typeparam name="TEntity"> The type of entity being queried. </typeparam>
        /// <param name="source"> The source query. </param>
        /// <returns>
        ///     A new query where the result set will not be tracked by the context.
        /// </returns>
        public static IQueryable<TEntity> AsNoTracking<TEntity>(
            [NotNull] this IQueryable<TEntity> source)
            where TEntity : class
        {
            Check.NotNull(source, nameof(source));

            return
                source.Provider is EntityQueryProvider
                    ? source.Provider.CreateQuery<TEntity>(
                        Expression.Call(
                            instance: null,
                            method: AsNoTrackingMethodInfo.MakeGenericMethod(typeof(TEntity)),
                            arguments: source.Expression))
                    : source;
        }

        internal static readonly MethodInfo AsTrackingMethodInfo
            = typeof(EntityFrameworkQueryableExtensions)
                .GetTypeInfo()
                .GetDeclaredMethods(nameof(AsTracking))
                .Single(m => m.GetParameters().Length == 1);

        /// <summary>
        ///     <para>
        ///         Returns a new query where the change tracker will keep track of changes for all entities that are returned.
        ///         Any modification to the entity instances will be detected and persisted to the database during
        ///         <see cref="DbContext.SaveChanges()" />.
        ///     </para>
        ///     <para>
        ///         The default tracking behavior for queries can be controlled by <see cref="ChangeTracker.QueryTrackingBehavior" />.
        ///     </para>
        /// </summary>
        /// <typeparam name="TEntity"> The type of entity being queried. </typeparam>
        /// <param name="source"> The source query. </param>
        /// <returns>
        ///     A new query where the result set will be tracked by the context.
        /// </returns>
        public static IQueryable<TEntity> AsTracking<TEntity>(
            [NotNull] this IQueryable<TEntity> source)
            where TEntity : class
        {
            Check.NotNull(source, nameof(source));

            return
                source.Provider is EntityQueryProvider
                    ? source.Provider.CreateQuery<TEntity>(
                        Expression.Call(
                            instance: null,
                            method: AsTrackingMethodInfo.MakeGenericMethod(typeof(TEntity)),
                            arguments: source.Expression))
                    : source;
        }

        /// <summary>
        ///     <para>
        ///         Returns a new query where the change tracker will either keep track of changes or not for all entities
        ///         that are returned, depending on the value of the 'track' parameter. When tracking, Any modification
        ///         to the entity instances will be detected and persisted to the database during
        ///         <see cref="DbContext.SaveChanges()" />. When not tracking, if the entity instances are modified, this will
        ///         not be detected by the change tracker and <see cref="DbContext.SaveChanges()" /> will not persist those
        ///         changes to the database.
        ///     </para>
        ///     <para>
        ///         Disabling change tracking is useful for read-only scenarios because it avoids the overhead of setting
        ///         up change tracking for each entity instance. You should not disable change tracking if you want to
        ///         manipulate entity instances and persist those changes to the database using
        ///         <see cref="DbContext.SaveChanges()" />.
        ///     </para>
        ///     <para>
        ///         The default tracking behavior for queries can be controlled by <see cref="ChangeTracker.QueryTrackingBehavior" />.
        ///     </para>
        /// </summary>
        /// <typeparam name="TEntity"> The type of entity being queried. </typeparam>
        /// <param name="source"> The source query. </param>
        /// <param name="track"> Indicates whether the query will track results or not. </param>
        /// <returns>
        ///     A new query where the result set will be tracked by the context.
        /// </returns>
        public static IQueryable<TEntity> AsTracking<TEntity>(
            [NotNull] this IQueryable<TEntity> source,
            QueryTrackingBehavior track)
            where TEntity : class
            => track == QueryTrackingBehavior.TrackAll
                ? source.AsTracking()
                : source.AsNoTracking();

        #endregion

        #region Tagging

        internal static readonly MethodInfo TagWithMethodInfo
            = typeof(EntityFrameworkQueryableExtensions)
                .GetTypeInfo().GetDeclaredMethod(nameof(TagWith));

        /// <summary>
        ///     Adds a tag to the collection of tags associated with an EF LINQ query. Tags are query annotations
        ///     that can provide contextual tracing information at different points in the query pipeline.
        /// </summary>
        /// <typeparam name="T"> The type of entity being queried. </typeparam>
        /// <param name="source"> The source query. </param>
        /// <param name="tag"> The tag. </param>
        /// <returns>
        ///     A new query annotated with the given tag.
        /// </returns>
        public static IQueryable<T> TagWith<T>(
            [NotNull] this IQueryable<T> source, [NotNull] [NotParameterized] string tag)
        {
            Check.NotNull(source, nameof(source));
            Check.NotEmpty(tag, nameof(tag));

            return
                source.Provider is EntityQueryProvider
                    ? source.Provider.CreateQuery<T>(
                        Expression.Call(
                            instance: null,
                            method: TagWithMethodInfo.MakeGenericMethod(typeof(T)),
                            arg0: source.Expression,
                            arg1: Expression.Constant(tag)))
                    : source;
        }

        #endregion

        #region Load

        /// <summary>
        ///     Enumerates the query. When using Entity Framework, this causes the results of the query to
        ///     be loaded into the associated context. This is equivalent to calling ToList
        ///     and then throwing away the list (without the overhead of actually creating the list).
        /// </summary>
        /// <param name="source"> The source query. </param>
        public static void Load<TSource>([NotNull] this IQueryable<TSource> source)
        {
            Check.NotNull(source, nameof(source));

            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                }
            }
        }

        /// <summary>
        ///     Asynchronously enumerates the query. When using Entity Framework, this causes the results of the query to
        ///     be loaded into the associated context. This is equivalent to calling ToList
        ///     and then throwing away the list (without the overhead of actually creating the list).
        /// </summary>
        /// <param name="source"> The source query. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns> A task that represents the asynchronous operation. </returns>
        public static async Task LoadAsync<TSource>(
            [NotNull] this IQueryable<TSource> source, CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));

            await using (var enumerator = source.AsAsyncEnumerable().GetAsyncEnumerator(cancellationToken))
            {
                while (await enumerator.MoveNextAsync())
                {
                }
            }
        }

        #endregion

        #region ToDictionary

        /// <summary>
        ///     Creates a <see cref="Dictionary{TKey, TValue}" /> from an <see cref="IQueryable{T}" /> by enumerating it
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
        ///     An <see cref="IQueryable{T}" /> to create a <see cref="Dictionary{TKey, TValue}" /> from.
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
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Func<TSource, TKey> keySelector,
            CancellationToken cancellationToken = default)
            => ToDictionaryAsync(source, keySelector, e => e, comparer: null, cancellationToken);

        /// <summary>
        ///     Creates a <see cref="Dictionary{TKey, TValue}" /> from an <see cref="IQueryable{T}" /> by enumerating it
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
        ///     An <see cref="IQueryable{T}" /> to create a <see cref="Dictionary{TKey, TValue}" /> from.
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
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Func<TSource, TKey> keySelector,
            [NotNull] IEqualityComparer<TKey> comparer,
            CancellationToken cancellationToken = default)
            => ToDictionaryAsync(source, keySelector, e => e, comparer, cancellationToken);

        /// <summary>
        ///     Creates a <see cref="Dictionary{TKey, TValue}" /> from an <see cref="IQueryable{T}" /> by enumerating it
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
        ///     An <see cref="IQueryable{T}" /> to create a <see cref="Dictionary{TKey, TValue}" /> from.
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
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Func<TSource, TKey> keySelector,
            [NotNull] Func<TSource, TElement> elementSelector,
            CancellationToken cancellationToken = default)
            => ToDictionaryAsync(source, keySelector, elementSelector, comparer: null, cancellationToken);

        /// <summary>
        ///     Creates a <see cref="Dictionary{TKey, TValue}" /> from an <see cref="IQueryable{T}" /> by enumerating it
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
        ///     An <see cref="IQueryable{T}" /> to create a <see cref="Dictionary{TKey, TValue}" /> from.
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
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Func<TSource, TKey> keySelector,
            [NotNull] Func<TSource, TElement> elementSelector,
            [NotNull] IEqualityComparer<TKey> comparer,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(keySelector, nameof(keySelector));
            Check.NotNull(elementSelector, nameof(elementSelector));

            var d = new Dictionary<TKey, TElement>(comparer);
            await foreach (var element in source.AsAsyncEnumerable().WithCancellation(cancellationToken))
            {
                d.Add(keySelector(element), elementSelector(element));
            }

            return d;
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
        ///     An <see cref="IQueryable{T}" /> to enumerate.
        /// </param>
        /// <param name="action"> The action to perform on each element. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns> A task that represents the asynchronous operation. </returns>
        public static async Task ForEachAsync<T>(
            [NotNull] this IQueryable<T> source,
            [NotNull] Action<T> action,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(action, nameof(action));

            await foreach (var element in source.AsAsyncEnumerable().WithCancellation(cancellationToken))
            {
                action(element);
            }
        }

        #endregion

        #region AsAsyncEnumerable

        /// <summary>
        ///     Returns an <see cref="IAsyncEnumerable{T}" /> which can be enumerated asynchronously.
        /// </summary>
        /// <remarks>
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        ///     An <see cref="IQueryable{T}" /> to enumerate.
        /// </param>
        /// <returns> The query results. </returns>
        public static IAsyncEnumerable<TSource> AsAsyncEnumerable<TSource>(
            [NotNull] this IQueryable<TSource> source)
        {
            Check.NotNull(source, nameof(source));

            if (source is IAsyncEnumerable<TSource> asyncEnumerable)
            {
                return asyncEnumerable;
            }

            throw new InvalidOperationException(CoreStrings.IQueryableNotAsync(typeof(TSource)));
        }

        #endregion

        #region Impl.

        private static TResult ExecuteAsync<TSource, TResult>(
            MethodInfo operatorMethodInfo,
            IQueryable<TSource> source,
            Expression expression,
            CancellationToken cancellationToken = default)
        {
            if (source.Provider is IAsyncQueryProvider provider)
            {
                if (operatorMethodInfo.IsGenericMethod)
                {
                    operatorMethodInfo
                        = operatorMethodInfo.GetGenericArguments().Length == 2
                            ? operatorMethodInfo.MakeGenericMethod(typeof(TSource), typeof(TResult).GetGenericArguments().Single())
                            : operatorMethodInfo.MakeGenericMethod(typeof(TSource));
                }

                return provider.ExecuteAsync<TResult>(
                    Expression.Call(
                        instance: null,
                        method: operatorMethodInfo,
                        arguments: expression == null
                            ? new[] { source.Expression }
                            : new[] { source.Expression, expression }),
                    cancellationToken);
            }

            throw new InvalidOperationException(CoreStrings.IQueryableProviderNotAsync);
        }

        private static TResult ExecuteAsync<TSource, TResult>(
            MethodInfo operatorMethodInfo,
            IQueryable<TSource> source,
            LambdaExpression expression,
            CancellationToken cancellationToken = default)
            => ExecuteAsync<TSource, TResult>(
                operatorMethodInfo, source, Expression.Quote(expression), cancellationToken);

        private static TResult ExecuteAsync<TSource, TResult>(
            MethodInfo operatorMethodInfo,
            IQueryable<TSource> source,
            CancellationToken cancellationToken = default)
            => ExecuteAsync<TSource, TResult>(
                operatorMethodInfo, source, (Expression)null, cancellationToken);

        private static MethodInfo GetMethod<TResult>(
            string name, int parameterCount = 0, Func<MethodInfo, bool> predicate = null)
            => GetMethod(
                name,
                parameterCount,
                mi => mi.ReturnType == typeof(TResult)
                      && (predicate == null || predicate(mi)));

        private static MethodInfo GetMethod(
            string name, int parameterCount = 0, Func<MethodInfo, bool> predicate = null)
            => typeof(Queryable).GetTypeInfo().GetDeclaredMethods(name)
                .Single(
                    mi => mi.GetParameters().Length == parameterCount + 1
                          && (predicate == null || predicate(mi)));

        #endregion
    }
}
