// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Utilities;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Internal
{
    public class XGDateTimeMemberTranslator : IMemberTranslator
    {
        private static readonly Dictionary<string, (string Part, int Divisor)> _datePartMapping
            = new Dictionary<string, (string, int)>
            {
                { nameof(DateTime.Year), ("year", 1) },
                { nameof(DateTime.Month), ("month", 1) },
                { nameof(DateTime.Day), ("day", 1) },
                { nameof(DateTime.Hour), ("hour", 1) },
                { nameof(DateTime.Minute), ("minute", 1) },
                { nameof(DateTime.Second), ("second", 1) },
                { nameof(DateTime.Millisecond), ("microsecond", 1000) },
            };
        private readonly XGSqlExpressionFactory _sqlExpressionFactory;
        private readonly IXGOptions _xgOptions;

        public XGDateTimeMemberTranslator(ISqlExpressionFactory sqlExpressionFactory, IXGOptions xgOptions)
        {
            _sqlExpressionFactory = (XGSqlExpressionFactory)sqlExpressionFactory;
            _xgOptions = xgOptions;
        }

        public virtual SqlExpression Translate(
            SqlExpression instance,
            MemberInfo member,
            Type returnType,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            var declaringType = member.DeclaringType;

            if (declaringType == typeof(DateTime)
                || declaringType == typeof(DateTimeOffset)
                || declaringType == typeof(DateOnly)
                || declaringType == typeof(TimeOnly))
            {
                var memberName = member.Name;

                if (_datePartMapping.TryGetValue(memberName, out var datePart))
                {
                    var extract = _sqlExpressionFactory.NullableFunction(
                        "EXTRACT",
                        new[]
                        {
                            _sqlExpressionFactory.ComplexFunctionArgument(
                                new [] {
                                    _sqlExpressionFactory.Fragment($"{datePart.Part} FROM"),
                                    instance
                                },
                                " ",
                                typeof(string))
                        },
                        returnType,
                        false);

                    if (datePart.Divisor != 1)
                    {
                        return _sqlExpressionFactory.XGIntegerDivide(
                            extract,
                            _sqlExpressionFactory.Constant(datePart.Divisor));
                    }

                    return extract;
                }

                switch (memberName)
                {
                    case nameof(DateTime.DayOfYear):
                        return _sqlExpressionFactory.NullableFunction(
                        "DAYOFYEAR",
                        new[] { instance },
                        returnType,
                        false);

                    case nameof(DateTime.Date):
                        return _sqlExpressionFactory.NullableFunction(
                            "CONVERT",
                            new[]{
                                instance,
                                _sqlExpressionFactory.Fragment("date")
                            },
                            returnType,
                            false);

                    case nameof(DateTime.TimeOfDay):
                        return _sqlExpressionFactory.Convert(instance, returnType);

                    case nameof(DateTime.Now):
                        return _sqlExpressionFactory.NonNullableFunction(
                            declaringType == typeof(DateTimeOffset)
                                ? "UTC_TIMESTAMP"
                                : "CURRENT_TIMESTAMP",
                            _xgOptions.ServerVersion.Supports.DateTime6 ?
                                new [] { _sqlExpressionFactory.Constant(6)} :
                                Array.Empty<SqlExpression>(),
                            returnType);

                    case nameof(DateTime.UtcNow):
                        return _sqlExpressionFactory.NonNullableFunction(
                            "UTC_TIMESTAMP",
                            _xgOptions.ServerVersion.Supports.DateTime6 ?
                                new [] { _sqlExpressionFactory.Constant(6)} :
                                ArraySegment<SqlExpression>.Empty,
                            returnType);

                    case nameof(DateTime.Today):
                        return _sqlExpressionFactory.NonNullableFunction(
                            declaringType == typeof(DateTimeOffset)
                                ? "UTC_DATE"
                                : "CURDATE",
                            Array.Empty<SqlExpression>(),
                            returnType);

                    case nameof(DateTime.DayOfWeek):
                        return _sqlExpressionFactory.Subtract(
                            _sqlExpressionFactory.NullableFunction(
                                "DAYOFWEEK",
                                new[] { instance },
                                returnType,
                                false),
                            _sqlExpressionFactory.Constant(1));
                }
            }

            if (declaringType == typeof(DateOnly))
            {
                if (member.Name == nameof(DateOnly.DayNumber))
                {
                    return _sqlExpressionFactory.Subtract(
                        _sqlExpressionFactory.NullableFunction(
                            "TO_DAYS",
                            new[] { instance },
                            returnType),
                        _sqlExpressionFactory.Constant(366));
                }
            }

            if (declaringType == typeof(DateTimeOffset))
            {
                switch (member.Name)
                {
                    case nameof(DateTimeOffset.DateTime):
                    case nameof(DateTimeOffset.UtcDateTime):
                        // We represent `DateTimeOffset` values as UTC datetime values in the database. Therefore, `DateTimeOffset`,
                        // `DateTimeOffset.DateTime` and `DateTimeOffset.UtcDateTime` are all the same.
                        return _sqlExpressionFactory.Convert(instance, typeof(DateTime));

                    case nameof(DateTimeOffset.LocalDateTime):
                        return _sqlExpressionFactory.NullableFunction(
                            "CONVERT_TZ",
                            [instance, _sqlExpressionFactory.Constant("+00:00"), _sqlExpressionFactory.Fragment("@@session.time_zone")],
                            typeof(DateTime),
                            null,
                            false,
                            Statics.GetTrueValues(3));
                }
            }

            return null;
        }
    }
}
