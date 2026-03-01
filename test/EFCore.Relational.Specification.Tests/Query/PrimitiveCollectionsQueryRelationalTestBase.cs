// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class PrimitiveCollectionsQueryRelationalTestBase<TFixture>(TFixture fixture) : PrimitiveCollectionsQueryTestBase<TFixture>(fixture)
    where TFixture : PrimitiveCollectionsQueryTestBase<TFixture>.PrimitiveCollectionsQueryFixtureBase, new()
{
    protected abstract DbContextOptionsBuilder SetParameterizedCollectionMode(
        DbContextOptionsBuilder optionsBuilder,
        ParameterTranslationMode parameterizedCollectionMode);

    [ConditionalFact]
    public override async Task Inline_collection_Count_with_zero_values()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Inline_collection_Count_with_zero_values());

        Assert.Equal(RelationalStrings.EmptyCollectionNotSupportedAsInlineQueryRoot, exception.Message);
    }

    protected static IEnumerable<object[]> ParameterTranslationModeValues()
        => Enum.GetValues<ParameterTranslationMode>().Select<ParameterTranslationMode, object[]>(x => [x]);

    [ConditionalTheory, MemberData(nameof(ParameterTranslationModeValues))]
    public virtual async Task Parameter_collection_Count_with_column_predicate_with_default_mode(ParameterTranslationMode mode)
    {
        var contextFactory = await InitializeNonSharedTest<TestContext>(
            onConfiguring: b => SetParameterizedCollectionMode(b, mode),
            seed: context =>
            {
                context.AddRange(
                    new TestEntity { Id = 1 },
                    new TestEntity { Id = 100 });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var ids = new[] { 2, 999 };
        var result = await context.Set<TestEntity>().Where(c => ids.Count(i => i > c.Id) == 1).Select(x => x.Id).ToListAsync();
        Assert.Equivalent(new[] { 100 }, result);
    }

    [ConditionalTheory, MemberData(nameof(ParameterTranslationModeValues))]
    public virtual async Task Parameter_collection_Contains_with_default_mode(ParameterTranslationMode mode)
    {
        var contextFactory = await InitializeNonSharedTest<TestContext>(
            onConfiguring: b => SetParameterizedCollectionMode(b, mode),
            seed: context =>
            {
                context.AddRange(
                    new TestEntity { Id = 1 },
                    new TestEntity { Id = 2 },
                    new TestEntity { Id = 100 });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var ints = new[] { 2, 999 };
        var result = await context.Set<TestEntity>().Where(c => ints.Contains(c.Id)).Select(x => x.Id).ToListAsync();
        Assert.Equivalent(new[] { 2 }, result);
    }

    [ConditionalTheory, MemberData(nameof(ParameterTranslationModeValues))]
    public virtual async Task Parameter_collection_Count_with_column_predicate_with_default_mode_EF_Constant(ParameterTranslationMode mode)
    {
        var contextFactory = await InitializeNonSharedTest<TestContext>(
            onConfiguring: b => SetParameterizedCollectionMode(b, mode),
            seed: context =>
            {
                context.AddRange(
                    new TestEntity { Id = 1 },
                    new TestEntity { Id = 100 });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var ids = new[] { 2, 999 };
        var result = await context.Set<TestEntity>().Where(c => EF.Constant(ids).Count(i => i > c.Id) == 1).Select(x => x.Id).ToListAsync();
        Assert.Equivalent(new[] { 100 }, result);
    }

    [ConditionalTheory, MemberData(nameof(ParameterTranslationModeValues))]
    public virtual async Task Parameter_collection_Contains_with_default_mode_EF_Constant(ParameterTranslationMode mode)
    {
        var contextFactory = await InitializeNonSharedTest<TestContext>(
            onConfiguring: b => SetParameterizedCollectionMode(b, mode),
            seed: context =>
            {
                context.AddRange(
                    new TestEntity { Id = 1 },
                    new TestEntity { Id = 2 },
                    new TestEntity { Id = 100 });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var ints = new[] { 2, 999 };
        var result = await context.Set<TestEntity>().Where(c => EF.Constant(ints).Contains(c.Id)).Select(x => x.Id).ToListAsync();
        Assert.Equivalent(new[] { 2 }, result);
    }

    [ConditionalTheory, MemberData(nameof(ParameterTranslationModeValues))]
    public virtual async Task Parameter_collection_Count_with_column_predicate_with_default_mode_EF_Parameter(ParameterTranslationMode mode)
    {
        var contextFactory = await InitializeNonSharedTest<TestContext>(
            onConfiguring: b => SetParameterizedCollectionMode(b, mode),
            seed: context =>
            {
                context.AddRange(
                    new TestEntity { Id = 1 },
                    new TestEntity { Id = 100 });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var ids = new[] { 2, 999 };
        var result = await context.Set<TestEntity>().Where(c => EF.Parameter(ids).Count(i => i > c.Id) == 1).Select(x => x.Id)
            .ToListAsync();
        Assert.Equivalent(new[] { 100 }, result);
    }

    [ConditionalTheory, MemberData(nameof(ParameterTranslationModeValues))]
    public virtual async Task Parameter_collection_Contains_with_default_mode_EF_Parameter(ParameterTranslationMode mode)
    {
        var contextFactory = await InitializeNonSharedTest<TestContext>(
            onConfiguring: b => SetParameterizedCollectionMode(b, mode),
            seed: context =>
            {
                context.AddRange(
                    new TestEntity { Id = 1 },
                    new TestEntity { Id = 2 },
                    new TestEntity { Id = 100 });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var ints = new[] { 2, 999 };
        var result = await context.Set<TestEntity>().Where(c => EF.Parameter(ints).Contains(c.Id)).Select(x => x.Id).ToListAsync();
        Assert.Equivalent(new[] { 2 }, result);
    }

    [ConditionalTheory, MemberData(nameof(ParameterTranslationModeValues))]
    public virtual async Task Parameter_collection_Count_with_column_predicate_with_default_mode_EF_MultipleParameters(
        ParameterTranslationMode mode)
    {
        var contextFactory = await InitializeNonSharedTest<TestContext>(
            onConfiguring: b => SetParameterizedCollectionMode(b, mode),
            seed: context =>
            {
                context.AddRange(
                    new TestEntity { Id = 1 },
                    new TestEntity { Id = 100 });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var ids = new[] { 2, 999 };
        var result = await context.Set<TestEntity>().Where(c => EF.MultipleParameters(ids).Count(i => i > c.Id) == 1).Select(x => x.Id)
            .ToListAsync();
        Assert.Equivalent(new[] { 100 }, result);
    }

    [ConditionalTheory, MemberData(nameof(ParameterTranslationModeValues))]
    public virtual async Task Parameter_collection_Contains_with_default_mode_EF_MultipleParameters(ParameterTranslationMode mode)
    {
        var contextFactory = await InitializeNonSharedTest<TestContext>(
            onConfiguring: b => SetParameterizedCollectionMode(b, mode),
            seed: context =>
            {
                context.AddRange(
                    new TestEntity { Id = 1 },
                    new TestEntity { Id = 2 },
                    new TestEntity { Id = 100 });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var ints = new[] { 2, 999 };
        var result = await context.Set<TestEntity>().Where(c => EF.MultipleParameters(ints).Contains(c.Id)).Select(x => x.Id).ToListAsync();
        Assert.Equivalent(new[] { 2 }, result);
    }

    [ConditionalFact]
    public virtual async Task Parameter_collection_Contains_parameter_bucketization()
    {
        var contextFactory = await InitializeNonSharedTest<TestContext>(
            onConfiguring: b => SetParameterizedCollectionMode(b, ParameterTranslationMode.MultipleParameters),
            seed: context =>
            {
                context.AddRange(
                    new TestEntity { Id = 1 },
                    new TestEntity { Id = 2 },
                    new TestEntity { Id = 100 });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var ints = new[] { 2, 999, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
        var result = await context.Set<TestEntity>().Where(c => ints.Contains(c.Id)).Select(c => c.Id).ToListAsync();
        Assert.Equivalent(new[] { 2 }, result);
    }

#pragma warning disable EF8001 // Owned JSON entities are obsolete
    [ConditionalFact]
    public virtual async Task Column_collection_inside_json_owned_entity()
    {
        var contextFactory = await InitializeNonSharedTest<TestContext>(
            onModelCreating: mb => mb.Entity<TestOwner>().OwnsOne(t => t.Owned, b => b.ToJson()),
            seed: context =>
            {
                context.AddRange(
                    new TestOwner { Owned = new TestOwned { Strings = ["foo", "bar"] } },
                    new TestOwner { Owned = new TestOwned { Strings = ["baz"] } });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var result = await context.Set<TestOwner>().SingleAsync(o => o.Owned.Strings.Count() == 2);
        Assert.Equivalent(new[] { "foo", "bar" }, result.Owned.Strings);

        result = await context.Set<TestOwner>().SingleAsync(o => o.Owned.Strings[1] == "bar");
        Assert.Equivalent(new[] { "foo", "bar" }, result.Owned.Strings);
    }
#pragma warning restore EF8001

    public override Task Column_collection_Concat_parameter_collection_equality_inline_collection()
        => AssertTranslationFailed(base.Column_collection_Concat_parameter_collection_equality_inline_collection);

    public override Task Column_collection_equality_inline_collection_with_parameters()
        => AssertTranslationFailed(base.Column_collection_equality_inline_collection_with_parameters);

    // TODO: Requires converting the results of a subquery (relational rowset) to a primitive collection for comparison,
    // not yet supported (#33792)
    public override async Task Column_collection_Where_equality_inline_collection()
        => await AssertTranslationFailed(base.Column_collection_Where_equality_inline_collection);

    [ConditionalFact]
    public override void Parameter_collection_in_subquery_and_Convert_as_compiled_query()
    {
        // The array indexing is translated as a subquery over e.g. OPENJSON with LIMIT/OFFSET.
        // Since there's a CAST over that, the type mapping inference from the other side (p.String) doesn't propagate inside to the
        // subquery. In this case, the CAST operand gets the default CLR type mapping, but that's object in this case.
        // We should apply the default type mapping to the parameter, but need to figure out the exact rules when to do this.
        var exception =
            Assert.Throws<InvalidOperationException>(() => base.Parameter_collection_in_subquery_and_Convert_as_compiled_query());

        Assert.Contains("in the SQL tree does not have a type mapping assigned", exception.Message);
    }

    public override async Task Parameter_collection_in_subquery_Union_another_parameter_collection_as_compiled_query()
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            base.Parameter_collection_in_subquery_Union_another_parameter_collection_as_compiled_query)).Message;

        Assert.Equal(RelationalStrings.SetOperationsRequireAtLeastOneSideWithValidTypeMapping("Union"), message);
    }

    public override async Task Project_inline_collection_with_Concat()
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(base.Project_inline_collection_with_Concat)).Message;

        Assert.Equal(RelationalStrings.InsufficientInformationToIdentifyElementOfCollectionJoin, message);
    }

    protected class TestOwner
    {
        public int Id { get; set; }
        public TestOwned Owned { get; set; } = null!;
    }

    [Owned]
    protected class TestOwned
    {
        public string[] Strings { get; set; } = null!;
    }
}
