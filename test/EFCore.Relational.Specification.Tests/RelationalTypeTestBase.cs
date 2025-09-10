// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public abstract class RelationalTypeTestBase<T, TFixture>(TFixture fixture) : TypeTestBase<T, TFixture>(fixture)
    where TFixture : RelationalTypeTestBase<T, TFixture>.RelationalTypeTestFixture
    where T : notnull
{
    public RelationalTypeTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : this(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    #region SaveChanges

    [ConditionalFact]
    public virtual async Task SaveChanges_within_json()
        => await TestHelpers.ExecuteWithStrategyInTransactionAsync(
            Fixture.CreateContext,
            Fixture.UseTransaction,
            async context =>
            {
                JsonTypeEntity entity;

                using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
                {
                    entity = await context.Set<JsonTypeEntity>().SingleAsync(e => e.Id == 1);
                }

                entity.JsonContainer.Value = Fixture.OtherValue;
                await context.SaveChangesAsync();

                using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
                {
                    var result = await context.Set<JsonTypeEntity>().Where(e => e.Id == 1).SingleAsync();
                    Assert.Equal(Fixture.OtherValue, result.JsonContainer.Value, Fixture.Comparer);
                }
            });

    #endregion SaveChanges

    #region ExecuteUpdate

    [ConditionalFact]
    public virtual async Task ExecuteUpdate_within_json_to_parameter()
        => await TestHelpers.ExecuteWithStrategyInTransactionAsync(
            Fixture.CreateContext,
            Fixture.UseTransaction,
            async context =>
            {
                await context.Set<JsonTypeEntity>().ExecuteUpdateAsync(s => s.SetProperty(e => e.JsonContainer.Value, e => Fixture.OtherValue));

                using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
                {
                    var result = await context.Set<JsonTypeEntity>().Where(e => e.Id == 1).SingleAsync();
                    Assert.Equal(Fixture.OtherValue, result.JsonContainer.Value, Fixture.Comparer);
                }
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

                using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
                {
                    var result = await context.Set<JsonTypeEntity>().Where(e => e.Id == 1).SingleAsync();
                    Assert.Equal(Fixture.OtherValue, result.JsonContainer.Value, Fixture.Comparer);
                }
            });

    [ConditionalFact]
    public virtual async Task ExecuteUpdate_within_json_to_another_json_property()
        => await TestHelpers.ExecuteWithStrategyInTransactionAsync(
            Fixture.CreateContext,
            Fixture.UseTransaction,
            async context =>
            {
                await context.Set<JsonTypeEntity>().ExecuteUpdateAsync(s => s.SetProperty(e => e.JsonContainer.Value, e => e.JsonContainer.OtherValue));

                using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
                {
                    var result = await context.Set<JsonTypeEntity>().Where(e => e.Id == 1).SingleAsync();
                    Assert.Equal(Fixture.OtherValue, result.JsonContainer.Value, Fixture.Comparer);
                }
            });

    [ConditionalFact]
    public virtual async Task ExecuteUpdate_within_json_to_nonjson_column()
        => await TestHelpers.ExecuteWithStrategyInTransactionAsync(
            Fixture.CreateContext,
            Fixture.UseTransaction,
            async context =>
            {
                await context.Set<JsonTypeEntity>().ExecuteUpdateAsync(s => s.SetProperty(e => e.JsonContainer.Value, e => e.OtherValue));

                using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
                {
                    var result = await context.Set<JsonTypeEntity>().Where(e => e.Id == 1).SingleAsync();
                    Assert.Equal(Fixture.OtherValue, result.JsonContainer.Value, Fixture.Comparer);
                }
            });

    #endregion ExecuteUpdate

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

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected void AssertExecuteUpdateSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected, forUpdate: true);

    public abstract class RelationalTypeTestFixture : TypeTestFixture, ITestSqlLoggerFactory
    {
        public virtual string? StoreType => null;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<TypeEntity>(b =>
            {
                b.Property(e => e.Value).HasColumnType(StoreType);
                b.Property(e => e.OtherValue).HasColumnType(StoreType);
            });

            modelBuilder.Entity<JsonTypeEntity>(b =>
            {
                modelBuilder.Entity<JsonTypeEntity>().Property(e => e.Id).ValueGeneratedNever();

                b.ComplexProperty(e => e.JsonContainer, jc =>
                {
                    jc.ToJson();

                    jc.Property(e => e.Value).HasColumnType(StoreType);
                    jc.Property(e => e.OtherValue).HasColumnType(StoreType);
                });
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

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        public virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());
    }
}
