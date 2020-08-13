// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class CompositeKeyEndToEndSqlServerTest : CompositeKeyEndToEndTestBase<
        CompositeKeyEndToEndSqlServerTest.CompositeKeyEndToEndSqlServerFixture>
    {
        public CompositeKeyEndToEndSqlServerTest(CompositeKeyEndToEndSqlServerFixture fixture)
            : base(fixture)
        {
        }

        public class CompositeKeyEndToEndSqlServerFixture : CompositeKeyEndToEndFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
        }
    }
}
