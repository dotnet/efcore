// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
    public class SqliteCharMethodTranslator : IMethodCallTranslator
    {
        private static readonly Dictionary<MethodInfo, string> _supportedMethods = new()
        {
            { typeof(char).GetRequiredRuntimeMethod(nameof(char.ToLower), new[] { typeof(char) }), "lower" },
            { typeof(char).GetRequiredRuntimeMethod(nameof(char.ToUpper), new[] { typeof(char) }), "upper" }
        };

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqliteCharMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
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

            if (_supportedMethods.TryGetValue(method, out var sqlFunctionName))
            {
                return _sqlExpressionFactory.Function(
                    sqlFunctionName,
                    arguments,
                    nullable: true,
                    argumentsPropagateNullability: arguments.Select(a => true).ToList(),
                    method.ReturnType,
                    arguments[0].TypeMapping);
            }

            return null;
        }
    }
}
