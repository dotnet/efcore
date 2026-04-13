// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class NonSharedPrimitiveCollectionsQueryRelationalTestBase(NonSharedFixture fixture)
    : NonSharedPrimitiveCollectionsQueryTestBase(fixture)
{
    // On relational databases, byte[] gets mapped to a special binary data type, which isn't queryable as a regular primitive collection.
    [ConditionalFact]
    public override Task Array_of_byte()
        => AssertTranslationFailed(() => TestArray((byte)1, (byte)2));

    protected abstract DbContextOptionsBuilder SetParameterizedCollectionMode(
        DbContextOptionsBuilder optionsBuilder,
        ParameterTranslationMode parameterizedCollectionMode);

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

    protected static IEnumerable<object[]> ParameterTranslationModeValues()
        => Enum.GetValues<ParameterTranslationMode>().Select<ParameterTranslationMode, object[]>(x => [x]);

    [ConditionalTheory, MemberData(nameof(ParameterTranslationModeValues))]
    public virtual async Task Parameter_collection_Count_with_column_predicate_with_default_mode(ParameterTranslationMode mode)
    {
        var contextFactory = await InitializeAsync<TestContext>(
            onConfiguring: b => SetParameterizedCollectionMode(b, mode),
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

    [ConditionalTheory, MemberData(nameof(ParameterTranslationModeValues))]
    public virtual async Task Parameter_collection_Contains_with_default_mode(ParameterTranslationMode mode)
    {
        var contextFactory = await InitializeAsync<TestContext>(
            onConfiguring: b => SetParameterizedCollectionMode(b, mode),
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

    [ConditionalTheory, MemberData(nameof(ParameterTranslationModeValues))]
    public virtual async Task Parameter_collection_Count_with_column_predicate_with_default_mode_EF_Constant(ParameterTranslationMode mode)
    {
        var contextFactory = await InitializeAsync<TestContext>(
            onConfiguring: b => SetParameterizedCollectionMode(b, mode),
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

    [ConditionalTheory, MemberData(nameof(ParameterTranslationModeValues))]
    public virtual async Task Parameter_collection_Contains_with_default_mode_EF_Constant(ParameterTranslationMode mode)
    {
        var contextFactory = await InitializeAsync<TestContext>(
            onConfiguring: b => SetParameterizedCollectionMode(b, mode),
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

    [ConditionalTheory, MemberData(nameof(ParameterTranslationModeValues))]
    public virtual async Task Parameter_collection_Count_with_column_predicate_with_default_mode_EF_Parameter(ParameterTranslationMode mode)
    {
        var contextFactory = await InitializeAsync<TestContext>(
            onConfiguring: b => SetParameterizedCollectionMode(b, mode),
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

    [ConditionalTheory, MemberData(nameof(ParameterTranslationModeValues))]
    public virtual async Task Parameter_collection_Contains_with_default_mode_EF_Parameter(ParameterTranslationMode mode)
    {
        var contextFactory = await InitializeAsync<TestContext>(
            onConfiguring: b => SetParameterizedCollectionMode(b, mode),
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

    [ConditionalTheory, MemberData(nameof(ParameterTranslationModeValues))]
    public virtual async Task Parameter_collection_Count_with_column_predicate_with_default_mode_EF_MultipleParameters(
        ParameterTranslationMode mode)
    {
        var contextFactory = await InitializeAsync<TestContext>(
            onConfiguring: b => SetParameterizedCollectionMode(b, mode),
            seed: context =>
            {
                context.AddRange(
                    new TestEntity { Id = 1 },
                    new TestEntity { Id = 100 });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateContext();

        var ids = new[] { 2, 999 };
        var result = await context.Set<TestEntity>().Where(c => EF.MultipleParameters(ids).Count(i => i > c.Id) == 1).Select(x => x.Id)
            .ToListAsync();
        Assert.Equivalent(new[] { 100 }, result);
    }

    [ConditionalTheory, MemberData(nameof(ParameterTranslationModeValues))]
    public virtual async Task Parameter_collection_Contains_with_default_mode_EF_MultipleParameters(ParameterTranslationMode mode)
    {
        var contextFactory = await InitializeAsync<TestContext>(
            onConfiguring: b => SetParameterizedCollectionMode(b, mode),
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
        var result = await context.Set<TestEntity>().Where(c => EF.MultipleParameters(ints).Contains(c.Id)).Select(x => x.Id).ToListAsync();
        Assert.Equivalent(new[] { 2 }, result);
    }

    [ConditionalFact]
    public virtual async Task Parameter_collection_Contains_parameter_bucketization()
    {
        var contextFactory = await InitializeAsync<TestContext>(
            onConfiguring: b => SetParameterizedCollectionMode(b, ParameterTranslationMode.MultipleParameters),
            seed: context =>
            {
                context.AddRange(
                    new TestEntity { Id = 1 },
                    new TestEntity { Id = 2 },
                    new TestEntity { Id = 100 });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateContext();

        var ints = new[] { 2, 999, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
        var result = await context.Set<TestEntity>().Where(c => ints.Contains(c.Id)).Select(c => c.Id).ToListAsync();
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

    // #38008
    [ConditionalTheory, MemberData(nameof(ParameterTranslationModeValues))]
    public virtual async Task Parameter_collection_of_enum_Cast_from_different_enum_type(ParameterTranslationMode mode)
    {
        var contextFactory = await InitializeAsync<Context38008>(
            onConfiguring: b => SetParameterizedCollectionMode(b, mode),
            seed: context =>
            {
                context.AddRange(
                    new Context38008.TestEntity38008 { Id = 1, Status = Context38008.EntityEnum.Clean },
                    new Context38008.TestEntity38008 { Id = 2, Status = Context38008.EntityEnum.Malware });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateContext();

        // Cast<EntityEnum>() returns a lazy IEnumerable whose boxed values retain the ViewModelEnum runtime type.
        var filter = new[] { Context38008.ViewModelEnum.Malware }.Cast<Context38008.EntityEnum>();
        var result = await context.Set<Context38008.TestEntity38008>()
            .Where(a => filter.Any(f => f == a.Status))
            .Select(a => a.Id)
            .ToListAsync();

        Assert.Equivalent(new[] { 2 }, result);
    }

    protected class Context38008(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<TestEntity38008>().Property(e => e.Id).ValueGeneratedNever();

        public class TestEntity38008
        {
            public int Id { get; set; }
            public EntityEnum Status { get; set; }
        }

        [Flags]
        public enum EntityEnum
        {
            Clean = 1,
            Malware = 2
        }

        [Flags]
        public enum ViewModelEnum
        {
            Clean = 1,
            Malware = 2
        }
    }
}
