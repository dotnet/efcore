// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class OwnedQueryInMemoryTest : OwnedQueryTestBase<OwnedQueryInMemoryTest.OwnedQueryInMemoryFixture>
    {
        public OwnedQueryInMemoryTest(OwnedQueryInMemoryFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        public override void No_ignored_include_warning_when_implicit_load()
        {
        }

        public override void Query_for_base_type_loads_all_owned_navs()
        {
        }

        public override void Query_for_branch_type_loads_all_owned_navs()
        {
        }

        public override void Query_for_leaf_type_loads_all_owned_navs()
        {
        }

        public override void Query_when_group_by()
        {
        }

        public override void Query_when_subquery()
        {
        }

        public class OwnedQueryInMemoryFixture : OwnedQueryFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => InMemoryTestStoreFactory.Instance;
        }
    }
}
