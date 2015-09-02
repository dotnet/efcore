// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Expressions;
using Microsoft.Data.Entity.Query.Sql;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query
{
    public class SqlServerQuerySqlGenerator : DefaultQuerySqlGenerator
    {
        public SqlServerQuerySqlGenerator(
            [NotNull] SelectExpression selectExpression,
            [NotNull] IRelationalTypeMapper typeMapper)
            : base(selectExpression, typeMapper)
        {
        }

        protected override string DelimitIdentifier(string identifier)
            => "[" + identifier.Replace("]", "]]") + "]";

        public override Expression VisitCount(CountExpression countExpression)
        {
            Check.NotNull(countExpression, nameof(countExpression));

            if (countExpression.Type == typeof(long))
            {
                Sql.Append("COUNT_BIG(*)");

                return countExpression;
            }

            return base.VisitCount(countExpression);
        }

        protected override void GenerateLimitOffset(SelectExpression selectExpression)
        {
            if (selectExpression.Offset != null
                && !selectExpression.OrderBy.Any())
            {
                Sql.AppendLine().Append("ORDER BY @@ROWCOUNT");
            }

            base.GenerateLimitOffset(selectExpression);
        }
    }
}
