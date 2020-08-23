// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
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
        SqlExpression Translate(
            [CanBeNull] SqlExpression instance,
            [NotNull] MethodInfo method,
            [NotNull] IReadOnlyList<SqlExpression> arguments,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Query> logger);
    }
}
