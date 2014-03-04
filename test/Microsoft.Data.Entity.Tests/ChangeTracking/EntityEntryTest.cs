// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Threading;
using Microsoft.Data.Entity.ChangeTracking;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public class EntityEntryTest
    {
        [Fact]
        public void Members_check_arguments()
        {
            Assert.Equal(
                "stateEntry",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new EntityEntry(null)).ParamName);

            Assert.Equal(
                "stateEntry",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new EntityEntry<Random>(null)).ParamName);

            var entry = new EntityEntry(new Mock<StateEntry>().Object);

            Assert.Equal(
                Strings.ArgumentIsEmpty("propertyName"),
                Assert.Throws<ArgumentException>(() => entry.Property("")).Message);

            var genericEntry = new EntityEntry<Random>(new Mock<StateEntry>().Object);

            Assert.Equal(
                "propertyExpression",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => genericEntry.Property((Expression<Func<Random, int>>)null)).ParamName);
        }

        [Fact]
        public void Can_obtain_entity_instance()
        {
            var entity = new Random();

            var stateEntryMock = new Mock<StateEntry>();
            stateEntryMock.Setup(m => m.Entity).Returns(entity);

            Assert.Same(entity, new EntityEntry(stateEntryMock.Object).Entity);
            Assert.Same(entity, new EntityEntry<Random>(stateEntryMock.Object).Entity);
        }

        [Fact]
        public void Can_obtain_underlying_state_entry()
        {
            var stateEntry = new Mock<StateEntry>().Object;

            Assert.Same(stateEntry, new EntityEntry(stateEntry).StateEntry);
            Assert.Same(stateEntry, new EntityEntry<Random>(stateEntry).StateEntry);
        }

        [Fact]
        public void State_change_is_delegated_to_low_level_object()
        {
            var stateEntryMock = new Mock<StateEntry>();

            new EntityEntry<Random>(stateEntryMock.Object) { State = EntityState.Added };

            stateEntryMock.Verify(m => m.SetEntityStateAsync(EntityState.Added, CancellationToken.None));
        }

        [Fact]
        public void Reading_State_is_delegated_to_low_level_object()
        {
            var stateEntryMock = new Mock<StateEntry>();
            stateEntryMock.Setup(m => m.EntityState).Returns(EntityState.Modified);

            Assert.Equal(EntityState.Modified, new EntityEntry<Random>(stateEntryMock.Object).State);
        }

        [Fact]
        public void Can_get_property_entry_by_name()
        {
            var entry = new EntityEntry<Random>(new Mock<StateEntry>().Object);

            Assert.Equal("Name", entry.Property("Name").Name);
        }

        [Fact]
        public void Can_get_property_entry_by_lambda()
        {
            var entry = new EntityEntry<Chunky>(new Mock<StateEntry>().Object);

            Assert.Equal("Monkey", entry.Property(e => e.Monkey).Name);
        }

        private class Chunky
        {
            public int Monkey { get; set; }
        }
    }
}
