// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class TPTTableSplittingSqliteTest : TPTTableSplittingTestBase
{
    public TPTTableSplittingSqliteTest(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    public override Task Can_insert_dependent_with_just_one_parent()
        // This scenario is not valid for TPT
        => Task.CompletedTask;

    protected override ITestStoreFactory TestStoreFactory
        => SqliteTestStoreFactory.Instance;
}
