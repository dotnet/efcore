// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public abstract class TPTFiltersInheritanceQueryTestBase<TFixture>(TFixture fixture) : FiltersInheritanceQueryTestBase<TFixture>(fixture)
    where TFixture : TPTInheritanceQueryFixture, new();
