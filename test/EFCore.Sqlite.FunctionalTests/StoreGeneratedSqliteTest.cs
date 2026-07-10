// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class StoreGeneratedSqliteTest(StoreGeneratedSqliteTest.StoreGeneratedSqliteFixture fixture)
    : StoreGeneratedTestBase<StoreGeneratedSqliteTest.StoreGeneratedSqliteFixture>(fixture)
{
    public override Task Fields_used_correctly_for_store_generated_values()
        // Computed columns not supported
        => Task.CompletedTask;

    [ConditionalFact]
    public Task Identity_key_works_when_not_aliasing_rowid()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entry = context.Add(new Zach());

                await context.SaveChangesAsync();
                var id = entry.Entity.Id;

                Assert.Equal(16, id?.Length ?? 0);
            });

    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    public class StoreGeneratedSqliteFixture : StoreGeneratedFixtureBase
    {
        protected override string StoreName
            => "StoreGeneratedTest";

        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => builder
                .EnableSensitiveDataLogging()
                .ConfigureWarnings(
                    b => b.Default(WarningBehavior.Throw)
                        .Ignore(CoreEventId.SensitiveDataLoggingEnabledWarning)
                        .Ignore(RelationalEventId.BoolWithDefaultWarning));

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<Gumball>(
                b =>
                {
                    b.Property(e => e.Identity).HasDefaultValue("Banana Joe");
                    b.Property(e => e.IdentityReadOnlyBeforeSave).HasDefaultValue("Doughnut Sheriff");
                    b.Property(e => e.IdentityReadOnlyAfterSave).HasDefaultValue("Anton");
                    b.Property(e => e.AlwaysIdentity).HasDefaultValue("Banana Joe");
                    b.Property(e => e.AlwaysIdentityReadOnlyBeforeSave).HasDefaultValue("Doughnut Sheriff");
                    b.Property(e => e.AlwaysIdentityReadOnlyAfterSave).HasDefaultValue("Anton");
                    b.Property(e => e.Computed).HasDefaultValue("Alan");
                    b.Property(e => e.ComputedReadOnlyBeforeSave).HasDefaultValue("Carmen");
                    b.Property(e => e.ComputedReadOnlyAfterSave).HasDefaultValue("Tina Rex");
                    b.Property(e => e.AlwaysComputed).HasDefaultValue("Alan");
                    b.Property(e => e.AlwaysComputedReadOnlyBeforeSave).HasDefaultValue("Carmen");
                    b.Property(e => e.AlwaysComputedReadOnlyAfterSave).HasDefaultValue("Tina Rex");
                });

            modelBuilder.Entity<Anais>(
                b =>
                {
                    b.Property(e => e.OnAdd).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddUseBeforeUseAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddIgnoreBeforeUseAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddThrowBeforeUseAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddUseBeforeIgnoreAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddIgnoreBeforeIgnoreAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddThrowBeforeIgnoreAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddUseBeforeThrowAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddIgnoreBeforeThrowAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddThrowBeforeThrowAfter).HasDefaultValue("Rabbit");

                    b.Property(e => e.OnAddOrUpdate).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddOrUpdateUseBeforeUseAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddOrUpdateIgnoreBeforeUseAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddOrUpdateThrowBeforeUseAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddOrUpdateUseBeforeIgnoreAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddOrUpdateIgnoreBeforeIgnoreAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddOrUpdateThrowBeforeIgnoreAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddOrUpdateUseBeforeThrowAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddOrUpdateIgnoreBeforeThrowAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnAddOrUpdateThrowBeforeThrowAfter).HasDefaultValue("Rabbit");

                    b.Property(e => e.OnUpdate).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnUpdateUseBeforeUseAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnUpdateIgnoreBeforeUseAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnUpdateThrowBeforeUseAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnUpdateUseBeforeIgnoreAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnUpdateIgnoreBeforeIgnoreAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnUpdateThrowBeforeIgnoreAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnUpdateUseBeforeThrowAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnUpdateIgnoreBeforeThrowAfter).HasDefaultValue("Rabbit");
                    b.Property(e => e.OnUpdateThrowBeforeThrowAfter).HasDefaultValue("Rabbit");
                });

            modelBuilder.Entity<WithNoBackingFields>(
                b =>
                {
                    b.Property(e => e.TrueDefault).HasDefaultValue(true);
                    b.Property(e => e.NonZeroDefault).HasDefaultValue(-1);
                    b.Property(e => e.FalseDefault).HasDefaultValue(false);
                    b.Property(e => e.ZeroDefault).HasDefaultValue(0);
                });

            modelBuilder.Entity<WithNullableBackingFields>(
                b =>
                {
                    b.Property(e => e.NullableBackedBoolTrueDefault).HasDefaultValue(true);
                    b.Property(e => e.NullableBackedIntNonZeroDefault).HasDefaultValue(-1);
                    b.Property(e => e.NullableBackedBoolFalseDefault).HasDefaultValue(false);
                    b.Property(e => e.NullableBackedIntZeroDefault).HasDefaultValue(0);
                });

            modelBuilder.Entity<WithObjectBackingFields>(
                b =>
                {
                    b.Property(e => e.NullableBackedBoolTrueDefault).HasDefaultValue(true);
                    b.Property(e => e.NullableBackedIntNonZeroDefault).HasDefaultValue(-1);
                    b.Property(e => e.NullableBackedBoolFalseDefault).HasDefaultValue(false);
                    b.Property(e => e.NullableBackedIntZeroDefault).HasDefaultValue(0);
                });

            modelBuilder.Entity<Zach>().Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("randomblob(16)");

            modelBuilder.Entity<NonStoreGenDependent>().Property(e => e.HasTemp).HasDefaultValue(777);

            base.OnModelCreating(modelBuilder, context);
        }
    }

    private class Zach
    {
        public byte[] Id { get; set; }
    }
}
