// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

#nullable disable

public abstract class TPHInheritanceBulkUpdatesTestBase<TFixture>(TFixture fixture, ITestOutputHelper testOutputHelper)
    : InheritanceBulkUpdatesRelationalTestBase<TFixture>(fixture, testOutputHelper)
    where TFixture : InheritanceBulkUpdatesRelationalFixtureBase, new();
