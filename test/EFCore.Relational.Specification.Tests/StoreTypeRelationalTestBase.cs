// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public abstract class StoreTypeRelationalTestBase : StoreTypeTestBase
{
    protected override async Task TestType<T>(
        T value,
        T otherValue,
        ContextFactory<DbContext> contextFactory,
        Func<T, T, bool> comparer)
    {
        await base.TestType<T>(value, otherValue, contextFactory, comparer);

        // Extra test scenarios for relational
        await TestExecuteUpdateWithinJsonToParameter(contextFactory, value, otherValue, comparer);
        await TestExecuteUpdateWithinJsonToConstant(contextFactory, value, otherValue, comparer);
        await TestExecuteUpdateWithinJsonToJsonProperty(contextFactory, value, otherValue, comparer);
        await TestExecuteUpdateWithinJsonToNonJsonColumn(contextFactory, value, otherValue, comparer);
    }

    protected virtual async Task TestExecuteUpdateWithinJsonToParameter<T>(
        ContextFactory<DbContext> contextFactory,
        T value,
        T otherValue,
        Func<T, T, bool> comparer)
        => await TestHelpers.ExecuteWithStrategyInTransactionAsync(
            contextFactory.CreateContext,
            UseTransaction,
            async context =>
            {
                await context.Set<StoreTypeEntity<T>>().ExecuteUpdateAsync(s => s.SetProperty(e => e.Container.Value, e => otherValue));
                var result = await context.Set<StoreTypeEntity<T>>().Where(e => e.Id == 1).SingleAsync();
                Assert.Equal(otherValue, result.Container.Value, comparer);
            });

    protected virtual async Task TestExecuteUpdateWithinJsonToConstant<T>(
        ContextFactory<DbContext> contextFactory,
        T value,
        T otherValue,
        Func<T, T, bool> comparer)
        => await TestHelpers.ExecuteWithStrategyInTransactionAsync(
            contextFactory.CreateContext,
            UseTransaction,
            async context =>
            {
                var parameter = Expression.Parameter(typeof(StoreTypeEntity<T>));
                var valueExpression = Expression.Lambda<Func<StoreTypeEntity<T>, T>>(
                    Expression.Constant(otherValue, typeof(T)),
                    parameter);

                await context.Set<StoreTypeEntity<T>>().ExecuteUpdateAsync(s => s.SetProperty(e => e.Container.Value, valueExpression));
                var result = await context.Set<StoreTypeEntity<T>>().Where(e => e.Id == 1).SingleAsync();
                Assert.Equal(otherValue, result.Container.Value, comparer);
            });

    protected virtual async Task TestExecuteUpdateWithinJsonToJsonProperty<T>(
        ContextFactory<DbContext> contextFactory,
        T value,
        T otherValue,
        Func<T, T, bool> comparer)
        => await TestHelpers.ExecuteWithStrategyInTransactionAsync(
            contextFactory.CreateContext,
            UseTransaction,
            async context =>
            {
                await context.Set<StoreTypeEntity<T>>().ExecuteUpdateAsync(s => s.SetProperty(e => e.Container.Value, e => e.Container.OtherValue));
                var result = await context.Set<StoreTypeEntity<T>>().Where(e => e.Id == 1).SingleAsync();
                Assert.Equal(otherValue, result.Container.Value, comparer);
            });

    protected virtual async Task TestExecuteUpdateWithinJsonToNonJsonColumn<T>(
        ContextFactory<DbContext> contextFactory,
        T value,
        T otherValue,
        Func<T, T, bool> comparer)
        => await TestHelpers.ExecuteWithStrategyInTransactionAsync(
            contextFactory.CreateContext,
            UseTransaction,
            async context =>
            {
                await context.Set<StoreTypeEntity<T>>().ExecuteUpdateAsync(s => s.SetProperty(e => e.Container.Value, e => e.OtherValue));
                var result = await context.Set<StoreTypeEntity<T>>().Where(e => e.Id == 1).SingleAsync();
                Assert.Equal(otherValue, result.Container.Value, comparer);
            });

    public override void OnModelCreating<T>(ModelBuilder modelBuilder)
    {
        base.OnModelCreating<T>(modelBuilder);

        modelBuilder.Entity<StoreTypeEntity<T>>(b =>
        {
            b.ToTable("StoreTypeEntity");
            b.ComplexProperty(e => e.Container, cb => cb.ToJson());
        });
    }

    public override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());
}
