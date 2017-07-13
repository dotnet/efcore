// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class AsyncFromSqlQuerySqliteTest : AsyncFromSqlQueryTestBase<NorthwindQuerySqliteFixture<NoopModelCustomizer>>
    {
        public AsyncFromSqlQuerySqliteTest(NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }
    }
}
