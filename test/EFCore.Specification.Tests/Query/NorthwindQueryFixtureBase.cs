// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class NorthwindQueryFixtureBase<TModelCustomizer> : SharedStoreFixtureBase<NorthwindContext>, IQueryFixtureBase
        where TModelCustomizer : IModelCustomizer, new()
    {
        protected NorthwindQueryFixtureBase()
        {
            var entitySorters = new Dictionary<Type, Func<dynamic, object>>
            {
                { typeof(Customer), e => e?.CustomerID },
                { typeof(CustomerView), e => e?.CompanyName },
                { typeof(Order), e => e?.OrderID },
                { typeof(OrderQuery), e => e?.CustomerID },
                { typeof(Employee), e => e?.EmployeeID },
                { typeof(Product), e => e?.ProductID },
                { typeof(OrderDetail), e => (e?.OrderID.ToString(), e?.ProductID.ToString()) }
            }.ToDictionary(e => e.Key, e => (object)e.Value);

            var entityAsserters = new Dictionary<Type, object>();

            QueryAsserter = new QueryAsserter<NorthwindContext>(
                CreateContext,
                new NorthwindData(),
                entitySorters,
                entityAsserters);
        }

        protected override string StoreName { get; } = "Northwind";

        protected override bool UsePooling => typeof(TModelCustomizer) == typeof(NoopModelCustomizer);

        public QueryAsserterBase QueryAsserter { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            => new TModelCustomizer().Customize(modelBuilder, context);

        protected override void Seed(NorthwindContext context) => NorthwindData.Seed(context);

        protected override Task SeedAsync(NorthwindContext context) => NorthwindData.SeedAsync(context);

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(
                c => c
                    .Log(CoreEventId.PossibleUnintendedCollectionNavigationNullComparisonWarning)
                    .Log(CoreEventId.PossibleUnintendedReferenceComparisonWarning));
    }
}
