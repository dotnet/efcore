// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

public class StateDataTest
{
    [ConditionalFact]
    public void Can_read_and_manipulate_modification_flags()
    {
        for (var i = 0; i < 70; i++)
        {
            PropertyManipulation(
                i,
                InternalEntityEntry.PropertyFlag.Modified,
                InternalEntityEntry.PropertyFlag.Null,
                InternalEntityEntry.PropertyFlag.Unknown,
                InternalEntityEntry.PropertyFlag.IsLoaded,
                InternalEntityEntry.PropertyFlag.IsTemporary,
                InternalEntityEntry.PropertyFlag.IsStoreGenerated);
        }
    }

    [ConditionalFact]
    public void Can_read_and_manipulate_null_flags()
    {
        for (var i = 0; i < 70; i++)
        {
            PropertyManipulation(
                i,
                InternalEntityEntry.PropertyFlag.Null,
                InternalEntityEntry.PropertyFlag.Modified,
                InternalEntityEntry.PropertyFlag.Unknown,
                InternalEntityEntry.PropertyFlag.IsLoaded,
                InternalEntityEntry.PropertyFlag.IsTemporary,
                InternalEntityEntry.PropertyFlag.IsStoreGenerated);
        }
    }

    [ConditionalFact]
    public void Can_read_and_manipulate_not_set_flags()
    {
        for (var i = 0; i < 70; i++)
        {
            PropertyManipulation(
                i,
                InternalEntityEntry.PropertyFlag.Unknown,
                InternalEntityEntry.PropertyFlag.Modified,
                InternalEntityEntry.PropertyFlag.Null,
                InternalEntityEntry.PropertyFlag.IsLoaded,
                InternalEntityEntry.PropertyFlag.IsTemporary,
                InternalEntityEntry.PropertyFlag.IsStoreGenerated);
        }
    }

    [ConditionalFact]
    public void Can_read_and_manipulate_is_loaded_flags()
    {
        for (var i = 0; i < 70; i++)
        {
            PropertyManipulation(
                i,
                InternalEntityEntry.PropertyFlag.IsLoaded,
                InternalEntityEntry.PropertyFlag.Modified,
                InternalEntityEntry.PropertyFlag.Null,
                InternalEntityEntry.PropertyFlag.Unknown,
                InternalEntityEntry.PropertyFlag.IsTemporary,
                InternalEntityEntry.PropertyFlag.IsStoreGenerated);
        }
    }

    [ConditionalFact]
    public void Can_read_and_manipulate_temporary_flags()
    {
        for (var i = 0; i < 70; i++)
        {
            PropertyManipulation(
                i,
                InternalEntityEntry.PropertyFlag.IsTemporary,
                InternalEntityEntry.PropertyFlag.IsLoaded,
                InternalEntityEntry.PropertyFlag.Modified,
                InternalEntityEntry.PropertyFlag.Null,
                InternalEntityEntry.PropertyFlag.Unknown,
                InternalEntityEntry.PropertyFlag.IsStoreGenerated);
        }
    }

    [ConditionalFact]
    public void Can_read_and_manipulate_store_generated_flags()
    {
        for (var i = 0; i < 70; i++)
        {
            PropertyManipulation(
                i,
                InternalEntityEntry.PropertyFlag.IsStoreGenerated,
                InternalEntityEntry.PropertyFlag.IsLoaded,
                InternalEntityEntry.PropertyFlag.Modified,
                InternalEntityEntry.PropertyFlag.Null,
                InternalEntityEntry.PropertyFlag.Unknown,
                InternalEntityEntry.PropertyFlag.IsTemporary);
        }
    }

