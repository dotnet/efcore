// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;
using Xunit;

namespace Microsoft.Data.Entity.Redis.FunctionalTests
{
    public class RedisQueryTest : QueryTestBase<RedisNorthwindQueryFixture>
    {
        [Fact]
        public virtual void Include_throws_not_implemented()
        {
            Assert.Throws<NotImplementedException>(() =>
                {
                    using (var context = Fixture.CreateContext())
                    {
                        // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                        context.Set<Customer>().Include(c => c.Orders).ToList();
                    }
                });
        }

        public RedisQueryTest(RedisNorthwindQueryFixture fixture)
            : base(fixture)
        {
        }
    }
}
