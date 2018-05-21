// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Query
{
    public class SimpleQueryCosmosSqlTest : QueryTestBase<NorthwindQueryCosmosSqlFixture<NoopModelCustomizer>>
    {
        public SimpleQueryCosmosSqlTest(NorthwindQueryCosmosSqlFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }

        protected NorthwindContext CreateContext()
        {
            return Fixture.CreateContext();
        }

        protected virtual void ClearLog()
        {
        }

        [ConditionalFact]
        public virtual void Test()
        {
            Assert.True(true);
        }
    }
}
