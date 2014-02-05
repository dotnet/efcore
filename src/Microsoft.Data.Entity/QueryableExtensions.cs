// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;


namespace Microsoft.Data.Entity
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> Include<T, TProperty>(
            this IQueryable<T> source, Expression<Func<T, TProperty>> path)
        {
            // TODO
            return source;
        }

        public static Task<bool> AnyAsync<TSource>(this IQueryable<TSource> source)
        {
            // TODO
            return source.AnyAsync(CancellationToken.None);
        }

        public static Task<bool> AnyAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken)
        {
            // TODO
            return Task.FromResult(false);
        }

        public static Task<bool> AnyAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            // TODO
            return source.AnyAsync(predicate, CancellationToken.None);
        }

        public static Task<bool> AnyAsync<TSource>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, bool>> predicate,
            CancellationToken cancellationToken)
        {
            // TODO
            return Task.FromResult(false);
        }

        public static Task<List<TSource>> ToListAsync<TSource>(this IQueryable<TSource> source)
        {
            // TODO
            return ToListAsync(source, CancellationToken.None);
        }

        public static Task<List<TSource>> ToListAsync<TSource>(
            this IQueryable<TSource> source,
            CancellationToken cancellationToken)
        {
            // TODO
            return Task.FromResult<List<TSource>>(null);
        }

        public static Task<TSource> SingleAsync<TSource>(this IQueryable<TSource> source)
        {
            // TODO
            return source.SingleAsync(CancellationToken.None);
        }

        public static Task<TSource> SingleAsync<TSource>(
            this IQueryable<TSource> source,
            CancellationToken cancellationToken)
        {
            // TODO
            return Task.FromResult(default(TSource));
        }

        public static Task<TSource> SingleAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            // TODO
            return source.SingleAsync(predicate, CancellationToken.None);
        }

        public static Task<TSource> SingleAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate,
            CancellationToken cancellationToken)
        {
            // TODO
            return Task.FromResult(default(TSource));
        }

        public static Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source)
        {
            // TODO
            return source.SingleOrDefaultAsync(CancellationToken.None);
        }

        public static Task<TSource> SingleOrDefaultAsync<TSource>(
            this IQueryable<TSource> source,
            CancellationToken cancellationToken)
        {
            // TODO
            return Task.FromResult(default(TSource));
        }

        public static Task<TSource> SingleOrDefaultAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            // TODO
            return source.SingleOrDefaultAsync(predicate, CancellationToken.None);
        }

        public static Task<TSource> SingleOrDefaultAsync<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate,
            CancellationToken cancellationToken)
        {
            // TODO
            return Task.FromResult(default(TSource));
        }

        public static Task<decimal> SumAsync(this IQueryable<decimal> source)
        {
            // TODO
            return source.SumAsync(CancellationToken.None);
        }

        public static Task<decimal> SumAsync(this IQueryable<decimal> source, CancellationToken cancellationToken)
        {
            // TODO
            return Task.FromResult(default(decimal));
        }

        public static Task<int> SumAsync(this IQueryable<int> source)
        {
            // TODO
            return source.SumAsync(CancellationToken.None);
        }

        public static Task<int> SumAsync(this IQueryable<int> source, CancellationToken cancellationToken)
        {
            // TODO
            return Task.FromResult(0);
        }
    }
}