    private void PropertyManipulation(
        int propertyCount,
        InternalEntityEntry.PropertyFlag propertyFlag,
        InternalEntityEntry.PropertyFlag unusedFlag1,
        InternalEntityEntry.PropertyFlag unusedFlag2,
        InternalEntityEntry.PropertyFlag unusedFlag3,
        InternalEntityEntry.PropertyFlag unusedFlag4,
        InternalEntityEntry.PropertyFlag unusedFlag5)
    {
        var data = new InternalEntityEntry.StateData(propertyCount, propertyCount);

        Assert.False(data.AnyPropertiesFlagged(propertyFlag));
        Assert.False(data.AnyPropertiesFlagged(unusedFlag1));
        Assert.False(data.AnyPropertiesFlagged(unusedFlag2));
        Assert.False(data.AnyPropertiesFlagged(unusedFlag3));
        Assert.False(data.AnyPropertiesFlagged(unusedFlag4));
        Assert.False(data.AnyPropertiesFlagged(unusedFlag5));

        for (var i = 0; i < propertyCount; i++)
        {
            data.FlagProperty(i, propertyFlag, true);

            for (var j = 0; j < propertyCount; j++)
            {
                Assert.Equal(j <= i, data.IsPropertyFlagged(j, propertyFlag));
                Assert.False(data.IsPropertyFlagged(j, unusedFlag1));
                Assert.False(data.IsPropertyFlagged(j, unusedFlag2));
                Assert.False(data.IsPropertyFlagged(j, unusedFlag3));
                Assert.False(data.IsPropertyFlagged(j, unusedFlag4));
                Assert.False(data.IsPropertyFlagged(j, unusedFlag5));
            }

            Assert.True(data.AnyPropertiesFlagged(propertyFlag));
            Assert.False(data.AnyPropertiesFlagged(unusedFlag1));
            Assert.False(data.AnyPropertiesFlagged(unusedFlag2));
            Assert.False(data.AnyPropertiesFlagged(unusedFlag3));
            Assert.False(data.AnyPropertiesFlagged(unusedFlag4));
            Assert.False(data.AnyPropertiesFlagged(unusedFlag5));
        }

        for (var i = 0; i < propertyCount; i++)
        {
            data.FlagProperty(i, propertyFlag, false);

            for (var j = 0; j < propertyCount; j++)
            {
                Assert.Equal(j > i, data.IsPropertyFlagged(j, propertyFlag));
                Assert.False(data.IsPropertyFlagged(j, unusedFlag1));
                Assert.False(data.IsPropertyFlagged(j, unusedFlag2));
                Assert.False(data.IsPropertyFlagged(j, unusedFlag3));
                Assert.False(data.IsPropertyFlagged(j, unusedFlag4));
                Assert.False(data.IsPropertyFlagged(j, unusedFlag5));
            }

            Assert.Equal(i < propertyCount - 1, data.AnyPropertiesFlagged(propertyFlag));
            Assert.False(data.AnyPropertiesFlagged(unusedFlag1));
            Assert.False(data.AnyPropertiesFlagged(unusedFlag2));
            Assert.False(data.AnyPropertiesFlagged(unusedFlag3));
            Assert.False(data.AnyPropertiesFlagged(unusedFlag4));
            Assert.False(data.AnyPropertiesFlagged(unusedFlag5));
        }

        for (var i = 0; i < propertyCount; i++)
        {
            Assert.False(data.IsPropertyFlagged(i, propertyFlag));
            Assert.False(data.IsPropertyFlagged(i, unusedFlag1));
            Assert.False(data.IsPropertyFlagged(i, unusedFlag2));
            Assert.False(data.IsPropertyFlagged(i, unusedFlag3));
            Assert.False(data.IsPropertyFlagged(i, unusedFlag4));
            Assert.False(data.IsPropertyFlagged(i, unusedFlag5));
        }

        data.FlagAllProperties(propertyCount, propertyFlag, flagged: true);

        Assert.Equal(propertyCount > 0, data.AnyPropertiesFlagged(propertyFlag));
        Assert.False(data.AnyPropertiesFlagged(unusedFlag1));
        Assert.False(data.AnyPropertiesFlagged(unusedFlag2));
        Assert.False(data.AnyPropertiesFlagged(unusedFlag3));
        Assert.False(data.AnyPropertiesFlagged(unusedFlag4));
        Assert.False(data.AnyPropertiesFlagged(unusedFlag5));

        for (var i = 0; i < propertyCount; i++)
        {
            Assert.True(data.IsPropertyFlagged(i, propertyFlag));
            Assert.False(data.IsPropertyFlagged(i, unusedFlag1));
            Assert.False(data.IsPropertyFlagged(i, unusedFlag2));
            Assert.False(data.IsPropertyFlagged(i, unusedFlag3));
            Assert.False(data.IsPropertyFlagged(i, unusedFlag4));
            Assert.False(data.IsPropertyFlagged(i, unusedFlag5));
        }

        data.FlagAllProperties(propertyCount, propertyFlag, flagged: false);

        Assert.False(data.AnyPropertiesFlagged(propertyFlag));
        Assert.False(data.AnyPropertiesFlagged(unusedFlag1));
        Assert.False(data.AnyPropertiesFlagged(unusedFlag2));
        Assert.False(data.AnyPropertiesFlagged(unusedFlag3));
        Assert.False(data.AnyPropertiesFlagged(unusedFlag4));
        Assert.False(data.AnyPropertiesFlagged(unusedFlag5));

        for (var i = 0; i < propertyCount; i++)
        {
            Assert.False(data.IsPropertyFlagged(i, propertyFlag));
            Assert.False(data.IsPropertyFlagged(i, unusedFlag1));
            Assert.False(data.IsPropertyFlagged(i, unusedFlag2));
            Assert.False(data.IsPropertyFlagged(i, unusedFlag3));
            Assert.False(data.IsPropertyFlagged(i, unusedFlag4));
            Assert.False(data.IsPropertyFlagged(i, unusedFlag5));
        }
    }

    [ConditionalFact]
    public void Can_get_and_set_EntityState()
    {
        var data = new InternalEntityEntry.StateData(70, 0);

        Assert.Equal(EntityState.Detached, data.EntityState);

        data.EntityState = EntityState.Unchanged;
        Assert.Equal(EntityState.Unchanged, data.EntityState);

        data.EntityState = EntityState.Modified;
        Assert.Equal(EntityState.Modified, data.EntityState);

        data.EntityState = EntityState.Added;
        Assert.Equal(EntityState.Added, data.EntityState);

        data.EntityState = EntityState.Deleted;
        Assert.Equal(EntityState.Deleted, data.EntityState);

        data.FlagAllProperties(70, InternalEntityEntry.PropertyFlag.Modified, flagged: true);

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
