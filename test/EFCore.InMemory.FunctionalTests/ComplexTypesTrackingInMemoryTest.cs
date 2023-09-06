// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class ComplexTypesTrackingInMemoryTest : ComplexTypesTrackingTestBase<ComplexTypesTrackingInMemoryTest.InMemoryFixture>
{
    public ComplexTypesTrackingInMemoryTest(InMemoryFixture fixture)
        : base(fixture)
    {
    }

    protected override void ExecuteWithStrategyInTransaction(
        Action<DbContext> testOperation,
        Action<DbContext> nestedTestOperation1 = null,
        Action<DbContext> nestedTestOperation2 = null)
    {
        try
        {
            base.ExecuteWithStrategyInTransaction(testOperation, nestedTestOperation1, nestedTestOperation2);
        }
        finally
        {
            Fixture.Reseed();
        }
    }

    protected override async Task ExecuteWithStrategyInTransactionAsync(
        Func<DbContext, Task> testOperation,
        Func<DbContext, Task> nestedTestOperation1 = null,
        Func<DbContext, Task> nestedTestOperation2 = null)
    {
        try
        {
            await base.ExecuteWithStrategyInTransactionAsync(testOperation, nestedTestOperation1, nestedTestOperation2);
        }
        finally
        {
            Fixture.Reseed();
        }
    }

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
