// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class ConcurrencyDetectorSqliteTest : ConcurrencyDetectorRelationalTestBase<NorthwindQuerySqliteFixture<NoopModelCustomizer>>
    {
        public ConcurrencyDetectorSqliteTest(NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }
    }
}
