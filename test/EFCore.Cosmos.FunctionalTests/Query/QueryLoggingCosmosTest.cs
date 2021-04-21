// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public class QueryLoggingCosmosTest : QueryLoggingCosmosTestBase, IClassFixture<QueryLoggingCosmosTest.NorthwindQueryCosmosFixtureInsensitive<NoopModelCustomizer>>
    {
        public QueryLoggingCosmosTest(NorthwindQueryCosmosFixtureInsensitive<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }

        public class NorthwindQueryCosmosFixtureInsensitive<TModelCustomizer> : NorthwindQueryCosmosFixture<TModelCustomizer>
            where TModelCustomizer : IModelCustomizer, new()
        {
            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder).EnableSensitiveDataLogging(false);
        }

        protected override bool ExpectSensitiveData
            => false;
    }
}
