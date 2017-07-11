// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    public class F1InMemoryFixture : F1FixtureBase<InMemoryTestStore>
    {
        protected override TestStoreFactory<InMemoryTestStore> TestStoreFactory => InMemoryTestStoreFactory.Instance;
    }
}
