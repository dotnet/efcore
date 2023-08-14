﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public class TPHFiltersInheritanceBulkUpdatesSqlServerFixture : TPHInheritanceBulkUpdatesSqlServerFixture
{
    protected override string StoreName
        => "FiltersInheritanceBulkUpdatesTest";

    public override bool EnableFilters
        => true;
}
