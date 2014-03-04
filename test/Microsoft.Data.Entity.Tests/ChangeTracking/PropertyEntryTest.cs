// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.ChangeTracking;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public class PropertyEntryTest
    {
        [Fact]
        public void Members_check_arguments()
        {
            Assert.Equal(
                Strings.ArgumentIsEmpty("name"),
                Assert.Throws<ArgumentException>(() => new PropertyEntry(new Mock<StateEntry>().Object, "")).Message);
            Assert.Equal(
                "stateEntry",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new PropertyEntry(null, "Kake")).ParamName);

            Assert.Equal(
                Strings.ArgumentIsEmpty("name"),
                Assert.Throws<ArgumentException>(() => new PropertyEntry<Random, int>(new Mock<StateEntry>().Object, "")).Message);
            Assert.Equal(
                "stateEntry",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new PropertyEntry<Random, int>(null, "Kake")).ParamName);
        }

        [Fact]
        public void Can_get_name()
        {
            Assert.Equal("Maturity", new PropertyEntry(new Mock<StateEntry>().Object, "Maturity").Name);
            Assert.Equal("Maturity", new PropertyEntry<Random, string>(new Mock<StateEntry>().Object, "Maturity").Name);
        }

        [Fact]
        public void IsModified_delegates_to_state_object()
        {
            var stateEntryMock = new Mock<StateEntry>();
            stateEntryMock.Setup(m => m.IsPropertyModified("Name")).Returns(true);

            var propertyEntry = new PropertyEntry<Random, string>(stateEntryMock.Object, "Name");

            Assert.True(propertyEntry.IsModified);
            stateEntryMock.Verify(m => m.IsPropertyModified("Name"));

            propertyEntry.IsModified = true;

            stateEntryMock.Verify(m => m.SetPropertyModified("Name", true));
        }
    }
}
