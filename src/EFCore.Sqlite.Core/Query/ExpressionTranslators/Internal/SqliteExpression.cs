// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class SqliteExpression
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static SqlFunctionExpression Strftime(
            Type returnType,
            string format,
            Expression timestring,
            IEnumerable<Expression> modifiers = null)
        {
            modifiers = modifiers ?? Enumerable.Empty<Expression>();

            // If the inner call is another strftime then shortcut a double call
            if (timestring is SqlFunctionExpression rtrimFunction
                && rtrimFunction.FunctionName == "rtrim"
                && rtrimFunction.Arguments.Count == 2
                && rtrimFunction.Arguments[0] is SqlFunctionExpression rtrimFunction2
                && rtrimFunction2.FunctionName == "rtrim"
                && rtrimFunction2.Arguments.Count == 2
                && rtrimFunction2.Arguments[0] is SqlFunctionExpression strftimeFunction
                && strftimeFunction.FunctionName == "strftime"
                && strftimeFunction.Arguments.Count > 1)
            {
                // Use its timestring parameter directly in place of ours
                timestring = strftimeFunction.Arguments[1];

                // Prepend its modifier arguments (if any) to the current call
                modifiers = strftimeFunction.Arguments.Skip(2).Concat(modifiers);
            }

            return new SqlFunctionExpression(
                "strftime",
                returnType,
                new[] { Expression.Constant(format), timestring }.Concat(
                    modifiers));
        }
    }
}
