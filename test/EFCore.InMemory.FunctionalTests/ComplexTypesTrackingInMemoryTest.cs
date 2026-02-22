// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class ComplexTypesTrackingInMemoryTest(ComplexTypesTrackingInMemoryTest.InMemoryFixture fixture)
    : ComplexTypesTrackingTestBase<ComplexTypesTrackingInMemoryTest.InMemoryFixture>(fixture)
{
    protected override async Task ExecuteWithStrategyInTransactionAsync(
        Func<DbContext, Task> testOperation,
        Func<DbContext, Task> nestedTestOperation1 = null,
        Func<DbContext, Task> nestedTestOperation2 = null,
        Func<DbContext, Task> nestedTestOperation3 = null)
    {
        try
        {
            await base.ExecuteWithStrategyInTransactionAsync(testOperation, nestedTestOperation1, nestedTestOperation2, nestedTestOperation3);
        }
        finally
        {
            await Fixture.ReseedAsync();
        }
    }

    public override Task Can_save_default_values_in_optional_complex_property_with_multiple_properties(bool async)
        // InMemory provider has issues with complex type query compilation and materialization
        // See https://github.com/dotnet/efcore/issues/31464
        => Task.CompletedTask;

    public class InMemoryFixture : FixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(w => w.Log(InMemoryEventId.TransactionIgnoredWarning));

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.UsePropertyAccessMode(PropertyAccessMode.PreferProperty);
            base.OnModelCreating(modelBuilder, context);
        }
    }
}
