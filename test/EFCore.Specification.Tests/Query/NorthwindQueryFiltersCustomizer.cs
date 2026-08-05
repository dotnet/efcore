// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindQueryFiltersCustomizer : NoopModelCustomizer
{
    public override void Customize(ModelBuilder modelBuilder, DbContext context)
        => ((NorthwindContext)context).ConfigureFilters(modelBuilder);
}
