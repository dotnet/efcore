// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

[SqlServerCondition(SqlServerCondition.IsNotAzureSql)]
public class ConvertToProviderTypesSqlServerTest(ConvertToProviderTypesSqlServerTest.ConvertToProviderTypesSqlServerFixture fixture)
    : ConvertToProviderTypesTestBase<
        ConvertToProviderTypesSqlServerTest.ConvertToProviderTypesSqlServerFixture>(fixture)
{
    public override Task Object_to_string_conversion()
        // Return values are not string
        => Task.CompletedTask;

    public class ConvertToProviderTypesSqlServerFixture : ConvertToProviderTypesFixtureBase
    {
        public override bool StrictEquality
            => true;

        public override bool SupportsAnsi
            => true;

        public override bool SupportsUnicodeToAnsiConversion
            => true;

        public override bool SupportsLargeStringComparisons
            => true;

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        public override bool SupportsBinaryKeys
            => true;

        public override bool SupportsDecimalComparisons
            => true;

        public override DateTime DefaultDateTime
            => new();

        public override bool PreservesDateTimeKind
            => false;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base
                .AddOptions(builder)
                .ConfigureWarnings(c => c.Log(SqlServerEventId.DecimalTypeDefaultWarning));

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<BuiltInDataTypes>().Property(e => e.Enum8).IsFixedLength();
        }
    }
}
