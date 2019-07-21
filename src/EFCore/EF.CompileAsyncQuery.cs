// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore
{
    // ReSharper disable once InconsistentNaming
    public static partial class EF
    {
        /// <summary>
        ///     Creates a compiled query delegate that when invoked will execute the specified LINQ query.
        /// </summary>
        /// <typeparam name="TContext">The target DbContext type.</typeparam>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <param name="queryExpression">The LINQ query expression.</param>
        /// <returns>A delegate that can be invoked to execute the compiled query.</returns>
        public static Func<TContext, IAsyncEnumerable<TResult>> CompileAsyncQuery<TContext, TResult>(
            [NotNull] Expression<Func<TContext, DbSet<TResult>>> queryExpression)
            where TContext : DbContext
            where TResult : class
            => new CompiledAsyncEnumerableQuery<TContext, TResult>(queryExpression).Execute;

        /// <summary>
        ///     Creates a compiled query delegate that when invoked will execute the specified LINQ query.
        /// </summary>
        /// <typeparam name="TContext">The target DbContext type.</typeparam>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <param name="queryExpression">The LINQ query expression.</param>
        /// <returns>A delegate that can be invoked to execute the compiled query.</returns>
        [Obsolete("Use DbSet instead")]
        public static Func<TContext, IAsyncEnumerable<TResult>> CompileAsyncQuery<TContext, TResult>(
            [NotNull] Expression<Func<TContext, DbQuery<TResult>>> queryExpression)
            where TContext : DbContext
            where TResult : class
            => new CompiledAsyncEnumerableQuery<TContext, TResult>(queryExpression).Execute;

        /// <summary>
        ///     Creates a compiled query delegate that when invoked will execute the specified LINQ query.
        /// </summary>
        /// <typeparam name="TContext">The target DbContext type.</typeparam>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <param name="queryExpression">The LINQ query expression.</param>
        /// <returns>A delegate that can be invoked to execute the compiled query.</returns>
        public static Func<TContext, IAsyncEnumerable<TResult>> CompileAsyncQuery<TContext, TResult>(
            [NotNull] Expression<Func<TContext, IQueryable<TResult>>> queryExpression)
            where TContext : DbContext
            => new CompiledAsyncEnumerableQuery<TContext, TResult>(queryExpression).Execute;

        /// <summary>
        ///     Creates a compiled query delegate that when invoked will execute the specified LINQ query.
        /// </summary>
        /// <typeparam name="TContext">The target DbContext type.</typeparam>
        /// <typeparam name="TParam1">The type of the first query parameter.</typeparam>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <param name="queryExpression">The LINQ query expression.</param>
        /// <returns>A delegate that can be invoked to execute the compiled query.</returns>
        public static Func<TContext, TParam1, IAsyncEnumerable<TResult>> CompileAsyncQuery<TContext, TParam1, TResult>(
            [NotNull] Expression<Func<TContext, TParam1, IQueryable<TResult>>> queryExpression)
            where TContext : DbContext
            => new CompiledAsyncEnumerableQuery<TContext, TResult>(queryExpression).Execute;

        /// <summary>
        ///     Creates a compiled query delegate that when invoked will execute the specified LINQ query.
        /// </summary>
        /// <typeparam name="TContext">The target DbContext type.</typeparam>
        /// <typeparam name="TParam1">The type of the first query parameter.</typeparam>
        /// <typeparam name="TParam2">The type of the second query parameter.</typeparam>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <param name="queryExpression">The LINQ query expression.</param>
        /// <returns>A delegate that can be invoked to execute the compiled query.</returns>
        public static Func<TContext, TParam1, TParam2, IAsyncEnumerable<TResult>> CompileAsyncQuery<
            TContext, TParam1, TParam2, TResult>(
            [NotNull] Expression<Func<TContext, TParam1, TParam2, IQueryable<TResult>>> queryExpression)
            where TContext : DbContext
            => new CompiledAsyncEnumerableQuery<TContext, TResult>(queryExpression).Execute;

        /// <summary>
        ///     Creates a compiled query delegate that when invoked will execute the specified LINQ query.
        /// </summary>
        /// <typeparam name="TContext">The target DbContext type.</typeparam>
        /// <typeparam name="TParam1">The type of the first query parameter.</typeparam>
        /// <typeparam name="TParam2">The type of the second query parameter.</typeparam>
        /// <typeparam name="TParam3">The type of the third query parameter.</typeparam>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <param name="queryExpression">The LINQ query expression.</param>
        /// <returns>A delegate that can be invoked to execute the compiled query.</returns>
        public static Func<TContext, TParam1, TParam2, TParam3, IAsyncEnumerable<TResult>> CompileAsyncQuery<
            TContext, TParam1, TParam2, TParam3, TResult>(
            [NotNull] Expression<Func<TContext, TParam1, TParam2, TParam3, IQueryable<TResult>>> queryExpression)
            where TContext : DbContext
            => new CompiledAsyncEnumerableQuery<TContext, TResult>(queryExpression).Execute;

        /// <summary>
        ///     Creates a compiled query delegate that when invoked will execute the specified LINQ query.
        /// </summary>
        /// <typeparam name="TContext">The target DbContext type.</typeparam>
        /// <typeparam name="TParam1">The type of the first query parameter.</typeparam>
        /// <typeparam name="TParam2">The type of the second query parameter.</typeparam>
        /// <typeparam name="TParam3">The type of the third query parameter.</typeparam>
        /// <typeparam name="TParam4">The type of the fourth query parameter.</typeparam>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <param name="queryExpression">The LINQ query expression.</param>
        /// <returns>A delegate that can be invoked to execute the compiled query.</returns>
        public static Func<TContext, TParam1, TParam2, TParam3, TParam4, IAsyncEnumerable<TResult>> CompileAsyncQuery<
            TContext, TParam1, TParam2, TParam3, TParam4, TResult>(
            [NotNull] Expression<Func<TContext, TParam1, TParam2, TParam3, TParam4, IQueryable<TResult>>> queryExpression)
            where TContext : DbContext
            => new CompiledAsyncEnumerableQuery<TContext, TResult>(queryExpression).Execute;

        /// <summary>
        ///     Creates a compiled query delegate that when invoked will execute the specified LINQ query.
        /// </summary>
        /// <typeparam name="TContext">The target DbContext type.</typeparam>
        /// <typeparam name="TParam1">The type of the first query parameter.</typeparam>
        /// <typeparam name="TParam2">The type of the second query parameter.</typeparam>
        /// <typeparam name="TParam3">The type of the third query parameter.</typeparam>
        /// <typeparam name="TParam4">The type of the fourth query parameter.</typeparam>
        /// <typeparam name="TParam5">The type of the fifth query parameter.</typeparam>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <param name="queryExpression">The LINQ query expression.</param>
        /// <returns>A delegate that can be invoked to execute the compiled query.</returns>
        public static Func<TContext, TParam1, TParam2, TParam3, TParam4, TParam5, IAsyncEnumerable<TResult>> CompileAsyncQuery<
            TContext, TParam1, TParam2, TParam3, TParam4, TParam5, TResult>(
            [NotNull] Expression<Func<TContext, TParam1, TParam2, TParam3, TParam4, TParam5, IQueryable<TResult>>> queryExpression)
            where TContext : DbContext
            => new CompiledAsyncEnumerableQuery<TContext, TResult>(queryExpression).Execute;

        /// <summary>
        ///     Creates a compiled query delegate that when invoked will execute the specified LINQ query.
        /// </summary>
        /// <typeparam name="TContext">The target DbContext type.</typeparam>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <param name="queryExpression">The LINQ query expression.</param>
        /// <returns>A delegate that can be invoked to execute the compiled query.</returns>
        public static Func<TContext, Task<TResult>> CompileAsyncQuery<TContext, TResult>(
            [NotNull] Expression<Func<TContext, TResult>> queryExpression)
            where TContext : DbContext
            => new CompiledAsyncTaskQuery<TContext, TResult>(queryExpression).ExecuteAsync;

        /// <summary>
        ///     Creates a compiled query delegate that when invoked will execute the specified LINQ query.
        /// </summary>
        /// <typeparam name="TContext">The target DbContext type.</typeparam>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <param name="queryExpression">The LINQ query expression.</param>
        /// <returns>A delegate that can be invoked to execute the compiled query.</returns>
        public static Func<TContext, CancellationToken, Task<TResult>> CompileAsyncQuery<TContext, TResult>(
            [NotNull] Expression<Func<TContext, CancellationToken, TResult>> queryExpression)
            where TContext : DbContext
            => new CompiledAsyncTaskQuery<TContext, TResult>(queryExpression).ExecuteAsync;

        /// <summary>
        ///     Creates a compiled query delegate that when invoked will execute the specified LINQ query.
        /// </summary>
        /// <typeparam name="TContext">The target DbContext type.</typeparam>
        /// <typeparam name="TParam1">The type of the first query parameter.</typeparam>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <param name="queryExpression">The LINQ query expression.</param>
        /// <returns>A delegate that can be invoked to execute the compiled query.</returns>
        public static Func<TContext, TParam1, Task<TResult>> CompileAsyncQuery<TContext, TParam1, TResult>(
            [NotNull] Expression<Func<TContext, TParam1, TResult>> queryExpression)
            where TContext : DbContext
            => new CompiledAsyncTaskQuery<TContext, TResult>(queryExpression).ExecuteAsync;

        /// <summary>
        ///     Creates a compiled query delegate that when invoked will execute the specified LINQ query.
        /// </summary>
        /// <typeparam name="TContext">The target DbContext type.</typeparam>
        /// <typeparam name="TParam1">The type of the first query parameter.</typeparam>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <param name="queryExpression">The LINQ query expression.</param>
        /// <returns>A delegate that can be invoked to execute the compiled query.</returns>
        public static Func<TContext, TParam1, CancellationToken, Task<TResult>> CompileAsyncQuery<TContext, TParam1, TResult>(
            [NotNull] Expression<Func<TContext, TParam1, CancellationToken, TResult>> queryExpression)
            where TContext : DbContext
            => new CompiledAsyncTaskQuery<TContext, TResult>(queryExpression).ExecuteAsync;

        /// <summary>
        ///     Creates a compiled query delegate that when invoked will execute the specified LINQ query.
        /// </summary>
        /// <typeparam name="TContext">The target DbContext type.</typeparam>
        /// <typeparam name="TParam1">The type of the first query parameter.</typeparam>
        /// <typeparam name="TParam2">The type of the second query parameter.</typeparam>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <param name="queryExpression">The LINQ query expression.</param>
        /// <returns>A delegate that can be invoked to execute the compiled query.</returns>
        public static Func<TContext, TParam1, TParam2, Task<TResult>> CompileAsyncQuery<
            TContext, TParam1, TParam2, TResult>(
            [NotNull] Expression<Func<TContext, TParam1, TParam2, TResult>> queryExpression)
            where TContext : DbContext
            => new CompiledAsyncTaskQuery<TContext, TResult>(queryExpression).ExecuteAsync;

        /// <summary>
        ///     Creates a compiled query delegate that when invoked will execute the specified LINQ query.
        /// </summary>
        /// <typeparam name="TContext">The target DbContext type.</typeparam>
        /// <typeparam name="TParam1">The type of the first query parameter.</typeparam>
        /// <typeparam name="TParam2">The type of the second query parameter.</typeparam>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <param name="queryExpression">The LINQ query expression.</param>
        /// <returns>A delegate that can be invoked to execute the compiled query.</returns>
        public static Func<TContext, TParam1, TParam2, CancellationToken, Task<TResult>> CompileAsyncQuery<
            TContext, TParam1, TParam2, TResult>(
            [NotNull] Expression<Func<TContext, TParam1, TParam2, CancellationToken, TResult>> queryExpression)
            where TContext : DbContext
            => new CompiledAsyncTaskQuery<TContext, TResult>(queryExpression).ExecuteAsync;

        /// <summary>
        ///     Creates a compiled query delegate that when invoked will execute the specified LINQ query.
        /// </summary>
        /// <typeparam name="TContext">The target DbContext type.</typeparam>
        /// <typeparam name="TParam1">The type of the first query parameter.</typeparam>
        /// <typeparam name="TParam2">The type of the second query parameter.</typeparam>
        /// <typeparam name="TParam3">The type of the third query parameter.</typeparam>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <param name="queryExpression">The LINQ query expression.</param>
        /// <returns>A delegate that can be invoked to execute the compiled query.</returns>
        public static Func<TContext, TParam1, TParam2, TParam3, Task<TResult>> CompileAsyncQuery<
            TContext, TParam1, TParam2, TParam3, TResult>(
            [NotNull] Expression<Func<TContext, TParam1, TParam2, TParam3, TResult>> queryExpression)
            where TContext : DbContext
            => new CompiledAsyncTaskQuery<TContext, TResult>(queryExpression).ExecuteAsync;

        /// <summary>
        ///     Creates a compiled query delegate that when invoked will execute the specified LINQ query.
        /// </summary>
        /// <typeparam name="TContext">The target DbContext type.</typeparam>
        /// <typeparam name="TParam1">The type of the first query parameter.</typeparam>
        /// <typeparam name="TParam2">The type of the second query parameter.</typeparam>
        /// <typeparam name="TParam3">The type of the third query parameter.</typeparam>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <param name="queryExpression">The LINQ query expression.</param>
        /// <returns>A delegate that can be invoked to execute the compiled query.</returns>
        public static Func<TContext, TParam1, TParam2, TParam3, CancellationToken, Task<TResult>> CompileAsyncQuery<
            TContext, TParam1, TParam2, TParam3, TResult>(
            [NotNull] Expression<Func<TContext, TParam1, TParam2, TParam3, CancellationToken, TResult>> queryExpression)
            where TContext : DbContext
            => new CompiledAsyncTaskQuery<TContext, TResult>(queryExpression).ExecuteAsync;

        /// <summary>
        ///     Creates a compiled query delegate that when invoked will execute the specified LINQ query.
        /// </summary>
        /// <typeparam name="TContext">The target DbContext type.</typeparam>
        /// <typeparam name="TParam1">The type of the first query parameter.</typeparam>
        /// <typeparam name="TParam2">The type of the second query parameter.</typeparam>
        /// <typeparam name="TParam3">The type of the third query parameter.</typeparam>
        /// <typeparam name="TParam4">The type of the fourth query parameter.</typeparam>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <param name="queryExpression">The LINQ query expression.</param>
        /// <returns>A delegate that can be invoked to execute the compiled query.</returns>
        public static Func<TContext, TParam1, TParam2, TParam3, TParam4, Task<TResult>> CompileAsyncQuery<
            TContext, TParam1, TParam2, TParam3, TParam4, TResult>(
            [NotNull] Expression<Func<TContext, TParam1, TParam2, TParam3, TParam4, TResult>> queryExpression)
            where TContext : DbContext
            => new CompiledAsyncTaskQuery<TContext, TResult>(queryExpression).ExecuteAsync;

        /// <summary>
        ///     Creates a compiled query delegate that when invoked will execute the specified LINQ query.
        /// </summary>
        /// <typeparam name="TContext">The target DbContext type.</typeparam>
        /// <typeparam name="TParam1">The type of the first query parameter.</typeparam>
        /// <typeparam name="TParam2">The type of the second query parameter.</typeparam>
        /// <typeparam name="TParam3">The type of the third query parameter.</typeparam>
        /// <typeparam name="TParam4">The type of the fourth query parameter.</typeparam>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <param name="queryExpression">The LINQ query expression.</param>
        /// <returns>A delegate that can be invoked to execute the compiled query.</returns>
        public static Func<TContext, TParam1, TParam2, TParam3, TParam4, CancellationToken, Task<TResult>> CompileAsyncQuery<
            TContext, TParam1, TParam2, TParam3, TParam4, TResult>(
            [NotNull] Expression<Func<TContext, TParam1, TParam2, TParam3, TParam4, CancellationToken, TResult>> queryExpression)
            where TContext : DbContext
            => new CompiledAsyncTaskQuery<TContext, TResult>(queryExpression).ExecuteAsync;

        /// <summary>
        ///     Creates a compiled query delegate that when invoked will execute the specified LINQ query.
        /// </summary>
        /// <typeparam name="TContext">The target DbContext type.</typeparam>
        /// <typeparam name="TParam1">The type of the first query parameter.</typeparam>
        /// <typeparam name="TParam2">The type of the second query parameter.</typeparam>
        /// <typeparam name="TParam3">The type of the third query parameter.</typeparam>
        /// <typeparam name="TParam4">The type of the fourth query parameter.</typeparam>
        /// <typeparam name="TParam5">The type of the fifth query parameter.</typeparam>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <param name="queryExpression">The LINQ query expression.</param>
        /// <returns>A delegate that can be invoked to execute the compiled query.</returns>
        public static Func<TContext, TParam1, TParam2, TParam3, TParam4, TParam5, Task<TResult>> CompileAsyncQuery<
            TContext, TParam1, TParam2, TParam3, TParam4, TParam5, TResult>(
            [NotNull] Expression<Func<TContext, TParam1, TParam2, TParam3, TParam4, TParam5, TResult>> queryExpression)
            where TContext : DbContext
            => new CompiledAsyncTaskQuery<TContext, TResult>(queryExpression).ExecuteAsync;

        /// <summary>
        ///     Creates a compiled query delegate that when invoked will execute the specified LINQ query.
        /// </summary>
        /// <typeparam name="TContext">The target DbContext type.</typeparam>
        /// <typeparam name="TParam1">The type of the first query parameter.</typeparam>
        /// <typeparam name="TParam2">The type of the second query parameter.</typeparam>
        /// <typeparam name="TParam3">The type of the third query parameter.</typeparam>
        /// <typeparam name="TParam4">The type of the fourth query parameter.</typeparam>
        /// <typeparam name="TParam5">The type of the fifth query parameter.</typeparam>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <param name="queryExpression">The LINQ query expression.</param>
        /// <returns>A delegate that can be invoked to execute the compiled query.</returns>
        public static Func<TContext, TParam1, TParam2, TParam3, TParam4, TParam5, CancellationToken, Task<TResult>> CompileAsyncQuery<
            TContext, TParam1, TParam2, TParam3, TParam4, TParam5, TResult>(
            [NotNull] Expression<Func<TContext, TParam1, TParam2, TParam3, TParam4, TParam5, CancellationToken, TResult>> queryExpression)
            where TContext : DbContext
            => new CompiledAsyncTaskQuery<TContext, TResult>(queryExpression).ExecuteAsync;
    }
}
