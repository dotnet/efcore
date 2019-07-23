// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class ContainsTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _containsMethod = typeof(Enumerable).GetTypeInfo()
            .GetDeclaredMethods(nameof(Enumerable.Contains))
            .Single(mi => mi.GetParameters().Length == 2)
            .GetGenericMethodDefinition();

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public ContainsTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            if (method.IsGenericMethod
                && method.GetGenericMethodDefinition().Equals(_containsMethod))
            {
                return _sqlExpressionFactory.In(arguments[1], arguments[0], false);
            }
            else if (method.DeclaringType.GetInterfaces().Contains(typeof(IList))
                && string.Equals(method.Name, nameof(IList.Contains)))
            {
                return _sqlExpressionFactory.In(arguments[0], instance, false);
            }

            return null;
        }
    }
}
