// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.Redis.FunctionalTests
{
    public class AsNoTrackingTest : IClassFixture<NorthwindQueryFixture>
    {
        private readonly NorthwindQueryFixture _fixture;

        public AsNoTrackingTest(NorthwindQueryFixture fixture)
        {
            _fixture = fixture;
        }

        protected DbContext CreateContext()
        {
            return _fixture.CreateContext();
        }
    }
}
