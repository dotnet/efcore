// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.InMemory.Internal;

namespace Microsoft.EntityFrameworkCore;

public class UpdatesInMemoryWithoutSensitiveDataLoggingTest
    : UpdatesInMemoryTestBase<UpdatesInMemoryWithoutSensitiveDataLoggingTest.UpdatesInMemoryWithoutSensitiveDataLoggingFixture>
{
    public UpdatesInMemoryWithoutSensitiveDataLoggingTest(UpdatesInMemoryWithoutSensitiveDataLoggingFixture fixture)
        : base(fixture)
    {
    }

    protected override string UpdateConcurrencyTokenMessage
        => InMemoryStrings.UpdateConcurrencyTokenException("Product", "{'Price'}");

    public class UpdatesInMemoryWithoutSensitiveDataLoggingFixture : UpdatesInMemoryFixtureBase
    {
        protected override string StoreName
            => "UpdateTestInsensitive";

        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).EnableSensitiveDataLogging(false);
    }
}
