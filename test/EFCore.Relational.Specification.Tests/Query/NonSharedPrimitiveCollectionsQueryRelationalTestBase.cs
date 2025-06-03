// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class NonSharedPrimitiveCollectionsQueryRelationalTestBase(NonSharedFixture fixture) : NonSharedPrimitiveCollectionsQueryTestBase(fixture)
{
    // On relational databases, byte[] gets mapped to a special binary data type, which isn't queryable as a regular primitive collection.
    [ConditionalFact]
    public override Task Array_of_byte()
        => AssertTranslationFailed(() => TestArray((byte)1, (byte)2));

    protected abstract DbContextOptionsBuilder SetTranslateParameterizedCollectionsToConstants(DbContextOptionsBuilder optionsBuilder);

    protected abstract DbContextOptionsBuilder SetTranslateParameterizedCollectionsToParameters(DbContextOptionsBuilder optionsBuilder);

    [ConditionalFact]
    public virtual async Task Column_collection_inside_json_owned_entity()
    {
        var contextFactory = await InitializeAsync<TestContext>(
            onModelCreating: mb => mb.Entity<TestOwner>().OwnsOne(t => t.Owned, b => b.ToJson()),
            seed: context =>
            {
                context.AddRange(
                    new TestOwner { Owned = new TestOwned { Strings = ["foo", "bar"] } },
                    new TestOwner { Owned = new TestOwned { Strings = ["baz"] } });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateContext();

        var result = await context.Set<TestOwner>().SingleAsync(o => o.Owned.Strings.Count() == 2);
        Assert.Equivalent(new[] { "foo", "bar" }, result.Owned.Strings);

        result = await context.Set<TestOwner>().SingleAsync(o => o.Owned.Strings[1] == "bar");
        Assert.Equivalent(new[] { "foo", "bar" }, result.Owned.Strings);
    }

    [ConditionalFact]
    public virtual async Task Parameter_collection_Count_with_column_predicate_with_default_constants()
    {
        var contextFactory = await InitializeAsync<TestContext>(
            onConfiguring: b => SetTranslateParameterizedCollectionsToConstants(b),
            seed: context =>
            {
                context.AddRange(
                    new TestEntity { Id = 1 },
                    new TestEntity { Id = 100 });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateContext();

        var ids = new[] { 2, 999 };
        var result = await context.Set<TestEntity>().Where(c => ids.Count(i => i > c.Id) == 1).Select(x => x.Id).ToListAsync();
        Assert.Equivalent(new[] { 100 }, result);
    }

    [ConditionalFact]
    public virtual async Task Parameter_collection_of_ints_Contains_int_with_default_constants()
    {
        var contextFactory = await InitializeAsync<TestContext>(
            onConfiguring: b => SetTranslateParameterizedCollectionsToConstants(b),
            seed: context =>
            {
                context.AddRange(
                    new TestEntity { Id = 1 },
                    new TestEntity { Id = 2 },
                    new TestEntity { Id = 100 });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateContext();

        var ints = new[] { 2, 999 };
        var result = await context.Set<TestEntity>().Where(c => ints.Contains(c.Id)).Select(x => x.Id).ToListAsync();
        Assert.Equivalent(new[] { 2 }, result);
    }

    [ConditionalFact]
    public virtual async Task Parameter_collection_Count_with_column_predicate_with_default_constants_EF_Parameter()
    {
        var contextFactory = await InitializeAsync<TestContext>(
            onConfiguring: b => SetTranslateParameterizedCollectionsToConstants(b),
            seed: context =>
            {
                context.AddRange(
                    new TestEntity { Id = 1 },
                    new TestEntity { Id = 100 });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateContext();

        var ids = new[] { 2, 999 };
        var result = await context.Set<TestEntity>().Where(c => EF.Parameter(ids).Count(i => i > c.Id) == 1).Select(x => x.Id)
            .ToListAsync();
        Assert.Equivalent(new[] { 100 }, result);
    }

    [ConditionalFact]
    public virtual async Task Parameter_collection_of_ints_Contains_int_with_default_constants_EF_Parameter()
    {
        var contextFactory = await InitializeAsync<TestContext>(
            onConfiguring: b => SetTranslateParameterizedCollectionsToConstants(b),
            seed: context =>
            {
                context.AddRange(
                    new TestEntity { Id = 1 },
                    new TestEntity { Id = 2 },
                    new TestEntity { Id = 100 });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateContext();

        var ints = new[] { 2, 999 };
        var result = await context.Set<TestEntity>().Where(c => EF.Parameter(ints).Contains(c.Id)).Select(x => x.Id).ToListAsync();
        Assert.Equivalent(new[] { 2 }, result);
    }

    [ConditionalFact]
    public virtual async Task Parameter_collection_Count_with_column_predicate_with_default_parameters()
    {
        var contextFactory = await InitializeAsync<TestContext>(
            onConfiguring: b => SetTranslateParameterizedCollectionsToParameters(b),
            seed: context =>
            {
                context.AddRange(
                    new TestEntity { Id = 1 },
                    new TestEntity { Id = 100 });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateContext();

        var ids = new[] { 2, 999 };
        var result = await context.Set<TestEntity>().Where(c => ids.Count(i => i > c.Id) == 1).Select(x => x.Id).ToListAsync();
        Assert.Equivalent(new[] { 100 }, result);
    }

    [ConditionalFact]
    public virtual async Task Parameter_collection_of_ints_Contains_int_with_default_parameters()
    {
        var contextFactory = await InitializeAsync<TestContext>(
            onConfiguring: b => SetTranslateParameterizedCollectionsToParameters(b),
            seed: context =>
            {
                context.AddRange(
                    new TestEntity { Id = 1 },
                    new TestEntity { Id = 2 },
                    new TestEntity { Id = 100 });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateContext();

        var ints = new[] { 2, 999 };
        var result = await context.Set<TestEntity>().Where(c => ints.Contains(c.Id)).Select(x => x.Id).ToListAsync();
        Assert.Equivalent(new[] { 2 }, result);
    }

    [ConditionalFact]
    public virtual async Task Parameter_collection_Count_with_column_predicate_with_default_parameters_EF_Constant()
    {
        var contextFactory = await InitializeAsync<TestContext>(
            onConfiguring: b => SetTranslateParameterizedCollectionsToParameters(b),
            seed: context =>
            {
                context.AddRange(
                    new TestEntity { Id = 1 },
                    new TestEntity { Id = 100 });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateContext();

        var ids = new[] { 2, 999 };
        var result = await context.Set<TestEntity>().Where(c => EF.Constant(ids).Count(i => i > c.Id) == 1).Select(x => x.Id).ToListAsync();
        Assert.Equivalent(new[] { 100 }, result);
    }

    [ConditionalFact]
    public virtual async Task Parameter_collection_of_ints_Contains_int_with_default_parameters_EF_Constant()
    {
        var contextFactory = await InitializeAsync<TestContext>(
            onConfiguring: b => SetTranslateParameterizedCollectionsToParameters(b),
            seed: context =>
            {
                context.AddRange(
                    new TestEntity { Id = 1 },
                    new TestEntity { Id = 2 },
                    new TestEntity { Id = 100 });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateContext();

        var ints = new[] { 2, 999 };
        var result = await context.Set<TestEntity>().Where(c => EF.Constant(ints).Contains(c.Id)).Select(x => x.Id).ToListAsync();
        Assert.Equivalent(new[] { 2 }, result);
    }

    protected class TestOwner
    {
        public int Id { get; set; }
        public TestOwned Owned { get; set; }
    }

    [Owned]
    protected class TestOwned
    {
        public string[] Strings { get; set; }
    }

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected void ClearLog()
        => TestSqlLoggerFactory.Clear();

    protected void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);
}
