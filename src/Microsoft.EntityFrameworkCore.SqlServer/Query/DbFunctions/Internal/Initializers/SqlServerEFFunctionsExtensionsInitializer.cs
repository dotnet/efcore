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
using Microsoft.EntityFrameworkCore.SqlServer;

namespace Microsoft.EntityFrameworkCore.Query.DbFunctions.Internal.Initializers
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerEFFunctionsExtensionsInitializer : IDbFunctionInitalizer
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Initialize(ModelBuilder modelBuilder)
        {
            modelBuilder.DbFunction(typeof(EFFunctionsExtensions), nameof(EFFunctionsExtensions.Left));
            modelBuilder.DbFunction(typeof(EFFunctionsExtensions), nameof(EFFunctionsExtensions.Right));
            modelBuilder.DbFunction(typeof(EFFunctionsExtensions), nameof(EFFunctionsExtensions.Reverse));

            /* Truncate */
            modelBuilder.DbFunction(typeof(EFFunctionsExtensions).GetTypeInfo().GetDeclaredMethods(nameof(EFFunctionsExtensions.Truncate)), b =>
            {
                b.HasName("Round");
                b.Parameter("function").HasParameterIndex(2).HasValue(1);
            });

            /* DiffYears */
            modelBuilder.DbFunction(typeof(EFFunctionsExtensions).GetTypeInfo().GetDeclaredMethods(nameof(EFFunctionsExtensions.DiffYears)), b =>
            {
                b.HasName("DATEDIFF");
                b.Parameter("datepart").HasParameterIndex(0, true).IsIdentifier(true).HasValue(DatePart.Year);
            });

            /* DiffMonths */
            modelBuilder.DbFunction(typeof(EFFunctionsExtensions).GetTypeInfo().GetDeclaredMethods(nameof(EFFunctionsExtensions.DiffMonths)), b =>
            {
                b.HasName("DATEDIFF");
                b.Parameter("datepart").HasParameterIndex(0, true).IsIdentifier(true).HasValue(DatePart.Month);
            });

            /* DiffDays */
            modelBuilder.DbFunction(typeof(EFFunctionsExtensions).GetTypeInfo().GetDeclaredMethods(nameof(EFFunctionsExtensions.DiffDays)), b =>
            {
                b.HasName("DATEDIFF");
                b.Parameter("datepart").HasParameterIndex(0, true).IsIdentifier(true).HasValue(DatePart.Day);
            });

            /* DiffHours */
            modelBuilder.DbFunction(typeof(EFFunctionsExtensions).GetTypeInfo().GetDeclaredMethods(nameof(EFFunctionsExtensions.DiffHours)), b =>
            {
                b.HasName("DATEDIFF");
                b.Parameter("datepart").HasParameterIndex(0, true).IsIdentifier(true).HasValue(DatePart.Hour);
            });

            /* DiffMinutes */
            modelBuilder.DbFunction(typeof(EFFunctionsExtensions).GetTypeInfo().GetDeclaredMethods(nameof(EFFunctionsExtensions.DiffMinutes)), b =>
            {
                b.HasName("DATEDIFF");
                b.Parameter("datepart").HasParameterIndex(0, true).IsIdentifier(true).HasValue(DatePart.Minute);
            });

            /* DiffSeconds */
            modelBuilder.DbFunction(typeof(EFFunctionsExtensions).GetTypeInfo().GetDeclaredMethods(nameof(EFFunctionsExtensions.DiffSeconds)), b =>
            {
                b.HasName("DATEDIFF");
                b.Parameter("datepart").HasParameterIndex(0, true).IsIdentifier(true).HasValue(DatePart.Second);
            });

            /* DiffMilliSeconds */
            modelBuilder.DbFunction(typeof(EFFunctionsExtensions).GetTypeInfo().GetDeclaredMethods(nameof(EFFunctionsExtensions.DiffMilliseconds)), b =>
            {
                b.HasName("DATEDIFF");
                b.Parameter("datepart").HasParameterIndex(0, true).IsIdentifier(true).HasValue(DatePart.Millisecond);
            });

            /* TruncateTime */
            modelBuilder.DbFunction(typeof(EFFunctionsExtensions).GetTypeInfo().GetDeclaredMethods(nameof(EFFunctionsExtensions.TruncateTime)), b =>
            {
                b.HasName("CONVERT");
                b.Parameter("data_type").HasParameterIndex(0, true).IsIdentifier(true).HasValue("date");
            });
        }
    }
}
