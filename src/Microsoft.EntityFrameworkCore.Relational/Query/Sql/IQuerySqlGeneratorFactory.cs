// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.Sql
{
    /// <summary>
    ///     A factory for instances of <see cref="IQuerySqlGenerator" />.
    /// </summary>
    public interface IQuerySqlGeneratorFactory
    {
        /// <summary>
        ///     Creates the default SQL generator.
        /// </summary>
        /// <param name="selectExpression"> The select expression. </param>
        /// <returns>
        ///     The new default.
        /// </returns>
        IQuerySqlGenerator CreateDefault([NotNull] SelectExpression selectExpression);

        /// <summary>
        ///     Creates a FromSql SQL generator.
        /// </summary>
        /// <param name="selectExpression"> The select expression. </param>
        /// <param name="sql"> The SQL. </param>
        /// <param name="arguments"> The arguments. </param>
        /// <returns>
        ///     The new from SQL.
        /// </returns>
        IQuerySqlGenerator CreateFromSql(
            [NotNull] SelectExpression selectExpression,
            [NotNull] string sql,
            [NotNull] Expression arguments);
    }
}
