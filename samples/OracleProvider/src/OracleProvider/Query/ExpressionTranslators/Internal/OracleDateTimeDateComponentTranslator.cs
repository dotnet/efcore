// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class OracleDateTimeMemberTranslator : IMemberTranslator
    {
        private static readonly Dictionary<string, string> _datePartMapping
            = new Dictionary<string, string>
            {
                { nameof(DateTime.Year), "YEAR" },
                { nameof(DateTime.Month), "MONTH" },
                { nameof(DateTime.Day), "DAY" },
                { nameof(DateTime.Hour), "HOUR" },
                { nameof(DateTime.Minute), "MINUTE" },
                { nameof(DateTime.Second), "SECOND" },
            };

        public virtual Expression Translate(MemberExpression memberExpression)
        {
            var declaringType = memberExpression.Member.DeclaringType;
            if (declaringType == typeof(DateTime)
                || declaringType == typeof(DateTimeOffset))
            {
                var member = memberExpression.Member.Name;

                if (_datePartMapping.TryGetValue(member, out var datePart))
                {
                    if (declaringType == typeof(DateTimeOffset)
                        && (string.Equals(datePart, "HOUR")
                            || string.Equals(datePart, "MINUTE")))
                    {
                        // TODO: See issue#10515
                        return null;
                        //datePart = "TIMEZONE_" + datePart;
                    }

                    return new SqlFunctionExpression(
                        "EXTRACT",
                        memberExpression.Type,
                        arguments: new[] { new SqlFragmentExpression(datePart), memberExpression.Expression });
                }

                switch (memberExpression.Member.Name)
                {
                    case nameof(DateTime.Now):
                        var sysDate = new SqlFragmentExpression("SYSDATE");
                        return declaringType == typeof(DateTimeOffset)
                            ? (Expression)new ExplicitCastExpression(sysDate, typeof(DateTimeOffset))
                            : sysDate;
                    case nameof(DateTime.UtcNow):
                        var sysTimeStamp = new SqlFragmentExpression("SYSTIMESTAMP");
                        return declaringType == typeof(DateTimeOffset)
                            ? (Expression)new ExplicitCastExpression(sysTimeStamp, typeof(DateTimeOffset))
                            : sysTimeStamp;
                    case nameof(DateTime.Date):
                        return new SqlFunctionExpression(
                            "TRUNC",
                            memberExpression.Type,
                            new[] { memberExpression.Expression });
                }
            }

            return null;
        }
    }
}
