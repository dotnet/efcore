// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindQueryFiltersCustomizer : IModelCustomizer
    {
        public void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            ((NorthwindContext)context).ConfigureFilters(modelBuilder);
        }
    }
}
