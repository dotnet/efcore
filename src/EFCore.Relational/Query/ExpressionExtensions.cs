// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         Extension methods for <see cref="Expression" /> types.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public static class ExpressionExtensions
{
    /// <summary>
    ///     Infers type mapping from given <see cref="SqlExpression" />s.
    /// </summary>
    /// <param name="expressions">Expressions to search for to find the type mapping.</param>
    /// <returns>A relational type mapping inferred from the expressions.</returns>
    public static RelationalTypeMapping? InferTypeMapping(params SqlExpression[] expressions)
    {
        for (var i = 0; i < expressions.Length; i++)
        {
            var sql = expressions[i];
            if (sql.TypeMapping != null)
            {
                return sql.TypeMapping;
            }
        }

        return null;
    }

    /// <summary>
    ///     Infers type mapping from given <see cref="SqlExpression" />s.
    /// </summary>
    /// <param name="expressions">Expressions to search for to find the type mapping.</param>
    /// <returns>A relational type mapping inferred from the expressions.</returns>
    public static RelationalTypeMapping? InferTypeMapping(IReadOnlyList<SqlExpression> expressions)
    {
        for (var i = 0; i < expressions.Count; i++)
        {
            var sql = expressions[i];
            if (sql.TypeMapping != null)
            {
                return sql.TypeMapping;
            }
        }

        return null;
    }
}
