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
    public class OracleDateDiffTranslator : IMethodCallTranslator
    {
        private readonly Dictionary<MethodInfo, string> _methodInfoDateDiffMapping
            = new Dictionary<MethodInfo, string>
            {
                {
                    typeof(OracleDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(OracleDbFunctionsExtensions.DateDiffYear),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "YEAR"
                },
                {
                    typeof(OracleDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(OracleDbFunctionsExtensions.DateDiffYear),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "YEAR"
                },
                {
                    typeof(OracleDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(OracleDbFunctionsExtensions.DateDiffYear),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "YEAR"
                },
                {
                    typeof(OracleDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(OracleDbFunctionsExtensions.DateDiffYear),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "YEAR"
                },
                {
                    typeof(OracleDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(OracleDbFunctionsExtensions.DateDiffMonth),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "MONTH"
                },
                {
                    typeof(OracleDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(OracleDbFunctionsExtensions.DateDiffMonth),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "MONTH"
                },
                {
                    typeof(OracleDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(OracleDbFunctionsExtensions.DateDiffMonth),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "MONTH"
                },
                {
                    typeof(OracleDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(OracleDbFunctionsExtensions.DateDiffMonth),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "MONTH"
                },
                 {
                    typeof(OracleDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(OracleDbFunctionsExtensions.DateDiffDay),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "DAY"
                },
                {
                    typeof(OracleDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(OracleDbFunctionsExtensions.DateDiffDay),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "DAY"
                },
                {
                    typeof(OracleDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(OracleDbFunctionsExtensions.DateDiffDay),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "DAY"
                },
                {
                    typeof(OracleDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(OracleDbFunctionsExtensions.DateDiffDay),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "DAY"
                },
                {
                    typeof(OracleDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(OracleDbFunctionsExtensions.DateDiffHour),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "HOUR"
                },
                {
                    typeof(OracleDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(OracleDbFunctionsExtensions.DateDiffHour),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "HOUR"
                },
                {
                    typeof(OracleDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(OracleDbFunctionsExtensions.DateDiffHour),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "HOUR"
                },
                {
                    typeof(OracleDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(OracleDbFunctionsExtensions.DateDiffHour),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "HOUR"
                },
                {
                    typeof(OracleDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(OracleDbFunctionsExtensions.DateDiffMinute),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "MINUTE"
                },
                {
                    typeof(OracleDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(OracleDbFunctionsExtensions.DateDiffMinute),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "MINUTE"
                },
                {
                    typeof(OracleDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(OracleDbFunctionsExtensions.DateDiffMinute),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "MINUTE"
                },
                {
                    typeof(OracleDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(OracleDbFunctionsExtensions.DateDiffMinute),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "MINUTE"
                },
                {
                    typeof(OracleDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(OracleDbFunctionsExtensions.DateDiffSecond),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "SECOND"
                },
                {
                    typeof(OracleDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(OracleDbFunctionsExtensions.DateDiffSecond),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "SECOND"
                },
                {
                    typeof(OracleDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(OracleDbFunctionsExtensions.DateDiffSecond),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "SECOND"
                },
                {
                    typeof(OracleDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(OracleDbFunctionsExtensions.DateDiffSecond),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "SECOND"
                }
            };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            if (_methodInfoDateDiffMapping.TryGetValue(methodCallExpression.Method, out var datePart))
            {
                var startDate = methodCallExpression.Arguments[1];
                var endDate = methodCallExpression.Arguments[2];
                switch (methodCallExpression.Method.Name)
                {
                    case nameof(OracleDbFunctionsExtensions.DateDiffYear):
                        return DateDiffYear(startDate, endDate);
                    case nameof(OracleDbFunctionsExtensions.DateDiffMonth):
                        return DateDiffMonth(startDate, endDate);
                    case nameof(OracleDbFunctionsExtensions.DateDiffDay):
                        return DateDiffDay(startDate, endDate);
                    case nameof(OracleDbFunctionsExtensions.DateDiffHour):
                        return DateDiffHour(startDate, endDate);
                    case nameof(OracleDbFunctionsExtensions.DateDiffMinute):
                        return DateDiffMinute(startDate, endDate);
                    case nameof(OracleDbFunctionsExtensions.DateDiffSecond):
                        return DateDiffSecond(startDate, endDate);
                }
            }

            return null;
        }

        private Expression DateDiffYear(Expression startDate, Expression endDate)
        {
            return new ExplicitCastExpression(
                Expression.Subtract(
                    GetExtractYear(endDate),
                    GetExtractYear(startDate)),
                 typeof(int?));
        }

        private Expression DateDiffMonth(Expression startDate, Expression endDate)
        {
            return new SqlFunctionExpression(
                "TRUNC",
                typeof(int?),
                new[]
                {
                    new SqlFunctionExpression(
                        "MONTHS_BETWEEN",
                        typeof(int?),
                        new[]
                        {
                            endDate,
                            startDate
                        })
                });
        }

        private Expression DateDiffDay(Expression startDate, Expression endDate)
        {
            return new SqlFunctionExpression(
                "EXTRACT",
                typeof(int?),
                new[]
                {
                    new SqlFragmentExpression("DAY"),
                    Expression.Subtract(
                        ToDate(endDate),
                        ToDate(startDate)) as Expression
                });
        }

        private Expression DateDiffHour(Expression startDate, Expression endDate)
        {
            return new SqlFunctionExpression(
                "EXTRACT",
                typeof(int?),
                new[]
                {
                    new SqlFragmentExpression("DAY"),
                    MultiplyDateDiff(SubtractInterval(startDate, endDate), 24)
                });
        }

        private Expression DateDiffMinute(Expression startDate, Expression endDate)
        {
            return new SqlFunctionExpression(
                "EXTRACT",
                typeof(int?),
                new[]
                {
                    new SqlFragmentExpression("DAY"),
                    MultiplyDateDiff(
                        MultiplyDateDiff(
                            SubtractInterval(startDate, endDate),
                            24),
                        60)
            });
        }

        private Expression DateDiffSecond(Expression startDate, Expression endDate)
        {
            var minute = MultiplyDateDiff(
                MultiplyDateDiff(
                    SubtractInterval(startDate, endDate),
                    24),
                60);

            return new SqlFunctionExpression(
                "EXTRACT",
                typeof(int?),
                new[]
                {
                    new SqlFragmentExpression("DAY"),
                    MultiplyDateDiff(minute, 60)
                });
        }

        private Expression MultiplyDateDiff(Expression expression, int multiplier)
        {
           return Expression.Multiply(
              Expression.Constant(multiplier, typeof(int?)),
              expression);
        }

        private SqlFunctionExpression GetExtractYear(Expression date)
        {
            return new SqlFunctionExpression(
                "EXTRACT",
                date.Type,
                new[]
                {
                    new SqlFragmentExpression("YEAR"),
                    date
                });
        }

        private SqlFunctionExpression SubtractInterval(Expression startDate, Expression endDate)
        {
            return new SqlFunctionExpression(
                "TO_DSINTERVAL",
                typeof(int?),
                new[]
                {
                    Expression.Subtract(endDate, startDate)
                });
        }

        private Expression ToDate(Expression date)
        {
            if (date.NodeType is ExpressionType.Convert)
            {
                return new SqlFunctionExpression(
                    "TO_DATE",
                    date.Type,
                    new[]
                    {
                        date
                    });
            }
            return date;
        }
    }
}
