// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Data.Entity.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.Redis.FunctionalTests
{
    public class RedisAsyncQueryTest : AsyncQueryTestBase<RedisNorthwindQueryFixture>
    {
        public RedisAsyncQueryTest(RedisNorthwindQueryFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public override Task GroupBy_Distinct()
        {
            // TODO: there is a bug in the base test which we don't see
            // with the other providers - raised issue #784 to get that working
            return Task.FromResult(0);
        }
    }
}
