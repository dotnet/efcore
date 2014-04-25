// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class DbSetySourceTest
    {
        [Fact]
        public void Can_create_new_generic_DbSet()
        {
            var context = Mock.Of<DbContext>();

            var factorySource = new DbSetSource();

            var set = factorySource.Create(context, typeof(Random));

            Assert.IsType<DbSet<Random>>(set);
            Assert.Same(context.Configuration, set.Configuration);
        }

        [Fact]
        public void Always_creates_a_new_DbSet_instance()
        {
            var context = Mock.Of<DbContext>();

            var factorySource = new DbSetSource();

            Assert.NotSame(factorySource.Create(context, typeof(Random)), factorySource.Create(context, typeof(Random)));
        }
    }
}
