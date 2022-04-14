// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class UpdatesInMemoryWithSensitiveDataLoggingFixture : UpdatesInMemoryFixtureBase
{
    protected override string StoreName { get; } = "UpdateTestSensitive";

    protected override ITestStoreFactory TestStoreFactory
        => InMemoryTestStoreFactory.Instance;

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).EnableSensitiveDataLogging();
}
