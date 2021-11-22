// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.



// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query;

public class QueryLoggingCosmosTest : QueryLoggingCosmosTestBase,
    IClassFixture<QueryLoggingCosmosTest.NorthwindQueryCosmosFixtureInsensitive<NoopModelCustomizer>>
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
