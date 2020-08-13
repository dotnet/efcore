// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
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
        public static Func<TContext, IEnumerable<TResult>> CompileQuery<TContext, TResult>(
            [NotNull] Expression<Func<TContext, DbSet<TResult>>> queryExpression)
            where TContext : DbContext
            where TResult : class
            => new CompiledQuery<TContext, IEnumerable<TResult>>(queryExpression).Execute;

        /// <summary>
        ///     Creates a compiled query delegate that when invoked will execute the specified LINQ query.
        /// </summary>
        /// <typeparam name="TContext">The target DbContext type.</typeparam>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <param name="queryExpression">The LINQ query expression.</param>
        /// <returns>A delegate that can be invoked to execute the compiled query.</returns>
        [Obsolete("Use DbSet instead")]
        public static Func<TContext, IEnumerable<TResult>> CompileQuery<TContext, TResult>(
            [NotNull] Expression<Func<TContext, DbQuery<TResult>>> queryExpression)
            where TContext : DbContext
            where TResult : class
            => new CompiledQuery<TContext, IEnumerable<TResult>>(queryExpression).Execute;

        /// <summary>
        ///     Creates a compiled query delegate that when invoked will execute the specified LINQ query.
        /// </summary>
        /// <typeparam name="TContext">The target DbContext type.</typeparam>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <param name="queryExpression">The LINQ query expression.</param>
        /// <returns>A delegate that can be invoked to execute the compiled query.</returns>
        public static Func<TContext, IEnumerable<TResult>> CompileQuery<TContext, TResult>(
            [NotNull] Expression<Func<TContext, IQueryable<TResult>>> queryExpression)
            where TContext : DbContext
            => new CompiledQuery<TContext, IEnumerable<TResult>>(queryExpression).Execute;

        /// <summary>
        ///     Creates a compiled query delegate that when invoked will execute the specified LINQ query.
        /// </summary>
        /// <typeparam name="TContext">The target DbContext type.</typeparam>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <typeparam name="TProperty">The included property type.</typeparam>
        /// <param name="queryExpression">The LINQ query expression.</param>
        /// <returns>A delegate that can be invoked to execute the compiled query.</returns>
        public static Func<TContext, IEnumerable<TResult>> CompileQuery<TContext, TResult, TProperty>(
            [NotNull] Expression<Func<TContext, IIncludableQueryable<TResult, TProperty>>> queryExpression)
            where TContext : DbContext
            => new CompiledQuery<TContext, IEnumerable<TResult>>(queryExpression).Execute;

        /// <summary>
        ///     Creates a compiled query delegate that when invoked will execute the specified LINQ query.
        /// </summary>
        /// <typeparam name="TContext">The target DbContext type.</typeparam>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <param name="queryExpression">The LINQ query expression.</param>
        /// <returns>A delegate that can be invoked to execute the compiled query.</returns>
        public static Func<TContext, TResult> CompileQuery<TContext, TResult>(
            [NotNull] Expression<Func<TContext, TResult>> queryExpression)
            where TContext : DbContext
            => new CompiledQuery<TContext, TResult>(queryExpression).Execute;

        /// <summary>
        ///     Creates a compiled query delegate that when invoked will execute the specified LINQ query.
        /// </summary>
        /// <typeparam name="TContext">The target DbContext type.</typeparam>
        /// <typeparam name="TParam1">The type of the first query parameter.</typeparam>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <param name="queryExpression">The LINQ query expression.</param>
        /// <returns>A delegate that can be invoked to execute the compiled query.</returns>
        public static Func<TContext, TParam1, IEnumerable<TResult>> CompileQuery<TContext, TParam1, TResult>(
            [NotNull] Expression<Func<TContext, TParam1, IQueryable<TResult>>> queryExpression)
            where TContext : DbContext
            => new CompiledQuery<TContext, IEnumerable<TResult>>(queryExpression).Execute;

        /// <summary>
        ///     Creates a compiled query delegate that when invoked will execute the specified LINQ query.
        /// </summary>
        /// <typeparam name="TContext">The target DbContext type.</typeparam>
        /// <typeparam name="TParam1">The type of the first query parameter.</typeparam>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <typeparam name="TProperty">The included property type.</typeparam>
        /// <param name="queryExpression">The LINQ query expression.</param>
        /// <returns>A delegate that can be invoked to execute the compiled query.</returns>
        public static Func<TContext, TParam1, IEnumerable<TResult>> CompileQuery<TContext, TParam1, TResult, TProperty>(
            [NotNull] Expression<Func<TContext, TParam1, IIncludableQueryable<TResult, TProperty>>> queryExpression)
            where TContext : DbContext
            => new CompiledQuery<TContext, IEnumerable<TResult>>(queryExpression).Execute;

        /// <summary>
        ///     Creates a compiled query delegate that when invoked will execute the specified LINQ query.
        /// </summary>
        /// <typeparam name="TContext">The target DbContext type.</typeparam>
        /// <typeparam name="TParam1">The type of the first query parameter.</typeparam>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <param name="queryExpression">The LINQ query expression.</param>
        /// <returns>A delegate that can be invoked to execute the compiled query.</returns>
        public static Func<TContext, TParam1, TResult> CompileQuery<TContext, TParam1, TResult>(
            [NotNull] Expression<Func<TContext, TParam1, TResult>> queryExpression)
            where TContext : DbContext
            => new CompiledQuery<TContext, TResult>(queryExpression).Execute;

        /// <summary>
        ///     Creates a compiled query delegate that when invoked will execute the specified LINQ query.
        /// </summary>
        /// <typeparam name="TContext">The target DbContext type.</typeparam>
        /// <typeparam name="TParam1">The type of the first query parameter.</typeparam>
        /// <typeparam name="TParam2">The type of the second query parameter.</typeparam>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <param name="queryExpression">The LINQ query expression.</param>
        /// <returns>A delegate that can be invoked to execute the compiled query.</returns>
        public static Func<TContext, TParam1, TParam2, IEnumerable<TResult>> CompileQuery<
            TContext, TParam1, TParam2, TResult>(
            [NotNull] Expression<Func<TContext, TParam1, TParam2, IQueryable<TResult>>> queryExpression)
            where TContext : DbContext
            => new CompiledQuery<TContext, IEnumerable<TResult>>(queryExpression).Execute;

        /// <summary>
        ///     Creates a compiled query delegate that when invoked will execute the specified LINQ query.
        /// </summary>
        /// <typeparam name="TContext">The target DbContext type.</typeparam>
        /// <typeparam name="TParam1">The type of the first query parameter.</typeparam>
        /// <typeparam name="TParam2">The type of the second query parameter.</typeparam>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <typeparam name="TProperty">The included property type.</typeparam>
        /// <param name="queryExpression">The LINQ query expression.</param>
        /// <returns>A delegate that can be invoked to execute the compiled query.</returns>
        public static Func<TContext, TParam1, TParam2, IEnumerable<TResult>> CompileQuery<
            TContext, TParam1, TParam2, TResult, TProperty>(
            [NotNull] Expression<Func<TContext, TParam1, TParam2, IIncludableQueryable<TResult, TProperty>>> queryExpression)
            where TContext : DbContext
            => new CompiledQuery<TContext, IEnumerable<TResult>>(queryExpression).Execute;

        /// <summary>
        ///     Creates a compiled query delegate that when invoked will execute the specified LINQ query.
        /// </summary>
        /// <typeparam name="TContext">The target DbContext type.</typeparam>
        /// <typeparam name="TParam1">The type of the first query parameter.</typeparam>
        /// <typeparam name="TParam2">The type of the second query parameter.</typeparam>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <param name="queryExpression">The LINQ query expression.</param>
        /// <returns>A delegate that can be invoked to execute the compiled query.</returns>
        public static Func<TContext, TParam1, TParam2, TResult> CompileQuery<
            TContext, TParam1, TParam2, TResult>(
            [NotNull] Expression<Func<TContext, TParam1, TParam2, TResult>> queryExpression)
            where TContext : DbContext
            => new CompiledQuery<TContext, TResult>(queryExpression).Execute;

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
        public static Func<TContext, TParam1, TParam2, TParam3, IEnumerable<TResult>> CompileQuery<
            TContext, TParam1, TParam2, TParam3, TResult>(
            [NotNull] Expression<Func<TContext, TParam1, TParam2, TParam3, IQueryable<TResult>>> queryExpression)
            where TContext : DbContext
            => new CompiledQuery<TContext, IEnumerable<TResult>>(queryExpression).Execute;

        /// <summary>
        ///     Creates a compiled query delegate that when invoked will execute the specified LINQ query.
        /// </summary>
        /// <typeparam name="TContext">The target DbContext type.</typeparam>
        /// <typeparam name="TParam1">The type of the first query parameter.</typeparam>
        /// <typeparam name="TParam2">The type of the second query parameter.</typeparam>
        /// <typeparam name="TParam3">The type of the third query parameter.</typeparam>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <typeparam name="TProperty">The included property type.</typeparam>
        /// <param name="queryExpression">The LINQ query expression.</param>
        /// <returns>A delegate that can be invoked to execute the compiled query.</returns>
        public static Func<TContext, TParam1, TParam2, TParam3, IEnumerable<TResult>> CompileQuery<
            TContext, TParam1, TParam2, TParam3, TResult, TProperty>(
            [NotNull] Expression<Func<TContext, TParam1, TParam2, TParam3, IIncludableQueryable<TResult, TProperty>>> queryExpression)
            where TContext : DbContext
            => new CompiledQuery<TContext, IEnumerable<TResult>>(queryExpression).Execute;

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
        public static Func<TContext, TParam1, TParam2, TParam3, TResult> CompileQuery<
            TContext, TParam1, TParam2, TParam3, TResult>(
            [NotNull] Expression<Func<TContext, TParam1, TParam2, TParam3, TResult>> queryExpression)
            where TContext : DbContext
            => new CompiledQuery<TContext, TResult>(queryExpression).Execute;

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
        public static Func<TContext, TParam1, TParam2, TParam3, TParam4, IEnumerable<TResult>> CompileQuery<
            TContext, TParam1, TParam2, TParam3, TParam4, TResult>(
            [NotNull] Expression<Func<TContext, TParam1, TParam2, TParam3, TParam4, IQueryable<TResult>>> queryExpression)
            where TContext : DbContext
            => new CompiledQuery<TContext, IEnumerable<TResult>>(queryExpression).Execute;

        /// <summary>
        ///     Creates a compiled query delegate that when invoked will execute the specified LINQ query.
        /// </summary>
        /// <typeparam name="TContext">The target DbContext type.</typeparam>
        /// <typeparam name="TParam1">The type of the first query parameter.</typeparam>
        /// <typeparam name="TParam2">The type of the second query parameter.</typeparam>
        /// <typeparam name="TParam3">The type of the third query parameter.</typeparam>
        /// <typeparam name="TParam4">The type of the fourth query parameter.</typeparam>
        /// <typeparam name="TResult">The query result type.</typeparam>
        /// <typeparam name="TProperty">The included property type.</typeparam>
        /// <param name="queryExpression">The LINQ query expression.</param>
        /// <returns>A delegate that can be invoked to execute the compiled query.</returns>
        public static Func<TContext, TParam1, TParam2, TParam3, TParam4, IEnumerable<TResult>> CompileQuery<
            TContext, TParam1, TParam2, TParam3, TParam4, TResult, TProperty>(
            [NotNull] Expression<Func<TContext, TParam1, TParam2, TParam3, TParam4, IIncludableQueryable<TResult, TProperty>>> queryExpression)
            where TContext : DbContext
            => new CompiledQuery<TContext, IEnumerable<TResult>>(queryExpression).Execute;

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
        public static Func<TContext, TParam1, TParam2, TParam3, TParam4, TResult> CompileQuery<
            TContext, TParam1, TParam2, TParam3, TParam4, TResult>(
            [NotNull] Expression<Func<TContext, TParam1, TParam2, TParam3, TParam4, TResult>> queryExpression)
            where TContext : DbContext
            => new CompiledQuery<TContext, TResult>(queryExpression).Execute;

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
        public static Func<TContext, TParam1, TParam2, TParam3, TParam4, TParam5, IEnumerable<TResult>> CompileQuery<
            TContext, TParam1, TParam2, TParam3, TParam4, TParam5, TResult>(
            [NotNull] Expression<Func<TContext, TParam1, TParam2, TParam3, TParam4, TParam5, IQueryable<TResult>>> queryExpression)
            where TContext : DbContext
            => new CompiledQuery<TContext, IEnumerable<TResult>>(queryExpression).Execute;

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
        /// <typeparam name="TProperty">The included property type.</typeparam>
        /// <param name="queryExpression">The LINQ query expression.</param>
        /// <returns>A delegate that can be invoked to execute the compiled query.</returns>
        public static Func<TContext, TParam1, TParam2, TParam3, TParam4, TParam5, IEnumerable<TResult>> CompileQuery<
            TContext, TParam1, TParam2, TParam3, TParam4, TParam5, TResult, TProperty>(
            [NotNull] Expression<Func<TContext, TParam1, TParam2, TParam3, TParam4, TParam5, IIncludableQueryable<TResult, TProperty>>> queryExpression)
            where TContext : DbContext
            => new CompiledQuery<TContext, IEnumerable<TResult>>(queryExpression).Execute;

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
        public static Func<TContext, TParam1, TParam2, TParam3, TParam4, TParam5, TResult> CompileQuery<
            TContext, TParam1, TParam2, TParam3, TParam4, TParam5, TResult>(
            [NotNull] Expression<Func<TContext, TParam1, TParam2, TParam3, TParam4, TParam5, TResult>> queryExpression)
            where TContext : DbContext
            => new CompiledQuery<TContext, TResult>(queryExpression).Execute;
    }
}
