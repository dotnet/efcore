// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class StringMethodTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _isNullOrEmptyMethodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.IsNullOrEmpty), new[] { typeof(string) });

        private static readonly MethodInfo _concatMethodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.Concat), new[] { typeof(string), typeof(string) });

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public StringMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            if (Equals(method, _isNullOrEmptyMethodInfo))
            {
                var argument = arguments[0];

                return _sqlExpressionFactory.OrElse(
                    _sqlExpressionFactory.IsNull(argument),
                    _sqlExpressionFactory.Equal(
                        argument,
                        _sqlExpressionFactory.Constant(string.Empty)));
            }

            if (Equals(method, _concatMethodInfo))
            {
                return _sqlExpressionFactory.Add(
                    arguments[0],
                    arguments[1]);
            }

            return null;
        }
    }
}
