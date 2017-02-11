// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.DbFunctions.Internal.Initializers
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqliteDateTimeInitializer : IDbFunctionInitalizer
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Initialize(ModelBuilder modelBuilder)
        {
            var concatMi = typeof(string).GetTypeInfo().GetDeclaredMethods(nameof(string.Concat))
                          .Where(mi =>
                          {
                              var parameters = mi.GetParameters();

                              return parameters.Length == 2 && parameters[0].ParameterType == typeof(object)
                                      && parameters[1].ParameterType == typeof(object);
                          }).Single();

            /* AddYears */
            modelBuilder.DbFunction(typeof(DateTime), nameof(DateTime.AddYears), b =>
            {
                b.Parameter("dateObject").HasParameterIndex(0).IsObjectParameter(true);
                b.Parameter("value").HasParameterIndex(1);

                b.TranslateWith((args, dbFunc) =>
                {
                    return new SqlFunctionExpression(
                        "datetime",
                        typeof(DateTime),
                        new[]
                        {
                            args.ElementAt(0),
                            Expression.MakeBinary(ExpressionType.Add,
                                Expression.Convert(args.ElementAt(1), typeof(object)),
                                Expression.Constant(" year"),
                                false,
                                concatMi) as Expression
                        });
                });
            });

            /* AddDays */
            modelBuilder.DbFunction(typeof(DateTime), nameof(DateTime.AddDays), b =>
            {
                b.Parameter("dateObject").HasParameterIndex(0, true).IsObjectParameter(true);

                b.TranslateWith((args, dbFunc) =>
                {
                    return new SqlFunctionExpression(
                        "datetime",
                        typeof(DateTime),
                        new[]
                        {
                            args.ElementAt(0),
                            Expression.MakeBinary(ExpressionType.Add,
                                Expression.Convert(args.ElementAt(1), typeof(object)),
                                Expression.Constant(" day"),
                                false,
                                concatMi) as Expression
                        });
                });
            });

            /* AddHours */
            modelBuilder.DbFunction(typeof(DateTime), nameof(DateTime.AddHours), b =>
            {
                b.Parameter("dateObject").HasParameterIndex(0, true).IsObjectParameter(true);

                b.TranslateWith((args, dbFunc) =>
                {
                    return new SqlFunctionExpression(
                        "datetime",
                        typeof(DateTime),
                        new[]
                        {
                            args.ElementAt(0),
                            Expression.MakeBinary(ExpressionType.Add,
                                Expression.Convert(args.ElementAt(1), typeof(object)),
                                Expression.Constant(" hour"),
                                false,
                                concatMi) as Expression
                        });
                });
            });

            /* AddMinutes */
            modelBuilder.DbFunction(typeof(DateTime), nameof(DateTime.AddMinutes), b =>
            {
                b.Parameter("dateObject").HasParameterIndex(0, true).IsObjectParameter(true);

                b.TranslateWith((args, dbFunc) =>
                {
                    return new SqlFunctionExpression(
                        "datetime",
                        typeof(DateTime),
                        new[]
                        {
                            args.ElementAt(0),
                            Expression.MakeBinary(ExpressionType.Add,
                                Expression.Convert(args.ElementAt(1), typeof(object)),
                                Expression.Constant(" minute"),
                                false,
                                concatMi) as Expression
                        });
                });
            });

            /* AddSeconds */
            modelBuilder.DbFunction(typeof(DateTime), nameof(DateTime.AddSeconds), b =>
            {
                b.Parameter("dateObject").HasParameterIndex(0, true).IsObjectParameter(true);

                b.TranslateWith((args, dbFunc) =>
                {
                    return new SqlFunctionExpression(
                        "datetime",
                        typeof(DateTime),
                        new[]
                        {
                            args.ElementAt(0),
                            Expression.MakeBinary(ExpressionType.Add,
                                Expression.Convert(args.ElementAt(1), typeof(object)),
                                Expression.Constant(" second"),
                                false,
                                concatMi) as Expression
                        });
                });
            });
        }
    }
}
