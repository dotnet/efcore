// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
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
}
