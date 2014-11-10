// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
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

            stateEntryMock.VerifySet(m => m.EntityState = EntityState.Added);
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
            var stateEntryMock = CreateStateEntryMock();
            var entry = new EntityEntry<Random>(stateEntryMock.Object);

            Assert.Equal("Monkey", entry.Property("Monkey").Name);
        }

        [Fact]
        public void Can_get_property_entry_by_lambda()
        {
            var stateEntryMock = CreateStateEntryMock();
            var entry = new EntityEntry<Chunky>(stateEntryMock.Object);

            Assert.Equal("Monkey", entry.Property(e => e.Monkey).Name);
        }

        private static Mock<StateEntry> CreateStateEntryMock()
        {
            var propertyMock = new Mock<IProperty>();
            propertyMock.Setup(m => m.Name).Returns("Monkey");

            var entityTypeMock = new Mock<IEntityType>();
            entityTypeMock.Setup(m => m.GetProperty("Monkey")).Returns(propertyMock.Object);

            var stateEntryMock = new Mock<StateEntry>();
            stateEntryMock.Setup(m => m.EntityType).Returns(entityTypeMock.Object);
            return stateEntryMock;
        }

        private class Chunky
        {
            public int Monkey { get; set; }
        }
    }
}
