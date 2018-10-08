using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query
{
    public partial class SimpleQueryCosmosTest
    {
        public override async Task QueryType_simple(bool isAsync)
        {
            await base.QueryType_simple(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task QueryType_where_simple(bool isAsync)
        {
            await base.QueryType_where_simple(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override void Query_backed_by_database_view()
        {
            base.Query_backed_by_database_view();

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND NOT(c[""Discontinued""]))");
        }

        public override void QueryType_with_nav_defining_query()
        {
            base.QueryType_with_nav_defining_query();

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task QueryType_with_mixed_tracking(bool isAsync)
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

        public override async Task QueryType_with_defining_query(bool isAsync)
        {
            await base.QueryType_with_defining_query(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task QueryType_with_defining_query_and_correlated_collection(bool isAsync)
        {
            await base.QueryType_with_defining_query_and_correlated_collection(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task QueryType_with_included_nav(bool isAsync)
        {
            await base.QueryType_with_included_nav(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task QueryType_with_included_navs_multi_level(bool isAsync)
        {
            await base.QueryType_with_included_navs_multi_level(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task QueryType_select_where_navigation(bool isAsync)
        {
            await base.QueryType_select_where_navigation(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task QueryType_select_where_navigation_multi_level(bool isAsync)
        {
            await AssertQuery<OrderQuery>(
                isAsync,
                ovs => from ov in ovs.Where(o => o.CustomerID == "ALFKI")
                       where ov.Customer.Orders.Any()
                       select ov);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }
    }
}
