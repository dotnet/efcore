// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
        internal static readonly MethodInfo FromSqlMethodInfo
            = typeof(RelationalQueryableExtensions)
                .GetTypeInfo().GetDeclaredMethod(nameof(FromSql));

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
        ///         You can also construct a DbParameter and supply it to as a parameter value. This allows you to use named
        ///         parameters in the SQL query string -
        ///         <code>context.Blogs.FromSql("SELECT * FROM [dbo].[SearchBlogs]({@searchTerm})", new SqlParameter("@searchTerm", userSuppliedSearchTerm))</code>
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
        public static IQueryable<TEntity> FromSql<TEntity>(
            [NotNull] this IQueryable<TEntity> source,
            [NotNull] [NotParameterized] string sql,
            [NotNull] params object[] parameters)
            where TEntity : class
        {
            Check.NotNull(source, nameof(source));
            Check.NotEmpty(sql, nameof(sql));
            Check.NotNull(parameters, nameof(parameters));

            return source.Provider.CreateQuery<TEntity>(
                Expression.Call(
                    null,
                    FromSqlMethodInfo.MakeGenericMethod(typeof(TEntity)),
                    source.Expression,
                    Expression.Constant(sql),
                    Expression.Constant(parameters)));
        }
    }
}
