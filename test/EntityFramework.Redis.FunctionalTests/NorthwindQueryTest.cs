// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Xunit;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;

namespace Microsoft.Data.Entity.Redis.FunctionalTests
{
    public class NorthwindQueryTest : IClassFixture<NorthwindQueryFixture>
    {
        [Fact]
        public virtual void Include_throws_not_implemented()
        {
            Assert.Throws<NotImplementedException>(() =>
                {
                    using (var context = CreateContext())
                    {
                        // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                        context.Set<Customer>().Include(c => c.Orders).ToList();
                    }
                });
        }

        private readonly NorthwindQueryFixture _fixture;

        public NorthwindQueryTest(NorthwindQueryFixture fixture)
        {
            _fixture = fixture;
        }

        protected DbContext CreateContext()
        {
            return _fixture.CreateContext();
        }
    }
}
