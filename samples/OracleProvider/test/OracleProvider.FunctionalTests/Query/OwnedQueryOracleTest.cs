// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class OwnedQueryOracleTest : RelationalOwnedQueryTestBase<OwnedQueryOracleTest.OwnedQueryOracleFixture>
    {
        public OwnedQueryOracleTest(OwnedQueryOracleFixture fixture)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        [Fact(Skip = "#8973")]
        public override void No_ignored_include_warning_when_implicit_load()
        {
            base.No_ignored_include_warning_when_implicit_load();

            AssertSql("");
        }

        [Fact(Skip = "#8973")]
        public override void Query_for_base_type_loads_all_owned_navs()
        {
            base.Query_for_base_type_loads_all_owned_navs();

            AssertSql("");
        }

        [Fact(Skip = "#8973")]
        public override void Query_for_branch_type_loads_all_owned_navs()
        {
            base.Query_for_branch_type_loads_all_owned_navs();

            AssertSql("");
        }

        [Fact(Skip = "#8973")]
        public override void Query_for_leaf_type_loads_all_owned_navs()
        {
            base.Query_for_leaf_type_loads_all_owned_navs();

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [o].[Id], [o].[Id], [o].[LeafAAddress_Country_Name], [o].[Id], [o].[Id], [o].[BranchAddress_Country_Name], [o].[Id], [o].[Id], [o].[PersonAddress_Country_Name]
FROM [OwnedPerson] AS [o]
WHERE [o].[Discriminator] = N'LeafA'");
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

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        public class OwnedQueryOracleFixture : RelationalOwnedQueryFixture
        {
            protected override ITestStoreFactory TestStoreFactory => OracleTestStoreFactory.Instance;
        }
    }
}
