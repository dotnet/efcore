// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqliteDateTimeDateComponentTranslator : IMemberTranslator
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MemberExpression memberExpression)
        {
            if (memberExpression.Expression != null
                && (memberExpression.Member.Name == nameof(DateTime.Date)
                    || memberExpression.Member.Name == nameof(DateTime.Today))
                && (memberExpression.Expression.Type == typeof(DateTime)
                    || memberExpression.Expression.Type == typeof(DateTimeOffset)))
            {
                switch (memberExpression.Member.Name)
                {
                    case nameof(DateTime.Today):
                        return new SqlFunctionExpression(
                            SqliteDateTimeHelper.SqliteFunctionDateFormat,
                            memberExpression.Type,
                            new[]
                            {
                                new SqlFragmentExpression(SqliteDateTimeHelper.SqliteFormatDate),
                                memberExpression.Expression,
                                new SqlFragmentExpression(SqliteDateTimeHelper.SqliteLocalTime),
                                new SqlFragmentExpression(SqliteDateTimeHelper.SqliteStartOfDay)
                            });
                    case nameof(DateTime.Date):
                        return new SqlFunctionExpression(
                            SqliteDateTimeHelper.SqliteFunctionDateFormat,
                            memberExpression.Type,
                            new[]
                            {
                                new SqlFragmentExpression(SqliteDateTimeHelper.SqliteFormatDate),
                                memberExpression.Expression,
                                new SqlFragmentExpression(SqliteDateTimeHelper.SqliteStartOfDay)
                            });
                    default:
                        return null;
                }
            }

            return null;
        }
    }
}
