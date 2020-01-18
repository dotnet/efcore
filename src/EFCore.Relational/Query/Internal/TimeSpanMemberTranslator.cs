// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class TimeSpanMemberTranslator : IMemberTranslator
    {
        private static readonly Dictionary<string, string> _datePartMapping
            = new Dictionary<string, string>
            {
                { nameof(TimeSpan.Hours), "hour" },
                { nameof(TimeSpan.Minutes), "minute" },
                { nameof(TimeSpan.Seconds), "second" },
                { nameof(TimeSpan.Milliseconds), "millisecond" }
            };

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public TimeSpanMemberTranslator([NotNull] ISqlExpressionFactory sqlExpressionFactory)
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
                    return _sqlExpressionFactory.Function(
                        "DATEPART",
                        new[] { _sqlExpressionFactory.Fragment(datePart), instance },
                        returnType);
                }

                switch (memberName)
                {
                    case nameof(TimeSpan.TotalMilliseconds):
                        return GetTotalTimeExpression(instance, 1000 * 1000);
                    case nameof(TimeSpan.TotalSeconds):
                        return GetTotalTimeExpression(instance, 1000 * 1000 * 1000);
                    case nameof(TimeSpan.TotalMinutes):
                        return GetTotalTimeExpression(instance, 60L * 1000 * 1000 * 1000);
                    case nameof(TimeSpan.TotalHours):
                        return GetTotalTimeExpression(instance, 60L * 60 * 1000 * 1000 * 1000);
                    case nameof(TimeSpan.TotalDays):
                        return GetTotalTimeExpression(instance, 24L * 60 * 60 * 1000 * 1000 * 1000);
                }
            }

            return null;
        }

        private SqlExpression GetTotalTimeExpression(SqlExpression timeSpan, long nanosFactor)
        {
            var totalNanoSeconds = GetNanosecondsExpression(timeSpan);
                        
            return _sqlExpressionFactory.Divide(_sqlExpressionFactory.Convert(totalNanoSeconds, typeof(double)), _sqlExpressionFactory.Fragment(nanosFactor.ToString()));
        }

        private SqlExpression GetNanosecondsExpression(SqlExpression timeSpan)
        {
            var hoursExpression = _sqlExpressionFactory.Function(
                "DATEPART",
                new[] { _sqlExpressionFactory.Fragment("hour"), timeSpan },
                typeof(int));
            var hoursInNanos =
                _sqlExpressionFactory.Multiply(hoursExpression, _sqlExpressionFactory.Fragment((60 * 60 * 1000000000L).ToString()));

            var minutesExpression = _sqlExpressionFactory.Function(
                "DATEPART",
                new[] { _sqlExpressionFactory.Fragment("minute"), timeSpan },
                typeof(int));
            var minutesInNanos = _sqlExpressionFactory.Multiply(
                minutesExpression, _sqlExpressionFactory.Fragment((60 * 1000000000L).ToString()));

            var secondsExpression = _sqlExpressionFactory.Function(
                "DATEPART",
                new[] { _sqlExpressionFactory.Fragment("second"), timeSpan },
                typeof(int));
            var secondsInNanos = _sqlExpressionFactory.Multiply(
                secondsExpression, _sqlExpressionFactory.Fragment((1000000000).ToString()));

            var nanosecondsExpression = _sqlExpressionFactory.Function(
                "DATEPART",
                new[] { _sqlExpressionFactory.Fragment("nanosecond"), timeSpan },
                typeof(int));

            return _sqlExpressionFactory.Add(
                hoursInNanos, _sqlExpressionFactory.Add(minutesInNanos, _sqlExpressionFactory.Add(secondsInNanos, nanosecondsExpression)));
        }
    }
}
