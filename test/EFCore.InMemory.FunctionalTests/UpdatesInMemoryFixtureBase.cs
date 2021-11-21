// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore;

public abstract class UpdatesInMemoryFixtureBase : UpdatesFixtureBase
{
    protected override ITestStoreFactory TestStoreFactory
        => InMemoryTestStoreFactory.Instance;

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).ConfigureWarnings(w => w.Log(InMemoryEventId.TransactionIgnoredWarning));
}
