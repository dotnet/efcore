// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class ContextEntitySetsTest
    {
        [Fact]
        public void Generic_method_creates_new_generic_EntitySet()
        {
            var context = Mock.Of<EntityContext>();

            var sets = new ContextEntitySets(context, new EntitySetSource());

            var set = sets.GetEntitySet<string>();

            Assert.IsType<EntitySet<string>>(set);
            Assert.Same(context, set.Context);
        }

        [Fact]
        public void Non_generic_method_still_creates_new_generic_EntitySet()
        {
            var context = Mock.Of<EntityContext>();

            var sets = new ContextEntitySets(context, new EntitySetSource());

            var set = sets.GetEntitySet(typeof(string));

            Assert.IsType<EntitySet<string>>(set);
            Assert.Same(context, set.Context);
        }

        [Fact]
        public void Set_created_using_generic_method_is_cached_and_returned()
        {
            var sets = new ContextEntitySets(Mock.Of<EntityContext>(), new EntitySetSource());

            var set = sets.GetEntitySet<string>();

            Assert.Same(set, sets.GetEntitySet(typeof(string)));
        }

        [Fact]
        public void Set_created_using_non_generic_method_is_cached_and_returned()
        {
            var sets = new ContextEntitySets(Mock.Of<EntityContext>(), new EntitySetSource());

            var set = sets.GetEntitySet(typeof(string));

            Assert.Same(set, sets.GetEntitySet<string>());
        }
    }
}
