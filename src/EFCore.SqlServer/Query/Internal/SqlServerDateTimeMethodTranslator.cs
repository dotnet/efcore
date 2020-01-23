// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    public class SqlServerDateTimeMethodTranslator : IMethodCallTranslator
    {
        private readonly Dictionary<MethodInfo, string> _methodInfoDatePartMapping = new Dictionary<MethodInfo, string>
        {
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddYears), new[] { typeof(int) }), "year" },
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddMonths), new[] { typeof(int) }), "month" },
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddDays), new[] { typeof(double) }), "day" },
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddHours), new[] { typeof(double) }), "hour" },
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddMinutes), new[] { typeof(double) }), "minute" },
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddSeconds), new[] { typeof(double) }), "second" },
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddMilliseconds), new[] { typeof(double) }), "millisecond" },
            { typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddYears), new[] { typeof(int) }), "year" },
            { typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddMonths), new[] { typeof(int) }), "month" },
            { typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddDays), new[] { typeof(double) }), "day" },
            { typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddHours), new[] { typeof(double) }), "hour" },
            { typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddMinutes), new[] { typeof(double) }), "minute" },
            { typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddSeconds), new[] { typeof(double) }), "second" },
            { typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddMilliseconds), new[] { typeof(double) }), "millisecond" }
        };

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqlServerDateTimeMethodTranslator([NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            Check.NotNull(method, nameof(method));
            Check.NotNull(arguments, nameof(arguments));

            if (_methodInfoDatePartMapping.TryGetValue(method, out var datePart))
            {
                // DateAdd does not accept number argument outside of int range
                // AddYears/AddMonths take int argument so no need to check for range
                return !datePart.Equals("year")
                    && !datePart.Equals("month")
                    && arguments[0] is SqlConstantExpression sqlConstant
                    && ((double)sqlConstant.Value >= int.MaxValue
                        || (double)sqlConstant.Value <= int.MinValue)
                        ? null
                        : _sqlExpressionFactory.Function(
                            "DATEADD",
                            new[]
                            {
                                _sqlExpressionFactory.Fragment(datePart),
                                _sqlExpressionFactory.Convert(arguments[0], typeof(int)),
                                instance
                            },
                            nullResultAllowed: true,
                            argumentsPropagateNullability: new[] { false, true, true },
                            instance.Type,
                            instance.TypeMapping);
            }

            return null;
        }
    }
}
