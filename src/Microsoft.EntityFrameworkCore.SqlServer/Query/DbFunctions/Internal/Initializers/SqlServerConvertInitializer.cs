// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.DbFunctions.Internal.Initializers
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerConvertInitializer : IDbFunctionInitalizer
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Initialize(ModelBuilder modelBuilder)
        {
            Dictionary<string, DbType> typeMappingConvert = new Dictionary<string, DbType>
            {
                [nameof(Convert.ToByte)] = DbType.Byte,
                [nameof(Convert.ToDecimal)] = DbType.Decimal,
                [nameof(Convert.ToDouble)] = DbType.Double,
                [nameof(Convert.ToInt16)] = DbType.Int16,
                [nameof(Convert.ToInt32)] = DbType.Int32,
                [nameof(Convert.ToInt64)] = DbType.Int64,
                [nameof(Convert.ToString)] = DbType.String
            };

            List<Type> supportedTypesConvert = new List<Type>
            {
                typeof(bool),
                typeof(byte),
                typeof(decimal),
                typeof(double),
                typeof(float),
                typeof(int),
                typeof(long),
                typeof(short),
                typeof(string)
            };

            var convertMis
               = typeMappingConvert.Keys
               .SelectMany(t => typeof(Convert).GetTypeInfo().GetDeclaredMethods(t)
                   .Where(m => m.GetParameters().Length == 1
                               && supportedTypesConvert.Contains(m.GetParameters().First().ParameterType)));

            modelBuilder.DbFunction(convertMis, b =>
            {
                b.TranslateWith((args, dbFunc) =>
                {
                    return new SqlFunctionExpression(
                            "CONVERT",
                            dbFunc.ReturnType,
                            new[]
                            {
                                Expression.Constant(typeMappingConvert[dbFunc.MethodInfo.Name]),
                                args.ElementAt(0)
                            });
                });
            });
        }
    }
}
