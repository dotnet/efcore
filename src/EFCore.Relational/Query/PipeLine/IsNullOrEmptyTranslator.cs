// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class IsNullOrEmptyTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _methodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.IsNullOrEmpty), new[] { typeof(string) });

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public IsNullOrEmptyTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public SqlExpression Translate(SqlExpression instance, MethodInfo method, IList<SqlExpression> arguments)
        {
            if (Equals(method, _methodInfo))
            {
                var argument = arguments[0];
                Debug.Assert(argument.TypeMapping != null, "Must have typeMapping.");

                return _sqlExpressionFactory.OrElse(
                    _sqlExpressionFactory.IsNull(argument),
                    _sqlExpressionFactory.Equal(
                        argument,
                        _sqlExpressionFactory.Constant(string.Empty)));
            }

            return null;
        }
    }
}
