// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class OwnedQuerySqliteTest : OwnedQueryTestBase<OwnedQuerySqliteTest.OwnedQuerySqliteFixture>
    {
        public OwnedQuerySqliteTest(OwnedQuerySqliteFixture fixture)
            : base(fixture)
        {
        }

        [Fact(Skip = "#8973")]
        public override void Query_for_base_type_loads_all_owned_navs()
        {
            base.Query_for_base_type_loads_all_owned_navs();
        }

        [Fact(Skip = "#8973")]
        public override void Query_for_branch_type_loads_all_owned_navs()
        {
            base.Query_for_branch_type_loads_all_owned_navs();
        }

        [Fact(Skip = "#8973")]
        public override void Query_when_group_by()
        {
            base.Query_when_group_by();
        }

        [Fact(Skip = "#8973")]
        public override void Query_when_subquery()
        {
            base.Query_when_subquery();
        }

        [Fact(Skip = "#8973")]
        public override void Query_for_leaf_type_loads_all_owned_navs()
        {
            base.Query_for_leaf_type_loads_all_owned_navs();
        }
        
        public class OwnedQuerySqliteFixture : OwnedQueryFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();
        }
    }
}
