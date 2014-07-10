// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class StateDataTest
    {
        [Fact]
        public void Can_read_and_manipulate_property_state()
        {
            for (var i = 0; i < 70; i++)
            {
                PropertyManipulation(i);
            }
        }

        public void PropertyManipulation(int propertyCount)
        {
            var data = new StateEntry.StateData(propertyCount);

            Assert.False(data.AnyPropertiesModified());

            for (var i = 0; i < propertyCount; i++)
            {
                data.SetPropertyModified(i, true);

                for (var j = 0; j < propertyCount; j++)
                {
                    Assert.Equal(j <= i, data.IsPropertyModified(j));
                }

                Assert.True(data.AnyPropertiesModified());
            }

            for (var i = 0; i < propertyCount; i++)
            {
                data.SetPropertyModified(i, false);

                for (var j = 0; j < propertyCount; j++)
                {
                    Assert.Equal(j > i, data.IsPropertyModified(j));
                }

                Assert.Equal(i < propertyCount - 1, data.AnyPropertiesModified());
            }

            for (var i = 0; i < propertyCount; i++)
            {
                Assert.False(data.IsPropertyModified(i));
            }

            data.SetAllPropertiesModified(propertyCount);

            Assert.Equal(propertyCount > 0, data.AnyPropertiesModified());

            for (var i = 0; i < propertyCount; i++)
            {
                Assert.True(data.IsPropertyModified(i));
            }
        }

        [Fact]
        public void Can_get_and_set_EntityState()
        {
            var data = new StateEntry.StateData(70);

            Assert.Equal(EntityState.Unknown, data.EntityState);

            data.EntityState = EntityState.Unchanged;
            Assert.Equal(EntityState.Unchanged, data.EntityState);

            data.EntityState = EntityState.Modified;
            Assert.Equal(EntityState.Modified, data.EntityState);

            data.EntityState = EntityState.Added;
            Assert.Equal(EntityState.Added, data.EntityState);

            data.EntityState = EntityState.Deleted;
            Assert.Equal(EntityState.Deleted, data.EntityState);

            data.SetAllPropertiesModified(70);

            Assert.Equal(EntityState.Deleted, data.EntityState);

            data.EntityState = EntityState.Unchanged;
            Assert.Equal(EntityState.Unchanged, data.EntityState);

            data.EntityState = EntityState.Modified;
            Assert.Equal(EntityState.Modified, data.EntityState);

            data.EntityState = EntityState.Added;
            Assert.Equal(EntityState.Added, data.EntityState);

            data.EntityState = EntityState.Unknown;
            Assert.Equal(EntityState.Unknown, data.EntityState);
        }

        [Fact]
        public void Can_get_and_set_sidecar_flag()
        {
            var data = new StateEntry.StateData(70);

            Assert.False(data.TransparentSidecarInUse);

            data.TransparentSidecarInUse = true;
            Assert.True(data.TransparentSidecarInUse);

            data.TransparentSidecarInUse = false;
            Assert.False(data.TransparentSidecarInUse);

            data.SetAllPropertiesModified(70);

            Assert.False(data.TransparentSidecarInUse);

            data.TransparentSidecarInUse = true;
            Assert.True(data.TransparentSidecarInUse);

            data.TransparentSidecarInUse = false;
        }
    }
}
