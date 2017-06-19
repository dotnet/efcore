// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Xunit;
// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class OwnedQueryTestBase
    {
        [Fact]
        public virtual void Query_for_base_type_loads_all_owned_navs()
        {
            using (var context = CreateContext())
            {
                var people = context.Set<OwnedPerson>().ToList();
                
                Assert.Equal(4, people.Count);
                Assert.True(people.All(p => p.PersonAddress != null));
                Assert.True(people.OfType<Branch>().All(b => b.BranchAddress != null));
                Assert.True(people.OfType<LeafA>().All(a => a.LeafAAddress != null));
                Assert.True(people.OfType<LeafB>().All(b => b.LeafBAddress != null));
            }
        }
        
        [Fact]
        public virtual void Query_for_branch_type_loads_all_owned_navs()
        {
            using (var context = CreateContext())
            {
                var people = context.Set<Branch>().ToList();

                Assert.Equal(2, people.Count);
                Assert.True(people.All(p => p.PersonAddress != null));
                Assert.True(people.All(b => b.BranchAddress != null));
                Assert.True(people.OfType<LeafA>().All(a => a.LeafAAddress != null));
            }
        }

        [Fact]
        public virtual void Query_for_leaf_type_loads_all_owned_navs()
        {
            using (var context = CreateContext())
            {
                var people = context.Set<LeafA>().ToList();

                Assert.Equal(1, people.Count);
                Assert.True(people.All(p => p.PersonAddress != null));
                Assert.True(people.All(b => b.BranchAddress != null));
                Assert.True(people.All(a => a.LeafAAddress != null));
            }
        }

        protected abstract DbContext CreateContext();
    }
}
