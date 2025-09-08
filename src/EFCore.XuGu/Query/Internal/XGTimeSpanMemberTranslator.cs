// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Internal
{
    public class XGTimeSpanMemberTranslator : IMemberTranslator
    {
        private static readonly Dictionary<string, (string Part, int Divisor)> _datePartMapping
            = new Dictionary<string, (string, int)>
            {
                { nameof(TimeSpan.Hours), ("hour", 1) },
                { nameof(TimeSpan.Minutes), ("minute", 1) },
                { nameof(TimeSpan.Seconds), ("second", 1) },
                { nameof(TimeSpan.Milliseconds), ("microsecond", 1000) },
            };
        private readonly XGSqlExpressionFactory _sqlExpressionFactory;

        public XGTimeSpanMemberTranslator(XGSqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(
            SqlExpression instance,
            MemberInfo member,
            Type returnType,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            var declaringType = member.DeclaringType;
            var memberName = member.Name;

            if (declaringType == typeof(TimeSpan) &&
                _datePartMapping.TryGetValue(memberName, out var datePart))
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

            return null;
        }
    }
}
