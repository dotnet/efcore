// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class QueryNoClientEvalSqlServerTest : QueryNoClientEvalTestBase<NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
    {
        public QueryNoClientEvalSqlServerTest(NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }
    }
}
