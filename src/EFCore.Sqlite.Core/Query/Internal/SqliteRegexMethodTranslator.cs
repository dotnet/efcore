﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
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
    public class SqliteRegexMethodTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _regexIsMatchMethodInfo
            = typeof(Regex).GetRequiredRuntimeMethod(nameof(Regex.IsMatch), new Type[] { typeof(string), typeof(string) });

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqliteRegexMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            Check.NotNull(sqlExpressionFactory, nameof(sqlExpressionFactory));

            _sqlExpressionFactory = sqlExpressionFactory;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlExpression? Translate(
            SqlExpression? instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            Check.NotNull(method, nameof(method));
            Check.NotNull(arguments, nameof(arguments));
            Check.NotNull(logger, nameof(logger));

            if (method.Equals(_regexIsMatchMethodInfo))
            {
                var input = arguments[0];
                var pattern = arguments[1];
                var stringTypeMapping = ExpressionExtensions.InferTypeMapping(input, pattern);

                return _sqlExpressionFactory.Function(
                    "regexp",
                    new[]
                    {
                        _sqlExpressionFactory.ApplyTypeMapping(pattern, stringTypeMapping),
                        _sqlExpressionFactory.ApplyTypeMapping(input, stringTypeMapping)
                    },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true, true },
                    typeof(bool));
            }

            return null;
        }
    }
}
