// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public class FiltersInheritanceBulkUpdatesSqlServerFixture : InheritanceBulkUpdatesSqlServerFixture
{
    protected override bool EnableFilters
        => true;
}
