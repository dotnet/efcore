// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.Query.Expressions;
using Microsoft.Data.Entity.Relational.Query.Sql;

namespace Microsoft.Data.Entity.SQLite.Query
{
    public class SQLiteSqlQueryGenerator : DefaultSqlQueryGenerator
    {
        protected override void GenerateTop(SelectExpression selectExpression)
        {
            // not supported in SQLite
        }

        protected override void GenerateLimitOffset(SelectExpression selectExpression)
        {
            if (selectExpression.Limit != null)
            {
                Sql.AppendLine()
                    .Append("LIMIT ")
                    .Append(selectExpression.Limit);

                if (selectExpression.Offset != null)
                {
                    Sql.Append(" OFFSET ")
                        .Append(selectExpression.Offset);
                }
            }
            else if (selectExpression.Offset != null)
            {
                Sql.AppendLine()
                    .Append("LIMIT ")
                    .Append(selectExpression.Offset)
                    .Append(", ")
                    .Append(-1);
            }
        }

        protected override string ConcatOperator
        {
            get { return "||"; }
        }
    }
}
