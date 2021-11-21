// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore;

public class PropertyValuesSqlServerTest : PropertyValuesTestBase<PropertyValuesSqlServerTest.PropertyValuesSqlServerFixture>
{
    public PropertyValuesSqlServerTest(PropertyValuesSqlServerFixture fixture)
        : base(fixture)
    {
    }

    public class PropertyValuesSqlServerFixture : PropertyValuesFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<Building>()
                .Property(b => b.Value).HasColumnType("decimal(18,2)");

            modelBuilder.Entity<CurrentEmployee>()
                .Property(ce => ce.LeaveBalance).HasColumnType("decimal(18,2)");
        }
    }
}
