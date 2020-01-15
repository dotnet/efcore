// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal
{
    public class SqliteTimeSpanMemberTranslator : IMemberTranslator
    {
        private const int HoursPerDay = 24;
        private const int MinutesPerDay = 60 * HoursPerDay;
        private const int SecondsPerDay = 60 * MinutesPerDay;
        private const int MillisecondsPerDay = 1000 * SecondsPerDay;
        private const double DaysPerHour = 1.0 / HoursPerDay;
        private const double DaysPerMinute = 1.0 / MinutesPerDay;
        private const double DaysPerSecond = 1.0 / SecondsPerDay;

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqliteTimeSpanMemberTranslator([NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
            Check.NotNull(member, nameof(member));
            Check.NotNull(returnType, nameof(returnType));

            if (member.DeclaringType != typeof(TimeSpan))
                return null;

            switch (member.Name)
            {
                case nameof(TimeSpan.Days):
                    return _sqlExpressionFactory.Convert(
                        SqliteExpression.Days(
                            _sqlExpressionFactory,
                            instance),
                        typeof(int));

                case nameof(TimeSpan.Hours):
                    return _sqlExpressionFactory.Multiply(
                        _sqlExpressionFactory.Function(
                            "ef_mod",
                            new[]
                            {
                                SqliteExpression.Days(
                                    _sqlExpressionFactory,
                                    instance),
                                _sqlExpressionFactory.Constant(1.0)
                            },
                            nullable: true,
                            argumentsPropagateNullability: new[] { true, true },
                            typeof(double)),
                        _sqlExpressionFactory.Constant(HoursPerDay));

                case nameof(TimeSpan.Minutes):
                    return _sqlExpressionFactory.Multiply(
                        _sqlExpressionFactory.Function(
                            "ef_mod",
                            new[]
                            {
                                SqliteExpression.Days(
                                    _sqlExpressionFactory,
                                    instance),
                                _sqlExpressionFactory.Constant(DaysPerHour)
                            },
                            nullable: true,
                            argumentsPropagateNullability: new[] { true, true },
                            typeof(double)),
                        _sqlExpressionFactory.Constant(MinutesPerDay));

                case nameof(TimeSpan.Seconds):
                    // TODO: Round in more places. Cast to INTEGER
                    return _sqlExpressionFactory.Function(
                            "round",
                            new SqlExpression[]
                            {
                                _sqlExpressionFactory.Multiply(
                                    _sqlExpressionFactory.Function(
                                        "ef_mod",
                                        new[]
                                        {
                                            SqliteExpression.Days(
                                                _sqlExpressionFactory,
                                                instance),
                                            _sqlExpressionFactory.Constant(DaysPerMinute)
                                        },
                                        nullable: true,
                                        argumentsPropagateNullability: new[] { true, true },
                                        typeof(double)),
                                    _sqlExpressionFactory.Constant(SecondsPerDay)),
                                _sqlExpressionFactory.Constant(3)
                            },
                            nullable: true,
                            argumentsPropagateNullability: new[] { true, true },
                            typeof(double));

                case nameof(TimeSpan.Milliseconds):
                    return _sqlExpressionFactory.Multiply(
                        _sqlExpressionFactory.Function(
                            "ef_mod",
                            new[]
                            {
                                SqliteExpression.Days(
                                    _sqlExpressionFactory,
                                    instance),
                                _sqlExpressionFactory.Constant(DaysPerSecond)
                            },
                            nullable: true,
                            argumentsPropagateNullability: new[] { true, true },
                            typeof(double)),
                        _sqlExpressionFactory.Constant(MillisecondsPerDay));

                case nameof(TimeSpan.Ticks):
                    return _sqlExpressionFactory.Convert(
                        _sqlExpressionFactory.Multiply(
                            SqliteExpression.Days(
                                _sqlExpressionFactory,
                                instance),
                            _sqlExpressionFactory.Constant(TimeSpan.TicksPerDay)),
                        typeof(long));

                case nameof(TimeSpan.TotalDays):
                    return SqliteExpression.Days(
                        _sqlExpressionFactory,
                        instance);

                case nameof(TimeSpan.TotalHours):
                    return _sqlExpressionFactory.Multiply(
                        SqliteExpression.Days(
                            _sqlExpressionFactory,
                            instance),
                        _sqlExpressionFactory.Constant(HoursPerDay));

                case nameof(TimeSpan.TotalMinutes):
                    return _sqlExpressionFactory.Multiply(
                        SqliteExpression.Days(
                            _sqlExpressionFactory,
                            instance),
                        _sqlExpressionFactory.Constant(MinutesPerDay));

                case nameof(TimeSpan.TotalSeconds):
                    return _sqlExpressionFactory.Multiply(
                        SqliteExpression.Days(
                            _sqlExpressionFactory,
                            instance),
                        _sqlExpressionFactory.Constant(SecondsPerDay));

                case nameof(TimeSpan.TotalMilliseconds):
                    return _sqlExpressionFactory.Multiply(
                        SqliteExpression.Days(
                            _sqlExpressionFactory,
                            instance),
                        _sqlExpressionFactory.Constant(MillisecondsPerDay));
            }

            return null;
        }
    }
}
