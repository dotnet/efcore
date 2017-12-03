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
        private const string _sqliteLocalTime = "'localtime'";
        private static string _sqliteFormatDate = "'%Y-%m-%d %H:%M:%S'";
        private static string _sqliteFunctionDateFormat = "strftime"; 
        private static string _sqliteStartOfDay = "'start of day'";

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
                            _sqliteFunctionDateFormat,
                            memberExpression.Type,
                            new[]
                            {
                                new SqlFragmentExpression(_sqliteFormatDate),
                                memberExpression.Expression,
                                new SqlFragmentExpression(_sqliteLocalTime),
                                new SqlFragmentExpression(_sqliteStartOfDay)
                            });
                    case nameof(DateTime.Date):
                        return new SqlFunctionExpression(
                            _sqliteFunctionDateFormat,
                            memberExpression.Type,
                            new[]
                            {
                                new SqlFragmentExpression(_sqliteFormatDate),
                                memberExpression.Expression,
                                new SqlFragmentExpression(_sqliteStartOfDay)
                            });
                    default:
                        return null;
                }
            }

            return null;
        }
    }
}
