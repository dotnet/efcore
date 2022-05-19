// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class TPCTableSplittingSqliteTest : TPCTableSplittingTestBase
{
    public TPCTableSplittingSqliteTest(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    protected override ITestStoreFactory TestStoreFactory
        => SqliteTestStoreFactory.Instance;
}
