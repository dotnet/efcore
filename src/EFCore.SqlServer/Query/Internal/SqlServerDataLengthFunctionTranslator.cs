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
        private static readonly List<string> _longReturningTypes = new List<string> { "nvarchar(max)", "varchar(max)", "varbinary(max)" };

        private static readonly HashSet<MethodInfo> _methodInfoDataLengthMapping
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

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqlServerDataLengthFunctionTranslator([NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            Check.NotNull(method, nameof(method));
            Check.NotNull(arguments, nameof(arguments));

            if (_methodInfoDataLengthMapping.Contains(method))
            {
                var argument = arguments[1];
                if (argument.TypeMapping == null)
                {
                    argument = _sqlExpressionFactory.ApplyDefaultTypeMapping(argument);
                }

                if (_longReturningTypes.Contains(argument.TypeMapping.StoreType))
                {
                    var result = _sqlExpressionFactory.Function(
                        "DATALENGTH",
                        arguments.Skip(1),
                        nullable: true,
                        argumentsPropagateNullability: new[] { true },
                        typeof(long));

                    return _sqlExpressionFactory.Convert(result, method.ReturnType);
                }

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
