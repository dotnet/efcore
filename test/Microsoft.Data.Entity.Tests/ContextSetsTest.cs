// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using Microsoft.Data.Entity.Infrastructure;
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
