// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class GetValueOrDefaultTranslator : IMethodCallTranslator
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        public GetValueOrDefaultTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            if (method.Name == nameof(Nullable<int>.GetValueOrDefault)
                && method.ReturnType.IsNumeric())
            {
                return _sqlExpressionFactory.Coalesce(
                    instance,
                    arguments.Count == 0
                            ? GetDefaultConstant(method.ReturnType)
                            : arguments[0],
                    instance.TypeMapping);
            }

            return null;
        }

        private SqlConstantExpression GetDefaultConstant(Type type)
        {
            return (SqlConstantExpression)_generateDefaultValueConstantMethod
                .MakeGenericMethod(type).Invoke(null, Array.Empty<object>());
        }

        private static readonly MethodInfo _generateDefaultValueConstantMethod =
            typeof(GetValueOrDefaultTranslator).GetTypeInfo().GetDeclaredMethod(nameof(GenerateDefaultValueConstant));

        private static SqlConstantExpression GenerateDefaultValueConstant<TDefault>()
            => new SqlConstantExpression(Expression.Constant(default(TDefault)), null);
    }
}
