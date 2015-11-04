// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.ChangeTracking.Internal;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking.Internal
{
    public class StateDataTest
    {
        [Fact]
        public void Can_read_and_manipulate_modification_flags()
        {
            for (var i = 0; i < 70; i++)
            {
                PropertyManipulation(
                    i,
                    InternalEntityEntry.PropertyFlag.TemporaryOrModified,
                    InternalEntityEntry.PropertyFlag.Null);
            }
        }

        [Fact]
        public void Can_read_and_manipulate_null_flags()
        {
            for (var i = 0; i < 70; i++)
            {
                PropertyManipulation(
                    i,
                    InternalEntityEntry.PropertyFlag.Null,
                    InternalEntityEntry.PropertyFlag.TemporaryOrModified);
            }
        }

        private void PropertyManipulation(
            int propertyCount,
            InternalEntityEntry.PropertyFlag propertyFlag,
            InternalEntityEntry.PropertyFlag unusedFlag)
        {
            var data = new InternalEntityEntry.StateData(propertyCount);

            Assert.False(data.AnyPropertiesFlagged(propertyFlag));
            Assert.False(data.AnyPropertiesFlagged(unusedFlag));

            for (var i = 0; i < propertyCount; i++)
            {
                data.FlagProperty(i, propertyFlag, true);

                for (var j = 0; j < propertyCount; j++)
                {
                    Assert.Equal(j <= i, data.IsPropertyFlagged(j, propertyFlag));
                    Assert.False(data.IsPropertyFlagged(j, unusedFlag));
                }

                Assert.True(data.AnyPropertiesFlagged(propertyFlag));
                Assert.False(data.AnyPropertiesFlagged(unusedFlag));
            }

            for (var i = 0; i < propertyCount; i++)
            {
                data.FlagProperty(i, propertyFlag, false);

                for (var j = 0; j < propertyCount; j++)
                {
                    Assert.Equal(j > i, data.IsPropertyFlagged(j, propertyFlag));
                    Assert.False(data.IsPropertyFlagged(j, unusedFlag));
                }

                Assert.Equal(i < propertyCount - 1, data.AnyPropertiesFlagged(propertyFlag));
                Assert.False(data.AnyPropertiesFlagged(unusedFlag));
            }

            for (var i = 0; i < propertyCount; i++)
            {
                Assert.False(data.IsPropertyFlagged(i, propertyFlag));
                Assert.False(data.IsPropertyFlagged(i, unusedFlag));
            }

            data.FlagAllProperties(propertyCount, propertyFlag, flagged: true);

            Assert.Equal(propertyCount > 0, data.AnyPropertiesFlagged(propertyFlag));
            Assert.False(data.AnyPropertiesFlagged(unusedFlag));

            for (var i = 0; i < propertyCount; i++)
            {
                Assert.True(data.IsPropertyFlagged(i, propertyFlag));
                Assert.False(data.IsPropertyFlagged(i, unusedFlag));
            }

            data.FlagAllProperties(propertyCount, propertyFlag, flagged: false);

            Assert.False(data.AnyPropertiesFlagged(propertyFlag));
            Assert.False(data.AnyPropertiesFlagged(unusedFlag));

            for (var i = 0; i < propertyCount; i++)
            {
                Assert.False(data.IsPropertyFlagged(i, propertyFlag));
                Assert.False(data.IsPropertyFlagged(i, unusedFlag));
            }
        }

        [Fact]
        public void Can_get_and_set_EntityState()
        {
            var data = new InternalEntityEntry.StateData(70);

            Assert.Equal(EntityState.Detached, data.EntityState);

            data.EntityState = EntityState.Unchanged;
            Assert.Equal(EntityState.Unchanged, data.EntityState);

            data.EntityState = EntityState.Modified;
            Assert.Equal(EntityState.Modified, data.EntityState);

            data.EntityState = EntityState.Added;
            Assert.Equal(EntityState.Added, data.EntityState);

            data.EntityState = EntityState.Deleted;
            Assert.Equal(EntityState.Deleted, data.EntityState);

            data.FlagAllProperties(70, InternalEntityEntry.PropertyFlag.TemporaryOrModified, flagged: true);

            Assert.Equal(EntityState.Deleted, data.EntityState);

            data.EntityState = EntityState.Unchanged;
            Assert.Equal(EntityState.Unchanged, data.EntityState);

            data.EntityState = EntityState.Modified;
            Assert.Equal(EntityState.Modified, data.EntityState);

            data.EntityState = EntityState.Added;
            Assert.Equal(EntityState.Added, data.EntityState);

            data.EntityState = EntityState.Detached;
            Assert.Equal(EntityState.Detached, data.EntityState);
        }
    }
}
