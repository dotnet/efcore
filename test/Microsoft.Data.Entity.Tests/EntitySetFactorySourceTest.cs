// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class EntitySetFactorySourceTest
    {
        [Fact]
        public void Can_create_new_generic_EntitySet()
        {
            var context = Mock.Of<EntityContext>();

            var factorySource = new EntitySetSource();

            var set = factorySource.Create(context, typeof(Random));

            Assert.IsType<EntitySet<Random>>(set);
            Assert.Same(context, set.Context);
        }

        [Fact]
        public void Always_creates_a_new_EntitySet_instance()
        {
            var context = Mock.Of<EntityContext>();

            var factorySource = new EntitySetSource();

            Assert.NotSame(factorySource.Create(context, typeof(Random)), factorySource.Create(context, typeof(Random)));
        }
    }
}
