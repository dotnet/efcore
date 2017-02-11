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
    public class SqlServerMathInitializer : IDbFunctionInitalizer
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Initialize(ModelBuilder modelBuilder)
        {
            /* Round */
            modelBuilder.DbFunction(typeof(Math).GetTypeInfo().GetDeclaredMethods(nameof(Math.Round)).Where(m => (m.GetParameters().Length == 1)
                        || ((m.GetParameters().Length == 2) && (m.GetParameters()[1].ParameterType == typeof(int)))), b =>
            {
                if (b.Metadata.MethodInfo.GetParameters().Length == 1)
                    b.Parameter("length").HasParameterIndex(1).HasValue(0);
            });

            /* Truncate */
            modelBuilder.DbFunction(typeof(Math).GetTypeInfo().GetDeclaredMethods(nameof(Math.Truncate)), b =>
            {
                b.HasName("Round");

                b.Parameter("length").HasParameterIndex(1).HasValue(0);
                b.Parameter("function").HasParameterIndex(2).HasValue(1);
            });

            /* POW */
            modelBuilder.DbFunction(typeof(Math), nameof(Math.Pow), b =>
            {
                b.HasName("POWER");
            });

            /*Floor */
            modelBuilder.DbFunction(typeof(Math).GetTypeInfo().GetDeclaredMethods(nameof(Math.Floor)));

            /* Ceiling */
            modelBuilder.DbFunction(typeof(Math).GetTypeInfo().GetDeclaredMethods(nameof(Math.Ceiling)));

            /* ABS */
            modelBuilder.DbFunction(typeof(Math).GetTypeInfo().GetDeclaredMethods(nameof(Math.Abs))) ;

            /* EXP */
            modelBuilder.DbFunction(typeof(Math), nameof(Math.Exp));

            /* LOG10 */
            modelBuilder.DbFunction(typeof(Math), nameof(Math.Log10));

            /* LOG - loads both overloads which are supported by TSQL */
            modelBuilder.DbFunction(typeof(Math).GetTypeInfo().GetDeclaredMethods(nameof(Math.Log)));

            /* SQRT */
            modelBuilder.DbFunction(typeof(Math), nameof(Math.Sqrt));

            /* ACOS */
            modelBuilder.DbFunction(typeof(Math), nameof(Math.Acos));

            /* ASIN */
            modelBuilder.DbFunction(typeof(Math), nameof(Math.Asin));

            /* ATAN */
            modelBuilder.DbFunction(typeof(Math), nameof(Math.Atan)); 

            /* ATN2 */
            modelBuilder.DbFunction(typeof(Math), nameof(Math.Atan2), b =>
            {
                b.HasName("Atn2");
            });

            /* COS */
            modelBuilder.DbFunction(typeof(Math), nameof(Math.Cos)); 

            /* SIN */
            modelBuilder.DbFunction(typeof(Math), nameof(Math.Sin));

            /* TAN */
            modelBuilder.DbFunction(typeof(Math), nameof(Math.Tan));

            /* SIGN */
            modelBuilder.DbFunction(typeof(Math).GetTypeInfo().GetDeclaredMethods(nameof(Math.Sign)));
        }
    }
}
