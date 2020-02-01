// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal
{
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

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqliteDateTimeMemberTranslator([NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
            Check.NotNull(member, nameof(member));
            Check.NotNull(returnType, nameof(returnType));

            if (member.DeclaringType == typeof(DateTime))
            {
                var memberName = member.Name;

                if (_datePartMapping.TryGetValue(memberName, out var datePart))
                {
                    return _sqlExpressionFactory.Convert(
                        SqliteExpression.Strftime(
                            _sqlExpressionFactory,
                            typeof(string),
                            datePart,
                            instance),
                        returnType);
                }

                if (string.Equals(memberName, nameof(DateTime.Ticks)))
                {
                    return _sqlExpressionFactory.Convert(
                        _sqlExpressionFactory.Multiply(
                            _sqlExpressionFactory.Subtract(
                                _sqlExpressionFactory.Function(
                                    "julianday",
                                    new[] { instance },
                                    nullResultAllowed: true,
                                    argumentsPropagateNullability: new[] { true },
                                    typeof(double)),
                                _sqlExpressionFactory.Constant(1721425.5)), // NB: Result of julianday('0001-01-01 00:00:00')
                            _sqlExpressionFactory.Constant(TimeSpan.TicksPerDay)),
                        typeof(long));
                }

                if (string.Equals(memberName, nameof(DateTime.Millisecond)))
                {
                    return _sqlExpressionFactory.Modulo(
                        _sqlExpressionFactory.Multiply(
                            _sqlExpressionFactory.Convert(
                                SqliteExpression.Strftime(
                                    _sqlExpressionFactory,
                                    typeof(string),
                                    "%f",
                                    instance),
                                typeof(double)),
                            _sqlExpressionFactory.Constant(1000)),
                        _sqlExpressionFactory.Constant(1000));
                }

                var format = "%Y-%m-%d %H:%M:%f";
                SqlExpression timestring;
                var modifiers = new List<SqlExpression>();

                switch (memberName)
                {
                    case nameof(DateTime.Now):
                        timestring = _sqlExpressionFactory.Constant("now");
                        modifiers.Add(_sqlExpressionFactory.Constant("localtime"));
                        break;

                    case nameof(DateTime.UtcNow):
                        timestring = _sqlExpressionFactory.Constant("now");
                        break;

                    case nameof(DateTime.Date):
                        timestring = instance;
                        modifiers.Add(_sqlExpressionFactory.Constant("start of day"));
                        break;

                    case nameof(DateTime.Today):
                        timestring = _sqlExpressionFactory.Constant("now");
                        modifiers.Add(_sqlExpressionFactory.Constant("localtime"));
                        modifiers.Add(_sqlExpressionFactory.Constant("start of day"));
                        break;

                    case nameof(DateTime.TimeOfDay):
                        format = "%H:%M:%f";
                        timestring = instance;
                        break;

                    default:
                        return null;
                }

                Check.DebugAssert(timestring != null, "timestring is null");

                return _sqlExpressionFactory.Function(
                    "rtrim",
                    new SqlExpression[]
                    {
                        _sqlExpressionFactory.Function(
                            "rtrim",
                            new SqlExpression[]
                            {
                                SqliteExpression.Strftime(
                                    _sqlExpressionFactory,
                                    returnType,
                                    format,
                                    timestring,
                                    modifiers),
                                _sqlExpressionFactory.Constant("0")
                            },
                            nullResultAllowed: true,
                            argumentsPropagateNullability: new[] { true, false },
                            returnType),
                        _sqlExpressionFactory.Constant(".")
                    },
                    nullResultAllowed: true,
                    argumentsPropagateNullability: new[] { true, false },
                    returnType);
            }

            return null;
        }
    }
}
