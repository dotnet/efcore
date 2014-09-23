// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class IncludeNorthwindTest : IncludeNorthwindTestBase, IClassFixture<NorthwindQueryFixture>
    {
        public override void Include_reference_dependent_already_tracked()
        {
            base.Include_reference_dependent_already_tracked();
        }

        private readonly NorthwindQueryFixture _fixture;

        public IncludeNorthwindTest(NorthwindQueryFixture fixture)
        {
            _fixture = fixture;
        }

        protected override DbContext CreateContext()
        {
            return _fixture.CreateContext();
        }
    }
}
