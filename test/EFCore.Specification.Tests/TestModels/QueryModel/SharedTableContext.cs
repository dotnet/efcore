// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;


namespace Microsoft.EntityFrameworkCore.TestModels.QueryModel
{
    public class SharedTableContext : PoolableDbContext
    {
        public SharedTableContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<MeterReading> MeterReadings { get; set; }
        public DbSet<MeterReadingDetail> MeterReadingDetails { get; set; }
    }
}
