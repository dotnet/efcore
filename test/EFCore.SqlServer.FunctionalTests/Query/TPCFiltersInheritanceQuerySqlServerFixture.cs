// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class TPCFiltersInheritanceQuerySqlServerFixture : TPCInheritanceQuerySqlServerFixture
{
    protected override bool EnableFilters
        => true;

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.UseKeySequence();

        base.OnModelCreating(modelBuilder, context);
    }
}
