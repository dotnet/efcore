// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class SupplementalBuiltInDataTypesFixture : SupplementalBuiltInDataTypesFixtureBase
    {
        public override DbContext CreateContext()
        {
            var options = new DbContextOptions()
                .UseModel(CreateModel())
                .UseInMemoryStore();

            return new DbContext(options);
        }
    }
}
