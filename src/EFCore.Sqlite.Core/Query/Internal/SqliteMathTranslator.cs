// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqliteMathTranslator : IMethodCallTranslator
    {
        private static readonly Dictionary<MethodInfo, string> _supportedMethods = new()
        {
            { typeof(Math).GetRequiredMethod(nameof(Math.Abs), typeof(double)), "abs" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Abs), typeof(float)), "abs" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Abs), typeof(int)), "abs" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Abs), typeof(long)), "abs" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Abs), typeof(sbyte)), "abs" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Abs), typeof(short)), "abs" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Max), typeof(byte), typeof(byte)), "max" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Max), typeof(double), typeof(double)), "max" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Max), typeof(float), typeof(float)), "max" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Max), typeof(int), typeof(int)), "max" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Max), typeof(long), typeof(long)), "max" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Max), typeof(sbyte), typeof(sbyte)), "max" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Max), typeof(short), typeof(short)), "max" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Max), typeof(uint), typeof(uint)), "max" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Max), typeof(ushort), typeof(ushort)), "max" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Min), typeof(byte), typeof(byte)), "min" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Min), typeof(double), typeof(double)), "min" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Min), typeof(float), typeof(float)), "min" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Min), typeof(int), typeof(int)), "min" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Min), typeof(long), typeof(long)), "min" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Min), typeof(sbyte), typeof(sbyte)), "min" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Min), typeof(short), typeof(short)), "min" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Min), typeof(uint), typeof(uint)), "min" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Min), typeof(ushort), typeof(ushort)), "min" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Round), typeof(double)), "round" },
            { typeof(Math).GetRequiredMethod(nameof(Math.Round), typeof(double), typeof(int)), "round" },
            { typeof(MathF).GetRequiredMethod(nameof(MathF.Abs), typeof(float)), "abs" },
            { typeof(MathF).GetRequiredMethod(nameof(MathF.Max), typeof(float), typeof(float)), "max" },
            { typeof(MathF).GetRequiredMethod(nameof(MathF.Min), typeof(float), typeof(float)), "min" },
            { typeof(MathF).GetRequiredMethod(nameof(MathF.Round), typeof(float)), "round" },
            { typeof(MathF).GetRequiredMethod(nameof(MathF.Round), typeof(float), typeof(int)), "round" }
        };

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqliteMathTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlExpression? Translate(
            SqlExpression? instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            Check.NotNull(method, nameof(method));
            Check.NotNull(arguments, nameof(arguments));
            Check.NotNull(logger, nameof(logger));

            if (_supportedMethods.TryGetValue(method, out var sqlFunctionName))
            {
                RelationalTypeMapping? typeMapping;
                List<SqlExpression>? newArguments = null;
                if (sqlFunctionName == "max" || sqlFunctionName == "max")
                {
                    typeMapping = ExpressionExtensions.InferTypeMapping(arguments![0]!, arguments[1]!);
                    newArguments = new List<SqlExpression>
                    {
                        _sqlExpressionFactory.ApplyTypeMapping(arguments[0], typeMapping),
                        _sqlExpressionFactory.ApplyTypeMapping(arguments[1], typeMapping)
                    };
                }
                else
                {
                    typeMapping = arguments[0].TypeMapping;
                }

                var finalArguments = newArguments ?? arguments;

                return _sqlExpressionFactory.Function(
                    sqlFunctionName,
                    finalArguments,
                    nullable: true,
                    argumentsPropagateNullability: finalArguments.Select(a => true).ToList(),
                    method.ReturnType,
                    typeMapping);
            }

            return null;
        }
    }
}
