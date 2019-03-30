// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query.Sql
{
    /// <summary>
    ///     <para>
    ///         A factory for instances of <see cref="IQuerySqlGenerator" />.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>. This means a single instance
    ///         is used by many <see cref="DbContext"/> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
    /// </summary>
    public interface IQuerySqlGeneratorFactory
    {
        /// <summary>
        ///     Creates the default SQL generator.
        /// </summary>
        /// <param name="selectExpression"> The select expression. </param>
        /// <returns>
        ///     The default SQL generator.
        /// </returns>
        IQuerySqlGenerator CreateDefault(
            [NotNull] SelectExpression selectExpression);

        /// <summary>
        ///     Creates a FromSql SQL generator.
        /// </summary>
        /// <param name="selectExpression"> The select expression. </param>
        /// <param name="sql"> The SQL. </param>
        /// <param name="arguments"> The arguments. </param>
        /// <returns>
        ///     The FromSql SQL generator.
        /// </returns>
        IQuerySqlGenerator CreateFromSql(
            [NotNull] SelectExpression selectExpression,
            [NotNull] string sql,
            [NotNull] Expression arguments);
    }
}
