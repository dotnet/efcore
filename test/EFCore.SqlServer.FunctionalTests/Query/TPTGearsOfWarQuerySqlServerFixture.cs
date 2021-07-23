﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class TPTGearsOfWarQuerySqlServerFixture : TPTGearsOfWarQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<City>().Property(g => g.Location).HasColumnType("varchar(100)");

            // No support yet for DateOnly/TimeOnly (#24507)
            modelBuilder.Entity<Mission>(
                b =>
                {
                    b.Ignore(m => m.Date);
                    b.Ignore(m => m.Time);
                });
        }
    }
}
