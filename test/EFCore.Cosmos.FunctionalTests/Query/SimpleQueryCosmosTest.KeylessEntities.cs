// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public partial class SimpleQueryCosmosTest
    {
        [ConditionalTheory]
        public override async Task KeylessEntity_simple(bool isAsync)
        {
            await base.KeylessEntity_simple(isAsync);

            AssertSql(
                @"SELECT c[""Address""], c[""City""], c[""CompanyName""], c[""ContactName""], c[""ContactTitle""]
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory]
        public override async Task KeylessEntity_where_simple(bool isAsync)
        {
            await base.KeylessEntity_where_simple(isAsync);

            AssertSql(
                @"SELECT c[""Address""], c[""City""], c[""CompanyName""], c[""ContactName""], c[""ContactTitle""]
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = ""London""))");
        }

        [ConditionalFact]
        public override void KeylessEntity_by_database_view()
        {
            base.KeylessEntity_by_database_view();

            AssertSql(
                @"SELECT c[""ProductID""], c[""ProductName""], ""Food"" AS CategoryName
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND NOT(c[""Discontinued""]))");
        }

        [ConditionalFact(Skip = "issue #12086")] // collection support
        public override void KeylessEntity_with_nav_defining_query()
        {
            base.KeylessEntity_with_nav_defining_query();

            AssertSql(
                @"");
        }

        public override async Task KeylessEntity_with_mixed_tracking(bool isAsync)
        {
            await AssertQuery<Customer, OrderQuery>(
                isAsync,
                (cs, ovs)
                    => from c in cs.Where(ct => ct.City == "London")
                       from o in ovs.Where(ov => ov.CustomerID == c.CustomerID)
                       select new
                       {
                           c,
                           o
                       },
                e => e.c.CustomerID);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = ""London""))");
        }

        public override async Task KeylessEntity_with_defining_query(bool isAsync)
        {
            await base.KeylessEntity_with_defining_query(isAsync);

            AssertSql(
                @"SELECT c[""CustomerID""]
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        public override async Task KeylessEntity_with_defining_query_and_correlated_collection(bool isAsync)
        {
            await base.KeylessEntity_with_defining_query_and_correlated_collection(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "issue 312086")] // left join translation
        public override async Task KeylessEntity_select_where_navigation(bool isAsync)
        {
            await base.KeylessEntity_select_where_navigation(isAsync);

            AssertSql(@"");
        }

        [ConditionalTheory(Skip = "issue 312086")] // left join translation
        public override async Task KeylessEntity_select_where_navigation_multi_level(bool isAsync)
        {
            await AssertQuery<OrderQuery>(
                isAsync,
                ovs => from ov in ovs.Where(o => o.CustomerID == "ALFKI")
                       where ov.Customer.Orders.Any()
                       select ov);

            AssertSql(@"");
        }
    }
}
