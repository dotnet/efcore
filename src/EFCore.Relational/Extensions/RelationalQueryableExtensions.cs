// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Relational database specific extension methods for LINQ queries.
    /// </summary>
    public static class RelationalQueryableExtensions
    {
        internal static readonly MethodInfo FromSqlOnQueryableMethodInfo
            = typeof(RelationalQueryableExtensions)
                .GetTypeInfo().GetDeclaredMethods(nameof(FromSqlOnQueryable))
                .Single();

        /// <summary>
        ///     <para>
        ///         Creates a LINQ query based on a raw SQL query.
        ///     </para>
        ///     <para>
        ///         If the database provider supports composing on the supplied SQL, you can compose on top of the raw SQL query using
        ///         LINQ operators - <code>context.Blogs.FromSql("SELECT * FROM dbo.Blogs").OrderBy(b => b.Name)</code>.
        ///     </para>
        ///     <para>
        ///         As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection
        ///         attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional
        ///         arguments. Any parameter values you supply will automatically be converted to a DbParameter -
        ///         <code>context.Blogs.FromSql("SELECT * FROM [dbo].[SearchBlogs]({0})", userSuppliedSearchTerm)</code>.
        ///     </para>
        ///     <para>
        ///         This overload also accepts DbParameter instances as parameter values. This allows you to use named
        ///         parameters in the SQL query string -
        ///         <code>context.Blogs.FromSql("SELECT * FROM [dbo].[SearchBlogs]({@searchTerm})", new SqlParameter("@searchTerm", userSuppliedSearchTerm))</code>
        ///     </para>
        /// </summary>
        /// <typeparam name="TEntity"> The type of the elements of <paramref name="source" />. </typeparam>
        /// <param name="source">
        ///     An <see cref="IQueryable{T}" /> to use as the base of the raw SQL query (typically a <see cref="DbSet{TEntity}" />).
        /// </param>
        /// <param name="sql">
        ///     The raw SQL query. NB. A string literal may be passed here because <see cref="RawSqlString" />
        ///     is implicitly convertible to string.
        /// </param>
        /// <param name="parameters"> The values to be assigned to parameters. </param>
        /// <returns> An <see cref="IQueryable{T}" /> representing the raw SQL query. </returns>
        [StringFormatMethod("sql")]
        [Obsolete(
            "For returning objects from SQL queries using plain strings, use FromSqlRaw instead. "
            + "For returning objects from SQL queries using interpolated string syntax to create parameters, use FromSqlInterpolated instead. "
            + "Call either new method directly on the DbSet at the root of the query.",
            error: true)]
        public static IQueryable<TEntity> FromSql<TEntity>(
            [NotNull] this IQueryable<TEntity> source,
            [NotParameterized] RawSqlString sql,
            [NotNull] params object[] parameters)
            where TEntity : class
        {
            Check.NotNull(source, nameof(source));
            Check.NotEmpty(sql.Format, nameof(sql));
            Check.NotNull(parameters, nameof(parameters));

            return source.Provider.CreateQuery<TEntity>(
                Expression.Call(
                    null,
                    FromSqlOnQueryableMethodInfo.MakeGenericMethod(typeof(TEntity)),
                    source.Expression,
                    Expression.Constant(sql.Format),
                    Expression.Constant(parameters)));
        }

        /// <summary>
        ///     <para>
        ///         Creates a LINQ query based on an interpolated string representing a SQL query.
        ///     </para>
        ///     <para>
        ///         If the database provider supports composing on the supplied SQL, you can compose on top of the raw SQL query using
        ///         LINQ operators - <code>context.Blogs.FromSql("SELECT * FROM dbo.Blogs").OrderBy(b => b.Name)</code>.
        ///     </para>
        ///     <para>
        ///         As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection
        ///         attack. You can include interpolated parameter place holders in the SQL query string. Any interpolated parameter values
        ///         you supply will automatically be converted to a DbParameter -
        ///         <code>context.Blogs.FromSql($"SELECT * FROM [dbo].[SearchBlogs]({userSuppliedSearchTerm})")</code>.
        ///     </para>
        /// </summary>
        /// <typeparam name="TEntity"> The type of the elements of <paramref name="source" />. </typeparam>
        /// <param name="source">
        ///     An <see cref="IQueryable{T}" /> to use as the base of the interpolated string SQL query (typically a <see cref="DbSet{TEntity}" />).
        /// </param>
        /// <param name="sql"> The interpolated string representing a SQL query. </param>
        /// <returns> An <see cref="IQueryable{T}" /> representing the interpolated string SQL query. </returns>
        [Obsolete(
            "For returning objects from SQL queries using plain strings, use FromSqlRaw instead. "
            + "For returning objects from SQL queries using interpolated string syntax to create parameters, use FromSqlInterpolated instead. "
            + "Call either new method directly on the DbSet at the root of the query.",
            error: true)]
        public static IQueryable<TEntity> FromSql<TEntity>(
            [NotNull] this IQueryable<TEntity> source,
            [NotNull] [NotParameterized] FormattableString sql)
            where TEntity : class
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(sql, nameof(sql));
            Check.NotEmpty(sql.Format, nameof(source));

            return source.Provider.CreateQuery<TEntity>(
                Expression.Call(
                    null,
                    FromSqlOnQueryableMethodInfo.MakeGenericMethod(typeof(TEntity)),
                    source.Expression,
                    Expression.Constant(sql.Format),
                    Expression.Constant(sql.GetArguments())));
        }

        /// <summary>
        ///     <para>
        ///         Creates a LINQ query based on a raw SQL query.
        ///     </para>
        ///     <para>
        ///         If the database provider supports composing on the supplied SQL, you can compose on top of the raw SQL query using
        ///         LINQ operators - <code>context.Blogs.FromSqlRaw("SELECT * FROM dbo.Blogs").OrderBy(b => b.Name)</code>.
        ///     </para>
        ///     <para>
        ///         As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection
        ///         attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional
        ///         arguments. Any parameter values you supply will automatically be converted to a DbParameter -
        ///         <code>context.Blogs.FromSqlRaw("SELECT * FROM [dbo].[SearchBlogs]({0})", userSuppliedSearchTerm)</code>.
        ///         You can also consider using <see cref="FromSqlInterpolated{TEntity}" /> to use interpolated string syntax to create parameters.
        ///     </para>
        ///     <para>
        ///         This overload also accepts DbParameter instances as parameter values. This allows you to use named
        ///         parameters in the SQL query string -
        ///         <code>context.Blogs.FromSqlRaw("SELECT * FROM [dbo].[SearchBlogs]({@searchTerm})", new SqlParameter("@searchTerm", userSuppliedSearchTerm))</code>
        ///     </para>
        /// </summary>
        /// <typeparam name="TEntity"> The type of the elements of <paramref name="source" />. </typeparam>
        /// <param name="source">
        ///     An <see cref="IQueryable{T}" /> to use as the base of the raw SQL query (typically a <see cref="DbSet{TEntity}" />).
        /// </param>
        /// <param name="sql"> The raw SQL query. </param>
        /// <param name="parameters"> The values to be assigned to parameters. </param>
        /// <returns> An <see cref="IQueryable{T}" /> representing the raw SQL query. </returns>
        [StringFormatMethod("sql")]
        public static IQueryable<TEntity> FromSqlRaw<TEntity>(
            [NotNull] this DbSet<TEntity> source,
            [NotNull] [NotParameterized] string sql,
            [NotNull] params object[] parameters)
            where TEntity : class
        {
            Check.NotNull(source, nameof(source));
            Check.NotEmpty(sql, nameof(sql));
            Check.NotNull(parameters, nameof(parameters));

            var queryableSource = (IQueryable)source;
            return queryableSource.Provider.CreateQuery<TEntity>(
                Expression.Call(
                    null,
                    FromSqlOnQueryableMethodInfo.MakeGenericMethod(typeof(TEntity)),
                    queryableSource.Expression,
                    Expression.Constant(sql),
                    Expression.Constant(parameters)));
        }

        /// <summary>
        ///     <para>
        ///         Creates a LINQ query based on an interpolated string representing a SQL query.
        ///     </para>
        ///     <para>
        ///         If the database provider supports composing on the supplied SQL, you can compose on top of the raw SQL query using
        ///         LINQ operators - <code>context.Blogs.FromSqlInterpolated($"SELECT * FROM dbo.Blogs").OrderBy(b => b.Name)</code>.
        ///     </para>
        ///     <para>
        ///         As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection
        ///         attack. You can include interpolated parameter place holders in the SQL query string. Any interpolated parameter values
        ///         you supply will automatically be converted to a DbParameter -
        ///         <code>context.Blogs.FromSqlInterpolated($"SELECT * FROM [dbo].[SearchBlogs]({userSuppliedSearchTerm})")</code>.
        ///     </para>
        /// </summary>
        /// <typeparam name="TEntity"> The type of the elements of <paramref name="source" />. </typeparam>
        /// <param name="source">
        ///     An <see cref="IQueryable{T}" /> to use as the base of the interpolated string SQL query (typically a <see cref="DbSet{TEntity}" />).
        /// </param>
        /// <param name="sql"> The interpolated string representing a SQL query with parameters. </param>
        /// <returns> An <see cref="IQueryable{T}" /> representing the interpolated string SQL query. </returns>
        public static IQueryable<TEntity> FromSqlInterpolated<TEntity>(
            [NotNull] this DbSet<TEntity> source,
            [NotNull] [NotParameterized] FormattableString sql)
            where TEntity : class
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(sql, nameof(sql));
            Check.NotEmpty(sql.Format, nameof(source));

            var queryableSource = (IQueryable)source;
            return queryableSource.Provider.CreateQuery<TEntity>(
                Expression.Call(
                    null,
                    FromSqlOnQueryableMethodInfo.MakeGenericMethod(typeof(TEntity)),
                    queryableSource.Expression,
                    Expression.Constant(sql.Format),
                    Expression.Constant(sql.GetArguments())));
        }

        internal static IQueryable<TEntity> FromSqlOnQueryable<TEntity>(
            [NotNull] this IQueryable<TEntity> source,
            [NotParameterized] string sql,
            [NotNull] params object[] parameters)
            where TEntity : class
            => throw new NotImplementedException();
    }
}
