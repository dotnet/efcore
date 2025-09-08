// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class XGConvertTranslator : IMethodCallTranslator
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        private static readonly MethodInfo[] _supportedMethods
            = new []
                {
                    nameof(Convert.ToBoolean),
                    nameof(Convert.ToByte),
                    nameof(Convert.ToDecimal),
                    nameof(Convert.ToDouble),
                    nameof(Convert.ToInt16),
                    nameof(Convert.ToInt32),
                    nameof(Convert.ToInt64),
                    nameof(Convert.ToString),
                }
                .SelectMany(t => typeof(Convert).GetTypeInfo().GetDeclaredMethods(t)
                    .Where(m => m.GetParameters().Length == 1))
                .ToArray();

        public XGConvertTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual SqlExpression Translate(
            SqlExpression instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
            => _supportedMethods.Contains(method)
                ? _sqlExpressionFactory.Convert(
                    arguments[0],
                    method.ReturnType)
                : null;
    }
}
