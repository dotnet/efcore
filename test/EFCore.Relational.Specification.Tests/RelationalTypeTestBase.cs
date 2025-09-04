// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public abstract class RelationalTypeTestBase<T, TFixture>(TFixture fixture) : TypeTestBase<T, TFixture>(fixture)
    where TFixture : RelationalTypeTestBase<T, TFixture>.RelationalTypeTestFixture
    where T : notnull
{
    [ConditionalFact]
    public virtual async Task ExecuteUpdate_within_json_to_parameter()
        => await TestHelpers.ExecuteWithStrategyInTransactionAsync(
            Fixture.CreateContext,
            Fixture.UseTransaction,
            async context =>
            {
                await context.Set<JsonTypeEntity>().ExecuteUpdateAsync(s => s.SetProperty(e => e.JsonContainer.Value, e => Fixture.OtherValue));
                var result = await context.Set<JsonTypeEntity>().Where(e => e.Id == 1).SingleAsync();
                Assert.Equal(Fixture.OtherValue, result.JsonContainer.Value, Fixture.Comparer);
            });

    [ConditionalFact]
    public virtual async Task ExecuteUpdate_within_json_to_constant()
        => await TestHelpers.ExecuteWithStrategyInTransactionAsync(
            Fixture.CreateContext,
            Fixture.UseTransaction,
            async context =>
            {
                // Manually inject a constant node into the query tree
                var parameter = Expression.Parameter(typeof(JsonTypeEntity));
                var valueExpression = Expression.Lambda<Func<JsonTypeEntity, T>>(
                    Expression.Constant(Fixture.OtherValue, typeof(T)),
                    parameter);

                await context.Set<JsonTypeEntity>().ExecuteUpdateAsync(s => s.SetProperty(e => e.JsonContainer.Value, valueExpression));
                var result = await context.Set<JsonTypeEntity>().Where(e => e.Id == 1).SingleAsync();
                Assert.Equal(Fixture.OtherValue, result.JsonContainer.Value, Fixture.Comparer);
            });

    [ConditionalFact]
    public virtual async Task ExecuteUpdate_within_json_to_another_json_property()
        => await TestHelpers.ExecuteWithStrategyInTransactionAsync(
            Fixture.CreateContext,
            Fixture.UseTransaction,
            async context =>
            {
                await context.Set<JsonTypeEntity>().ExecuteUpdateAsync(s => s.SetProperty(e => e.JsonContainer.Value, e => e.JsonContainer.OtherValue));
                var result = await context.Set<JsonTypeEntity>().Where(e => e.Id == 1).SingleAsync();
                Assert.Equal(Fixture.OtherValue, result.JsonContainer.Value, Fixture.Comparer);
            });

    [ConditionalFact]
    public virtual async Task ExecuteUpdate_within_json_to_nonjson_column()
        => await TestHelpers.ExecuteWithStrategyInTransactionAsync(
            Fixture.CreateContext,
            Fixture.UseTransaction,
            async context =>
            {
                await context.Set<JsonTypeEntity>().ExecuteUpdateAsync(s => s.SetProperty(e => e.JsonContainer.Value, e => e.OtherValue));
                var result = await context.Set<JsonTypeEntity>().Where(e => e.Id == 1).SingleAsync();
                Assert.Equal(Fixture.OtherValue, result.JsonContainer.Value, Fixture.Comparer);
            });

    protected class JsonTypeEntity
    {
        public int Id { get; set; }

        public required T Value { get; set; }
        public required T OtherValue { get; set; }

        public required JsonContainer JsonContainer { get; set; }
    }

    public class JsonContainer
    {
        public required T Value { get; set; }
        public required T OtherValue { get; set; }
    }

    public abstract class RelationalTypeTestFixture(T value, T otherValue)
        : TypeTestFixture(value, otherValue)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<JsonTypeEntity>(b =>
            {
                modelBuilder.Entity<JsonTypeEntity>().Property(e => e.Id).ValueGeneratedNever();
                b.ComplexProperty(e => e.JsonContainer, cb => cb.ToJson());
            });
        }

        protected override async Task SeedAsync(DbContext context)
        {
            await base.SeedAsync(context);

            context.Set<JsonTypeEntity>().AddRange(
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

        public virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());
    }
}
