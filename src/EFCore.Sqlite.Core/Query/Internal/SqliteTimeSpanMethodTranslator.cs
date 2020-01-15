// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal
{
    public class SqliteTimeSpanMethodTranslator : IMethodCallTranslator
    {
        private const double HoursPerDay = 24;
        private const double MinutesPerDay = 60 * HoursPerDay;
        private const double SecondsPerDay = 60 * MinutesPerDay;
        private const double MillisecondsPerDay = 1000 * SecondsPerDay;
        private const double TicksPerDay = TimeSpan.TicksPerDay;

        private static readonly MethodInfo _add = typeof(TimeSpan)
            .GetRuntimeMethod(nameof(TimeSpan.Add), new[] { typeof(TimeSpan) });

        private static readonly MethodInfo _divideDouble = typeof(TimeSpan)
            .GetRuntimeMethod(nameof(TimeSpan.Divide), new[] { typeof(double) });

        private static readonly MethodInfo _divideTimeSpan = typeof(TimeSpan)
            .GetRuntimeMethod(nameof(TimeSpan.Divide), new[] { typeof(TimeSpan) });

        private static readonly MethodInfo _duration = typeof(TimeSpan)
            .GetRuntimeMethod(nameof(TimeSpan.Duration), Type.EmptyTypes);

        private static readonly MethodInfo _fromDays = typeof(TimeSpan)
            .GetRuntimeMethod(nameof(TimeSpan.FromDays), new[] { typeof(double) });

        private static readonly MethodInfo _fromHours = typeof(TimeSpan)
            .GetRuntimeMethod(nameof(TimeSpan.FromHours), new[] { typeof(double) });

        private static readonly MethodInfo _fromMilliseconds = typeof(TimeSpan)
            .GetRuntimeMethod(nameof(TimeSpan.FromMilliseconds), new[] { typeof(double) });

        private static readonly MethodInfo _fromMinutes = typeof(TimeSpan)
            .GetRuntimeMethod(nameof(TimeSpan.FromMinutes), new[] { typeof(double) });

        private static readonly MethodInfo _fromSeconds = typeof(TimeSpan)
            .GetRuntimeMethod(nameof(TimeSpan.FromSeconds), new[] { typeof(double) });

        private static readonly MethodInfo _fromTicks = typeof(TimeSpan)
            .GetRuntimeMethod(nameof(TimeSpan.FromTicks), new[] { typeof(long) });

        private static readonly MethodInfo _multiply = typeof(TimeSpan)
            .GetRuntimeMethod(nameof(TimeSpan.Multiply), new[] { typeof(double) });

        private static readonly MethodInfo _negate = typeof(TimeSpan)
            .GetRuntimeMethod(nameof(TimeSpan.Negate), Type.EmptyTypes);

        private static readonly MethodInfo _subtract = typeof(TimeSpan)
            .GetRuntimeMethod(nameof(TimeSpan.Subtract), new[] { typeof(TimeSpan) });

        private static readonly MethodInfo _dateTimeAdd = typeof(DateTime)
            .GetRuntimeMethod(nameof(DateTime.Add), new[] { typeof(TimeSpan) });

        private static readonly MethodInfo _dateTimeSubtractDateTime = typeof(DateTime)
            .GetRuntimeMethod(nameof(DateTime.Subtract), new[] { typeof(DateTime) });

        private static readonly MethodInfo _dateTimeSubtractTimeSpan = typeof(DateTime)
            .GetRuntimeMethod(nameof(DateTime.Subtract), new[] { typeof(TimeSpan) });

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqliteTimeSpanMethodTranslator([NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            if (Equals(method, _add))
            {
                return _sqlExpressionFactory.Function(
                    "ef_timespan",
                    new[]
                    {
                        _sqlExpressionFactory.Add(
                            SqliteExpression.Days(
                                _sqlExpressionFactory,
                                instance),
                            SqliteExpression.Days(
                                _sqlExpressionFactory,
                                arguments[0]))
                    },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true },
                    typeof(TimeSpan));
            }
            else if (Equals(method, _divideDouble))
            {
                return _sqlExpressionFactory.Function(
                    "ef_timespan",
                    new[]
                    {
                        _sqlExpressionFactory.Divide(
                            SqliteExpression.Days(
                                _sqlExpressionFactory,
                                instance),
                            arguments[0])
                    },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true },
                    typeof(TimeSpan));
            }
            else if (Equals(method, _divideTimeSpan))
            {
                return _sqlExpressionFactory.Function(
                    "ef_timespan",
                    new[]
                    {
                        _sqlExpressionFactory.Divide(
                            SqliteExpression.Days(
                                _sqlExpressionFactory,
                                instance),
                            SqliteExpression.Days(
                                _sqlExpressionFactory,
                                arguments[0]))
                    },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true },
                    typeof(TimeSpan));
            }
            else if (Equals(method, _duration))
            {
                return _sqlExpressionFactory.Function(
                    "ef_timespan",
                    new[]
                    {
                        _sqlExpressionFactory.Function(
                            "abs",
                            new[]
                            {
                                SqliteExpression.Days(
                                    _sqlExpressionFactory,
                                    instance)
                            },
                            nullable: true,
                            argumentsPropagateNullability: new[] { true },
                            typeof(double))
                    },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true },
                    typeof(TimeSpan));
            }
            else if (Equals(method, _fromDays))
            {
                return _sqlExpressionFactory.Function(
                    "ef_timespan",
                    new[] { arguments[0] },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true },
                    typeof(TimeSpan));
            }
            else if (Equals(method, _fromHours))
            {
                return _sqlExpressionFactory.Function(
                    "ef_timespan",
                    new[]
                    {
                        _sqlExpressionFactory.Divide(
                            arguments[0],
                            _sqlExpressionFactory.Constant(HoursPerDay))
                    },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true },
                    typeof(TimeSpan));
            }
            else if (Equals(method, _fromMilliseconds))
            {
                return _sqlExpressionFactory.Function(
                    "ef_timespan",
                    new[]
                    {
                        _sqlExpressionFactory.Divide(
                            arguments[0],
                            _sqlExpressionFactory.Constant(MillisecondsPerDay))
                    },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true },
                    typeof(TimeSpan));
            }
            else if (Equals(method, _fromMinutes))
            {
                return _sqlExpressionFactory.Function(
                    "ef_timespan",
                    new[]
                    {
                        _sqlExpressionFactory.Divide(
                            arguments[0],
                            _sqlExpressionFactory.Constant(MinutesPerDay))
                    },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true },
                    typeof(TimeSpan));
            }
            else if (Equals(method, _fromSeconds))
            {
                return _sqlExpressionFactory.Function(
                    "ef_timespan",
                    new[]
                    {
                        _sqlExpressionFactory.Divide(
                            arguments[0],
                            _sqlExpressionFactory.Constant(SecondsPerDay))
                    },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true },
                    typeof(TimeSpan));
            }
            else if (Equals(method, _fromTicks))
            {
                return _sqlExpressionFactory.Function(
                    "ef_timespan",
                    new[]
                    {
                        _sqlExpressionFactory.Divide(
                            arguments[0],
                            _sqlExpressionFactory.Constant(TicksPerDay))
                    },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true },
                    typeof(TimeSpan));
            }
            else if (Equals(method, _multiply))
            {
                return _sqlExpressionFactory.Function(
                    "ef_timespan",
                    new[]
                    {
                        _sqlExpressionFactory.Multiply(
                            SqliteExpression.Days(
                                _sqlExpressionFactory,
                                instance),
                            arguments[0])
                    },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true },
                    typeof(TimeSpan));
            }
            else if (Equals(method, _negate))
            {
                return _sqlExpressionFactory.Function(
                    "ef_timespan",
                    new[]
                    {
                        _sqlExpressionFactory.Negate(
                            SqliteExpression.Days(
                                _sqlExpressionFactory,
                                instance))
                    },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true },
                    typeof(TimeSpan));
            }
            else if (Equals(method, _subtract))
            {
                return _sqlExpressionFactory.Function(
                    "ef_timespan",
                    new[]
                    {
                        _sqlExpressionFactory.Subtract(
                            SqliteExpression.Days(
                                _sqlExpressionFactory,
                                instance),
                            SqliteExpression.Days(
                                _sqlExpressionFactory,
                                arguments[0]))
                    },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true },
                    typeof(TimeSpan));
            }
            else if (Equals(method, _dateTimeAdd))
            {
                return SqliteExpression.DateTime(
                    _sqlExpressionFactory,
                    typeof(DateTime),
                    _sqlExpressionFactory.Add(
                        SqliteExpression.JulianDay(
                            _sqlExpressionFactory,
                            instance),
                        SqliteExpression.Days(
                            _sqlExpressionFactory,
                            arguments[0])));
            }
            else if (Equals(method, _dateTimeSubtractDateTime))
            {
                return _sqlExpressionFactory.Function(
                    "ef_timespan",
                    new[]
                    {
                        _sqlExpressionFactory.Subtract(
                            SqliteExpression.JulianDay(
                                _sqlExpressionFactory,
                                instance),
                            SqliteExpression.JulianDay(
                                _sqlExpressionFactory,
                                arguments[0]))
                    },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true },
                    typeof(TimeSpan));
            }
            else if (Equals(method, _dateTimeSubtractTimeSpan))
            {
                return SqliteExpression.DateTime(
                    _sqlExpressionFactory,
                    typeof(DateTime),
                    _sqlExpressionFactory.Subtract(
                        SqliteExpression.JulianDay(
                            _sqlExpressionFactory,
                            instance),
                        SqliteExpression.Days(
                            _sqlExpressionFactory,
                            arguments[0])));
            }

            return null;
        }
    }
}
