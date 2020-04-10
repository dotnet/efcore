// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class CollateTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _methodInfo
            = typeof(RelationalDbFunctionsExtensions).GetMethod(nameof(RelationalDbFunctionsExtensions.Collate));

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public CollateTranslator([NotNull] ISqlExpressionFactory sqlExpressionFactory)
            => _sqlExpressionFactory = sqlExpressionFactory;

        public virtual SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            Check.NotNull(method, nameof(method));
            Check.NotNull(arguments, nameof(arguments));

            return method.IsGenericMethod
                && Equals(method.GetGenericMethodDefinition(), _methodInfo)
                && arguments[2] is SqlConstantExpression constantExpression
                && constantExpression.Value is string collation
                    ? new CollateExpression(arguments[1], collation)
                    : null;
        }
    }
}
