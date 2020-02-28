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

        private readonly HashSet<MethodInfo> _methodInfoDataLengthMapping
            = new HashSet<MethodInfo>
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DataLength),
                    new[] { typeof(DbFunctions), typeof(string) }),

                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DataLength),
                    new[] { typeof(DbFunctions), typeof(bool?) }),

                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DataLength),
                    new[] { typeof(DbFunctions), typeof(double?) }),

                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DataLength),
                    new[] { typeof(DbFunctions), typeof(decimal?) }),

                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DataLength),
                    new[] { typeof(DbFunctions), typeof(DateTime?) }),

                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DataLength),
                    new[] { typeof(DbFunctions), typeof(TimeSpan?) }),

                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DataLength),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset?) }),

                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DataLength),
                    new[] { typeof(DbFunctions), typeof(byte[]) }),

                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DataLength),
                    new[] { typeof(DbFunctions), typeof(Guid?) })
            };

        public SqlServerDataLengthFunctionTranslator([NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            Check.NotNull(method, nameof(method));
            Check.NotNull(arguments, nameof(arguments));

            if (_methodInfoDataLengthMapping.TryGetValue(method, out _))
            {
                return _sqlExpressionFactory.Function(
                    "DATALENGTH",
                    arguments.Skip(1),
                    nullable: true,
                    argumentsPropagateNullability: new[] { true },
                    method.ReturnType);
            }

            return null;
        }
    }
}
