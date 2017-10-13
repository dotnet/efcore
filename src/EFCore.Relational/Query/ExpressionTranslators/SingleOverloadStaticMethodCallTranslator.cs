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
    /// <summary>
    ///     A base LINQ expression translator for CLR <see cref="MethodCallExpression" /> expressions that
    ///     are static and are not overloaded.
    /// </summary>
    public abstract class SingleOverloadStaticMethodCallTranslator : IMethodCallTranslator
    {
        private readonly MethodInfo _methodInfo;
        private readonly string _sqlFunctionName;

        /// <summary>
        ///     Specialized constructor for use only by derived class.
        /// </summary>
        /// <param name="declaringType"> The declaring type of the method. </param>
        /// <param name="clrMethodName"> Name of the method. </param>
        /// <param name="sqlFunctionName"> The name of the target SQL function. </param>
        protected SingleOverloadStaticMethodCallTranslator(
            [NotNull] Type declaringType,
            [NotNull] string clrMethodName,
            [NotNull] string sqlFunctionName)
        {
            _methodInfo = declaringType.GetTypeInfo().GetDeclaredMethods(clrMethodName).Single();

            _sqlFunctionName = sqlFunctionName;
        }

        /// <summary>
        ///     Translates the given method call expression.
        /// </summary>
        /// <param name="methodCallExpression"> The method call expression. </param>
        /// <returns>
        ///     A SQL expression representing the translated MethodCallExpression.
        /// </returns>
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
            => _methodInfo.Equals(methodCallExpression.Method)
                ? new SqlFunctionExpression(_sqlFunctionName, methodCallExpression.Type, methodCallExpression.Arguments)
                : null;
    }
}
