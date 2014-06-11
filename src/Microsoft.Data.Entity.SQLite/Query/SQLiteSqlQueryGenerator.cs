// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.Query.Expressions;
using Microsoft.Data.Entity.Relational.Query.Sql;

namespace Microsoft.Data.Entity.SQLite.Query
{
    public class SQLiteSqlQueryGenerator : DefaultSqlQueryGenerator
    {
        protected override void GenerateTop(SelectExpression expression)
        {
            // not supported in SQLite
        }

        protected override void GenerateLimitOffset(SelectExpression expression)
        {
            if (expression.Limit != null)
            {
                Sql.AppendLine()
                    .Append("LIMIT ")
                    .Append(expression.Limit);
            }
        }

        protected override string ConcatOperator
        {
            get { return "||"; }
        }
    }
}
