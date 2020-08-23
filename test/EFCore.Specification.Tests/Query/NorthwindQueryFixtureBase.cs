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
        public Func<DbContext> GetContextCreator()
            => () => CreateContext();

        public ISetSource GetExpectedData()
            => new NorthwindData();

        public IReadOnlyDictionary<Type, object> GetEntitySorters()
            => new Dictionary<Type, Func<object, object>>
            {
                { typeof(Customer), e => ((Customer)e)?.CustomerID },
                { typeof(CustomerQuery), e => ((CustomerQuery)e)?.CompanyName },
                { typeof(Order), e => ((Order)e)?.OrderID },
                { typeof(OrderQuery), e => ((OrderQuery)e)?.CustomerID },
                { typeof(Employee), e => ((Employee)e)?.EmployeeID },
                { typeof(Product), e => ((Product)e)?.ProductID },
                { typeof(ProductQuery), e => ((ProductQuery)e)?.ProductID },
                { typeof(OrderDetail), e => (((OrderDetail)e)?.OrderID.ToString(), ((OrderDetail)e)?.ProductID.ToString()) }
            }.ToDictionary(e => e.Key, e => (object)e.Value);

        public IReadOnlyDictionary<Type, object> GetEntityAsserters()
            => null;

        protected override string StoreName { get; } = "Northwind";

        protected override bool UsePooling
            => typeof(TModelCustomizer) == typeof(NoopModelCustomizer);

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            => new TModelCustomizer().Customize(modelBuilder, context);

        protected override void Seed(NorthwindContext context)
            => NorthwindData.Seed(context);

        protected override Task SeedAsync(NorthwindContext context)
            => NorthwindData.SeedAsync(context);

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(
                c => c
                    .Log(CoreEventId.RowLimitingOperationWithoutOrderByWarning)
                    .Log(CoreEventId.FirstWithoutOrderByAndFilterWarning)
                    .Log(CoreEventId.PossibleUnintendedCollectionNavigationNullComparisonWarning)
                    .Log(CoreEventId.PossibleUnintendedReferenceComparisonWarning));
    }
}
