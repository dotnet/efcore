// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Data.Entity.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.Redis.FunctionalTests
{
    public class NorthwindAsyncQueryTest : IClassFixture<NorthwindQueryFixture>
    {
        private readonly NorthwindQueryFixture _fixture;

        public NorthwindAsyncQueryTest(NorthwindQueryFixture fixture)
        {
            _fixture = fixture;
        }

        protected DbContext CreateContext()
        {
            return _fixture.CreateContext();
        }
    }
}
