// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class InheritanceQueryCosmosFixture : InheritanceQueryFixtureBase
{
    protected override ITestStoreFactory TestStoreFactory
        => CosmosTestStoreFactory.Instance;

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();

    protected override bool UseGeneratedKeys
        => false;
}
