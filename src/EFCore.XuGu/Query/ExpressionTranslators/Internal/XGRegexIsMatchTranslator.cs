// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.XuGu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionTranslators.Internal
{
    public class XGRegexIsMatchTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _methodInfo
            = typeof(Regex).GetRuntimeMethod(nameof(Regex.IsMatch), new[] { typeof(string), typeof(string) });

        private readonly XGSqlExpressionFactory _sqlExpressionFactory;

        public XGRegexIsMatchTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = (XGSqlExpressionFactory)sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(
            SqlExpression instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
            => _methodInfo.Equals(method)
                ? _sqlExpressionFactory.Regexp(
                    arguments[0],
                    arguments[1])
                : null;
    }
}
