// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class PropertyValuesSqlServerTest : PropertyValuesTestBase<PropertyValuesSqlServerTest.PropertyValuesSqlServerFixture>
    {
        public PropertyValuesSqlServerTest(PropertyValuesSqlServerFixture fixture)
            : base(fixture)
        {
        }

        public class PropertyValuesSqlServerFixture : PropertyValuesFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;

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
}
