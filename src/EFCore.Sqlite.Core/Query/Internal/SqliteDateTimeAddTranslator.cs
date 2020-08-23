// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqliteDateTimeAddTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _addMilliseconds
            = typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddMilliseconds), new[] { typeof(double) });

        private static readonly MethodInfo _addTicks
            = typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddTicks), new[] { typeof(long) });

        private readonly Dictionary<MethodInfo, string> _methodInfoToUnitSuffix = new Dictionary<MethodInfo, string>
        {
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddYears), new[] { typeof(int) }), " years" },
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddMonths), new[] { typeof(int) }), " months" },
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddDays), new[] { typeof(double) }), " days" },
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddHours), new[] { typeof(double) }), " hours" },
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddMinutes), new[] { typeof(double) }), " minutes" },
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddSeconds), new[] { typeof(double) }), " seconds" }
        };

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqliteDateTimeAddTranslator([NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlExpression Translate(
            SqlExpression instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            Check.NotNull(method, nameof(method));
            Check.NotNull(arguments, nameof(arguments));
            Check.NotNull(logger, nameof(logger));

            SqlExpression modifier = null;
            if (_addMilliseconds.Equals(method))
            {
                modifier = _sqlExpressionFactory.Add(
                    _sqlExpressionFactory.Convert(
                        _sqlExpressionFactory.Divide(
                            arguments[0],
                            _sqlExpressionFactory.Constant(1000.0)),
                        typeof(string)),
                    _sqlExpressionFactory.Constant(" seconds"));
            }
            else if (_addTicks.Equals(method))
            {
                modifier = _sqlExpressionFactory.Add(
                    _sqlExpressionFactory.Convert(
                        _sqlExpressionFactory.Divide(
                            arguments[0],
                            _sqlExpressionFactory.Constant((double)TimeSpan.TicksPerDay)),
                        typeof(string)),
                    _sqlExpressionFactory.Constant(" seconds"));
            }
            else if (_methodInfoToUnitSuffix.TryGetValue(method, out var unitSuffix))
            {
                modifier = _sqlExpressionFactory.Add(
                    _sqlExpressionFactory.Convert(arguments[0], typeof(string)),
                    _sqlExpressionFactory.Constant(unitSuffix));
            }

            if (modifier != null)
            {
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
                                    method.ReturnType,
                                    "%Y-%m-%d %H:%M:%f",
                                    instance,
                                    new[] { modifier }),
                                _sqlExpressionFactory.Constant("0")
                            },
                            nullable: true,
                            argumentsPropagateNullability: new[] { true, false },
                            method.ReturnType),
                        _sqlExpressionFactory.Constant(".")
                    },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true, false },
                    method.ReturnType);
            }

            return null;
        }
    }
}
