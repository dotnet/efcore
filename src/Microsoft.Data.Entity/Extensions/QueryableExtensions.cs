// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace System.Linq
{
    public static class QueryableExtensions
    {
        private static readonly MethodInfo _any
            = GetMethod("Any", t => new[] { typeof(IQueryable<>).MakeGenericType(t) });

        public static Task<bool> AnyAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(source, "source");

            cancellationToken.ThrowIfCancellationRequested();

            var provider = source.Provider as IAsyncQueryProvider;

            if (provider != null)
            {
                return provider.ExecuteAsync<bool>(
                    Expression.Call(
                        null,
                        _any.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression }),
                    cancellationToken);
            }

            throw new InvalidOperationException(Strings.FormatIQueryableProviderNotAsync());
        }

        private static readonly MethodInfo _count
            = GetMethod("Count", t => new[] { typeof(IQueryable<>).MakeGenericType(t) });

        public static Task<int> CountAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(source, "source");

            cancellationToken.ThrowIfCancellationRequested();

            var provider = source.Provider as IAsyncQueryProvider;

            if (provider != null)
            {
                return provider.ExecuteAsync<int>(
                    Expression.Call(
                        null,
                        _count.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression }),
                    cancellationToken);
            }

            throw new InvalidOperationException(Strings.FormatIQueryableProviderNotAsync());
        }

        private static readonly MethodInfo _single
            = GetMethod("Single", t => new[] { typeof(IQueryable<>).MakeGenericType(t) });

        public static Task<TSource> SingleAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(source, "source");

            cancellationToken.ThrowIfCancellationRequested();

            var provider = source.Provider as IAsyncQueryProvider;

            if (provider != null)
            {
                return provider.ExecuteAsync<TSource>(
                    Expression.Call(
                        null,
                        _single.MakeGenericMethod(typeof(TSource)),
                        new[] { source.Expression }),
                    cancellationToken);
            }

            throw new InvalidOperationException(Strings.FormatIQueryableProviderNotAsync());
        }

        public static Task<List<TSource>> ToListAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(source, "source");

            cancellationToken.ThrowIfCancellationRequested();

            return source.ToAsyncEnumerable().ToList(cancellationToken);
        }

        public static Task<TSource[]> ToArrayAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(source, "source");

            cancellationToken.ThrowIfCancellationRequested();

            return source.ToAsyncEnumerable().ToArray(cancellationToken);
        }

        #region TODO

        public static IQueryable<T> Include<T, TProperty>(
            [NotNull] this IQueryable<T> source,
            [NotNull] Expression<Func<T, TProperty>> path)
        {
            // TODO
            return source;
        }

        public static Task<bool> AnyAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, bool>> predicate,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // TODO
            return Task.FromResult(false);
        }

        public static Task<TSource> SingleAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, bool>> predicate,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // TODO
            return Task.FromResult(default(TSource));
        }

        public static Task<TSource> SingleOrDefaultAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // TODO
            return Task.FromResult(default(TSource));
        }

        public static Task<TSource> SingleOrDefaultAsync<TSource>(
            [NotNull] this IQueryable<TSource> source,
            [NotNull] Expression<Func<TSource, bool>> predicate,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // TODO
            return Task.FromResult(default(TSource));
        }

        public static Task<decimal> SumAsync(
            [NotNull] this IQueryable<decimal> source,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // TODO
            return Task.FromResult(default(decimal));
        }

        public static Task<int> SumAsync(
            [NotNull] this IQueryable<int> source,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // TODO
            return Task.FromResult(0);
        }

        #endregion

        private static MethodInfo GetMethod(string methodName, Func<Type, Type[]> getParameterTypes)
        {
            return GetMethod(methodName, getParameterTypes.GetMethodInfo(), 1);
        }

        private static MethodInfo GetMethod(string methodName, MethodInfo getParameterTypesMethod, int genericArgumentsCount)
        {
            var candidates = typeof(Queryable).GetTypeInfo().GetDeclaredMethods(methodName);

            foreach (var candidate in candidates)
            {
                var genericArguments = candidate.GetGenericArguments();

                if (genericArguments.Length == genericArgumentsCount
                    && candidate.GetParameters().Select(p => p.ParameterType)
                        .SequenceEqual((Type[])getParameterTypesMethod.Invoke(null, genericArguments)))
                {
                    return candidate;
                }
            }

            return null;
        }
    }
}
