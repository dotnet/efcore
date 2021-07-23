// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         A SQL translator for LINQ <see cref="MethodCallExpression" /> expression.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IMethodCallTranslator
    {
        /// <summary>
        ///     Translates a LINQ <see cref="MethodCallExpression" /> to a SQL equivalent.
        /// </summary>
        /// <param name="instance"> A SQL representation of <see cref="MethodCallExpression.Object" />. </param>
        /// <param name="method"> The method info from <see cref="MethodCallExpression.Method" />. </param>
        /// <param name="arguments"> SQL representations of <see cref="MethodCallExpression.Arguments" />. </param>
        /// <param name="logger"> The query logger to use. </param>
        /// <returns> A SQL translation of the <see cref="MethodCallExpression" />. </returns>
        SqlExpression? Translate(
            SqlExpression? instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger);
    }
}
