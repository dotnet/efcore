// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class LikeTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _methodInfo
            = typeof(DbFunctionsExtensions).GetRequiredRuntimeMethod(
                nameof(DbFunctionsExtensions.Like), typeof(DbFunctions), typeof(string), typeof(string));

        private static readonly MethodInfo _methodInfoWithEscape
            = typeof(DbFunctionsExtensions).GetRequiredRuntimeMethod(
                nameof(DbFunctionsExtensions.Like), typeof(DbFunctions), typeof(string), typeof(string), typeof(string));

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public LikeTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
            if (Equals(method, _methodInfo))
            {
                return _sqlExpressionFactory.Like(arguments[1], arguments[2]);
            }

            if (Equals(method, _methodInfoWithEscape))
            {
                return _sqlExpressionFactory.Like(arguments[1], arguments[2], arguments[3]);
            }

            return null;
        }
    }
}
