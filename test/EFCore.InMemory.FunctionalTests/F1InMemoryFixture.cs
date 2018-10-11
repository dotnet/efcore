// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.InMemory.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class F1InMemoryFixture : F1FixtureBase
    {
        public override ModelBuilder CreateModelBuilder()
            => new ModelBuilder(InMemoryConventionSetBuilder.Build());

        protected override ITestStoreFactory TestStoreFactory => InMemoryTestStoreFactory.Instance;
    }
}
