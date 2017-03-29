// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NET46

using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.ChangeTracking.Internal
{
    public class ObservableHashSetListSourceTest
    {
        [Fact]
        public void ObservableHashSetListSource_exposes_ObervableCollection_parameterless_constructor()
        {
            var ols = new ObservableHashSetListSource<FakeEntity>();
            Assert.Equal(0, ols.Count);
        }

        [Fact]
        public void ObservableHashSetListSource_exposes_ObervableCollection_IEnumerable_constructor()
        {
            IEnumerable<FakeEntity> entities = new[] { new FakeEntity(), new FakeEntity() };
            var ols = new ObservableHashSetListSource<FakeEntity>(entities);
            Assert.Equal(2, ols.Count);
        }

        [Fact]
        public void ObservableHashSetListSource_exposes_ObervableCollection_List_constructor()
        {
            var entities = new List<FakeEntity>
            {
                new FakeEntity(),
                new FakeEntity()
            };
            var ols = new ObservableHashSetListSource<FakeEntity>(entities);
            Assert.Equal(2, ols.Count);
        }

        [Fact]
        public void ObservableHashSetListSource_ContainsListCollection_returns_false()
        {
            Assert.False(((IListSource)new ObservableHashSetListSource<FakeEntity>()).ContainsListCollection);
        }

        [Fact]
        public void ObservableHashSetListSource_GetList_returns_BindingList_attached_to_the_ObservableCollection()
        {
            var toRemove = new FakeEntity();

            var ols = new ObservableHashSetListSource<FakeEntity>
            {
                toRemove,
                new FakeEntity()
            };
            var bindingList = ((IListSource)ols).GetList();

            Assert.Equal(2, bindingList.Count);

            ols.Add(new FakeEntity());
            Assert.Equal(3, bindingList.Count);

            ols.Remove(toRemove);
            Assert.Equal(2, bindingList.Count);

            bindingList.Add(new FakeEntity());
            Assert.Equal(3, ols.Count);

            bindingList.RemoveAt(0);
            Assert.Equal(2, ols.Count);
        }

        [Fact]
        public void The_BindingList_returned_from_ObservableHashSetListSource_GetList_is_cached()
        {
            var ols = new ObservableHashSetListSource<FakeEntity>();
            var bindingList = ((IListSource)ols).GetList();

            Assert.Same(bindingList, ((IListSource)ols).GetList());
        }

        private class FakeEntity
        {
            public int Id { get; set; }
        }
    }
}
#elif NETCOREAPP2_0
#else
#error target frameworks need to be updated.
#endif
