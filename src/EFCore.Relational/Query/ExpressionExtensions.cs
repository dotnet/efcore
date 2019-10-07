// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query
{
    public static class ExpressionExtensions
    {
        public static RelationalTypeMapping InferTypeMapping(params SqlExpression[] expressions)
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
    }
}
