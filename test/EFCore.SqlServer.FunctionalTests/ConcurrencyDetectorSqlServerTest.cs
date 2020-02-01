// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class ConcurrencyDetectorSqlServerTest : ConcurrencyDetectorRelationalTestBase<
        NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
    {
        public ConcurrencyDetectorSqlServerTest(NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }

        protected override async Task ConcurrencyDetectorTest(Func<NorthwindContext, Task> test)
        {
            await base.ConcurrencyDetectorTest(test);

            Assert.Empty(Fixture.TestSqlLoggerFactory.SqlStatements);
        }
    }
}
