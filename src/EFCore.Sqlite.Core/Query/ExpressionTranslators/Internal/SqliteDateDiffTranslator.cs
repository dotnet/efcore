// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqliteDateDiffTranslator : IMethodCallTranslator
    {
        private static readonly List<MethodInfo> _dateDiffMethodInfos
            = new List<MethodInfo>
            {
                {
                    typeof(DbFunctionsExtensions).GetRuntimeMethod(
                        nameof(DbFunctionsExtensions.DateDiffYear),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) })
                },
                {
                    typeof(DbFunctionsExtensions).GetRuntimeMethod(
                        nameof(DbFunctionsExtensions.DateDiffYear),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) })
                },
                {
                    typeof(DbFunctionsExtensions).GetRuntimeMethod(
                        nameof(DbFunctionsExtensions.DateDiffMonth),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) })
                },
                {
                    typeof(DbFunctionsExtensions).GetRuntimeMethod(
                        nameof(DbFunctionsExtensions.DateDiffMonth),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) })
                },
                {
                    typeof(DbFunctionsExtensions).GetRuntimeMethod(
                        nameof(DbFunctionsExtensions.DateDiffDay),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) })
                },
                {
                    typeof(DbFunctionsExtensions).GetRuntimeMethod(
                        nameof(DbFunctionsExtensions.DateDiffDay),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) })
                },
                {
                    typeof(DbFunctionsExtensions).GetRuntimeMethod(
                        nameof(DbFunctionsExtensions.DateDiffHour),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) })
                },
                {
                    typeof(DbFunctionsExtensions).GetRuntimeMethod(
                        nameof(DbFunctionsExtensions.DateDiffHour),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) })
                },
                {
                    typeof(DbFunctionsExtensions).GetRuntimeMethod(
                        nameof(DbFunctionsExtensions.DateDiffMinute),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) })
                },
                {
                    typeof(DbFunctionsExtensions).GetRuntimeMethod(
                        nameof(DbFunctionsExtensions.DateDiffMinute),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) })
                },
                {
                    typeof(DbFunctionsExtensions).GetRuntimeMethod(
                        nameof(DbFunctionsExtensions.DateDiffSecond),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) })
                },
                {
                    typeof(DbFunctionsExtensions).GetRuntimeMethod(
                        nameof(DbFunctionsExtensions.DateDiffSecond),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) })
                }
            };

        private static readonly Dictionary<string, string> _datePartMapping
            = new Dictionary<string, string>
            {
                { nameof(DateTime.Year), "'%Y'" },
                { nameof(DateTime.Month), "'%m'" },
                { nameof(DateTime.Hour), "'%H'" },
                { nameof(DateTime.Minute), "'%M'" },
                { nameof(DateTime.Second), "'%S'" },
            };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            if (_dateDiffMethodInfos.Contains(methodCallExpression.Method))
            {
                var endDate = methodCallExpression.Arguments[2];
                var startDate = methodCallExpression.Arguments[1];

                switch (methodCallExpression.Method.Name)
                {
                    case nameof(DbFunctionsExtensions.DateDiffYear):
                        return DateDiffYear(startDate, endDate);

                    case nameof(DbFunctionsExtensions.DateDiffMonth):
                        return DateDiffMonth(startDate, endDate);

                    case nameof(DbFunctionsExtensions.DateDiffDay):
                        return DateDiffDay(startDate, endDate);

                    case nameof(DbFunctionsExtensions.DateDiffHour):
                        return DateDiffHour(startDate, endDate);

                    case nameof(DbFunctionsExtensions.DateDiffMinute):
                        return DateDiffMinute(startDate, endDate);

                    case nameof(DbFunctionsExtensions.DateDiffSecond):
                        return DateDiffSecond(startDate, endDate);
                }
            }

            return null;
        }

        private Expression DateDiffYear(Expression startDate, Expression endDate)
        {
            return Expression.Subtract(
                GetDatePart(endDate, nameof(DateTime.Year)),
                GetDatePart(startDate, nameof(DateTime.Year)));
        }

        private Expression DateDiffMonth(Expression startDate, Expression endDate)
        {
            return Expression.Add(
                Expression.Multiply(
                    Expression.Constant(12, typeof(int?)),
                    DateDiffYear(startDate, endDate)),
                    Expression.Subtract(
                        GetDatePart(endDate, nameof(DateTime.Month)),
                        GetDatePart(startDate, nameof(DateTime.Month))));
        }

        private Expression DateDiffDay(Expression startDate, Expression endDate)
        {
            return Expression.Subtract(GetJulianDay(endDate), GetJulianDay(startDate));
        }

        private Expression DateDiffHour(Expression startDate, Expression endDate)
        {
            return Expression.Add(
                Expression.Multiply(
                    Expression.Constant(24, typeof(int?)),
                    DateDiffDay(startDate, endDate)),
                Expression.Subtract(
                    GetDatePart(endDate, nameof(DateTime.Hour)),
                    GetDatePart(startDate, nameof(DateTime.Hour))));
        }

        private Expression DateDiffMinute(Expression startDate, Expression endDate)
        {
            return Expression.Add(
                Expression.Multiply(
                    Expression.Constant(60, typeof(int?)),
                    DateDiffHour(startDate, endDate)),
                Expression.Subtract(
                    GetDatePart(endDate, nameof(DateTime.Minute)),
                    GetDatePart(startDate, nameof(DateTime.Minute))));
        }

        private Expression DateDiffSecond(Expression startDate, Expression endDate)
        {
            return Expression.Add(
                Expression.Multiply(
                    Expression.Constant(60, typeof(int?)),
                    DateDiffMinute(startDate, endDate)),
                Expression.Subtract(
                    GetDatePart(endDate, nameof(DateTime.Second)),
                    GetDatePart(startDate, nameof(DateTime.Second))));
        }

        private SqlFunctionExpression GetJulianDay(Expression date)
        {
            return new SqlFunctionExpression(
                "julianday",
                typeof(int?),
                new Expression[]
                {
                    date,
                    new SqlFragmentExpression("'start of day'")
                });
        }

        private ExplicitCastExpression GetDatePart(Expression date, string memberName)
        {
            return new ExplicitCastExpression(
                new SqlFunctionExpression(
                    "strftime",
                    date.Type,
                    new[] {
                        new SqlFragmentExpression(_datePartMapping[memberName]),
                        date
                    }),
                typeof(int?));
        }
    }
}
