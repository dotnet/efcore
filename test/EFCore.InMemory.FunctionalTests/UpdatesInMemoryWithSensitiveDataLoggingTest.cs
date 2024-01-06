// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.InMemory.Internal;

namespace Microsoft.EntityFrameworkCore;

public class UpdatesInMemoryWithSensitiveDataLoggingTest(UpdatesInMemoryWithSensitiveDataLoggingTest.UpdatesInMemoryWithSensitiveDataLoggingFixture fixture)
        : UpdatesInMemoryTestBase<UpdatesInMemoryWithSensitiveDataLoggingTest.UpdatesInMemoryWithSensitiveDataLoggingFixture>(fixture)
{
    protected override string UpdateConcurrencyTokenMessage
        => InMemoryStrings.UpdateConcurrencyTokenExceptionSensitive(
            "Product", "{Id: 984ade3c-2f7b-4651-a351-642e92ab7146}", "{Price: 3.49}", "{Price: 1.49}");

    public class UpdatesInMemoryWithSensitiveDataLoggingFixture : UpdatesInMemoryFixtureBase
    {
        protected override string StoreName
            => "UpdateTestSensitive";

        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).EnableSensitiveDataLogging();
    }
}
