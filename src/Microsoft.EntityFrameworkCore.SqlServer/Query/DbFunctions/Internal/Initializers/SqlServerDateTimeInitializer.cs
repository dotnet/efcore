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
    public class SqlServerDateTimeInitializer : IDbFunctionInitalizer
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Initialize(ModelBuilder modelBuilder)
        {
            /* AddYears */
            modelBuilder.DbFunction(typeof(DateTime), nameof(DateTime.AddYears), b =>
            {
                b.HasName("DATEADD");
                b.Parameter("datepart").HasParameterIndex(0, true).IsIdentifier(true).HasValue(DatePart.Year);
                b.Parameter("dateObject").HasParameterIndex(2).IsObjectParameter(true);
            });

            /* AddDays */
            modelBuilder.DbFunction(typeof(DateTime), nameof(DateTime.AddDays), b =>
            {
                b.HasName("DATEADD");
                b.Parameter("datepart").HasParameterIndex(0, true).IsIdentifier(true).HasValue(DatePart.Day);
                b.Parameter("dateObject").HasParameterIndex(2).IsObjectParameter(true);
            });

            /* AddMonths */
            modelBuilder.DbFunction(typeof(DateTime), nameof(DateTime.AddMonths), b =>
            {
                b.HasName("DATEADD");
                b.Parameter("datepart").HasParameterIndex(0, true).IsIdentifier(true).HasValue(DatePart.Month);
                b.Parameter("dateObject").HasParameterIndex(2).IsObjectParameter(true);
            });

            /* AddHours */
            modelBuilder.DbFunction(typeof(DateTime), nameof(DateTime.AddHours), b =>
            {
                b.HasName("DATEADD");
                b.Parameter("datepart").HasParameterIndex(0, true).IsIdentifier(true).HasValue(DatePart.Hour);
                b.Parameter("dateObject").HasParameterIndex(2).IsObjectParameter(true);
            });

            /* AddMinutes */
            modelBuilder.DbFunction(typeof(DateTime), nameof(DateTime.AddMinutes), b =>
            {
                b.HasName("DATEADD");
                b.Parameter("datepart").HasParameterIndex(0, true).IsIdentifier(true).HasValue(DatePart.Minute);
                b.Parameter("dateObject").HasParameterIndex(2).IsObjectParameter(true);
            });

            /* AddSeconds */
            modelBuilder.DbFunction(typeof(DateTime), nameof(DateTime.AddSeconds), b =>
            {
                b.HasName("DATEADD");
                b.Parameter("datepart").HasParameterIndex(0, true).IsIdentifier(true).HasValue(DatePart.Second);
                b.Parameter("dateObject").HasParameterIndex(2).IsObjectParameter(true);
            });

            /* AddMillisecond */
            modelBuilder.DbFunction(typeof(DateTime), nameof(DateTime.AddMilliseconds), b =>
            {
                b.HasName("DATEADD");
                b.Parameter("datepart").HasParameterIndex(0, true).IsIdentifier(true).HasValue(DatePart.Millisecond);
                b.Parameter("dateObject").HasParameterIndex(2).IsObjectParameter(true);
            });
        }
    }
}
