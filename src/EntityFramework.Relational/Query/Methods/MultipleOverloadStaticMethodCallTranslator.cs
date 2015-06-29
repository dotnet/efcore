// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Expressions;

namespace Microsoft.Data.Entity.Query.Methods
{
    public abstract class MultipleOverloadStaticMethodCallTranslator : IMethodCallTranslator
    {
        private readonly Type _declaringType;
        private readonly string _clrMethodName;
        private readonly string _sqlFunctionName;

        public MultipleOverloadStaticMethodCallTranslator([NotNull] Type declaringType, [NotNull] string clrMethodName, [NotNull] string sqlFunctionName)
        {
            _declaringType = declaringType;
            _clrMethodName = clrMethodName;
            _sqlFunctionName = sqlFunctionName;
        }

        public virtual Expression Translate([NotNull] MethodCallExpression methodCallExpression)
        {
            var methodInfos = _declaringType.GetTypeInfo().GetDeclaredMethods(_clrMethodName);
            if (methodInfos.Contains(methodCallExpression.Method))
            {
                return new SqlFunctionExpression(_sqlFunctionName, methodCallExpression.Arguments, methodCallExpression.Type);
            }

            return null;
        }
    }
}
