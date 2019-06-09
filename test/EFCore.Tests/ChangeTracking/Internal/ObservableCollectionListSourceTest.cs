// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using Xunit;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public class ObservableCollectionListSourceTest
    {
        [ConditionalFact]
        public void ObservableCollectionListSource_exposes_ObservableCollection_parameterless_constructor()
        {
            var ols = new ObservableCollectionListSource<FakeEntity>();
            Assert.Equal(0, ols.Count);
        }

        [ConditionalFact]
        public void ObservableCollectionListSource_exposes_ObservableCollection_IEnumerable_constructor()
        {
            IEnumerable<FakeEntity> entities = new[] { new FakeEntity(), new FakeEntity() };
            var ols = new ObservableCollectionListSource<FakeEntity>(entities);
            Assert.Equal(2, ols.Count);
        }

        [ConditionalFact]
        public void ObservableCollectionListSource_exposes_ObservableCollection_List_constructor()
        {
            var entities = new List<FakeEntity>
            {
                new FakeEntity(),
                new FakeEntity()
            };
            var ols = new ObservableCollectionListSource<FakeEntity>(entities);
            Assert.Equal(2, ols.Count);
        }

        [ConditionalFact]
        public void ObservableCollectionListSource_ContainsListCollection_returns_false()
        {
            Assert.False(((IListSource)new ObservableCollectionListSource<FakeEntity>()).ContainsListCollection);
        }

        [ConditionalFact]
        public void ObservableCollectionListSource_GetList_returns_BindingList_attached_to_the_ObservableCollection()
        {
            var ols = new ObservableCollectionListSource<FakeEntity>
            {
                new FakeEntity(),
                new FakeEntity()
            };
            var bindingList = ((IListSource)ols).GetList();

            Assert.Equal(2, bindingList.Count);

            ols.Add(new FakeEntity());
            Assert.Equal(3, bindingList.Count);

            ols.Remove(ols[0]);
            Assert.Equal(2, bindingList.Count);

            bindingList.Add(new FakeEntity());
            Assert.Equal(3, ols.Count);

            bindingList.RemoveAt(0);
            Assert.Equal(2, ols.Count);
        }

        [ConditionalFact]
        public void The_BindingList_returned_from_ObservableCollectionListSource_GetList_is_cached()
        {
            var ols = new ObservableCollectionListSource<FakeEntity>();
            var bindingList = ((IListSource)ols).GetList();

            Assert.Same(bindingList, ((IListSource)ols).GetList());
        }

        private class FakeEntity
        {
            public int Id { get; set; }
        }
    }
}
