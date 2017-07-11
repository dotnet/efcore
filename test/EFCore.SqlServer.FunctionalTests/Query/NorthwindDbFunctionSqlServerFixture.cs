// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindDbFunctionSqlServerFixture : NorthwindQuerySqlServerFixture
    {
        public override NorthwindContext CreateContext(
            QueryTrackingBehavior queryTrackingBehavior = QueryTrackingBehavior.TrackAll,
            bool enableFilters = false)
        {
            EnableFilters = enableFilters;

            return new NorthwindDbFunctionContext(Options ?? (Options = BuildOptions()), queryTrackingBehavior);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var methodInfo = typeof(NorthwindDbFunctionContext).GetRuntimeMethod(nameof(NorthwindDbFunctionContext.MyCustomLength), new[] { typeof(string) });

            modelBuilder.HasDbFunction(methodInfo)
                .HasTranslation(args => new SqlFunctionExpression("len", methodInfo.ReturnType, args));

            modelBuilder.HasDbFunction(typeof(DateTimeExtensions).GetRuntimeMethod(nameof(DateTimeExtensions.IsDate), new[] { typeof(string) }));
        }
    }
}
