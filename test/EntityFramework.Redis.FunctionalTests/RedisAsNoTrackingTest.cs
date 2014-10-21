// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;

namespace Microsoft.Data.Entity.Redis.FunctionalTests
{
    public class RedisAsNoTrackingTest : AsNoTrackingTestBase<RedisNorthwindQueryFixture>
    {
        public RedisAsNoTrackingTest(RedisNorthwindQueryFixture fixture)
            : base(fixture)
        {
        }
    }
}
