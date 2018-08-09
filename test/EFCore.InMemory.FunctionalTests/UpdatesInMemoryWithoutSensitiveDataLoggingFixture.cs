// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class UpdatesInMemoryWithoutSensitiveDataLoggingFixture : UpdatesInMemoryFixtureBase
    {
        protected override string StoreName { get; } = "UpdateTestInsensitive";

        protected override ITestStoreFactory TestStoreFactory => InMemoryTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).EnableSensitiveDataLogging(false);
    }
}
