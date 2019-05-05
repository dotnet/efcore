// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Pipeline
{
    public class SqliteQuerySqlGenerator : QuerySqlGenerator
    {
        public SqliteQuerySqlGenerator(
            IRelationalCommandBuilderFactory relationalCommandBuilderFactory,
            ISqlGenerationHelper sqlGenerationHelper)
            : base(relationalCommandBuilderFactory, sqlGenerationHelper)
        {
        }

        protected override string GenerateOperator(SqlBinaryExpression binaryExpression)
            => binaryExpression.OperatorType == ExpressionType.Add
            && binaryExpression.Type == typeof(string)
                ? " || "
                : base.GenerateOperator(binaryExpression);

        protected override void GenerateTop(SelectExpression selectExpression)
        {
            // Handled by GenerateLimitOffset
        }

        protected override void GenerateLimitOffset(SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            if (selectExpression.Limit != null
                || selectExpression.Offset != null)
            {
                Sql.AppendLine()
                    .Append("LIMIT ");

                Visit(selectExpression.Limit
                    ?? new SqlConstantExpression(Expression.Constant(-1), selectExpression.Offset.TypeMapping));

                if (selectExpression.Offset != null)
                {
                    Sql.Append(" OFFSET ");

                    Visit(selectExpression.Offset);
                }
            }
        }
    }
}
