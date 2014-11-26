// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class InMemoryIncludeOneToOneTest : IncludeOneToOneTestBase, IClassFixture<OneToOneQueryFixture>
    {
        private readonly OneToOneQueryFixture _fixture;

        public InMemoryIncludeOneToOneTest(OneToOneQueryFixture fixture)
        {
            _fixture = fixture;
        }

        protected override DbContext CreateContext()
        {
            return _fixture.CreateContext();
        }
    }
}
