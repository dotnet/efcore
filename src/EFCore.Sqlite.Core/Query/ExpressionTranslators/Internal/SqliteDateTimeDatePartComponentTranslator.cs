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
    public class SqliteDateTimeDatePartComponentTranslator : IMemberTranslator
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MemberExpression memberExpression)
        {
            string datePart;
            if (memberExpression.Expression != null
                && (memberExpression.Expression.Type == typeof(DateTime) || memberExpression.Expression.Type == typeof(DateTimeOffset))
                && (datePart = GetDatePart(memberExpression.Member.Name)) != null)
            {
                return
                    new ExplicitCastExpression(
                        new SqlFunctionExpression(
                            functionName: SqliteDateTimeHelper.SqliteFunctionDateFormat,
                            returnType: memberExpression.Type,
                            arguments: new[]
                            {
                                new SqlFragmentExpression($"'{datePart}'"),
                                memberExpression.Expression
                            }),
                        memberExpression.Type);
            }
            return null;
        }

        private static string GetDatePart(string memberName)
        {
            switch (memberName)
            {
                case nameof(DateTime.Year):
                    return "%Y";
                case nameof(DateTime.Month):
                    return "%m";
                case nameof(DateTime.DayOfYear):
                    return "%j";
                case nameof(DateTime.DayOfWeek):
                    return "%w";
                case nameof(DateTime.Day):
                    return "%d";
                case nameof(DateTime.Hour):
                    return "%H";
                case nameof(DateTime.Minute):
                    return "%M";
                case nameof(DateTime.Second):
                    return "%S";
                default:
                    return null;
            }
        }
    }
}
