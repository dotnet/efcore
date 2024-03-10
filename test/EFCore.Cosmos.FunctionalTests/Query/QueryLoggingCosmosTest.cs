// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class QueryLoggingCosmosTest(QueryLoggingCosmosTest.NorthwindQueryCosmosFixtureInsensitive<NoopModelCustomizer> fixture) : QueryLoggingCosmosTestBase(fixture),
    IClassFixture<QueryLoggingCosmosTest.NorthwindQueryCosmosFixtureInsensitive<NoopModelCustomizer>>
{
    public class NorthwindQueryCosmosFixtureInsensitive<TModelCustomizer> : NorthwindQueryCosmosFixture<TModelCustomizer>
        where TModelCustomizer : ITestModelCustomizer, new()
    {
        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).EnableSensitiveDataLogging(false);
    }

    protected override bool ExpectSensitiveData
        => false;
}
