// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class F1SqliteFixture : F1RelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => PrivateCacheSqliteTestStoreFactory.Instance;

        public override TestHelpers TestHelpers
            => SqliteTestHelpers.Instance;
    }
}
