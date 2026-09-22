// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedTableSplitting;

public class OwnedTableSplittingMiscellaneousSqliteTest(
    OwnedTableSplittingSqliteFixture fixture,
    ITestOutputHelper testOutputHelper)
    : OwnedTableSplittingMiscellaneousRelationalTestBase<OwnedTableSplittingSqliteFixture>(fixture, testOutputHelper);
