// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class ChangeTrackerTest
    {
        [Fact]
        public void Members_check_arguments()
        {
            Assert.Equal(
                "stateManager",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new ChangeTracker(null, null)).ParamName);

            var changeTracker = new ChangeTracker(Mock.Of<StateManager>(), Mock.Of<ChangeDetector>());

            Assert.Equal(
                "entity",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => changeTracker.Entry(null)).ParamName);
            Assert.Equal(
                "entity",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => changeTracker.Entry<Random>(null)).ParamName);
        }

        [Fact]
        public void Entry_methods_delegate_to_underlying_state_manager()
        {
            var entity = new Random();
            var stateManagerMock = new Mock<StateManager>();
            var stateEntry = new Mock<StateEntry>().Object;
            stateManagerMock.Setup(m => m.GetOrCreateEntry(entity)).Returns(stateEntry);

            var changeTracker = new ChangeTracker(stateManagerMock.Object, Mock.Of<ChangeDetector>());

            Assert.Same(stateEntry, changeTracker.Entry(entity).StateEntry);
            Assert.Same(stateEntry, changeTracker.Entry((object)entity).StateEntry);
        }

        [Fact]
        public void Can_get_all_entities()
        {
            var stateEntries = new[] { new Mock<StateEntry>().Object, new Mock<StateEntry>().Object };
            var stateManagerMock = new Mock<StateManager>();
            stateManagerMock.Setup(m => m.StateEntries).Returns(stateEntries);

            Assert.Equal(
                stateEntries,
                new ChangeTracker(stateManagerMock.Object, Mock.Of<ChangeDetector>()).Entries().Select(e => e.StateEntry).ToArray());
        }

        [Fact]
        public void Can_get_all_entities_for_an_entity_of_a_given_type()
        {
            var stateEntryMock1 = new Mock<StateEntry>();
            stateEntryMock1.Setup(m => m.Entity).Returns(new Random());
            var stateEntryMock2 = new Mock<StateEntry>();
            stateEntryMock2.Setup(m => m.Entity).Returns("");
            var stateEntryMock3 = new Mock<StateEntry>();
            stateEntryMock3.Setup(m => m.Entity).Returns(new Random());

            var stateManagerMock = new Mock<StateManager>();
            stateManagerMock.Setup(m => m.StateEntries)
                .Returns(new[] { stateEntryMock1.Object, stateEntryMock2.Object, stateEntryMock3.Object });

            Assert.Equal(
                new[] { stateEntryMock1.Object, stateEntryMock3.Object },
                new ChangeTracker(stateManagerMock.Object, Mock.Of<ChangeDetector>()).Entries<Random>().Select(e => e.StateEntry).ToArray());

            Assert.Equal(
                new[] { stateEntryMock2.Object },
                new ChangeTracker(stateManagerMock.Object, Mock.Of<ChangeDetector>()).Entries<string>().Select(e => e.StateEntry).ToArray());

            Assert.Equal(
                new[] { stateEntryMock1.Object, stateEntryMock2.Object, stateEntryMock3.Object },
                new ChangeTracker(stateManagerMock.Object, Mock.Of<ChangeDetector>()).Entries<object>().Select(e => e.StateEntry).ToArray());
        }

        [Fact]
        public void Can_get_state_manager()
        {
            var stateManager = new Mock<StateManager>().Object;

            Assert.Same(stateManager, new ChangeTracker(stateManager, Mock.Of<ChangeDetector>()).StateManager);
        }
    }
}
