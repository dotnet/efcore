// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Pipeline
{
    public static class SqliteExpression
    {
        public static SqlFunctionExpression Strftime(
            Type returnType,
            RelationalTypeMapping typeMapping,
            SqlExpression formatExpression,
            SqlExpression timestring,
            IEnumerable<SqlExpression> modifiers = null)
        {
            modifiers = modifiers ?? Enumerable.Empty<SqlExpression>();

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
                new[]
                {
                        formatExpression,
                        timestring
                }.Concat(modifiers),
                returnType,
                typeMapping,
                false);
        }
    }
}
