// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    public class SqlServerQuerySqlGenerator : QuerySqlGenerator
    {
        public SqlServerQuerySqlGenerator([NotNull] QuerySqlGeneratorDependencies dependencies)
            : base(dependencies)
        {
        }

        protected override void GenerateTop(SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            if (selectExpression.Limit != null
                && selectExpression.Offset == null)
            {
                Sql.Append("TOP(");

                Visit(selectExpression.Limit);

                Sql.Append(") ");
            }
        }

        protected override void GenerateLimitOffset(SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            // Note: For Limit without Offset, SqlServer generates TOP()
            if (selectExpression.Offset != null)
            {
                Sql.AppendLine()
                    .Append("OFFSET ");

                Visit(selectExpression.Offset);

                Sql.Append(" ROWS");

                if (selectExpression.Limit != null)
                {
                    Sql.Append(" FETCH NEXT ");

                    Visit(selectExpression.Limit);

                    Sql.Append(" ROWS ONLY");
                }
            }
        }

        protected override Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
        {
            Check.NotNull(sqlFunctionExpression, nameof(sqlFunctionExpression));

            if (!sqlFunctionExpression.IsBuiltIn
                && string.IsNullOrEmpty(sqlFunctionExpression.Schema))
            {
                sqlFunctionExpression = SqlFunctionExpression.Create(
                    schema: "dbo",
                    sqlFunctionExpression.Name,
                    sqlFunctionExpression.Arguments,
                    sqlFunctionExpression.NullResultAllowed,
                    sqlFunctionExpression.ArgumentsPropagateNullability,
                    sqlFunctionExpression.Type,
                    sqlFunctionExpression.TypeMapping);
            }

            return base.VisitSqlFunction(sqlFunctionExpression);
        }
    }
}
