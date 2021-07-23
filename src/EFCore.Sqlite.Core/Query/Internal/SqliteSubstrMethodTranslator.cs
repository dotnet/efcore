﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    public class SqliteSubstrMethodTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _methodInfo = typeof(SqliteDbFunctionsExtensions)
            .GetRequiredMethod(nameof(SqliteDbFunctionsExtensions.Substr), typeof(DbFunctions), typeof(byte[]), typeof(int));

        private static readonly MethodInfo _methodInfoWithLength = typeof(SqliteDbFunctionsExtensions)
            .GetRequiredMethod(
                nameof(SqliteDbFunctionsExtensions.Substr), typeof(DbFunctions), typeof(byte[]), typeof(int), typeof(int));

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqliteSubstrMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
            => _sqlExpressionFactory = Check.NotNull(sqlExpressionFactory, nameof(sqlExpressionFactory));

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

            if (method.Equals(_methodInfo)
                || method.Equals(_methodInfoWithLength))
            {
                return _sqlExpressionFactory.Function(
                    "substr",
                    arguments.Skip(1),
                    nullable: true,
                    arguments.Skip(1).Select(a => true).ToArray(),
                    typeof(byte[]),
                    arguments[1].TypeMapping);
            }

            return null;
        }
    }
}
