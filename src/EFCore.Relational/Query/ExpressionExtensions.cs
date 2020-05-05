// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
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
        ///     Checks if the given sql unary expression represents a logical NOT operation.
        /// </summary>
        /// <param name="sqlUnaryExpression"> A sql unary expression to check. </param>
        /// <returns> A bool value indicating if the given expression represents a logical NOT operation. </returns>
        public static bool IsLogicalNot([NotNull] this SqlUnaryExpression sqlUnaryExpression)
            => sqlUnaryExpression.OperatorType == ExpressionType.Not
                && (sqlUnaryExpression.Type == typeof(bool)
                    || sqlUnaryExpression.Type == typeof(bool?));

        /// <summary>
        ///     Infers type mapping from given <see cref="SqlExpression"/>s.
        /// </summary>
        /// <param name="expressions"> Expressions to search for to find the type mapping. </param>
        /// <returns> A relational type mapping inferred from the expressions. </returns>
        public static RelationalTypeMapping InferTypeMapping([NotNull] params SqlExpression[] expressions)
        {
            Check.NotNull(expressions, nameof(expressions));

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
    }
}
