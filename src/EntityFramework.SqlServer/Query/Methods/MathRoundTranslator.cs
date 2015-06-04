// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Data.Entity.Relational.Query.Methods;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.SqlServer.Query.Methods
{
    public class MathRoundTranslator : IMethodCallTranslator
    {
        public virtual Expression Translate([NotNull] MethodCallExpression methodCallExpression)
        {
            var methodInfos = typeof(Math).GetTypeInfo().GetDeclaredMethods("Round").Where(m => 
                m.GetParameters().Count() == 1 
                || (m.GetParameters().Count() == 2 && m.GetParameters()[1].ParameterType == typeof(int)));

            if (methodInfos.Contains(methodCallExpression.Method))
            {
                return new SqlFunctionExpression("ROUND", methodCallExpression.Arguments, methodCallExpression.Type);
            }

            return null;
        }
    }
}
