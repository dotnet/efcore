// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class TPCFiltersInheritanceQueryTestBase<TFixture>(TFixture fixture) : FiltersInheritanceQueryTestBase<TFixture>(fixture)
    where TFixture : TPCInheritanceQueryFixture, new();
