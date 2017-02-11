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
    public class SqliteEFFunctionsExtensionsInitializer : IDbFunctionInitalizer
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Initialize(ModelBuilder modelBuilder)
        {
            /* Left */
            modelBuilder.DbFunction(typeof(EFFunctionsExtensions), nameof(EFFunctionsExtensions.Left), b =>
            {
                b.HasName("substr");
                b.Parameter("startIdx").HasParameterIndex(1).HasValue(1);
                b.Parameter("length").HasParameterIndex(2);
            });

            /* Right */
            modelBuilder.DbFunction(typeof(EFFunctionsExtensions), nameof(EFFunctionsExtensions.Right), b =>
            {
                //TODO - should there be a "negate" option on dbparameter?
                b.TranslateWith((args, dbFunc) =>
                {
                    return new SqlFunctionExpression(
                        "substr",
                        typeof(string),
                        new[]
                        {
                             args.ElementAt(0),
                             Expression.Negate(args.ElementAt(1))
                        });
                });
            });

            /* DiffDays */
            var dbDiffDays = typeof(EFFunctionsExtensions).GetTypeInfo().GetDeclaredMethods(nameof(EFFunctionsExtensions.DiffDays));
            modelBuilder.DbFunction(dbDiffDays, b =>
            {
                b.TranslateWith((args, dbFunc) =>
                {
                    return Expression.Negate(
                            Expression.MakeBinary(ExpressionType.Subtract,
                            new SqlFunctionExpression(
                                "julianday",
                                typeof(int),
                                new[]
                                {
                                     args.ElementAt(0)
                                }),
                            new SqlFunctionExpression(
                                "julianday",
                                typeof(int),
                                new[]
                                {
                                     args.ElementAt(1)
                                })
                            )
                        );
                });
            });

            /* DiffHours */
            var dbDiffHours = typeof(EFFunctionsExtensions).GetTypeInfo().GetDeclaredMethods(nameof(EFFunctionsExtensions.DiffHours));
            modelBuilder.DbFunction(dbDiffHours, b =>
            {
                b.TranslateWith((args, dbFunc) =>
                {
                    return Expression.Negate(
                            Expression.Multiply(
                                Expression.MakeBinary(ExpressionType.Subtract,
                                    new SqlFunctionExpression(
                                        "julianday",
                                        typeof(int),
                                        new[]
                                        {
                                             args.ElementAt(0)
                                        }),
                                    new SqlFunctionExpression(
                                        "julianday",
                                        typeof(int),
                                        new[]
                                        {
                                             args.ElementAt(1)
                                        })
                                    ),
                                Expression.Constant(24)
                            )
                        );
                });
            });

            /* DiffMinutes */
            var dbDiffMinutes = typeof(EFFunctionsExtensions).GetTypeInfo().GetDeclaredMethods(nameof(EFFunctionsExtensions.DiffMinutes));
            modelBuilder.DbFunction(dbDiffMinutes, b =>
            {
                b.TranslateWith((args, dbFunc) =>
                {
                    return Expression.Negate(
                            Expression.Multiply(
                                Expression.MakeBinary(ExpressionType.Subtract,
                                    new SqlFunctionExpression(
                                        "julianday",
                                        typeof(int),
                                        new[]
                                        {
                                             args.ElementAt(0)
                                        }),
                                    new SqlFunctionExpression(
                                        "julianday",
                                        typeof(int),
                                        new[]
                                        {
                                             args.ElementAt(1)
                                        })
                                    ),
                                Expression.Constant(1440)
                            )
                        );
                });
            });

            /* DiffSeconds */
            var dbDiffSeconds = typeof(EFFunctionsExtensions).GetTypeInfo().GetDeclaredMethods(nameof(EFFunctionsExtensions.DiffSeconds));
            modelBuilder.DbFunction(dbDiffSeconds, b =>
            {
                b.TranslateWith((args, dbFunc) =>
                {
                    return Expression.Negate(
                            Expression.Multiply(
                                Expression.MakeBinary(ExpressionType.Subtract,
                                    new SqlFunctionExpression(
                                        "julianday",
                                        typeof(int),
                                        new[]
                                        {
                                             args.ElementAt(0)
                                        }),
                                    new SqlFunctionExpression(
                                        "julianday",
                                        typeof(int),
                                        new[]
                                        {
                                             args.ElementAt(1)
                                        })
                                    ),
                                Expression.Constant(86400)
                            )
                        );
                });
            });

            /* TruncateTime */
            modelBuilder.DbFunction(typeof(EFFunctionsExtensions), nameof(EFFunctionsExtensions.TruncateTime)).HasName("date");
        }
    }
}
