// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     <para>
    ///         This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///         directly from your code. This API may change or be removed in future releases.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>. This means a single instance
    ///         is used by many <see cref="DbContext"/> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
    /// </summary>
    public class SqliteDateTimeMemberTranslator : IMemberTranslator
    {
        private static readonly Dictionary<string, string> _datePartMapping
            = new Dictionary<string, string>
            {
                { nameof(DateTime.Year), "%Y" },
                { nameof(DateTime.Month), "%m" },
                { nameof(DateTime.DayOfYear), "%j" },
                { nameof(DateTime.Day), "%d" },
                { nameof(DateTime.Hour), "%H" },
                { nameof(DateTime.Minute), "%M" },
                { nameof(DateTime.Second), "%S" },
                { nameof(DateTime.DayOfWeek), "%w" }
            };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MemberExpression memberExpression)
        {
            if (memberExpression.Member.DeclaringType != typeof(DateTime))
            {
                return null;
            }

            var memberName = memberExpression.Member.Name;
            if (memberName == nameof(DateTime.Millisecond))
            {
                return Expression.Modulo(
                    Expression.Convert(
                        Expression.Multiply(
                            new ExplicitCastExpression(
                                SqliteExpression.Strftime(
                                    typeof(string),
                                    "%f",
                                    memberExpression.Expression),
                                typeof(double)),
                            Expression.Convert(
                                Expression.Constant(1000),
                                typeof(double))),
                        typeof(int)),
                    Expression.Constant(1000));
            }

            if (memberName == nameof(DateTime.Ticks))
            {
                return new ExplicitCastExpression(
                    Expression.Multiply(
                        Expression.Subtract(
                            new SqlFunctionExpression(
                                "julianday",
                                typeof(double),
                                new[] { memberExpression.Expression }),
                            Expression.Constant(1721425.5)), // NB: Result of julianday('0001-01-01 00:00:00')
                        Expression.Convert(
                            Expression.Constant(TimeSpan.TicksPerDay),
                            typeof(double))),
                    typeof(long));
            }

            if (_datePartMapping.TryGetValue(memberName, out var datePart))
            {
                return new ExplicitCastExpression(
                    SqliteExpression.Strftime(
                        typeof(string),
                        datePart,
                        memberExpression.Expression),
                    memberExpression.Type);
            }

            string format = null;
            Expression timestring = null;
            var modifiers = new List<Expression>();

            var datetimeFormat = "%Y-%m-%d %H:%M:%f";
            switch (memberName)
            {
                case nameof(DateTime.Now):
                    format = datetimeFormat;
                    timestring = Expression.Constant("now");
                    modifiers.Add(Expression.Constant("localtime"));
                    break;

                case nameof(DateTime.UtcNow):
                    format = datetimeFormat;
                    timestring = Expression.Constant("now");
                    break;

                case nameof(DateTime.Date):
                    format = datetimeFormat;
                    timestring = memberExpression.Expression;
                    modifiers.Add(Expression.Constant("start of day"));
                    break;

                case nameof(DateTime.Today):
                    format = datetimeFormat;
                    timestring = Expression.Constant("now");
                    modifiers.Add(Expression.Constant("localtime"));
                    modifiers.Add(Expression.Constant("start of day"));
                    break;

                case nameof(DateTime.TimeOfDay):
                    format = "%H:%M:%f";
                    timestring = memberExpression.Expression;
                    break;

                default:
                    return null;
            }

            Debug.Assert(format != null);
            Debug.Assert(timestring != null);

            return new SqlFunctionExpression(
                "rtrim",
                memberExpression.Type,
                new Expression[]
                {
                    new SqlFunctionExpression(
                        "rtrim",
                        memberExpression.Type,
                        new Expression[]
                        {
                            SqliteExpression.Strftime(
                                memberExpression.Type,
                                format,
                                timestring,
                                modifiers),
                            Expression.Constant("0")
                        }),
                    Expression.Constant(".")
                });
        }
    }
}
