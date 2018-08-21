using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Query
{
    public partial class SimpleQueryCosmosSqlTest
    {
        public override async Task QueryType_simple(bool isAsync)
        {
            await base.QueryType_simple(isAsync);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task QueryType_where_simple(bool isAsync)
        {
            await base.QueryType_where_simple(isAsync);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override void Query_backed_by_database_view()
        {
            base.Query_backed_by_database_view();

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND NOT(c[""Discontinued""]))");
        }

        public override void QueryType_with_nav_defining_query()
        {
            base.QueryType_with_nav_defining_query();

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task QueryType_with_mixed_tracking(bool isAsync)
        {
            await base.QueryType_with_mixed_tracking(isAsync);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task QueryType_with_defining_query(bool isAsync)
        {
            await base.QueryType_with_defining_query(isAsync);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task QueryType_with_defining_query_and_correlated_collection(bool isAsync)
        {
            await base.QueryType_with_defining_query_and_correlated_collection(isAsync);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task QueryType_with_included_nav(bool isAsync)
        {
            await base.QueryType_with_included_nav(isAsync);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task QueryType_with_included_navs_multi_level(bool isAsync)
        {
            await base.QueryType_with_included_navs_multi_level(isAsync);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task QueryType_select_where_navigation(bool isAsync)
        {
            await base.QueryType_select_where_navigation(isAsync);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task QueryType_select_where_navigation_multi_level(bool isAsync)
        {
            await base.QueryType_select_where_navigation_multi_level(isAsync);

            AssertSql(
                @"SELECT c AS query
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }
    }
}
