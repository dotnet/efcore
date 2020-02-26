// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    public class SqlServerDataLengthFunctionTranslator : IMethodCallTranslator
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private const string DataLengthFunctionName = "DATALENGTH";

        private readonly Dictionary<MethodInfo, string> _methodInfoDataLengthMapping
            = new Dictionary<MethodInfo, string>
            {
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DataLength),
                        new[] { typeof(DbFunctions), typeof(string) }),
                    DataLengthFunctionName
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DataLength),
                        new[] { typeof(DbFunctions), typeof(bool?) }),
                    DataLengthFunctionName
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DataLength),
                        new[] { typeof(DbFunctions), typeof(double?) }),
                    DataLengthFunctionName
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DataLength),
                        new[] { typeof(DbFunctions), typeof(decimal?) }),
                    DataLengthFunctionName
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DataLength),
                        new[] { typeof(DbFunctions), typeof(DateTime?) }),
                    DataLengthFunctionName
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DataLength),
                        new[] { typeof(DbFunctions), typeof(TimeSpan?) }),
                    DataLengthFunctionName
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DataLength),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset?) }),
                    DataLengthFunctionName
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DataLength),
                        new[] { typeof(DbFunctions), typeof(byte[]) }),
                    DataLengthFunctionName
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DataLength),
                        new[] { typeof(DbFunctions), typeof(Guid?) }),
                    DataLengthFunctionName
                }
            };

        public SqlServerDataLengthFunctionTranslator([NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            Check.NotNull(method, nameof(method));
            Check.NotNull(arguments, nameof(arguments));

            if (_methodInfoDataLengthMapping.TryGetValue(method, out var sqlFunctionName))
            {
                return _sqlExpressionFactory.Function(
                    sqlFunctionName,
                    arguments.Skip(1),
                    nullable: true,
                    argumentsPropagateNullability: new[] { true },
                    method.ReturnType);
            }

            return null;
        }
    }
}
