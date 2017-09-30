// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class MappingQueryOracleTest : MappingQueryTestBase<MappingQueryOracleTest.MappingQueryOracleFixture>
    {
        public MappingQueryOracleTest(MappingQueryOracleFixture fixture)
            : base(fixture)
        {
        }

        public class MappingQueryOracleFixture : MappingQueryFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => OracleNorthwindTestStoreFactory.Instance;

            protected override string DatabaseSchema { get; } = null;

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                modelBuilder.Entity<MappedCustomer>(
                    e =>
                        {
                            e.Property(c => c.CompanyName2).Metadata.Oracle().ColumnName = "CompanyName";
                            e.Metadata.Relational().TableName = "Customers";
                        });
            }
        }
    }
}
