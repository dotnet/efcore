// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Query.Expressions.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Sql
{
    /// <summary>
    ///     A factory for instances of <see cref="ISqlGenerator" />.
    /// </summary>
    public interface ISqlGeneratorFactory
    {
        /// <summary>
        ///     Creates the default SQL generator.
        /// </summary>
        /// <param name="selectExpression"> The select expression. </param>
        /// <returns>
        ///     The default SQL generator.
        /// </returns>
        ISqlGenerator CreateDefault([NotNull] SelectExpression selectExpression);

        /// <summary>
        ///     Creates a FromSql SQL generator.
        /// </summary>
        /// <param name="selectExpression"> The select expression. </param>
        /// <param name="sql"> The SQL. </param>
        /// <param name="arguments"> The arguments. </param>
        /// <returns>
        ///     The FromSql SQL generator.
        /// </returns>
        ISqlGenerator CreateFromSql(
            [NotNull] SelectExpression selectExpression,
            [NotNull] string sql,
            [NotNull] Expression arguments);
    }
}
