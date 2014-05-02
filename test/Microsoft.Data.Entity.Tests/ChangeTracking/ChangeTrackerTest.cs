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

using System;
using System.Linq;
using Microsoft.Data.Entity.ChangeTracking;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public class ChangeTrackerTest
    {
        [Fact]
        public void Members_check_arguments()
        {
            Assert.Equal(
                "stateManager",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new ChangeTracker(null)).ParamName);

            var changeTracker = new ChangeTracker(new Mock<StateManager>().Object);

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

            var changeTracker = new ChangeTracker(stateManagerMock.Object);

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
                new ChangeTracker(stateManagerMock.Object).Entries().Select(e => e.StateEntry).ToArray());
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
                new ChangeTracker(stateManagerMock.Object).Entries<Random>().Select(e => e.StateEntry).ToArray());

            Assert.Equal(
                new[] { stateEntryMock2.Object },
                new ChangeTracker(stateManagerMock.Object).Entries<string>().Select(e => e.StateEntry).ToArray());

            Assert.Equal(
                new[] { stateEntryMock1.Object, stateEntryMock2.Object, stateEntryMock3.Object },
                new ChangeTracker(stateManagerMock.Object).Entries<object>().Select(e => e.StateEntry).ToArray());
        }

        [Fact]
        public void Can_get_state_manager()
        {
            var stateManager = new Mock<StateManager>().Object;

            Assert.Same(stateManager, new ChangeTracker(stateManager).StateManager);
        }
    }
}
