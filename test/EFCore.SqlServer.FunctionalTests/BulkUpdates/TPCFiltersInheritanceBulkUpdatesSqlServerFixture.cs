// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public class TPCFiltersInheritanceBulkUpdatesSqlServerFixture : TPCInheritanceBulkUpdatesSqlServerFixture
{
    protected override string StoreName
        => "TPCFiltersInheritanceBulkUpdatesTest";

    protected override bool EnableFilters
        => true;
}
