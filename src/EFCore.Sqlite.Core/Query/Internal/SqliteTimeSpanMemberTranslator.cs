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
    public class SqliteTimeSpanMemberTranslator : IMemberTranslator
    {
        private static readonly Dictionary<string, string> _datePartMapping
            = new Dictionary<string, string>
            {
                { nameof(TimeSpan.Hours), "%H" },
                { nameof(TimeSpan.Minutes), "%M" },
                { nameof(TimeSpan.Seconds), "%S" }
            };

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqliteTimeSpanMemberTranslator([NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType)
        {
            Check.NotNull(member, nameof(member));
            Check.NotNull(returnType, nameof(returnType));

            var declaringType = member.DeclaringType;

            if (declaringType == typeof(TimeSpan))
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
                    case nameof(TimeSpan.TotalMilliseconds):
                        return GetTotalTimeExpression(instance, 1.0 / 1000);
                    case nameof(TimeSpan.TotalSeconds):
                        return GetTotalTimeExpression(instance, 1);
                    case nameof(TimeSpan.TotalMinutes):
                        return GetTotalTimeExpression(instance, 60);
                    case nameof(TimeSpan.TotalHours):
                        return GetTotalTimeExpression(instance, 60 * 60);
                    case nameof(TimeSpan.TotalDays):
                        return GetTotalTimeExpression(instance, 24 * 60 * 60);
                }
            }

            return null;
        }

        private SqlExpression GetTotalTimeExpression(SqlExpression timeSpan, double secondsFactor)
        {
            var totalNanoSeconds = GetSecondsExpression(timeSpan);
                        
            return _sqlExpressionFactory.Divide(_sqlExpressionFactory.Convert(totalNanoSeconds, typeof(double)), _sqlExpressionFactory.Fragment(secondsFactor.ToString()));
        }

        private SqlExpression GetSecondsExpression(SqlExpression timeSpan)
        {
            var hoursExpression =_sqlExpressionFactory.Convert(
                SqliteExpression.Strftime(
                    _sqlExpressionFactory,
                    typeof(string),
                    "%H",
                    timeSpan),
                typeof(double));
            var hoursInSeconds =
                _sqlExpressionFactory.Multiply(hoursExpression, _sqlExpressionFactory.Fragment((60 * 60).ToString()));

            var minutesExpression =_sqlExpressionFactory.Convert(
                SqliteExpression.Strftime(
                    _sqlExpressionFactory,
                    typeof(string),
                    "%M",
                    timeSpan),
                typeof(double));
            var minutesInSeconds = _sqlExpressionFactory.Multiply(
                minutesExpression, _sqlExpressionFactory.Fragment(60.ToString()));

            var secondsExpression = _sqlExpressionFactory.Convert(
                SqliteExpression.Strftime(
                    _sqlExpressionFactory,
                    typeof(string),
                    "%S",
                    timeSpan),
                typeof(double));


            return _sqlExpressionFactory.Add(
                hoursInSeconds, _sqlExpressionFactory.Add(minutesInSeconds, secondsExpression));
        }
    }
}
