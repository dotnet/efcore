// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
