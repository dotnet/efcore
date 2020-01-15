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

                switch (memberName)
                {
                    case nameof(DateTime.Ticks):
                        return _sqlExpressionFactory.Convert(
                            _sqlExpressionFactory.Multiply(
                                _sqlExpressionFactory.Subtract(
                                        SqliteExpression.JulianDay(
                                            _sqlExpressionFactory,
                                            instance),
                                        _sqlExpressionFactory.Constant(1721425.5)), // NB: Result of julianday('0001-01-01 00:00:00')
                                    _sqlExpressionFactory.Constant(TimeSpan.TicksPerDay)),
                                typeof(long));

                    case nameof(DateTime.Millisecond):
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

                    case nameof(DateTime.Now):
                        return SqliteExpression.DateTime(
                            _sqlExpressionFactory,
                            returnType,
                            _sqlExpressionFactory.Constant("now"),
                            new SqlExpression[] { _sqlExpressionFactory.Constant("localtime") });

                    case nameof(DateTime.UtcNow):
                        return SqliteExpression.DateTime(
                            _sqlExpressionFactory,
                            returnType,
                            _sqlExpressionFactory.Constant("now"));

                    case nameof(DateTime.Date):
                        return SqliteExpression.DateTime(
                            _sqlExpressionFactory,
                            returnType,
                            instance,
                            new SqlExpression[] { _sqlExpressionFactory.Constant("start of day") });

                    case nameof(DateTime.Today):
                        return SqliteExpression.DateTime(
                            _sqlExpressionFactory,
                            returnType,
                            _sqlExpressionFactory.Constant("now"),
                            new SqlExpression[]
                            {
                                _sqlExpressionFactory.Constant("localtime"),
                                _sqlExpressionFactory.Constant("start of day")
                            });

                    case nameof(DateTime.TimeOfDay):
                        return SqliteExpression.Time(
                            _sqlExpressionFactory,
                            returnType,
                            instance);
                }
            }

            return null;
        }
    }
}
