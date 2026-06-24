// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

public class SnapshotTest
{
    [ConditionalFact]
    public void SetValue_sets_value()
    {
        ISnapshot snapshot = new Snapshot<int, string>(1, "A");

        snapshot.SetValue(0, 2);
        snapshot.SetValue(1, "B");

        Assert.Equal(2, snapshot.GetValue<int>(0));
        Assert.Equal("B", snapshot.GetValue<string>(1));
    }

    [ConditionalFact]
    public void SetValue_sets_value_on_multi_snapshot()
    {
        ISnapshot snapshot = new MultiSnapshot(
            [
                new Snapshot<int>(1),
                new Snapshot<string>("A")
            ]);

        snapshot.SetValue(0, 2);
        snapshot.SetValue(Snapshot.MaxGenericTypes, "B");

        Assert.Equal(2, snapshot.GetValue<int>(0));
        Assert.Equal("B", snapshot.GetValue<string>(Snapshot.MaxGenericTypes));
    }

    [ConditionalFact]
    public void SetValue_throws_for_empty_snapshot()
        => Assert.Throws<IndexOutOfRangeException>(() => Snapshot.Empty.SetValue(0, "A"));
}
