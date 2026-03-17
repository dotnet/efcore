// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Types;

public abstract class RelationalTypeTestBase<T, TFixture> : TypeTestBase<T, TFixture>
    where TFixture : RelationalTypeFixtureBase<T>
    where T : notnull
{
    public RelationalTypeTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public override async Task SaveChanges()
    {
        await using var context = Fixture.CreateContext();

        var entity = await context.Set<TypeEntity<T>>().SingleAsync(e => e.Id == 1);
        entity.Value = Fixture.OtherValue;

        Fixture.TestSqlLoggerFactory.Clear();

        await context.SaveChangesAsync();

        using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            var result = await context.Set<TypeEntity<T>>().Where(e => e.Id == 1).SingleAsync();
            Assert.Equal(Fixture.OtherValue, result.Value, Fixture.Comparer);

            // Revert back to the original value to avoid affecting other tests (note that test parallelization is disabled)
            entity.Value = Fixture.Value;
            await context.SaveChangesAsync();
        }
    }

    #region JSON

    [ConditionalFact]
    public virtual async Task Query_property_within_json()
    {
        await using var context = Fixture.CreateContext();

        Fixture.TestSqlLoggerFactory.Clear();

        var result = await context.Set<JsonTypeEntity<T>>().Where(e => e.JsonContainer.Value.Equals(Fixture.Value)).SingleAsync();

        Assert.Equal(Fixture.Value, result.JsonContainer.Value, Fixture.Comparer);
    }

    [ConditionalFact]
    public virtual async Task SaveChanges_within_json()
        => await TestHelpers.ExecuteWithStrategyInTransactionAsync(
            Fixture.CreateContext,
            Fixture.UseTransaction,
            async context =>
            {
                JsonTypeEntity<T> entity;

                entity = await context.Set<JsonTypeEntity<T>>().SingleAsync(e => e.Id == 1);

                Fixture.TestSqlLoggerFactory.Clear();

                entity.JsonContainer.Value = Fixture.OtherValue;
                await context.SaveChangesAsync();

                using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
                {
                    var result = await context.Set<JsonTypeEntity<T>>().Where(e => e.Id == 1).SingleAsync();
                    Assert.Equal(Fixture.OtherValue, result.JsonContainer.Value, Fixture.Comparer);
                }
            });

    [ConditionalFact]
    public virtual async Task ExecuteUpdate_within_json_to_parameter()
        => await TestHelpers.ExecuteWithStrategyInTransactionAsync(
            Fixture.CreateContext,
            Fixture.UseTransaction,
            async context =>
            {
                Fixture.TestSqlLoggerFactory.Clear();

                await context.Set<JsonTypeEntity<T>>().ExecuteUpdateAsync(s => s.SetProperty(e => e.JsonContainer.Value, e => Fixture.OtherValue));

                using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
                {
                    var result = await context.Set<JsonTypeEntity<T>>().Where(e => e.Id == 1).SingleAsync();
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
                Fixture.TestSqlLoggerFactory.Clear();

                // Manually inject a constant node into the query tree
                var parameter = Expression.Parameter(typeof(JsonTypeEntity<T>));
                var valueExpression = Expression.Lambda<Func<JsonTypeEntity<T>, T>>(
                    Expression.Constant(Fixture.OtherValue, typeof(T)),
                    parameter);

                await context.Set<JsonTypeEntity<T>>().ExecuteUpdateAsync(s => s.SetProperty(e => e.JsonContainer.Value, valueExpression));

                using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
                {
                    var result = await context.Set<JsonTypeEntity<T>>().Where(e => e.Id == 1).SingleAsync();
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
                Fixture.TestSqlLoggerFactory.Clear();

                await context.Set<JsonTypeEntity<T>>().ExecuteUpdateAsync(s => s.SetProperty(e => e.JsonContainer.Value, e => e.JsonContainer.OtherValue));

                using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
                {
                    var result = await context.Set<JsonTypeEntity<T>>().Where(e => e.Id == 1).SingleAsync();
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
                Fixture.TestSqlLoggerFactory.Clear();

                await context.Set<JsonTypeEntity<T>>().ExecuteUpdateAsync(s => s.SetProperty(e => e.JsonContainer.Value, e => e.OtherValue));

                using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
                {
                    var result = await context.Set<JsonTypeEntity<T>>().Where(e => e.Id == 1).SingleAsync();
                    Assert.Equal(Fixture.OtherValue, result.JsonContainer.Value, Fixture.Comparer);
                }
            });

    #endregion JSON

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected void AssertExecuteUpdateSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected, forUpdate: true);
}
