// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Pipeline
{
    public class SqlServerIsDateFunctionTranslator : IMethodCallTranslator
    {
        private const string IsDateFunctionName = "ISDATE";
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        private static readonly MethodInfo _isDateMethodInfo
            = typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                nameof(SqlServerDbFunctionsExtensions.IsDate),
                new[] { typeof(DbFunctions), typeof(string) });

        private static readonly IDictionary<MethodInfo, string> _functionMapping
            = new Dictionary<MethodInfo, string>
            {
                {_isDateMethodInfo, IsDateFunctionName },
            };

        public SqlServerIsDateFunctionTranslator(ISqlExpressionFactory sqlExpressionFactory)
            => _sqlExpressionFactory = sqlExpressionFactory;

        public SqlExpression Translate(SqlExpression instance, MethodInfo method, IList<SqlExpression> arguments)
        {
            if (_functionMapping.TryGetValue(method, out var _))
            {
                var propertyReference = arguments[1];
                if (!(propertyReference is ColumnExpression))
                {
                    throw new InvalidOperationException(SqlServerStrings.InvalidColumnNameForIsDate);
                }

                var typeMapping = ExpressionExtensions.InferTypeMapping(propertyReference);
                var isDateExpression = _sqlExpressionFactory.ApplyTypeMapping(propertyReference, typeMapping);

                return _sqlExpressionFactory.Function(
                    IsDateFunctionName,
                    new[]
                    {
                        isDateExpression
                    },
                    typeof(bool));
            }

            return null;
        }
    }
}
