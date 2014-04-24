// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class ContextSetsTest
    {
        [Fact]
        public void Generic_method_creates_new_generic_DbSet()
        {
            var context = Mock.Of<DbContext>();
            var sets = new ContextSets();

            var set = sets.GetSet<string>(context);

            Assert.IsType<DbSet<string>>(set);
            Assert.Same(context.Configuration, set.Configuration);
        }

        [Fact]
        public void Non_generic_method_still_creates_new_generic_DbSet()
        {
            var context = Mock.Of<DbContext>();
            var sets = new ContextSets();

            var set = sets.GetSet(context, typeof(string));

            Assert.IsType<DbSet<string>>(set);
            Assert.Same(context.Configuration, set.Configuration);
        }

        [Fact]
        public void Set_created_using_generic_method_is_cached_and_returned()
        {
            var context = Mock.Of<DbContext>();
            var sets = new ContextSets();

            var set = sets.GetSet<string>(context);

            Assert.Same(set, sets.GetSet(context, typeof(string)));
        }

        [Fact]
        public void Set_created_using_non_generic_method_is_cached_and_returned()
        {
            var context = Mock.Of<DbContext>();
            var sets = new ContextSets();

            var set = sets.GetSet(context, typeof(string));

            Assert.Same(set, sets.GetSet<string>(context));
        }
    }
}
