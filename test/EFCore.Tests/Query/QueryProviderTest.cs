﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class QueryProviderTest
{
    [ConditionalFact]
    public async Task ExecuteUpdate_and_ExecuteDelete_throw_when_provider_does_not_implement()
    {
        using var context = new TestContext();
        var set = context.TestEntities;

        Assert.Equal(
            CoreStrings.ExecuteQueriesNotSupported("ExecuteUpdate", "ExecuteUpdateAsync"),
            Assert.Throws<InvalidOperationException>(() => set.ExecuteUpdate(s => s.SetProperty(e => e.Id, 1))).Message);

        Assert.Equal(
            CoreStrings.ExecuteQueriesNotSupported("ExecuteUpdate", "ExecuteUpdateAsync"),
            (await Assert.ThrowsAsync<InvalidOperationException>(() => set.ExecuteUpdateAsync(s => s.SetProperty(e => e.Id, 1)))).Message);

        Assert.Equal(
            CoreStrings.ExecuteQueriesNotSupported("ExecuteDelete", "ExecuteDeleteAsync"),
            Assert.Throws<InvalidOperationException>(() => set.ExecuteDelete()).Message);

        Assert.Equal(
            CoreStrings.ExecuteQueriesNotSupported("ExecuteDelete", "ExecuteDeleteAsync"),
            (await Assert.ThrowsAsync<InvalidOperationException>(() => set.ExecuteDeleteAsync())).Message);
    }

    [ConditionalFact]
    public void Non_generic_ExecuteQuery_does_not_throw()
    {
        var context = new TestContext();
        Func<IQueryable<TestEntity>, int> func = Queryable.Count;
        IQueryable q = context.TestEntities;
        var expr = Expression.Call(null, func.GetMethodInfo(), q.Expression);
        Assert.Equal(0, q.Provider.Execute<int>(expr));
        Assert.Equal(0, (int)q.Provider.Execute(expr));
    }

    #region Fixture

    private class TestEntity
    {
        public int Id { get; set; }
    }

    private class TestContext : DbContext
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<TestEntity> TestEntities { get; set; }

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase(Guid.NewGuid().ToString());
    }

    #endregion
}
