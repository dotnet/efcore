// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Expressions;
using Microsoft.Data.Entity.Query.Methods;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Query.Methods
{
    public class ConvertTranslator : IMethodCallTranslator
    {
        private readonly string _convertMethodName;
        private readonly Dictionary<string, object> _typeMapping = new Dictionary<string, object>
        {
            [nameof(Convert.ToByte)] = DbType.Byte,
            [nameof(Convert.ToDecimal)] = DbType.Decimal,
            [nameof(Convert.ToDouble)] = DbType.Double,
            [nameof(Convert.ToInt16)] = DbType.Int16,
            [nameof(Convert.ToInt32)] = DbType.Int32,
            [nameof(Convert.ToInt64)] = DbType.Int64,
            [nameof(Convert.ToString)] = DbType.String,
        };

        public ConvertTranslator([NotNull] string convertMethodName)
        {
            Check.NotEmpty(convertMethodName, nameof(convertMethodName));

            _convertMethodName = convertMethodName;
        }

        public virtual Expression Translate([NotNull] MethodCallExpression methodCallExpression)
        {
            var methodInfos = typeof(Convert).GetTypeInfo().GetDeclaredMethods(_convertMethodName);
            if (methodInfos.Contains(methodCallExpression.Method))
            {
                var arguments = new[] { Expression.Constant(_typeMapping[_convertMethodName]), methodCallExpression.Arguments[0] };
                return new SqlFunctionExpression("CONVERT", arguments, methodCallExpression.Type);
            }

            return null;
        }
    }
}
