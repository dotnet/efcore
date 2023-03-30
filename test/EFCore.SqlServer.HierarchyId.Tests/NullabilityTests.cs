// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer;

public class NullabilityTests
{
    [ConditionalFact]
    public void Null_against_null()
    {
        Assert.True(null == (HierarchyId)null);
        Assert.False(null != (HierarchyId)null);
        Assert.False(null > (HierarchyId)null);
        Assert.True(null >= (HierarchyId)null);
        Assert.False(null < (HierarchyId)null);
        Assert.True(null <= (HierarchyId)null);
    }

    [ConditionalFact]
    public void Null_against_nonNull()
    {
        var hid = HierarchyId.GetRoot();
        Assert.False(hid == null);
        Assert.False(null == hid);

        Assert.True(hid != null);
        Assert.True(null != hid);

        Assert.True(hid > null);
        Assert.False(null > hid);

        Assert.True(hid >= null);
        Assert.False(null >= hid);

        Assert.False(hid < null);
        Assert.True(null < hid);

        Assert.False(hid <= null);
        Assert.True(null <= hid);
    }

    [ConditionalFact]
    public void NullOnly_aggregates_equalTo_null()
    {
        var hid = (HierarchyId)null;
        var collection = new[] { null, (HierarchyId)null, };
        var min = collection.Min();
        var max = collection.Max();

        Assert.True(hid == min);
        Assert.True(min == hid);
        Assert.False(hid != min);
        Assert.False(min != hid);

        Assert.True(hid == max);
        Assert.True(max == hid);
        Assert.False(hid != max);
        Assert.False(max != hid);
    }

    [ConditionalFact]
    public void Aggregates_including_nulls_equalTo_nonNull()
    {
        var hid = HierarchyId.GetRoot();
        var collection = new[] { null, null, HierarchyId.GetRoot(), HierarchyId.GetRoot(), };
        var min = collection.Min();
        var max = collection.Max();

        Assert.True(hid == min);
        Assert.True(min == hid);
        Assert.False(hid != min);
        Assert.False(min != hid);

        Assert.True(hid == max);
        Assert.True(max == hid);
        Assert.False(hid != max);
        Assert.False(max != hid);
    }
}
