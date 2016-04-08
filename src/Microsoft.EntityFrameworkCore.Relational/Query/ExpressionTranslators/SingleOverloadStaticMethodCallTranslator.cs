// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators
{
    public abstract class SingleOverloadStaticMethodCallTranslator : IMethodCallTranslator
    {
        private readonly Type _declaringType;
        private readonly string _clrMethodName;
        private readonly string _sqlFunctionName;

        protected SingleOverloadStaticMethodCallTranslator(
            [NotNull] Type declaringType,
            [NotNull] string clrMethodName,
            [NotNull] string sqlFunctionName)
        {
            _declaringType = declaringType;
            _clrMethodName = clrMethodName;
            _sqlFunctionName = sqlFunctionName;
        }

        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            var methodInfo = _declaringType.GetTypeInfo().GetDeclaredMethods(_clrMethodName).SingleOrDefault();
            return methodInfo == methodCallExpression.Method
                ? new SqlFunctionExpression(_sqlFunctionName, methodCallExpression.Type, methodCallExpression.Arguments)
                : null;
        }
    }
}
