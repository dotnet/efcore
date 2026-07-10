// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class F1InMemoryFixture : F1InMemoryFixtureBase<byte[]>;

public class F1ULongInMemoryFixture : F1InMemoryFixtureBase<ulong>;

public abstract class F1InMemoryFixtureBase<TRowVersion> : F1FixtureBase<TRowVersion>
{
    public override TestHelpers TestHelpers
        => InMemoryTestHelpers.Instance;

    protected override ITestStoreFactory TestStoreFactory
        => InMemoryTestStoreFactory.Instance;

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).ConfigureWarnings(e => e.Ignore(InMemoryEventId.TransactionIgnoredWarning));
}
