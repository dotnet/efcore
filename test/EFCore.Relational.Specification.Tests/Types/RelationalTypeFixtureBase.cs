// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Types;

public abstract class RelationalTypeFixtureBase<T> : TypeFixtureBase<T>, ITestSqlLoggerFactory
{
    public virtual string? StoreType => null;

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<TypeEntity<T>>(b =>
        {
            b.ToTable(nameof(TypeEntity<>));
            b.Property(e => e.Value).HasColumnType(StoreType);
            b.Property(e => e.OtherValue).HasColumnType(StoreType);
        });

        modelBuilder.Entity<JsonTypeEntity<T>>(b =>
        {
            b.ToTable(nameof(JsonTypeEntity<>));

            modelBuilder.Entity<JsonTypeEntity<T>>(b =>
            {
                b.Property(e => e.Id).ValueGeneratedNever();

                b.Property(e => e.Value).HasColumnType(StoreType);
                b.Property(e => e.OtherValue).HasColumnType(StoreType);

                b.ComplexProperty(e => e.JsonContainer, jc =>
                {
                    jc.ToJson();

                    jc.Property(e => e.Value).HasColumnType(StoreType);
                    jc.Property(e => e.OtherValue).HasColumnType(StoreType);
                });
            });
        });
    }

    protected override async Task SeedAsync(DbContext context)
    {
        await base.SeedAsync(context);

        context.Set<JsonTypeEntity<T>>().AddRange(
            new()
            {
                Id = 1,
                Value = Value,
                OtherValue = OtherValue,
                JsonContainer = new()
                {
                    Value = Value,
                    OtherValue = OtherValue
                }
            },
            new()
            {
                Id = 2,
                Value = OtherValue,
                OtherValue = Value,
                JsonContainer = new()
                {
                    Value = OtherValue,
                    OtherValue = Value
                }
            });

        await context.SaveChangesAsync();
    }

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    public virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());
}
