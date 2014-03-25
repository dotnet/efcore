// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
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
            var sets = new ContextEntitySets(new EntitySetSource(), Mock.Of<EntitySetInitializer>());

            var set = sets.GetEntitySet<string>(context);

            Assert.IsType<EntitySet<string>>(set);
            Assert.Same(context, set.Context);
        }

        [Fact]
        public void Non_generic_method_still_creates_new_generic_EntitySet()
        {
            var context = Mock.Of<EntityContext>();
            var sets = new ContextEntitySets(new EntitySetSource(), Mock.Of<EntitySetInitializer>());

            var set = sets.GetEntitySet(context, typeof(string));

            Assert.IsType<EntitySet<string>>(set);
            Assert.Same(context, set.Context);
        }

        [Fact]
        public void Set_created_using_generic_method_is_cached_and_returned()
        {
            var context = Mock.Of<EntityContext>();
            var sets = new ContextEntitySets(new EntitySetSource(), Mock.Of<EntitySetInitializer>());

            var set = sets.GetEntitySet<string>(context);

            Assert.Same(set, sets.GetEntitySet(context, typeof(string)));
        }

        [Fact]
        public void Set_created_using_non_generic_method_is_cached_and_returned()
        {
            var context = Mock.Of<EntityContext>();
            var sets = new ContextEntitySets(new EntitySetSource(), Mock.Of<EntitySetInitializer>());

            var set = sets.GetEntitySet(context, typeof(string));

            Assert.Same(set, sets.GetEntitySet<string>(context));
        }
    }
}
