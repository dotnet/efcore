// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class F1InMemoryFixture : F1InMemoryFixtureBase<byte[]>
    {
    }

    public class F1ULongInMemoryFixture : F1InMemoryFixtureBase<ulong>
    {
    }

    public abstract class F1InMemoryFixtureBase<TRowVersion> : F1FixtureBase<TRowVersion>
    {
        public override TestHelpers TestHelpers
            => InMemoryTestHelpers.Instance;

        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(e => e.Ignore(InMemoryEventId.TransactionIgnoredWarning));
    }
}
