// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexTableSplitting;

public class ComplexTableSplittingMiscellaneousSqliteTest(
    ComplexTableSplittingSqliteFixture fixture,
    ITestOutputHelper testOutputHelper)
    : ComplexTableSplittingMiscellaneousRelationalTestBase<ComplexTableSplittingSqliteFixture>(fixture, testOutputHelper);
