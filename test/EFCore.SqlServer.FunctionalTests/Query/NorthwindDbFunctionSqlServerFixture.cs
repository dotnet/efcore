// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindDbFunctionSqlServerFixture : NorthwindQuerySqlServerFixture
    {
        public NorthwindDbFunctionSqlServerFixture()
            : base(mb =>
                        p =>
                             new TestModelSource(
                                mb, 
                                p.GetRequiredService<ModelSourceDependencies>(),
                                (modelBuilder, context) =>
                                    {
                                        new RelationalModelCustomizer(p.GetRequiredService<ModelCustomizerDependencies>()).Customize(modelBuilder, context);
                                    })
                    )
        {
        }

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

            var methodInfo = typeof(NorthwindDbFunctionContext).GetRuntimeMethod(nameof(NorthwindDbFunctionContext.MyCustomLength), new[] { typeof(string) }) ;

            modelBuilder.HasDbFunction(methodInfo)
                .HasTranslation((args) => new SqlFunctionExpression("len", methodInfo.ReturnType, args));

            modelBuilder.HasDbFunction(typeof(DateTimeExtensions).GetRuntimeMethod(nameof(DateTimeExtensions.IsDate), new[] { typeof(string) }));
        }
    }
}
