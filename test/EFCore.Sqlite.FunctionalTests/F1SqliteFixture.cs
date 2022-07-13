// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class F1ULongSqliteFixture : F1SqliteFixtureBase<ulong?>
{
    protected override string StoreName { get; } = "F1ULongTest";
}

public class F1SqliteFixture : F1SqliteFixtureBase<byte[]>
{
}

public abstract class F1SqliteFixtureBase<TRowVersion> : F1RelationalFixture<TRowVersion>
{
    protected override ITestStoreFactory TestStoreFactory
        => PrivateCacheSqliteTestStoreFactory.Instance;

    public override TestHelpers TestHelpers
        => SqliteTestHelpers.Instance;
}
