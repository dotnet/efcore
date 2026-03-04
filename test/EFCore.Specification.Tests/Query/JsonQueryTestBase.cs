// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class JsonQueryTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : JsonQueryFixtureBase, new()
{
    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Basic_json_projection_owner_entity_NoTrackingWithIdentityResolution(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().AsNoTrackingWithIdentityResolution());

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Basic_json_projection_owner_entity_duplicated_NoTrackingWithIdentityResolution(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntitySingleOwned>().Select(x => new { First = x, Second = x }).AsNoTrackingWithIdentityResolution(),
            elementSorter: e => e.First.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.First, a.First);
                AssertEqual(e.Second, a.Second);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Basic_json_projection_owner_entity_twice_NoTrackingWithIdentityResolution(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new { First = x, Second = x }).AsNoTrackingWithIdentityResolution(),
            elementSorter: e => e.First.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.First, a.First);
                AssertEqual(e.Second, a.Second);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Basic_json_projection_owned_reference_root_NoTrackingWithIdentityResolution(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedReferenceRoot).AsNoTrackingWithIdentityResolution());

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Basic_json_projection_owned_reference_duplicated_NoTrackingWithIdentityResolution(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => new
                {
                    Root1 = x.OwnedReferenceRoot,
                    Branch1 = x.OwnedReferenceRoot.OwnedReferenceBranch,
                    Root2 = x.OwnedReferenceRoot,
                    Branch2 = x.OwnedReferenceRoot.OwnedReferenceBranch,
                }).AsNoTrackingWithIdentityResolution(),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Root1, a.Root1);
                AssertEqual(e.Root2, a.Root2);
                AssertEqual(e.Branch1, a.Branch1);
                AssertEqual(e.Branch2, a.Branch2);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Basic_json_projection_owned_reference_duplicated2_NoTrackingWithIdentityResolution(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => new
                {
                    Root1 = x.OwnedReferenceRoot,
                    Leaf1 = x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf,
                    Root2 = x.OwnedReferenceRoot,
                    Leaf2 = x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf,
                }).AsNoTrackingWithIdentityResolution(),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Root1, a.Root1);
                AssertEqual(e.Root2, a.Root2);
                AssertEqual(e.Leaf1, a.Leaf1);
                AssertEqual(e.Leaf2, a.Leaf2);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Basic_json_projection_owned_collection_root_NoTrackingWithIdentityResolution(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot).AsNoTrackingWithIdentityResolution(),
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Basic_json_projection_owned_reference_branch_NoTrackingWithIdentityResolution(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedReferenceRoot.OwnedReferenceBranch).AsNoTrackingWithIdentityResolution());

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Basic_json_projection_owned_collection_branch_NoTrackingWithIdentityResolution(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedReferenceRoot.OwnedCollectionBranch).AsNoTrackingWithIdentityResolution(),
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Basic_json_projection_owned_reference_leaf(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf).AsNoTracking());

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Basic_json_projection_owned_collection_leaf(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf).AsNoTracking(),
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Basic_json_projection_scalar(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedReferenceRoot.Name));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_scalar_length(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Where(x => x.OwnedReferenceRoot.Name.Length > 2).Select(x => x.Name));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Basic_json_projection_enum_inside_json_entity(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new
            {
                x.Id, x.OwnedReferenceRoot.OwnedReferenceBranch.Enum,
            }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Enum, a.Enum);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_projection_enum_with_custom_conversion(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityCustomNaming>().Select(x => new
            {
                x.Id, x.OwnedReferenceRoot.Enum,
            }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Enum, a.Enum);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_property_in_predicate(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .Where(x => x.OwnedReferenceRoot.OwnedReferenceBranch.Fraction < 20.5M).Select(x => x.Id));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_subquery_property_pushdown_length(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf.SomethingSomething)
                .Take(3)
                .Distinct()
                .Select(x => x.Length));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_subquery_reference_pushdown_reference(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => x.OwnedReferenceRoot)
                .Take(10)
                .Distinct()
                .Select(x => x.OwnedReferenceBranch).AsNoTracking());

    [ConditionalTheory(Skip = "issue #24263"), MemberData(nameof(IsAsyncData))]
    public virtual Task Json_subquery_reference_pushdown_reference_anonymous_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => new
                {
                    Entity = x.OwnedReferenceRoot,
                    Scalar = x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf.SomethingSomething
                })
                .Take(10)
                .Distinct()
                .Select(x => new { x.Entity.OwnedReferenceBranch, x.Scalar.Length }).AsNoTracking(),
            elementSorter: e => (e.OwnedReferenceBranch.Date, e.OwnedReferenceBranch.Fraction, e.Length),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.OwnedReferenceBranch, a.OwnedReferenceBranch);
                Assert.Equal(e.Length, a.Length);
            });

    [ConditionalTheory(Skip = "issue #24263"), MemberData(nameof(IsAsyncData))]
    public virtual Task Json_subquery_reference_pushdown_reference_pushdown_anonymous_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => new
                {
                    Root = x.OwnedReferenceRoot,
                    Scalar = x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf.SomethingSomething
                })
                .Take(10)
                .Distinct()
                .Select(x => new { Branch = x.Root.OwnedReferenceBranch, x.Scalar.Length })
                .OrderBy(x => x.Length)
                .Take(10)
                .Distinct()
                .Select(x => new
                {
                    x.Branch.OwnedReferenceLeaf,
                    x.Branch.OwnedCollectionLeaf,
                    x.Length
                })
                .AsNoTracking(),
            elementSorter: e => (e.OwnedReferenceLeaf.SomethingSomething, e.OwnedCollectionLeaf.Count, e.Length),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.OwnedReferenceLeaf, a.OwnedReferenceLeaf);
                AssertCollection(e.OwnedCollectionLeaf, e.OwnedCollectionLeaf, ordered: true);
                Assert.Equal(e.Length, a.Length);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_subquery_reference_pushdown_reference_pushdown_reference(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => x.OwnedReferenceRoot)
                .Take(10)
                .Distinct()
                .OrderBy(x => x.Name)
                .Select(x => x.OwnedReferenceBranch)
                .Take(10)
                .Distinct()
                .Select(x => x.OwnedReferenceLeaf).AsNoTracking());

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_subquery_reference_pushdown_reference_pushdown_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => x.OwnedReferenceRoot)
                .Take(10)
                .Distinct()
                .OrderBy(x => x.Name)
                .Select(x => x.OwnedReferenceBranch)
                .Take(10)
                .Distinct()
                .Select(x => x.OwnedCollectionLeaf).AsNoTracking(),
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_subquery_reference_pushdown_property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf)
                .Take(10)
                .Distinct()
                .Select(x => x.SomethingSomething));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Custom_naming_projection_owner_entity(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityCustomNaming>().Select(x => x));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Custom_naming_projection_owned_reference(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityCustomNaming>().Select(x => x.OwnedReferenceRoot.OwnedReferenceBranch).AsNoTracking());

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Custom_naming_projection_owned_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityCustomNaming>().OrderBy(x => x.Id).Select(x => x.OwnedCollectionRoot).AsNoTracking(),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Custom_naming_projection_owned_scalar(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityCustomNaming>().Select(x => x.OwnedReferenceRoot.OwnedReferenceBranch.Fraction));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Custom_naming_projection_everything(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityCustomNaming>().Select(x => new
            {
                root = x,
                referece = x.OwnedReferenceRoot,
                nested_reference = x.OwnedReferenceRoot.OwnedReferenceBranch,
                collection = x.OwnedCollectionRoot,
                nested_collection = x.OwnedReferenceRoot.OwnedCollectionBranch,
                scalar = x.OwnedReferenceRoot.Name,
                nested_scalar = x.OwnedReferenceRoot.OwnedReferenceBranch.Fraction,
            }).AsNoTracking(),
            elementSorter: e => e.root.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.root, a.root);
                AssertEqual(e.referece, a.referece);
                AssertEqual(e.nested_reference, a.nested_reference);
                AssertCollection(e.collection, a.collection, ordered: true);
                AssertCollection(e.nested_collection, a.nested_collection, ordered: true);
                Assert.Equal(e.scalar, a.scalar);
                Assert.Equal(e.nested_scalar, a.nested_scalar);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Project_entity_with_single_owned(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntitySingleOwned>());

    #region 32939

    [ConditionalFact]
    public virtual async Task Project_json_with_no_properties()
    {
        var contextFactory = await InitializeNonSharedTest<Context32939>(
            onModelCreating: OnModelCreating32939,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: Seed32939);

        using var context = contextFactory.CreateDbContext();
        await context.Set<Context32939.Entity>().ToListAsync();
    }

    protected virtual void OnModelCreating32939(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Context32939.Entity>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<Context32939.Entity>().OwnsOne(x => x.Empty);
        modelBuilder.Entity<Context32939.Entity>().OwnsOne(x => x.FieldOnly);
    }

    protected Task Seed32939(DbContext ctx)
    {
        var entity = new Context32939.Entity { Empty = new Context32939.JsonEmpty(), FieldOnly = new Context32939.JsonFieldOnly() };

        ctx.Add(entity);
        return ctx.SaveChangesAsync();
    }

    protected class Context32939(DbContextOptions options) : DbContext(options)
    {
        public class Entity
        {
            public int Id { get; set; }
            public JsonEmpty Empty { get; set; }
            public JsonFieldOnly FieldOnly { get; set; }
        }

        public class JsonEmpty
        {
        }

        public class JsonFieldOnly
        {
            public int Field;
        }
    }

    #endregion

    #region 21006

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Project_root_with_missing_scalars(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context21006>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            onModelCreating: OnModelCreating21006,
            seed: Seed21006);

        await using var context = contextFactory.CreateDbContext();

        var query = context.Set<Context21006.Entity>().Where(x => x.Id < 4);

        var result = async
            ? await query.ToListAsync()
            : query.ToList()!;

        var topLevel = result.Single(x => x.Id == 2);
        var nested = result.Single(x => x.Id == 3);

        Assert.Equal(default, topLevel.OptionalReference.Number);
        Assert.Equal(default, topLevel.RequiredReference.Number);
        Assert.True(topLevel.Collection.All(x => x.Number == default));

        Assert.Equal(default, nested.RequiredReference.NestedRequiredReference.DoB);
        Assert.Equal(default, nested.RequiredReference.NestedOptionalReference.DoB);
        Assert.Equal(default, nested.OptionalReference.NestedRequiredReference.DoB);
        Assert.Equal(default, nested.OptionalReference.NestedOptionalReference.DoB);
        Assert.True(nested.Collection.SelectMany(x => x.NestedCollection).All(x => x.DoB == default));
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Project_top_level_json_entity_with_missing_scalars(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context21006>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            onModelCreating: OnModelCreating21006,
            seed: Seed21006);

        await using var context = contextFactory.CreateDbContext();

        var query = context.Set<Context21006.Entity>().Where(x => x.Id < 4).Select(x => new
        {
            x.Id,
            x.OptionalReference,
            x.RequiredReference,
            x.Collection
        }).AsNoTracking();

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        var topLevel = result.Single(x => x.Id == 2);
        var nested = result.Single(x => x.Id == 3);

        Assert.Equal(default, topLevel.OptionalReference.Number);
        Assert.Equal(default, topLevel.RequiredReference.Number);
        Assert.True(topLevel.Collection.All(x => x.Number == default));

        Assert.Equal(default, nested.RequiredReference.NestedRequiredReference.DoB);
        Assert.Equal(default, nested.RequiredReference.NestedOptionalReference.DoB);
        Assert.Equal(default, nested.OptionalReference.NestedRequiredReference.DoB);
        Assert.Equal(default, nested.OptionalReference.NestedOptionalReference.DoB);
        Assert.True(nested.Collection.SelectMany(x => x.NestedCollection).All(x => x.DoB == default));
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Project_nested_json_entity_with_missing_scalars(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context21006>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            onModelCreating: OnModelCreating21006,
            seed: Seed21006);

        await using var context = contextFactory.CreateDbContext();

        var query = context.Set<Context21006.Entity>().Where(x => x.Id < 4).Select(x => new
        {
            x.Id,
            x.OptionalReference.NestedOptionalReference,
            x.RequiredReference.NestedRequiredReference,
            x.Collection[0].NestedCollection
        }).AsNoTracking();

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        var topLevel = result.Single(x => x.Id == 2);
        var nested = result.Single(x => x.Id == 3);

        Assert.Equal(default, nested.NestedOptionalReference.DoB);
        Assert.Equal(default, nested.NestedRequiredReference.DoB);
        Assert.True(nested.NestedCollection.All(x => x.DoB == default));
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Project_top_level_entity_with_null_value_required_scalars(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context21006>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            onModelCreating: OnModelCreating21006,
            seed: Seed21006);

        await using var context = contextFactory.CreateDbContext();

        var query = context.Set<Context21006.Entity>().Where(x => x.Id == 4).Select(x => new
        {
            x.Id, x.RequiredReference,
        }).AsNoTracking();

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        var nullScalars = result.Single();

        Assert.Equal(default, nullScalars.RequiredReference.Number);
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Project_root_entity_with_missing_required_navigation(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context21006>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            onModelCreating: OnModelCreating21006,
            seed: Seed21006);

        await using var context = contextFactory.CreateDbContext();

        var query = context.Set<Context21006.Entity>().Where(x => x.Id == 5).AsNoTracking();

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        var missingRequiredNav = result.Single();

        Assert.Equal(default, missingRequiredNav.RequiredReference.NestedRequiredReference);
        Assert.Equal(default, missingRequiredNav.OptionalReference.NestedRequiredReference);
        Assert.True(missingRequiredNav.Collection.All(x => x.NestedRequiredReference == default));
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Project_missing_required_navigation(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context21006>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            onModelCreating: OnModelCreating21006,
            seed: Seed21006);

        await using var context = contextFactory.CreateDbContext();

        var query = context.Set<Context21006.Entity>().Where(x => x.Id == 5).Select(x => x.RequiredReference.NestedRequiredReference)
            .AsNoTracking();

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        var missingRequiredNav = result.Single();

        Assert.Equal(default, missingRequiredNav);
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Project_root_entity_with_null_required_navigation(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context21006>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            onModelCreating: OnModelCreating21006,
            seed: Seed21006);

        await using var context = contextFactory.CreateDbContext();

        var query = context.Set<Context21006.Entity>().Where(x => x.Id == 6).AsNoTracking();

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        var nullRequiredNav = result.Single();

        Assert.Equal(default, nullRequiredNav.RequiredReference.NestedRequiredReference);
        Assert.Equal(default, nullRequiredNav.OptionalReference.NestedRequiredReference);
        Assert.True(nullRequiredNav.Collection.All(x => x.NestedRequiredReference == default));
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Project_null_required_navigation(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context21006>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            onModelCreating: OnModelCreating21006,
            seed: Seed21006);

        await using var context = contextFactory.CreateDbContext();

        var query = context.Set<Context21006.Entity>().Where(x => x.Id == 6).Select(x => x.RequiredReference).AsNoTracking();

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        var nullRequiredNav = result.Single();

        Assert.Equal(default, nullRequiredNav.NestedRequiredReference);
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Project_missing_required_scalar(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context21006>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            onModelCreating: OnModelCreating21006,
            seed: Seed21006);

        await using var context = contextFactory.CreateDbContext();

        var query = context.Set<Context21006.Entity>()
            .Where(x => x.Id == 2)
            .Select(x => new { x.Id, Number = (double?)x.RequiredReference.Number });

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Null(result.Single().Number);
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Project_null_required_scalar(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context21006>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            onModelCreating: OnModelCreating21006,
            seed: Seed21006);

        await using var context = contextFactory.CreateDbContext();

        var query = context.Set<Context21006.Entity>()
            .Where(x => x.Id == 4)
            .Select(x => new
            {
                x.Id, Number = (double?)x.RequiredReference.Number,
            });

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Null(result.Single().Number);
    }

    protected virtual void OnModelCreating21006(ModelBuilder modelBuilder)
        => modelBuilder.Entity<Context21006.Entity>(b =>
        {
            b.Property(x => x.Id).ValueGeneratedNever();
            b.OwnsOne(
                x => x.OptionalReference, bb =>
                {
                    bb.OwnsOne(x => x.NestedOptionalReference);
                    bb.OwnsOne(x => x.NestedRequiredReference);
                    bb.Navigation(x => x.NestedRequiredReference).IsRequired();
                    bb.OwnsMany(x => x.NestedCollection);
                });
            b.OwnsOne(
                x => x.RequiredReference, bb =>
                {
                    bb.OwnsOne(x => x.NestedOptionalReference);
                    bb.OwnsOne(x => x.NestedRequiredReference);
                    bb.Navigation(x => x.NestedRequiredReference).IsRequired();
                    bb.OwnsMany(x => x.NestedCollection);
                });
            b.Navigation(x => x.RequiredReference).IsRequired();
            b.OwnsMany(
                x => x.Collection, bb =>
                {
                    bb.OwnsOne(x => x.NestedOptionalReference);
                    bb.OwnsOne(x => x.NestedRequiredReference);
                    bb.Navigation(x => x.NestedRequiredReference).IsRequired();
                    bb.OwnsMany(x => x.NestedCollection);
                });
        });

    protected virtual async Task Seed21006(Context21006 context)
    {
        // everything
        var e1 = new Context21006.Entity
        {
            Id = 1,
            Name = "e1",
            OptionalReference = new Context21006.JsonEntity
            {
                Number = 7,
                Text = "e1 or",
                NestedOptionalReference = new Context21006.JsonEntityNested { DoB = new DateTime(2000, 1, 1), Text = "e1 or nor" },
                NestedRequiredReference = new Context21006.JsonEntityNested { DoB = new DateTime(2000, 1, 1), Text = "e1 or nrr" },
                NestedCollection =
                [
                    new() { DoB = new DateTime(2000, 1, 1), Text = "e1 or c1" },
                    new() { DoB = new DateTime(2000, 1, 1), Text = "e1 or c2" }
                ]
            },
            RequiredReference = new Context21006.JsonEntity
            {
                Number = 7,
                Text = "e1 rr",
                NestedOptionalReference = new Context21006.JsonEntityNested { DoB = new DateTime(2000, 1, 1), Text = "e1 rr nor" },
                NestedRequiredReference = new Context21006.JsonEntityNested { DoB = new DateTime(2000, 1, 1), Text = "e1 rr nrr" },
                NestedCollection =
                [
                    new() { DoB = new DateTime(2000, 1, 1), Text = "e1 rr c1" },
                    new() { DoB = new DateTime(2000, 1, 1), Text = "e1 rr c2" }
                ]
            },
            Collection =
            [
                new()
                {
                    Number = 7,
                    Text = "e1 c1",
                    NestedOptionalReference = new Context21006.JsonEntityNested { DoB = new DateTime(2000, 1, 1), Text = "e1 c1 nor" },
                    NestedRequiredReference = new Context21006.JsonEntityNested { DoB = new DateTime(2000, 1, 1), Text = "e1 c1 nrr" },
                    NestedCollection =
                    [
                        new() { DoB = new DateTime(2000, 1, 1), Text = "e1 c1 c1" },
                        new() { DoB = new DateTime(2000, 1, 1), Text = "e1 c1 c2" }
                    ]
                },

                new()
                {
                    Number = 7,
                    Text = "e1 c2",
                    NestedOptionalReference = new Context21006.JsonEntityNested { DoB = new DateTime(2000, 1, 1), Text = "e1 c2 nor" },
                    NestedRequiredReference = new Context21006.JsonEntityNested { DoB = new DateTime(2000, 1, 1), Text = "e1 c2 nrr" },
                    NestedCollection =
                    [
                        new() { DoB = new DateTime(2000, 1, 1), Text = "e1 c2 c1" },
                        new() { DoB = new DateTime(2000, 1, 1), Text = "e1 c2 c2" }
                    ]
                }

            ]
        };

        context.Add(e1);
        await context.SaveChangesAsync();
    }

    protected class Context21006(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity> Entities { get; set; }

        public class Entity
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public JsonEntity OptionalReference { get; set; }
            public JsonEntity RequiredReference { get; set; }
            public List<JsonEntity> Collection { get; set; }
        }

        public class JsonEntity
        {
            public string Text { get; set; }
            public double Number { get; set; }

            public JsonEntityNested NestedOptionalReference { get; set; }
            public JsonEntityNested NestedRequiredReference { get; set; }
            public List<JsonEntityNested> NestedCollection { get; set; }
        }

        public class JsonEntityNested
        {
            public DateTime DoB { get; set; }
            public string Text { get; set; }
        }
    }

    #endregion

    #region 34960

    [ConditionalFact]
    public virtual async Task Project_entity_with_json_null_values()
    {
        var contextFactory = await InitializeNonSharedTest<Context34960>(seed: Seed34960, onModelCreating: OnModelCreating34960);

        using var context = contextFactory.CreateDbContext();
        var query = await context.Entities.ToListAsync();
    }

    [ConditionalFact]
    public virtual async Task Try_project_collection_but_JSON_is_entity()
    {
        var contextFactory = await InitializeNonSharedTest<Context34960>(seed: Seed34960, onModelCreating: OnModelCreating34960);
        using var context = contextFactory.CreateDbContext();

        await context.Junk.AsNoTracking().Where(x => x.Id == 1).Select(x => x.Collection).FirstOrDefaultAsync();
    }

    [ConditionalFact]
    public virtual async Task Try_project_reference_but_JSON_is_collection()
    {
        var contextFactory = await InitializeNonSharedTest<Context34960>(seed: Seed34960, onModelCreating: OnModelCreating34960);
        using var context = contextFactory.CreateDbContext();

        await context.Junk.AsNoTracking().Where(x => x.Id == 2).Select(x => x.Reference).FirstOrDefaultAsync();
    }

    protected class Context34960(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity> Entities { get; set; }
        public DbSet<JunkEntity> Junk { get; set; }

        public class Entity
        {
            public int Id { get; set; }
            public JsonEntity Reference { get; set; }
            public List<JsonEntity> Collection { get; set; }
        }

        public class JsonEntity
        {
            public string Name { get; set; }
            public double Number { get; set; }

            public JsonEntityNested NestedReference { get; set; }
            public List<JsonEntityNested> NestedCollection { get; set; }
        }

        public class JsonEntityNested
        {
            public DateTime DoB { get; set; }
            public string Text { get; set; }
        }

        public class JunkEntity
        {
            public int Id { get; set; }
            public JsonEntity Reference { get; set; }
            public List<JsonEntity> Collection { get; set; }
        }
    }

    protected virtual void OnModelCreating34960(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Context34960.Entity>(b =>
        {
            b.Property(x => x.Id).ValueGeneratedNever();

            b.OwnsOne(
                x => x.Reference, b =>
                {
                    b.OwnsOne(x => x.NestedReference);
                    b.OwnsMany(x => x.NestedCollection);
                });

            b.OwnsMany(
                x => x.Collection, b =>
                {
                    b.OwnsOne(x => x.NestedReference);
                    b.OwnsMany(x => x.NestedCollection);
                });
        });

        modelBuilder.Entity<Context34960.JunkEntity>(b =>
        {
            b.Property(x => x.Id).ValueGeneratedNever();

            b.OwnsOne(
                x => x.Reference, b =>
                {
                    b.Ignore(x => x.NestedReference);
                    b.Ignore(x => x.NestedCollection);
                });

            b.OwnsMany(
                x => x.Collection, b =>
                {
                    b.Ignore(x => x.NestedReference);
                    b.Ignore(x => x.NestedCollection);
                });
        });
    }

    protected virtual async Task Seed34960(Context34960 ctx)
    {
        // everything
        var e1 = new Context34960.Entity
        {
            Id = 1,
            Reference = new Context34960.JsonEntity
            {
                Name = "ref1",
                Number = 1.5f,
                NestedReference = new Context34960.JsonEntityNested { DoB = new DateTime(2000, 1, 1), Text = "nested ref 1" },
                NestedCollection =
                [
                    new Context34960.JsonEntityNested { DoB = new DateTime(2001, 1, 1), Text = "nested col 1 1" },
                    new Context34960.JsonEntityNested { DoB = new DateTime(2001, 2, 2), Text = "nested col 1 2" },
                ],
            },
            Collection =
            [
                new Context34960.JsonEntity
                {
                    Name = "col 1 1",
                    Number = 2.5f,
                    NestedReference = new Context34960.JsonEntityNested { DoB = new DateTime(2010, 1, 1), Text = "nested col 1 1 ref 1" },
                    NestedCollection =
                    [
                        new Context34960.JsonEntityNested { DoB = new DateTime(2011, 1, 1), Text = "nested col 1 1 col 1 1" },
                        new Context34960.JsonEntityNested { DoB = new DateTime(2011, 2, 2), Text = "nested col 1 1 col 1 2" },
                    ],
                },
                new Context34960.JsonEntity
                {
                    Name = "col 1 2",
                    Number = 2.5f,
                    NestedReference = new Context34960.JsonEntityNested { DoB = new DateTime(2020, 1, 1), Text = "nested col 1 2 ref 1" },
                    NestedCollection =
                    [
                        new Context34960.JsonEntityNested { DoB = new DateTime(2021, 1, 1), Text = "nested col 1 2 col 1 1" },
                        new Context34960.JsonEntityNested { DoB = new DateTime(2021, 2, 2), Text = "nested col 1 2 col 1 2" },
                    ],
                },
            ],
        };

        // relational nulls
        var e2 = new Context34960.Entity
        {
            Id = 2,
            Reference = null,
            Collection = null
        };

        // nested relational nulls
        var e3 = new Context34960.Entity
        {
            Id = 3,
            Reference = new Context34960.JsonEntity
            {
                Name = "ref3",
                Number = 3.5f,
                NestedReference = null,
                NestedCollection = null
            },
            Collection =
            [
                new Context34960.JsonEntity
                {
                    Name = "col 3 1",
                    Number = 32.5f,
                    NestedReference = null,
                    NestedCollection = null,
                },
                new Context34960.JsonEntity
                {
                    Name = "col 3 2",
                    Number = 33.5f,
                    NestedReference = null,
                    NestedCollection = null,
                },
            ],
        };

        ctx.Entities.AddRange(e1, e2, e3);
        await ctx.SaveChangesAsync();
    }

    #endregion


    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task LeftJoin_json_entities(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntitySingleOwned>()
                .LeftJoin(ss.Set<JsonEntityBasic>(), e1 => e1.Id, e2 => e2.Id, (e1, e2) => new { e1, e2 }),
            elementSorter: e => (e.e1.Id, e.e2?.Id),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.e1, a.e1);
                AssertEqual(e.e2, a.e2);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task RightJoin_json_entities(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .RightJoin(
                    ss.Set<JsonEntitySingleOwned>(),
                    e1 => e1.Id,
                    e2 => e2.Id,
                    (e1, e2) => new { e1, e2 }),
            elementSorter: e => (e.e1?.Id, e.e2.Id),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.e1, a.e1);
                AssertEqual(e.e2, a.e2);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Left_join_json_entities_complex_projection(bool async)
        => AssertQuery(
            async,
            ss => (from e1 in ss.Set<JsonEntitySingleOwned>()
                   join e2 in ss.Set<JsonEntityBasic>() on e1.Id equals e2.Id into g
                   from e2 in g.DefaultIfEmpty()
                   select new
                   {
                       Id1 = e1.Id,
                       Id2 = (int?)e2.Id,
                       e2.OwnedReferenceRoot,
                       e2.OwnedReferenceRoot.OwnedReferenceBranch,
                       e2.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf,
                       e2.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf
                   }).AsNoTracking(),
            elementSorter: e => (e.Id1, e?.Id2),
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id1, a.Id1);
                Assert.Equal(e.Id2, a.Id2);
                AssertEqual(e.OwnedReferenceRoot, a.OwnedReferenceRoot);
                AssertEqual(e.OwnedReferenceBranch, a.OwnedReferenceBranch);
                AssertEqual(e.OwnedReferenceLeaf, a.OwnedReferenceLeaf);
                AssertCollection(e.OwnedCollectionLeaf, a.OwnedCollectionLeaf, ordered: true);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Left_join_json_entities_json_being_inner(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<JsonEntityBasic>()
                  join e2 in ss.Set<JsonEntitySingleOwned>() on e1.Id equals e2.Id into g
                  from e2 in g.DefaultIfEmpty()
                  select new { e1, e2 },
            elementSorter: e => (e.e1.Id, e.e2?.Id),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.e1, a.e1);
                AssertEqual(e.e2, a.e2);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Left_join_json_entities_complex_projection_json_being_inner(bool async)
        => AssertQuery(
            async,
            ss => (from e1 in ss.Set<JsonEntityBasic>()
                   join e2 in ss.Set<JsonEntitySingleOwned>() on e1.Id equals e2.Id into g
                   from e2 in g.DefaultIfEmpty()
                   select new
                   {
                       Id1 = e1.Id,
                       Id2 = (int?)e2.Id,
                       e1.OwnedReferenceRoot,
                       e1.OwnedReferenceRoot.OwnedReferenceBranch,
                       e1.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf,
                       e1.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf,
                       e2.Name
                   }).AsNoTracking(),
            elementSorter: e => (e.Id1, e?.Id2),
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id1, a.Id1);
                Assert.Equal(e.Id2, a.Id2);
                AssertEqual(e.OwnedReferenceRoot, a.OwnedReferenceRoot);
                AssertEqual(e.OwnedReferenceBranch, a.OwnedReferenceBranch);
                AssertEqual(e.OwnedReferenceLeaf, a.OwnedReferenceLeaf);
                AssertCollection(e.OwnedCollectionLeaf, a.OwnedCollectionLeaf, ordered: true);
                AssertEqual(e.Name, a.Name);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Project_json_entity_FirstOrDefault_subquery_with_binding_on_top(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => ss.Set<JsonEntityBasic>()
                    .OrderBy(xx => xx.Id)
                    .Select(xx => xx.OwnedReferenceRoot)
                    .FirstOrDefault().OwnedReferenceBranch.Date));

    [ConditionalTheory(Skip = "issue #28733"), MemberData(nameof(IsAsyncData))]
    public virtual Task Project_json_entity_FirstOrDefault_subquery_with_entity_comparison_on_top(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => ss.Set<JsonEntityBasic>()
                        .OrderBy(xx => xx.Id)
                        .Select(xx => xx.OwnedReferenceRoot)
                        .FirstOrDefault().OwnedReferenceBranch
                    == ss.Set<JsonEntityBasic>()
                        .OrderByDescending(x => x.Id)
                        .Select(x => ss.Set<JsonEntityBasic>()
                            .OrderBy(xx => xx.Id)
                            .Select(xx => xx.OwnedReferenceRoot)
                            .FirstOrDefault().OwnedReferenceBranch)));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_entity_with_inheritance_basic_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityInheritanceBase>());

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_entity_with_inheritance_project_derived(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityInheritanceBase>().OfType<JsonEntityInheritanceDerived>());

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_entity_with_inheritance_project_navigations(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityInheritanceBase>().Select(x => new
            {
                x.Id,
                x.ReferenceOnBase,
                x.CollectionOnBase
            }).AsNoTracking(),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                AssertEqual(e.ReferenceOnBase, a.ReferenceOnBase);
                AssertCollection(e.CollectionOnBase, a.CollectionOnBase, ordered: true);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_entity_with_inheritance_project_navigations_on_derived(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityInheritanceBase>().OfType<JsonEntityInheritanceDerived>().Select(x => new
            {
                x.Id,
                x.ReferenceOnBase,
                x.ReferenceOnDerived,
                x.CollectionOnBase,
                x.CollectionOnDerived
            }).AsNoTracking(),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertEqual(e.ReferenceOnBase, a.ReferenceOnBase);
                AssertEqual(e.ReferenceOnDerived, a.ReferenceOnDerived);
                AssertCollection(e.CollectionOnBase, a.CollectionOnBase, ordered: true);
                AssertCollection(e.CollectionOnDerived, a.CollectionOnDerived, ordered: true);
            });

    [ConditionalTheory(Skip = "issue #28645"), MemberData(nameof(IsAsyncData))]
    public virtual Task Json_entity_backtracking(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf.Parent.Date));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_projection_basic(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot[1]).AsNoTracking());

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_ElementAt_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot.AsQueryable().ElementAt(1)).AsNoTracking());

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_ElementAtOrDefault_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot.AsQueryable().ElementAtOrDefault(1)).AsNoTracking());

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_projection_project_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot[1].OwnedCollectionBranch).AsNoTracking(),
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_ElementAt_project_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .Select(x => x.OwnedCollectionRoot.AsQueryable().ElementAt(1).OwnedCollectionBranch)
                .AsNoTracking(),
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_ElementAtOrDefault_project_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .Select(x => x.OwnedCollectionRoot.AsQueryable().ElementAtOrDefault(1).OwnedCollectionBranch)
                .AsNoTracking(),
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_projection_using_parameter(bool async)
    {
        var prm = 0;

        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot[prm]).AsNoTracking());
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_projection_using_column(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot[x.Id]).AsNoTracking());

    private static int MyMethod(int value)
        => value;

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_projection_using_untranslatable_client_method(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot[MyMethod(x.Id)]).AsNoTracking());

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_projection_using_untranslatable_client_method2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot[0].OwnedReferenceBranch.OwnedCollectionLeaf[MyMethod(x.Id)])
                .AsNoTracking());

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_outside_bounds(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot[25]).AsNoTracking(),
            ss => ss.Set<JsonEntityBasic>().Select(x => (JsonOwnedRoot)null));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_outside_bounds2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf[25]).AsNoTracking(),
            ss => ss.Set<JsonEntityBasic>().Select(x => (JsonOwnedLeaf)null));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_outside_bounds_with_property_access(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityBasic>().OrderBy(x => x.Id).Select(x => (int?)x.OwnedCollectionRoot[25].Number),
            ss => ss.Set<JsonEntityBasic>().Select(x => (int?)null));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_projection_nested(bool async)
    {
        var prm = 1;

        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot[0].OwnedCollectionBranch[prm]).AsNoTracking());
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_projection_nested_project_scalar(bool async)
    {
        var prm = 1;

        return AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot[0].OwnedCollectionBranch[prm].Date));
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_projection_nested_project_reference(bool async)
    {
        var prm = 1;

        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot[0].OwnedCollectionBranch[prm].OwnedReferenceLeaf)
                .AsNoTracking());
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_projection_nested_project_collection(bool async)
    {
        var prm = 1;

        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => x.OwnedCollectionRoot[0].OwnedCollectionBranch[prm].OwnedCollectionLeaf)
                .AsNoTracking(),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_projection_nested_project_collection_anonymous_projection(bool async)
    {
        var prm = 1;

        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .Select(x => new { x.Id, x.OwnedCollectionRoot[0].OwnedCollectionBranch[prm].OwnedCollectionLeaf })
                .AsNoTracking(),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                AssertCollection(e.OwnedCollectionLeaf, a.OwnedCollectionLeaf, ordered: true);
            });
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_predicate_using_constant(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityBasic>().Where(x => x.OwnedCollectionRoot[0].Name != "Foo").Select(x => x.Id));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_predicate_using_variable(bool async)
    {
        var prm = 1;

        return AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityBasic>().Where(x => x.OwnedCollectionRoot[prm].Name != "Foo").Select(x => x.Id));
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_predicate_using_column(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Where(x => x.OwnedCollectionRoot[x.Id].Name == "e1_c2").Select(x => new { x.Id, x }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                AssertEqual(e.x, a.x);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_predicate_using_complex_expression1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Where(x => x.OwnedCollectionRoot[x.Id == 1 ? 0 : 1].Name == "e1_c1")
                .Select(x => new { x.Id, x }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                AssertEqual(e.x, a.x);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_predicate_using_complex_expression2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Where(x => x.OwnedCollectionRoot[ss.Set<JsonEntityBasic>().Max(x => x.Id)].Name == "e1_c2")
                .Select(x => new { x.Id, x }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                AssertEqual(e.x, a.x);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_ElementAt_in_predicate(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityBasic>().Where(x => x.OwnedCollectionRoot.AsQueryable().ElementAt(1).Name != "Foo").Select(x => x.Id));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_predicate_nested_mix(bool async)
    {
        var prm = 0;

        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Where(x
                => x.OwnedCollectionRoot[1].OwnedCollectionBranch[prm].OwnedCollectionLeaf[x.Id - 1].SomethingSomething
                == "e1_c2_c1_c1"));
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_ElementAt_and_pushdown(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .Select(x => new { x.Id, CollectionElement = x.OwnedCollectionRoot.Select(xx => xx.Number).ElementAt(0) }));

    #region 32310

    [ConditionalFact]
    public virtual async Task Contains_on_nested_collection_with_init_only_navigation()
    {
        var contextFactory = await InitializeNonSharedTest<Context32310>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            onModelCreating: OnModelCreating32310,
            seed: Seed32310);

        await using var context = contextFactory.CreateDbContext();

        var query = context.Set<Context32310.Pub>()
            .Where(u => u.Visits.DaysVisited.Contains(new DateOnly(2023, 1, 1)));

        var result = await query.FirstOrDefaultAsync();

        Assert.Equal("FBI", result.Name);
        Assert.Equal(new DateOnly(2023, 1, 1), result.Visits.DaysVisited.Single());
    }

    protected virtual void OnModelCreating32310(ModelBuilder modelBuilder)
        => modelBuilder.Entity<Context32310.Pub>().OwnsOne(e => e.Visits);

    protected virtual async Task Seed32310(DbContext context)
    {
        var user = new Context32310.Pub
        {
            Name = "FBI", Visits = new Context32310.Visits { LocationTag = "tag", DaysVisited = [new DateOnly(2023, 1, 1)] }
        };

        context.Add(user);
        await context.SaveChangesAsync();
    }

    protected class Context32310(DbContextOptions options) : DbContext(options)
    {
        public class Pub
        {
            public int Id { get; set; }
            public required string Name { get; set; }
            public Visits Visits { get; set; } = null!;
        }

        public class Visits
        {
            public string LocationTag { get; set; }
            public required List<DateOnly> DaysVisited { get; init; }
        }
    }

    #endregion


    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_Any_with_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Where(j
                => j.OwnedReferenceRoot.OwnedCollectionBranch.Any(b => b.OwnedReferenceLeaf.SomethingSomething == "e1_r_c1_r")));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_Where_ElementAt(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Where(j =>
                j.OwnedReferenceRoot.OwnedCollectionBranch
                    .Where(o => o.Enum == JsonEnum.Three)
                    .ElementAt(0).OwnedReferenceLeaf.SomethingSomething
                == "e1_r_c2_r"));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_Skip(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .Where(j => j.OwnedReferenceRoot.OwnedCollectionBranch
                        .Skip(1)
                        .ElementAt(0).OwnedReferenceLeaf.SomethingSomething
                    == "e1_r_c2_r"));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_OrderByDescending_Skip_ElementAt(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .Where(j => j.OwnedReferenceRoot.OwnedCollectionBranch
                        .OrderByDescending(b => b.Date)
                        .Skip(1)
                        .ElementAt(0).OwnedReferenceLeaf.SomethingSomething
                    == "e1_r_c1_r"));

    // If this test is failing because of DistinctAfterOrderByWithoutRowLimitingOperatorWarning, this is because EF warns/errors by
    // default for Distinct after OrderBy (without Skip/Take); but you likely have a naturally-ordered JSON collection, where the
    // ordering has been added by the provider as part of the collection translation.
    // Consider overriding RelationalQueryableMethodTranslatingExpressionVisitor.IsNaturallyOrdered() to identify such naturally-ordered
    // collections, exempting them from the warning.
    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_Distinct_Count_with_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .Where(j => j.OwnedReferenceRoot.OwnedCollectionBranch
                        .Distinct()
                        .Count(b => b.OwnedReferenceLeaf.SomethingSomething == "e1_r_c2_r")
                    == 1));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_within_collection_Count(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .Where(j => j.OwnedCollectionRoot.Any(c => c.OwnedCollectionBranch.Count == 2)));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_in_projection_with_composition_count(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => x.OwnedCollectionRoot.Count));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_in_projection_with_anonymous_projection_of_scalars(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => x.OwnedCollectionRoot
                    .Select(xx => new { xx.Name, xx.Number })
                    .ToList()));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_in_projection_with_composition_where_and_anonymous_projection_of_scalars(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => x.OwnedCollectionRoot
                    .Where(xx => xx.Name == "Foo")
                    .Select(xx => new { xx.Name, xx.Number })
                    .ToList()));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_in_projection_with_composition_where_and_anonymous_projection_of_primitive_arrays(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => x.OwnedCollectionRoot
                    .Where(xx => xx.Name == "Foo")
                    .Select(xx => new { xx.Names, xx.Numbers })
                    .ToList()));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_filter_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => x.OwnedCollectionRoot.Where(xx => xx.Name != "Foo").ToList())
                .AsNoTracking(),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertCollection(e, a, ordered: true, elementAsserter: (ee, aa) => AssertEqual(ee, aa));
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_nested_collection_filter_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => x.OwnedCollectionRoot
                    .Select(xx => xx.OwnedCollectionBranch.Where(xxx => xxx.Date != new DateTime(2000, 1, 1)).ToList()))
                .AsNoTracking(),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(
                e, a, ordered: true, elementAsserter: (ee, aa) => AssertCollection(ee, aa, ordered: true)));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_nested_collection_anonymous_projection_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => x.OwnedCollectionRoot
                    .Select(xx => xx.OwnedCollectionBranch.Select(xxx => new
                    {
                        xxx.Date,
                        xxx.Enum,
                        xxx.Enums,
                        xxx.Fraction,
                        xxx.OwnedReferenceLeaf,
                        xxx.OwnedCollectionLeaf
                    }).ToList()))
                .AsNoTracking(),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(
                e, a, ordered: true, elementAsserter: (ee, aa) => AssertCollection(
                    ee, aa, ordered: true, elementAsserter: (eee, aaa) =>
                    {
                        AssertEqual(eee.Date, aaa.Date);
                        AssertEqual(eee.Enum, aaa.Enum);
                        AssertCollection(eee.Enums, aaa.Enums, ordered: true);
                        AssertEqual(eee.Fraction, aaa.Fraction);
                        AssertEqual(eee.OwnedReferenceLeaf, aaa.OwnedReferenceLeaf);
                        AssertCollection(eee.OwnedCollectionLeaf, aaa.OwnedCollectionLeaf, ordered: true);
                    })));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_skip_take_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => x.OwnedCollectionRoot.OrderBy(xx => xx.Name).Skip(1).Take(5).ToList())
                .AsNoTracking(),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_skip_take_in_projection_project_into_anonymous_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => x.OwnedCollectionRoot
                    .OrderBy(xx => xx.Name)
                    .Skip(1)
                    .Take(5)
                    .Select(xx => new
                    {
                        xx.Name,
                        xx.Names,
                        xx.Number,
                        xx.Numbers,
                        xx.OwnedCollectionBranch,
                        xx.OwnedReferenceBranch
                    }).ToList())
                .AsNoTracking(),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertCollection(
                    e, a, ordered: true, elementAsserter: (ee, aa) =>
                    {
                        AssertEqual(ee.Name, aa.Name);
                        AssertCollection(ee.Names, aa.Names, ordered: true);
                        AssertEqual(ee.Number, aa.Number);
                        AssertCollection(ee.Numbers, aa.Numbers, ordered: true);
                        AssertCollection(ee.OwnedCollectionBranch, aa.OwnedCollectionBranch, ordered: true);
                        AssertEqual(ee.OwnedReferenceBranch, aa.OwnedReferenceBranch);
                    });
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_skip_take_in_projection_with_json_reference_access_as_final_operation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => x.OwnedCollectionRoot
                    .OrderBy(xx => xx.Name)
                    .Skip(1)
                    .Take(5)
                    .Select(xx => xx.OwnedReferenceBranch).ToList())
                .AsNoTracking(),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_distinct_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => x.OwnedCollectionRoot.Distinct())
                .AsNoTracking(),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: ee => (ee.Name, ee.Number)));

    [ConditionalTheory(Skip = "issue #31397"), MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_anonymous_projection_distinct_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => x.OwnedCollectionRoot.Select(xx => xx.Name).Distinct().ToList())
                .AsNoTracking(),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: ee => ee));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_leaf_filter_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf
                    .Where(xx => xx.SomethingSomething != "Baz").ToList())
                .AsNoTracking(),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_multiple_collection_projections(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => new
                {
                    First = x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf
                        .Where(xx => xx.SomethingSomething != "Baz").ToList(),
                    Second = x.OwnedCollectionRoot.Distinct().ToList(),
                    Third = x.OwnedCollectionRoot
                        .Select(xx => xx.OwnedCollectionBranch.Where(xxx => xxx.Date != new DateTime(2000, 1, 1)).ToList()),
                    Fourth = x.EntityCollection.ToList()
                })
                .AsNoTracking(),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertCollection(e.First, a.First, ordered: true);
                AssertCollection(e.Second, a.Second, elementSorter: ee => (ee.Name, ee.Number));
                AssertCollection(e.Third, a.Third, ordered: true, elementAsserter: (ee, aa) => AssertCollection(ee, aa, ordered: true));
                AssertCollection(e.Fourth, a.Fourth);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_branch_collection_distinct_and_other_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => new
                {
                    First = x.OwnedReferenceRoot.OwnedCollectionBranch.Distinct().ToList(), Second = x.EntityCollection.ToList()
                })
                .AsNoTracking(),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertCollection(e.First, a.First, ordered: true);
                AssertCollection(e.Second, a.Second);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_leaf_collection_distinct_and_other_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => new
                {
                    First = x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf.Distinct().ToList(),
                    Second = x.EntityCollection.ToList()
                })
                .AsNoTracking(),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertCollection(e.First, a.First, ordered: true);
                AssertCollection(e.Second, a.Second);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_of_primitives_SelectMany(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .SelectMany(x => x.OwnedReferenceRoot.Names));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_of_primitives_index_used_in_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Where(x => x.OwnedReferenceRoot.Names[0] == "e1_r1"));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_of_primitives_index_used_in_projection(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityBasic>().OrderBy(x => x.Id).Select(x => x.OwnedReferenceRoot.OwnedReferenceBranch.Enums[0]),
            assertOrder: true);

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_of_primitives_index_used_in_orderby(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().OrderBy(x => x.OwnedReferenceRoot.Numbers[0]),
            assertOrder: true);

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_of_primitives_contains_in_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Where(x => x.OwnedReferenceRoot.Names.Contains("e1_r1")),
            assertOrder: true);

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_collection_index_with_parameter_Select_ElementAt(bool async)
    {
        var prm = 0;

        await AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new
            {
                x.Id, CollectionElement = x.OwnedCollectionRoot[prm].OwnedCollectionBranch.Select(xx => "Foo").ElementAt(0)
            }));
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_collection_index_with_expression_Select_ElementAt(bool async)
    {
        var prm = 0;

        await AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(j => j.OwnedCollectionRoot[prm + j.Id].OwnedCollectionBranch
                .Select(b => b.OwnedReferenceLeaf.SomethingSomething)
                .ElementAt(0)),
            ss => ss.Set<JsonEntityBasic>().Select(j => j.OwnedCollectionRoot.Count > prm + j.Id
                ? j.OwnedCollectionRoot[prm + j.Id].OwnedCollectionBranch
                    .Select(b => b.OwnedReferenceLeaf.SomethingSomething)
                    .ElementAt(0)
                : null));
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_collection_Select_entity_collection_ElementAt(bool async)
        => await AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .AsNoTracking()
                .Select(x => x.OwnedCollectionRoot.Select(xx => xx.OwnedCollectionBranch).ElementAt(0)),
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Count, a.Count);
                for (var i = 0; i < e.Count; i++)
                {
                    JsonQueryFixtureBase.AssertOwnedBranch(e[i], a[i]);
                }
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_collection_Select_entity_ElementAt(bool async)
        => await AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().AsNoTracking().Select(x =>
                x.OwnedCollectionRoot.Select(xx => xx.OwnedReferenceBranch).ElementAt(0)));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_collection_Select_entity_in_anonymous_object_ElementAt(bool async)
        => await AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().AsNoTracking().OrderBy(x => x.Id).Select(x =>
                x.OwnedCollectionRoot.Select(xx => new { xx.OwnedReferenceBranch }).ElementAt(0)),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.OwnedReferenceBranch, a.OwnedReferenceBranch);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_collection_Select_entity_with_initializer_ElementAt(bool async)
        => await AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .Select(x => x.OwnedCollectionRoot.Select(xx => new JsonEntityBasic { Id = x.Id }).ElementAt(0)));

    #region 33046

    [ConditionalFact]
    public virtual async Task Query_with_nested_json_collection_mapped_to_private_field_via_IReadOnlyList()
    {
        var contextFactory = await InitializeNonSharedTest<Context33046>(
            onModelCreating: OnModelCreating33046,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: Seed33046);

        using var context = contextFactory.CreateDbContext();
        var query = await context.Set<Context33046.Review>().ToListAsync();
        Assert.Equal(1, query.Count);
    }

    protected virtual void OnModelCreating33046(ModelBuilder modelBuilder)
        => modelBuilder.Entity<Context33046.Review>(b =>
        {
            b.Property(x => x.Id).ValueGeneratedNever();
            b.OwnsMany(
                x => x.Rounds, ownedBuilder =>
                {
                    ownedBuilder.OwnsMany(r => r.SubRounds);
                });
        });

    protected abstract Task Seed33046(DbContext ctx);

    protected class Context33046(DbContextOptions options) : DbContext(options)
    {
        public class Review
        {
            public int Id { get; set; }

#pragma warning disable IDE0044 // Add readonly modifier
            private List<ReviewRound> _rounds = [];
#pragma warning restore IDE0044 // Add readonly modifier
            public IReadOnlyList<ReviewRound> Rounds
                => _rounds.AsReadOnly();
        }

        public class ReviewRound
        {
            public int RoundNumber { get; set; }

#pragma warning disable IDE0044 // Add readonly modifier
            private readonly List<SubRound> _subRounds = [];
#pragma warning restore IDE0044 // Add readonly modifier
            public IReadOnlyList<SubRound> SubRounds
                => _subRounds.AsReadOnly();
        }

        public class SubRound
        {
            public int SubRoundNumber { get; set; }
        }
    }

    #endregion

    #region ArrayOfPrimitives

    [ConditionalFact]
    public virtual async Task Project_json_array_of_primitives_on_reference()
    {
        var contextFactory = await InitializeNonSharedTest<ContextArrayOfPrimitives>(
            onModelCreating: OnModelCreatingArrayOfPrimitives,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedArrayOfPrimitives);

        using var context = contextFactory.CreateDbContext();
        var query = context.Set<ContextArrayOfPrimitives.MyEntity>().OrderBy(x => x.Id)
            .Select(x => new { x.Reference.IntArray, x.Reference.ListOfString });

        var result = await query.ToListAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal(3, result[0].IntArray.Length);
        Assert.Equal(3, result[0].ListOfString.Count);
        Assert.Equal(3, result[1].IntArray.Length);
        Assert.Equal(3, result[1].ListOfString.Count);
    }

    [ConditionalFact(Skip = "Issue #32611")]
    public virtual async Task Project_json_array_of_primitives_on_collection()
    {
        var contextFactory = await InitializeNonSharedTest<ContextArrayOfPrimitives>(
            onModelCreating: OnModelCreatingArrayOfPrimitives,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedArrayOfPrimitives);

        using var context = contextFactory.CreateDbContext();
        var query = context.Set<ContextArrayOfPrimitives.MyEntity>().OrderBy(x => x.Id)
            .Select(x => new { x.Collection[0].IntArray, x.Collection[1].ListOfString });

        var result = await query.ToListAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal(3, result[0].IntArray.Length);
        Assert.Equal(2, result[0].ListOfString.Count);
        Assert.Equal(3, result[1].IntArray.Length);
        Assert.Equal(2, result[1].ListOfString.Count);
    }

    [ConditionalFact]
    public virtual async Task Project_element_of_json_array_of_primitives()
    {
        var contextFactory = await InitializeNonSharedTest<ContextArrayOfPrimitives>(
            onModelCreating: OnModelCreatingArrayOfPrimitives,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedArrayOfPrimitives);

        using var context = contextFactory.CreateDbContext();
        var query = context.Set<ContextArrayOfPrimitives.MyEntity>().OrderBy(x => x.Id).Select(x
            => new { ArrayElement = x.Reference.IntArray[0], ListElement = x.Reference.ListOfString[1] });
        var result = await query.ToListAsync();
    }

    [ConditionalFact]
    public virtual async Task Predicate_based_on_element_of_json_array_of_primitives1()
    {
        var contextFactory = await InitializeNonSharedTest<ContextArrayOfPrimitives>(
            onModelCreating: OnModelCreatingArrayOfPrimitives,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedArrayOfPrimitives);

        using var context = contextFactory.CreateDbContext();
        var query = context.Set<ContextArrayOfPrimitives.MyEntity>().Where(x => x.Reference.IntArray[0] == 1);
        var result = await query.ToListAsync();

        Assert.Equal(1, result.Count);
        Assert.Equal(1, result[0].Reference.IntArray[0]);
    }

    [ConditionalFact]
    public virtual async Task Predicate_based_on_element_of_json_array_of_primitives2()
    {
        var contextFactory = await InitializeNonSharedTest<ContextArrayOfPrimitives>(
            onModelCreating: OnModelCreatingArrayOfPrimitives,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedArrayOfPrimitives);

        using var context = contextFactory.CreateDbContext();
        var query = context.Set<ContextArrayOfPrimitives.MyEntity>().Where(x => x.Reference.ListOfString[1] == "Bar");
        var result = await query.ToListAsync();

        Assert.Equal(1, result.Count);
        Assert.Equal("Bar", result[0].Reference.ListOfString[1]);
    }

    [ConditionalFact, MemberData(nameof(IsAsyncData))]
    public virtual async Task Predicate_based_on_element_of_json_array_of_primitives3()
    {
        var contextFactory = await InitializeNonSharedTest<ContextArrayOfPrimitives>(
            onModelCreating: OnModelCreatingArrayOfPrimitives,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedArrayOfPrimitives);

        using var context = contextFactory.CreateDbContext();
        var query = context.Set<ContextArrayOfPrimitives.MyEntity>()
            .Where(x => x.Reference.IntArray.AsQueryable().ElementAt(0) == 1
                || x.Reference.ListOfString.AsQueryable().ElementAt(1) == "Bar")
            .OrderBy(e => e.Id);
        var result = await query.ToListAsync();

        Assert.Equal(1, result.Count);
        Assert.Equal(1, result[0].Reference.IntArray[0]);
        Assert.Equal("Bar", result[0].Reference.ListOfString[1]);
    }

    protected Task SeedArrayOfPrimitives(DbContext ctx)
    {
        var entity1 = new ContextArrayOfPrimitives.MyEntity
        {
            Id = 1,
            Reference = new ContextArrayOfPrimitives.MyJsonEntity
            {
                IntArray = [1, 2, 3],
                ListOfString =
                [
                    "Foo",
                    "Bar",
                    "Baz"
                ]
            },
            Collection =
            [
                new ContextArrayOfPrimitives.MyJsonEntity { IntArray = [111, 112, 113], ListOfString = ["Foo11", "Bar11"] },
                new ContextArrayOfPrimitives.MyJsonEntity { IntArray = [211, 212, 213], ListOfString = ["Foo12", "Bar12"] }
            ]
        };

        var entity2 = new ContextArrayOfPrimitives.MyEntity
        {
            Id = 2,
            Reference = new ContextArrayOfPrimitives.MyJsonEntity
            {
                IntArray = [10, 20, 30],
                ListOfString =
                [
                    "A",
                    "B",
                    "C"
                ]
            },
            Collection =
            [
                new ContextArrayOfPrimitives.MyJsonEntity { IntArray = [110, 120, 130], ListOfString = ["A1", "Z1"] },
                new ContextArrayOfPrimitives.MyJsonEntity { IntArray = [210, 220, 230], ListOfString = ["A2", "Z2"] }
            ]
        };

        ctx.Set<ContextArrayOfPrimitives.MyEntity>().AddRange(entity1, entity2);

        return ctx.SaveChangesAsync();
    }

    protected virtual void OnModelCreatingArrayOfPrimitives(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContextArrayOfPrimitives.MyEntity>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<ContextArrayOfPrimitives.MyEntity>().OwnsOne(x => x.Reference);
        modelBuilder.Entity<ContextArrayOfPrimitives.MyEntity>().OwnsMany(x => x.Collection);
    }

    protected class ContextArrayOfPrimitives(DbContextOptions options) : DbContext(options)
    {
        public class MyEntity
        {
            public int Id { get; set; }
            public MyJsonEntity Reference { get; set; }
            public List<MyJsonEntity> Collection { get; set; }
        }

        public class MyJsonEntity
        {
            public int[] IntArray { get; set; }
            public List<string> ListOfString { get; set; }
        }
    }

    #endregion

    #region NotICollection

    [ConditionalFact]
    public virtual async Task Not_ICollection_basic_projection()
    {
        var contextFactory = await InitializeNonSharedTest<ContextNotICollection>(
            onModelCreating: OnModelCreatingNotICollection,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedNotICollection);

        using var context = contextFactory.CreateDbContext();
        var query = context.Set<ContextNotICollection.MyEntity>();
        var result = await query.ToListAsync();

        Assert.Equal(2, result.Count);
    }

    protected abstract Task SeedNotICollection(DbContext ctx);

    protected virtual void OnModelCreatingNotICollection(ModelBuilder modelBuilder)
        => modelBuilder.Entity<ContextNotICollection.MyEntity>(b =>
        {
            b.Property(x => x.Id).ValueGeneratedNever();
            b.OwnsOne(
                cr => cr.Json, nb =>
                {
                    nb.OwnsMany(x => x.Collection);
                });
        });

    protected class ContextNotICollection(DbContextOptions options) : DbContext(options)
    {
        public class MyEntity
        {
            public int Id { get; set; }

            public MyJsonEntity Json { get; set; }
        }

        public class MyJsonEntity
        {
            private readonly List<MyJsonNestedEntity> _collection = [];

            public IEnumerable<MyJsonNestedEntity> Collection
                => _collection.AsReadOnly();
        }

        public class MyJsonNestedEntity
        {
            public string Foo { get; set; }
            public int Bar { get; set; }
        }
    }

    #endregion


    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_projection_deduplication_with_collection_indexer_in_original(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new
            {
                x.Id,
                Duplicate1 = x.OwnedCollectionRoot[0].OwnedReferenceBranch,
                Original = x.OwnedCollectionRoot[0],
                Duplicate2 = x.OwnedCollectionRoot[0].OwnedReferenceBranch.OwnedCollectionLeaf
            }).AsNoTracking(),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertEqual(e.Original, a.Original);
                AssertEqual(e.Duplicate1, a.Duplicate1);
                AssertCollection(e.Duplicate2, a.Duplicate2, ordered: true);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_projection_deduplication_with_collection_indexer_in_target(bool async)
    {
        var prm = 1;

        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new
            {
                x.Id,
                Duplicate1 = x.OwnedReferenceRoot.OwnedCollectionBranch[1],
                Original = x.OwnedReferenceRoot,
                Duplicate2 = x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf[prm]
            }).AsNoTracking(),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertEqual(e.Original, a.Original);
                AssertEqual(e.Duplicate1, a.Duplicate1);
                AssertEqual(e.Duplicate2, a.Duplicate2);
            });
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_projection_deduplication_with_collection_in_original_and_collection_indexer_in_target(bool async)
    {
        var prm = 1;

        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new
            {
                DuplicateMix = x.OwnedReferenceRoot.OwnedCollectionBranch[0].OwnedCollectionLeaf[prm],
                DuplicatePrm = x.OwnedReferenceRoot.OwnedCollectionBranch[prm],
                x.Id,
                Original = x.OwnedReferenceRoot.OwnedCollectionBranch,
                DuplicateConstant = x.OwnedReferenceRoot.OwnedCollectionBranch[0],
            }).AsNoTracking(),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertCollection(e.Original, a.Original, ordered: true);
                AssertEqual(e.DuplicatePrm, a.DuplicatePrm);
                AssertEqual(e.DuplicateConstant, a.DuplicateConstant);
                AssertEqual(e.DuplicateMix, a.DuplicateMix);
            });
    }

    #region 30028

    [ConditionalFact]
    public virtual async Task Accessing_missing_navigation_works()
    {
        var contextFactory = await InitializeNonSharedTest<Context30028>(
            onModelCreating: OnModelCreating30028,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: Seed30028);

        using var context = contextFactory.CreateDbContext();
        var result = await context.Set<Context30028.MyEntity>().OrderBy(x => x.Id).ToListAsync();
        Assert.Equal(4, result.Count);
        Assert.NotNull(result[0].Json.Collection);
        Assert.NotNull(result[0].Json.OptionalReference);
        Assert.NotNull(result[0].Json.RequiredReference);

        Assert.Null(result[1].Json.Collection);
        Assert.NotNull(result[1].Json.OptionalReference);
        Assert.NotNull(result[1].Json.RequiredReference);

        Assert.NotNull(result[2].Json.Collection);
        Assert.Null(result[2].Json.OptionalReference);
        Assert.NotNull(result[2].Json.RequiredReference);

        Assert.NotNull(result[3].Json.Collection);
        Assert.NotNull(result[3].Json.OptionalReference);
        Assert.Null(result[3].Json.RequiredReference);
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Missing_navigation_works_with_deduplication(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: OnModelCreating30028,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: Seed30028);

        using var context = contextFactory.CreateDbContext();
        var queryable = context.Set<Context30028.MyEntity>().OrderBy(x => x.Id).Select(x => new
        {
            x,
            x.Json,
            x.Json.OptionalReference,
            x.Json.RequiredReference,
            NestedOptional = x.Json.OptionalReference.Nested,
            NestedRequired = x.Json.RequiredReference.Nested,
            x.Json.Collection,
        }).AsNoTracking();

        var result = async ? await queryable.ToListAsync() : queryable.ToList();

        Assert.Equal(4, result.Count);
        Assert.NotNull(result[0].OptionalReference);
        Assert.NotNull(result[0].RequiredReference);
        Assert.NotNull(result[0].NestedOptional);
        Assert.NotNull(result[0].NestedRequired);
        Assert.NotNull(result[0].Collection);

        Assert.NotNull(result[1].OptionalReference);
        Assert.NotNull(result[1].RequiredReference);
        Assert.NotNull(result[1].NestedOptional);
        Assert.NotNull(result[1].NestedRequired);
        Assert.Null(result[1].Collection);

        Assert.Null(result[2].OptionalReference);
        Assert.NotNull(result[2].RequiredReference);
        Assert.Null(result[2].NestedOptional);
        Assert.NotNull(result[2].NestedRequired);
        Assert.NotNull(result[2].Collection);

        Assert.NotNull(result[3].OptionalReference);
        Assert.Null(result[3].RequiredReference);
        Assert.NotNull(result[3].NestedOptional);
        Assert.Null(result[3].NestedRequired);
        Assert.NotNull(result[3].Collection);
    }

    protected virtual void OnModelCreating30028(ModelBuilder modelBuilder)
        => modelBuilder.Entity<Context30028.MyEntity>(b =>
        {
            b.Property(x => x.Id).ValueGeneratedNever();
            b.OwnsOne(
                x => x.Json, nb =>
                {
                    nb.OwnsMany(x => x.Collection, nnb => nnb.OwnsOne(x => x.Nested));
                    nb.OwnsOne(x => x.OptionalReference, nnb => nnb.OwnsOne(x => x.Nested));
                    nb.OwnsOne(x => x.RequiredReference, nnb => nnb.OwnsOne(x => x.Nested));
                    nb.Navigation(x => x.RequiredReference).IsRequired();
                });
        });

    protected abstract Task Seed30028(DbContext ctx);

    protected class Context30028(DbContextOptions options) : DbContext(options)
    {
        public class MyEntity
        {
            public int Id { get; set; }
            public MyJsonRootEntity Json { get; set; }
        }

        public class MyJsonRootEntity
        {
            public string RootName { get; set; }
            public MyJsonBranchEntity RequiredReference { get; set; }
            public MyJsonBranchEntity OptionalReference { get; set; }
            public List<MyJsonBranchEntity> Collection { get; set; }
        }

        public class MyJsonBranchEntity
        {
            public string BranchName { get; set; }
            public MyJsonLeafEntity Nested { get; set; }
        }

        public class MyJsonLeafEntity
        {
            public string LeafName { get; set; }
        }
    }

    #endregion


    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_projection_using_constant_when_owner_is_present(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new { x, CollectionElement = x.OwnedCollectionRoot[1] }).AsNoTracking(),
            elementSorter: e => e.x.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.x, a.x);
                AssertEqual(e.CollectionElement, a.CollectionElement);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_projection_using_constant_when_owner_is_not_present(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new { x.Id, CollectionElement = x.OwnedCollectionRoot[1] }).AsNoTracking(),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertEqual(e.CollectionElement, a.CollectionElement);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_projection_using_parameter_when_owner_is_present(bool async)
    {
        var prm = 1;

        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new { x, CollectionElement = x.OwnedCollectionRoot[prm] }).AsNoTracking(),
            elementSorter: e => e.x.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.x, a.x);
                AssertEqual(e.CollectionElement, a.CollectionElement);
            });
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_projection_using_parameter_when_owner_is_not_present(bool async)
    {
        var prm = 1;

        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new { x.Id, CollectionElement = x.OwnedCollectionRoot[prm] }).AsNoTracking(),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertEqual(e.CollectionElement, a.CollectionElement);
            });
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_after_collection_index_in_projection_using_constant_when_owner_is_present(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new { x, Collection = x.OwnedCollectionRoot[1].OwnedCollectionBranch })
                .AsNoTracking(),
            elementSorter: e => e.x.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.x, a.x);
                AssertCollection(e.Collection, a.Collection, ordered: true);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_after_collection_index_in_projection_using_constant_when_owner_is_not_present(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new { x.Id, Collection = x.OwnedCollectionRoot[1].OwnedCollectionBranch })
                .AsNoTracking(),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertCollection(e.Collection, a.Collection, ordered: true);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_after_collection_index_in_projection_using_parameter_when_owner_is_present(bool async)
    {
        var prm = 1;

        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new { x, Collection = x.OwnedCollectionRoot[prm].OwnedCollectionBranch })
                .AsNoTracking(),
            elementSorter: e => e.x.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.x, a.x);
                AssertCollection(e.Collection, a.Collection, ordered: true);
            });
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_after_collection_index_in_projection_using_parameter_when_owner_is_not_present(bool async)
    {
        var prm = 1;

        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new { x.Id, Collection = x.OwnedCollectionRoot[prm].OwnedCollectionBranch })
                .AsNoTracking(),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                AssertCollection(e.Collection, a.Collection, ordered: true);
            });
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_projection_when_owner_is_present_misc1(bool async)
    {
        var prm = 1;

        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new
            {
                x, CollectionElement = x.OwnedCollectionRoot[1].OwnedCollectionBranch[prm],
            }).AsNoTracking(),
            elementSorter: e => e.x.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.x, a.x);
                AssertEqual(e.CollectionElement, a.CollectionElement);
            });
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_projection_when_owner_is_not_present_misc1(bool async)
    {
        var prm = 1;

        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new
            {
                x.Id, CollectionElement = x.OwnedCollectionRoot[1].OwnedCollectionBranch[prm],
            }).AsNoTracking(),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertEqual(e.CollectionElement, a.CollectionElement);
            });
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_projection_when_owner_is_present_misc2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .Select(x => new { x, CollectionElement = x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf[1] })
                .AsNoTracking(),
            elementSorter: e => e.x.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.x, a.x);
                AssertEqual(e.CollectionElement, a.CollectionElement);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_projection_when_owner_is_not_present_misc2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .Select(x => new { x.Id, CollectionElement = x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf[1] })
                .AsNoTracking(),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertEqual(e.CollectionElement, a.CollectionElement);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_projection_when_owner_is_present_multiple(bool async)
    {
        var prm = 1;

        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new
            {
                x,
                CollectionElement1 = x.OwnedCollectionRoot[prm].OwnedCollectionBranch[1],
                CollectionElement2 = x.OwnedCollectionRoot[1].OwnedCollectionBranch[1].OwnedReferenceLeaf,
                CollectionElement3 = x.OwnedCollectionRoot[1].OwnedReferenceBranch,
                CollectionElement4 = x.OwnedCollectionRoot[prm].OwnedReferenceBranch,
                CollectionElement5 = x.OwnedCollectionRoot[prm].OwnedCollectionBranch[x.Id],
                CollectionElement6 = x.OwnedCollectionRoot[x.Id].OwnedCollectionBranch[1].OwnedReferenceLeaf,
                CollectionElement7 = x.OwnedCollectionRoot[1].OwnedReferenceBranch,
                CollectionElement8 = x.OwnedCollectionRoot[x.Id].OwnedReferenceBranch,
                CollectionElement9 = x.OwnedCollectionRoot[x.Id].OwnedCollectionBranch[x.Id],
            }).AsNoTracking(),
            elementSorter: e => e.x.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.x, a.x);
                AssertEqual(e.CollectionElement1, a.CollectionElement1);
                AssertEqual(e.CollectionElement2, a.CollectionElement2);
                AssertEqual(e.CollectionElement3, a.CollectionElement3);
                AssertEqual(e.CollectionElement4, a.CollectionElement4);
                AssertEqual(e.CollectionElement5, a.CollectionElement5);
                AssertEqual(e.CollectionElement6, a.CollectionElement6);
                AssertEqual(e.CollectionElement7, a.CollectionElement7);
                AssertEqual(e.CollectionElement8, a.CollectionElement8);
                AssertEqual(e.CollectionElement9, a.CollectionElement9);
            });
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_projection_when_owner_is_not_present_multiple(bool async)
    {
        var prm = 1;

        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new
            {
                x.Id,
                CollectionElement1 = x.OwnedCollectionRoot[prm].OwnedCollectionBranch[1],
                CollectionElement2 = x.OwnedCollectionRoot[1].OwnedCollectionBranch[1].OwnedReferenceLeaf,
                CollectionElement3 = x.OwnedCollectionRoot[1].OwnedReferenceBranch,
                CollectionElement4 = x.OwnedCollectionRoot[prm].OwnedReferenceBranch,
                CollectionElement5 = x.OwnedCollectionRoot[prm].OwnedCollectionBranch[x.Id],
                CollectionElement6 = x.OwnedCollectionRoot[x.Id].OwnedCollectionBranch[1].OwnedReferenceLeaf,
                CollectionElement7 = x.OwnedCollectionRoot[1].OwnedReferenceBranch,
                CollectionElement8 = x.OwnedCollectionRoot[x.Id].OwnedReferenceBranch,
                CollectionElement9 = x.OwnedCollectionRoot[x.Id].OwnedCollectionBranch[x.Id],
            }).AsNoTracking(),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertEqual(e.CollectionElement1, a.CollectionElement1);
                AssertEqual(e.CollectionElement2, a.CollectionElement2);
                AssertEqual(e.CollectionElement3, a.CollectionElement3);
                AssertEqual(e.CollectionElement4, a.CollectionElement4);
                AssertEqual(e.CollectionElement5, a.CollectionElement5);
                AssertEqual(e.CollectionElement6, a.CollectionElement6);
                AssertEqual(e.CollectionElement7, a.CollectionElement7);
                AssertEqual(e.CollectionElement8, a.CollectionElement8);
                AssertEqual(e.CollectionElement9, a.CollectionElement9);
            });
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_scalar_required_null_semantics(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .Where(x => x.OwnedReferenceRoot.Number != x.OwnedReferenceRoot.Name.Length)
                .Select(x => x.Name));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_scalar_optional_null_semantics(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .Where(x => x.OwnedReferenceRoot.Name != x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf.SomethingSomething)
                .Select(x => x.Name));

    #region 29219

    [ConditionalFact]
    public virtual async Task Optional_json_properties_materialized_as_null_when_the_element_in_json_is_not_present()
    {
        var contextFactory = await InitializeNonSharedTest<Context29219>(
            onModelCreating: OnModelCreating29219,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: Seed29219);

        using var context = contextFactory.CreateDbContext();
        var query = context.Set<Context29219.MyEntity>().Where(x => x.Id == 3);
        var result = await query.SingleAsync();

        Assert.Equal(3, result.Id);
        Assert.Null(result.Reference.NullableScalar);
        Assert.Null(result.Collection[0].NullableScalar);
    }

    [ConditionalFact]
    public virtual async Task Can_project_nullable_json_property_when_the_element_in_json_is_not_present()
    {
        var contextFactory = await InitializeNonSharedTest<Context29219>(
            onModelCreating: OnModelCreating29219,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: Seed29219);

        using var context = contextFactory.CreateDbContext();

        var query = context.Set<Context29219.MyEntity>().OrderBy(x => x.Id).Select(x => x.Reference.NullableScalar);
        var result = await query.ToListAsync();

        Assert.Equal(3, result.Count);
        Assert.Equal(11, result[0]);
        Assert.Null(result[1]);
        Assert.Null(result[2]);
    }

    protected virtual void OnModelCreating29219(ModelBuilder modelBuilder)
        => modelBuilder.Entity<Context29219.MyEntity>(b =>
        {
            b.Property(x => x.Id).ValueGeneratedNever();
            b.OwnsOne(x => x.Reference);
            b.OwnsMany(x => x.Collection);
        });

    protected virtual async Task Seed29219(DbContext ctx)
    {
        var entity1 = new Context29219.MyEntity
        {
            Id = 1,
            Reference = new Context29219.MyJsonEntity { NonNullableScalar = 10, NullableScalar = 11 },
            Collection =
            [
                new Context29219.MyJsonEntity { NonNullableScalar = 100, NullableScalar = 101 },
                new Context29219.MyJsonEntity { NonNullableScalar = 200, NullableScalar = 201 },
                new Context29219.MyJsonEntity { NonNullableScalar = 300, NullableScalar = null }
            ]
        };

        var entity2 = new Context29219.MyEntity
        {
            Id = 2,
            Reference = new Context29219.MyJsonEntity { NonNullableScalar = 20, NullableScalar = null },
            Collection = [new Context29219.MyJsonEntity { NonNullableScalar = 1001, NullableScalar = null }]
        };

        ctx.AddRange(entity1, entity2);
        await ctx.SaveChangesAsync();
    }

    protected class Context29219(DbContextOptions options) : DbContext(options)
    {
        public class MyEntity
        {
            public int Id { get; set; }
            public MyJsonEntity Reference { get; set; }
            public List<MyJsonEntity> Collection { get; set; }
        }

        public class MyJsonEntity
        {
            public int NonNullableScalar { get; set; }
            public int? NullableScalar { get; set; }
        }
    }

    #endregion


    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Group_by_on_json_scalar(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .GroupBy(x => x.OwnedReferenceRoot.Name).Select(x => new { x.Key, Count = x.Count() }));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Group_by_on_json_scalar_using_collection_indexer(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .GroupBy(x => x.OwnedCollectionRoot[0].Name).Select(x => new { x.Key, Count = x.Count() }));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Group_by_First_on_json_scalar(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .GroupBy(x => x.OwnedReferenceRoot.Name).Select(g => g.OrderBy(x => x.Id).First()));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Group_by_FirstOrDefault_on_json_scalar(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .GroupBy(x => x.OwnedReferenceRoot.Name).Select(g => g.OrderBy(x => x.Id).FirstOrDefault()));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Group_by_Skip_Take_on_json_scalar(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .GroupBy(x => x.OwnedReferenceRoot.Name).Select(g => g.OrderBy(x => x.Id).Skip(1).Take(5)));

    [ConditionalTheory(Skip = "issue #29287"), MemberData(nameof(IsAsyncData))]
    public virtual Task Group_by_json_scalar_Orderby_json_scalar_FirstOrDefault(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .GroupBy(x => x.OwnedReferenceRoot.OwnedReferenceBranch.Enum)
                .Select(g => g.OrderBy(x => x.OwnedReferenceRoot.Number).FirstOrDefault()));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Group_by_json_scalar_Skip_First_project_json_scalar(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .GroupBy(x => x.OwnedReferenceRoot.Name).Select(g => g.First().OwnedReferenceRoot.OwnedReferenceBranch.Enum));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_with_include_on_json_entity(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Include(x => x.OwnedReferenceRoot));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_with_include_on_entity_reference(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Include(x => x.EntityReference),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<JsonEntityBasic>(x => x.EntityReference)));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_with_include_on_entity_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Include(x => x.EntityCollection),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<JsonEntityBasic>(x => x.EntityCollection)));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Entity_including_collection_with_json(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityBasic>().Include(e => e.JsonEntityBasics),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<EntityBasic>(x => x.JsonEntityBasics)));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_with_include_on_entity_collection_and_reference(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Include(x => x.EntityReference).Include(x => x.EntityCollection),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<JsonEntityBasic>(x => x.EntityReference),
                new ExpectedInclude<JsonEntityBasic>(x => x.EntityCollection)));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_with_projection_of_json_reference_leaf_and_entity_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .Select(x => new { x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf, x.EntityCollection }).AsNoTracking(),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.OwnedReferenceLeaf, a.OwnedReferenceLeaf);
                AssertCollection(e.EntityCollection, a.EntityCollection);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_with_projection_of_json_reference_and_entity_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new { x.OwnedReferenceRoot, x.EntityCollection }).AsNoTracking(),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.OwnedReferenceRoot, a.OwnedReferenceRoot);
                AssertCollection(e.EntityCollection, a.EntityCollection);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_with_projection_of_multiple_json_references_and_entity_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new
            {
                Reference1 = x.OwnedReferenceRoot,
                Reference2 = x.OwnedCollectionRoot[0].OwnedReferenceBranch,
                x.EntityCollection,
                Reference3 = x.OwnedCollectionRoot[1].OwnedReferenceBranch.OwnedReferenceLeaf,
                Reference4 = x.OwnedCollectionRoot[0].OwnedCollectionBranch[0].OwnedReferenceLeaf,
            }).AsNoTracking(),
            elementAsserter: (e, a) =>
            {
                AssertCollection(e.EntityCollection, a.EntityCollection);
                AssertEqual(e.Reference1, a.Reference1);
                AssertEqual(e.Reference2, a.Reference2);
                AssertEqual(e.Reference3, a.Reference3);
                AssertEqual(e.Reference4, a.Reference4);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_with_projection_of_json_collection_leaf_and_entity_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .Select(x => new { x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf, x.EntityCollection }).AsNoTracking(),
            elementAsserter: (e, a) =>
            {
                AssertCollection(e.OwnedCollectionLeaf, a.OwnedCollectionLeaf, ordered: true);
                AssertCollection(e.EntityCollection, a.EntityCollection);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_with_projection_of_json_collection_and_entity_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new { x.OwnedCollectionRoot, x.EntityCollection }).AsNoTracking(),
            elementAsserter: (e, a) =>
            {
                AssertCollection(e.OwnedCollectionRoot, a.OwnedCollectionRoot, ordered: true);
                AssertCollection(e.EntityCollection, a.EntityCollection);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_with_projection_of_json_collection_element_and_entity_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new
            {
                JsonCollectionElement = x.OwnedCollectionRoot[0],
                x.EntityReference,
                x.EntityCollection
            }).AsNoTracking(),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.JsonCollectionElement, a.JsonCollectionElement);
                AssertEqual(e.EntityReference, a.EntityReference);
                AssertCollection(e.EntityCollection, a.EntityCollection);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_with_projection_of_mix_of_json_collections_json_references_and_entity_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new
            {
                Collection1 = x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf,
                x.EntityReference,
                Reference1 = x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf,
                x.EntityCollection,
                Reference2 = x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf[0],
                Collection2 = x.OwnedReferenceRoot.OwnedCollectionBranch,
                Collection3 = x.OwnedCollectionRoot,
                Reference3 = x.OwnedCollectionRoot[0].OwnedReferenceBranch,
                Collection4 = x.OwnedCollectionRoot[0].OwnedCollectionBranch
            }).AsNoTracking(),
            elementAsserter: (e, a) =>
            {
                AssertCollection(e.Collection1, a.Collection1, ordered: true);
                AssertCollection(e.Collection2, a.Collection2, ordered: true);
                AssertCollection(e.Collection3, a.Collection3, ordered: true);
                AssertCollection(e.Collection4, a.Collection4, ordered: true);
                AssertCollection(e.Collection1, a.Collection1, ordered: true);
                AssertEqual(e.Reference1, a.Reference1);
                AssertEqual(e.Reference2, a.Reference2);
                AssertEqual(e.Reference3, a.Reference3);
                AssertEqual(e.EntityReference, a.EntityReference);
                AssertCollection(e.EntityCollection, a.EntityCollection);
            });

    #region LazyLoadingProxies

    [ConditionalFact]
    public virtual async Task Project_proxies_entity_with_json()
    {
        var contextFactory = await InitializeNonSharedTest<ContextLazyLoadingProxies>(
            onModelCreating: OnModelCreatingLazyLoadingProxies,
            seed: SeedLazyLoadingProxies,
            onConfiguring: b =>
            {
                b = b.ConfigureWarnings(ConfigureWarnings);
                OnConfiguringLazyLoadingProxies(b);
            },
            addServices: AddServicesLazyLoadingProxies);

        using var context = contextFactory.CreateDbContext();
        var query = context.Set<ContextLazyLoadingProxies.MyEntity>();
        var result = await query.ToListAsync();

        Assert.Equal(2, result.Count);
    }

    protected void OnConfiguringLazyLoadingProxies(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseLazyLoadingProxies();

    protected IServiceCollection AddServicesLazyLoadingProxies(IServiceCollection addServices)
        => addServices.AddEntityFrameworkProxies();

    private Task SeedLazyLoadingProxies(DbContext ctx)
    {
        var r1 = new ContextLazyLoadingProxies.MyJsonEntityWithCtor("r1", 1);
        var c11 = new ContextLazyLoadingProxies.MyJsonEntity { Name = "c11", Number = 11 };
        var c12 = new ContextLazyLoadingProxies.MyJsonEntity { Name = "c12", Number = 12 };
        var c13 = new ContextLazyLoadingProxies.MyJsonEntity { Name = "c13", Number = 13 };

        var r2 = new ContextLazyLoadingProxies.MyJsonEntityWithCtor("r2", 2);
        var c21 = new ContextLazyLoadingProxies.MyJsonEntity { Name = "c21", Number = 21 };
        var c22 = new ContextLazyLoadingProxies.MyJsonEntity { Name = "c22", Number = 22 };

        var e1 = new ContextLazyLoadingProxies.MyEntity
        {
            Id = 1,
            Name = "e1",
            Reference = r1,
            Collection =
            [
                c11,
                c12,
                c13
            ]
        };

        var e2 = new ContextLazyLoadingProxies.MyEntity
        {
            Id = 2,
            Name = "e2",
            Reference = r2,
            Collection = [c21, c22]
        };

        ctx.Set<ContextLazyLoadingProxies.MyEntity>().AddRange(e1, e2);
        return ctx.SaveChangesAsync();
    }

    protected virtual void OnModelCreatingLazyLoadingProxies(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContextLazyLoadingProxies.MyEntity>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<ContextLazyLoadingProxies.MyEntity>().OwnsOne(x => x.Reference);
        modelBuilder.Entity<ContextLazyLoadingProxies.MyEntity>().OwnsMany(x => x.Collection);
    }

    public class ContextLazyLoadingProxies(DbContextOptions options) : DbContext(options)
    {
        public class MyEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public virtual MyJsonEntityWithCtor Reference { get; set; }
            public virtual List<MyJsonEntity> Collection { get; set; }
        }

        public class MyJsonEntityWithCtor(string name, int number)
        {
            public string Name { get; set; } = name;
            public int Number { get; set; } = number;
        }

        public class MyJsonEntity
        {
            public string Name { get; set; }
            public int Number { get; set; }
        }
    }

    #endregion


    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_all_types_entity_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>());

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_all_types_projection_from_owned_entity_reference(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Select(x => x.Reference).AsNoTracking(),
            elementSorter: e => e.TestInt32,
            elementAsserter: (e, a) => AssertEqual(e, a));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_all_types_projection_individual_properties(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Select(x => new
            {
                x.Reference.TestDefaultString,
                x.Reference.TestMaxLengthString,
                x.Reference.TestBoolean,
                x.Reference.TestByte,
                x.Reference.TestCharacter,
                x.Reference.TestDateTime,
                x.Reference.TestDateTimeOffset,
                x.Reference.TestDecimal,
                x.Reference.TestDouble,
                x.Reference.TestGuid,
                x.Reference.TestInt16,
                x.Reference.TestInt32,
                x.Reference.TestInt64,
                x.Reference.TestSignedByte,
                x.Reference.TestSingle,
                x.Reference.TestTimeSpan,
                x.Reference.TestDateOnly,
                x.Reference.TestTimeOnly,
                x.Reference.TestUnsignedInt16,
                x.Reference.TestUnsignedInt32,
                x.Reference.TestUnsignedInt64,
                x.Reference.TestEnum,
                x.Reference.TestEnumWithIntConverter,
                x.Reference.TestNullableEnum,
                x.Reference.TestNullableEnumWithIntConverter,
                x.Reference.TestNullableEnumWithConverterThatHandlesNulls,
            }));

    #region ShadowProperties

    [ConditionalFact]
    public virtual async Task Shadow_properties_basic_tracking()
    {
        var contextFactory = await InitializeNonSharedTest<ContextShadowProperties>(
            onModelCreating: OnModelCreatingShadowProperties,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedShadowProperties);

        using var context = contextFactory.CreateDbContext();
        var query = context.Set<ContextShadowProperties.MyEntity>();
        var result = await query.ToListAsync();

        Assert.Equal(1, result.Count);
        Assert.Equal(2, result[0].Collection.Count);
        Assert.Equal(2, result[0].CollectionWithCtor.Count);
        Assert.NotNull(result[0].Reference);
        Assert.NotNull(result[0].ReferenceWithCtor);

        var referenceEntry = context.ChangeTracker.Entries().Single(x => x.Entity == result[0].Reference);
        Assert.Equal("Foo", referenceEntry.Property("ShadowString").CurrentValue);

        var referenceCtorEntry = context.ChangeTracker.Entries().Single(x => x.Entity == result[0].ReferenceWithCtor);
        Assert.Equal(143, referenceCtorEntry.Property("Shadow_Int").CurrentValue);

        var collectionEntry1 = context.ChangeTracker.Entries().Single(x => x.Entity == result[0].Collection[0]);
        var collectionEntry2 = context.ChangeTracker.Entries().Single(x => x.Entity == result[0].Collection[1]);
        Assert.Equal(5.5, collectionEntry1.Property("ShadowDouble").CurrentValue);
        Assert.Equal(20.5, collectionEntry2.Property("ShadowDouble").CurrentValue);

        var collectionCtorEntry1 = context.ChangeTracker.Entries().Single(x => x.Entity == result[0].CollectionWithCtor[0]);
        var collectionCtorEntry2 = context.ChangeTracker.Entries().Single(x => x.Entity == result[0].CollectionWithCtor[1]);
        Assert.Equal((byte)6, collectionCtorEntry1.Property("ShadowNullableByte").CurrentValue);
        Assert.Null(collectionCtorEntry2.Property("ShadowNullableByte").CurrentValue);
    }

    [ConditionalFact]
    public virtual async Task Shadow_properties_basic_no_tracking()
    {
        var contextFactory = await InitializeNonSharedTest<ContextShadowProperties>(
            onModelCreating: OnModelCreatingShadowProperties,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedShadowProperties);

        using var context = contextFactory.CreateDbContext();
        var query = context.Set<ContextShadowProperties.MyEntity>().AsNoTracking();
        var result = await query.ToListAsync();

        Assert.Equal(1, result.Count);
        Assert.Equal(2, result[0].Collection.Count);
        Assert.Equal(2, result[0].CollectionWithCtor.Count);
        Assert.NotNull(result[0].Reference);
        Assert.NotNull(result[0].ReferenceWithCtor);
    }

    [ConditionalFact]
    public virtual async Task Project_shadow_properties_from_json_entity()
    {
        var contextFactory = await InitializeNonSharedTest<ContextShadowProperties>(
            onModelCreating: OnModelCreatingShadowProperties,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedShadowProperties);

        using var context = contextFactory.CreateDbContext();
        var query = context.Set<ContextShadowProperties.MyEntity>().Select(x => new
        {
            ShadowString = EF.Property<string>(x.Reference, "ShadowString"),
            ShadowInt = EF.Property<int>(x.ReferenceWithCtor, "Shadow_Int"),
        });

        var result = await query.ToListAsync();

        Assert.Equal(1, result.Count);
        Assert.Equal("Foo", result[0].ShadowString);
        Assert.Equal(143, result[0].ShadowInt);
    }

    protected abstract Task SeedShadowProperties(DbContext ctx);

    protected virtual void OnModelCreatingShadowProperties(ModelBuilder modelBuilder)
        => modelBuilder.Entity<ContextShadowProperties.MyEntity>(b =>
        {
            b.Property(x => x.Id).ValueGeneratedNever();

            b.OwnsOne(
                x => x.Reference, b =>
                {
                    b.Property<string>("ShadowString");
                });

            b.OwnsOne(
                x => x.ReferenceWithCtor, b =>
                {
                    b.Property<int>("Shadow_Int");
                });

            b.OwnsMany(
                x => x.Collection, b =>
                {
                    b.Property<double>("ShadowDouble");
                });

            b.OwnsMany(
                x => x.CollectionWithCtor, b =>
                {
                    b.Property<byte?>("ShadowNullableByte");
                });
        });

    protected class ContextShadowProperties(DbContextOptions options) : DbContext(options)
    {
        public class MyEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public MyJsonEntity Reference { get; set; }
            public List<MyJsonEntity> Collection { get; set; }
            public MyJsonEntityWithCtor ReferenceWithCtor { get; set; }
            public List<MyJsonEntityWithCtor> CollectionWithCtor { get; set; }
        }

        public class MyJsonEntity
        {
            public string Name { get; set; }
        }

        public class MyJsonEntityWithCtor(string name)
        {
            public string Name { get; set; } = name;
        }
    }

    #endregion


    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_boolean_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestBoolean));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_boolean_predicate_negated(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => !x.Reference.TestBoolean),
            assertEmpty: true);

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_boolean_projection(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Select(x => x.Reference.TestBoolean));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_boolean_projection_negated(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Select(x => !x.Reference.TestBoolean));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_default_string(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestDefaultString != "MyDefaultStringInReference1"));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_max_length_string(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestMaxLengthString != "Foo"));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_string_condition(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x
                => (!x.Reference.TestBoolean ? x.Reference.TestMaxLengthString : x.Reference.TestDefaultString)
                == "MyDefaultStringInReference1"));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_byte(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestByte != 3));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_byte_array(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestByteArray != new byte[] { 1, 2, 3 }),
            ss => ss.Set<JsonEntityAllTypes>().Where(x => !x.Reference.TestByteArray.SequenceEqual(new byte[] { 1, 2, 3 })));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_character(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestCharacter != 'z'));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_datetime(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestDateTime != new DateTime(2000, 1, 3)));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_datetimeoffset(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x
                => x.Reference.TestDateTimeOffset != new DateTimeOffset(new DateTime(2000, 1, 4), new TimeSpan(3, 2, 0))));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_decimal(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestDecimal != 1.35M));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_double(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestDouble != 33.25));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_guid(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestGuid != new Guid()));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_int16(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestInt16 != 3));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_int32(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestInt32 != 33));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_int64(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestInt64 != 333));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_signedbyte(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestSignedByte != 100));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_single(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestSingle != 10.4f));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_timespan(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestTimeSpan != new TimeSpan(3, 2, 0)));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_dateonly(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestDateOnly != new DateOnly(3, 2, 1)));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_timeonly(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestTimeOnly != new TimeOnly(3, 2, 0)));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_unisgnedint16(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestUnsignedInt16 != 100));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_unsignedint32(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestUnsignedInt32 != 1000));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_unsignedint64(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestUnsignedInt64 != 10000));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_enum(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestEnum != JsonEnum.Two));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_enumwithintconverter(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestEnumWithIntConverter != JsonEnum.Three));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_nullableenum1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestNullableEnum != JsonEnum.One));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_nullableenum2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestNullableEnum != null));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_nullableenumwithconverterthathandlesnulls1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestNullableEnumWithConverterThatHandlesNulls != JsonEnum.One));

    [ConditionalTheory(Skip = "issue #29416"), MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_nullableenumwithconverterthathandlesnulls2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestNullableEnumWithConverterThatHandlesNulls != null));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_nullableenumwithconverter1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestNullableEnumWithIntConverter != JsonEnum.Two));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_nullableenumwithconverter2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestNullableEnumWithIntConverter != null));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_nullableint321(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestNullableInt32 != 100));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_nullableint322(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestNullableInt32 != null));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_bool_converted_to_int_zero_one(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityConverters>().Where(x => x.Reference.BoolConvertedToIntZeroOne));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_bool_converted_to_int_zero_one_with_explicit_comparison(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityConverters>().Where(x => x.Reference.BoolConvertedToIntZeroOne == false));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_bool_converted_to_string_True_False(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityConverters>().Where(x => x.Reference.BoolConvertedToStringTrueFalse));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_bool_converted_to_string_True_False_with_explicit_comparison(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityConverters>().Where(x => x.Reference.BoolConvertedToStringTrueFalse == true));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_bool_converted_to_string_Y_N(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityConverters>().Where(x => x.Reference.BoolConvertedToStringYN));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_bool_converted_to_string_Y_N_with_explicit_comparison(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityConverters>().Where(x => x.Reference.BoolConvertedToStringYN == false));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_int_zero_one_converted_to_bool(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityConverters>().Where(x => x.Reference.IntZeroOneConvertedToBool == 1));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_string_True_False_converted_to_bool(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityConverters>().Where(x => x.Reference.StringTrueFalseConvertedToBool == "False"));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_string_Y_N_converted_to_bool(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityConverters>().Where(x => x.Reference.StringYNConvertedToBool == "N"));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_projection_collection_element_and_reference_AsNoTrackingWithIdentityResolution(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new
            {
                x.Id,
                CollectionElement = x.OwnedReferenceRoot.OwnedCollectionBranch[1],
                Reference = x.OwnedReferenceRoot.OwnedReferenceBranch,
            }).AsNoTrackingWithIdentityResolution(),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertEqual(e.CollectionElement, a.CollectionElement);
                AssertEqual(e.Reference, a.Reference);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_projection_nothing_interesting_AsNoTrackingWithIdentityResolution(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new { x.Id, x.Name }).AsNoTrackingWithIdentityResolution(),
            elementSorter: e => e.Id);

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_projection_owner_entity_AsNoTrackingWithIdentityResolution(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new { x.Id, x }).AsNoTrackingWithIdentityResolution(),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertEqual(e.x, a.x);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_nested_collection_anonymous_projection_of_primitives_in_projection_NoTrackingWithIdentityResolution(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => x.OwnedCollectionRoot
                    .Select(xx => xx.OwnedCollectionBranch.Select(xxx => new
                    {
                        xxx.Date,
                        xxx.Enum,
                        xxx.Enums,
                        xxx.Fraction,
                    }).ToList()))
                .AsNoTrackingWithIdentityResolution(),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(
                e, a, ordered: true, elementAsserter: (ee, aa) => AssertCollection(
                    ee, aa, ordered: true, elementAsserter: (eee, aaa) =>
                    {
                        AssertEqual(eee.Date, aaa.Date);
                        AssertEqual(eee.Enum, aaa.Enum);
                        AssertCollection(eee.Enums, aaa.Enums, ordered: true);
                        AssertEqual(eee.Fraction, aaa.Fraction);
                    })));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task
        Json_projection_second_element_through_collection_element_constant_projected_after_owner_nested_AsNoTrackingWithIdentityResolution(
            bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new
            {
                x.Id,
                Original = x.OwnedReferenceRoot.OwnedCollectionBranch[0].OwnedCollectionLeaf,
                Duplicate = x.OwnedReferenceRoot.OwnedCollectionBranch[0].OwnedCollectionLeaf[1],
            }).AsNoTrackingWithIdentityResolution(),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertCollection(e.Original, a.Original, ordered: true);
                AssertEqual(e.Duplicate, a.Duplicate);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_projection_reference_collection_and_collection_element_nested_AsNoTrackingWithIdentityResolution(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new
            {
                x.Id,
                Reference = x.OwnedReferenceRoot.OwnedCollectionBranch[0].OwnedReferenceLeaf,
                Collection = x.OwnedReferenceRoot.OwnedCollectionBranch[0].OwnedCollectionLeaf,
                CollectionElement = x.OwnedReferenceRoot.OwnedCollectionBranch[0].OwnedCollectionLeaf[1],
            }).AsNoTrackingWithIdentityResolution(),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertEqual(e.Reference, a.Reference);
                AssertCollection(e.Collection, a.Collection, ordered: true);
                AssertEqual(e.CollectionElement, a.CollectionElement);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task
        Json_projection_second_element_through_collection_element_parameter_correctly_projected_after_owner_nested_AsNoTrackingWithIdentityResolution(
            bool async)
    {
        var prm = 1;

        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new
            {
                x.Id,
                Original = x.OwnedReferenceRoot.OwnedCollectionBranch[0].OwnedCollectionLeaf,
                Duplicate = x.OwnedReferenceRoot.OwnedCollectionBranch[0].OwnedCollectionLeaf[prm],
            }).AsNoTrackingWithIdentityResolution(),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertCollection(e.Original, a.Original, ordered: true);
                AssertEqual(e.Duplicate, a.Duplicate);
            });
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task
        Json_projection_only_second_element_through_collection_element_constant_projected_nested_AsNoTrackingWithIdentityResolution(
            bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new
            {
                x.Id, Element = x.OwnedReferenceRoot.OwnedCollectionBranch[0].OwnedCollectionLeaf[1],
            }).AsNoTrackingWithIdentityResolution(),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertEqual(e.Element, a.Element);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task
        Json_projection_only_second_element_through_collection_element_parameter_projected_nested_AsNoTrackingWithIdentityResolution(
            bool async)
    {
        var prm1 = 0;
        var prm2 = 1;

        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new
            {
                x.Id, Element = x.OwnedReferenceRoot.OwnedCollectionBranch[prm1].OwnedCollectionLeaf[prm2],
            }).AsNoTrackingWithIdentityResolution(),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertEqual(e.Element, a.Element);
            });
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task
        Json_projection_second_element_through_collection_element_constant_different_values_projected_before_owner_nested_AsNoTrackingWithIdentityResolution(
            bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new
            {
                x.Id,
                Duplicate = x.OwnedReferenceRoot.OwnedCollectionBranch[0].OwnedCollectionLeaf[1],
                Original = x.OwnedReferenceRoot.OwnedCollectionBranch[1].OwnedCollectionLeaf,
            }).AsNoTrackingWithIdentityResolution(),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertEqual(e.Duplicate, a.Duplicate);
                AssertCollection(e.Original, a.Original, ordered: true);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_projection_nested_collection_and_element_correct_order_AsNoTrackingWithIdentityResolution(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new
            {
                x.Id,
                Original = x.OwnedReferenceRoot.OwnedCollectionBranch[0].OwnedCollectionLeaf,
                Duplicate = x.OwnedReferenceRoot.OwnedCollectionBranch[0].OwnedCollectionLeaf[1],
            }).AsNoTrackingWithIdentityResolution(),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertCollection(e.Original, a.Original, ordered: true);
                AssertEqual(e.Duplicate, a.Duplicate);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task
        Json_projection_nested_collection_element_using_parameter_and_the_owner_in_correct_order_AsNoTrackingWithIdentityResolution(
            bool async)
    {
        var prm = 0;
        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new
            {
                x.Id,
                Original = x.OwnedReferenceRoot,
                Duplicate = x.OwnedReferenceRoot.OwnedCollectionBranch[prm].OwnedCollectionLeaf[1],
            }).AsNoTrackingWithIdentityResolution(),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertEqual(e.Original, a.Original);
                AssertEqual(e.Duplicate, a.Duplicate);
            });
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_projection_second_element_projected_before_owner_as_well_as_root_AsNoTrackingWithIdentityResolution(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new
            {
                x.Id,
                Duplicate = x.OwnedReferenceRoot.OwnedCollectionBranch[1],
                Original = x.OwnedReferenceRoot,
                Owned = x
            }).AsNoTrackingWithIdentityResolution(),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertEqual(e.Original, a.Original);
                AssertEqual(e.Duplicate, a.Duplicate);
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Json_projection_second_element_projected_before_owner_nested_as_well_as_root_AsNoTrackingWithIdentityResolution(
        bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new
            {
                x.Id,
                Duplicate = x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf[1],
                Original = x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf,
                Parent = x.OwnedReferenceRoot.OwnedReferenceBranch,
                Owner = x
            }).AsNoTrackingWithIdentityResolution(),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertEqual(e.Duplicate, a.Duplicate);
                AssertCollection(e.Original, a.Original, ordered: true);
                AssertEqual(e.Owner, a.Owner);
            });

    #region JunkInJson

    [ConditionalFact]
    public virtual async Task Junk_in_json_basic_tracking()
    {
        var contextFactory = await InitializeNonSharedTest<ContextJunkInJson>(
            onModelCreating: OnModelCreatingJunkInJson,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedJunkInJson);

        using var context = contextFactory.CreateDbContext();
        var query = context.Set<ContextJunkInJson.MyEntity>();
        var result = await query.ToListAsync();

        Assert.Equal(1, result.Count);
        Assert.Equal(2, result[0].Collection.Count);
        Assert.Equal(2, result[0].CollectionWithCtor.Count);
        Assert.Equal(2, result[0].Reference.NestedCollection.Count);
        Assert.NotNull(result[0].Reference.NestedReference);
        Assert.Equal(2, result[0].ReferenceWithCtor.NestedCollection.Count);
        Assert.NotNull(result[0].ReferenceWithCtor.NestedReference);
    }

    [ConditionalFact]
    public virtual async Task Junk_in_json_basic_no_tracking()
    {
        var contextFactory = await InitializeNonSharedTest<ContextJunkInJson>(
            onModelCreating: OnModelCreatingJunkInJson,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedJunkInJson);

        using var context = contextFactory.CreateDbContext();
        var query = context.Set<ContextJunkInJson.MyEntity>().AsNoTracking();
        var result = await query.ToListAsync();

        Assert.Equal(1, result.Count);
        Assert.Equal(2, result[0].Collection.Count);
        Assert.Equal(2, result[0].CollectionWithCtor.Count);
        Assert.Equal(2, result[0].Reference.NestedCollection.Count);
        Assert.NotNull(result[0].Reference.NestedReference);
        Assert.Equal(2, result[0].ReferenceWithCtor.NestedCollection.Count);
        Assert.NotNull(result[0].ReferenceWithCtor.NestedReference);
    }

    protected abstract Task SeedJunkInJson(DbContext ctx);

    protected virtual void OnModelCreatingJunkInJson(ModelBuilder modelBuilder)
        => modelBuilder.Entity<ContextJunkInJson.MyEntity>(b =>
        {
            b.Property(x => x.Id).ValueGeneratedNever();

            b.OwnsOne(
                x => x.Reference, b =>
                {
                    b.OwnsOne(x => x.NestedReference);
                    b.OwnsMany(x => x.NestedCollection);
                });

            b.OwnsOne(
                x => x.ReferenceWithCtor, b =>
                {
                    b.OwnsOne(x => x.NestedReference);
                    b.OwnsMany(x => x.NestedCollection);
                });

            b.OwnsMany(
                x => x.Collection, b =>
                {
                    b.OwnsOne(x => x.NestedReference);
                    b.OwnsMany(x => x.NestedCollection);
                });

            b.OwnsMany(
                x => x.CollectionWithCtor, b =>
                {
                    b.OwnsOne(x => x.NestedReference);
                    b.OwnsMany(x => x.NestedCollection);
                });
        });

    protected class ContextJunkInJson(DbContextOptions options) : DbContext(options)
    {
        public class MyEntity
        {
            public int Id { get; set; }
            public MyJsonEntity Reference { get; set; }
            public MyJsonEntityWithCtor ReferenceWithCtor { get; set; }
            public List<MyJsonEntity> Collection { get; set; }
            public List<MyJsonEntityWithCtor> CollectionWithCtor { get; set; }
        }

        public class MyJsonEntity
        {
            public string Name { get; set; }
            public double Number { get; set; }

            public MyJsonEntityNested NestedReference { get; set; }
            public List<MyJsonEntityNested> NestedCollection { get; set; }
        }

        public class MyJsonEntityNested
        {
            public DateTime DoB { get; set; }
        }

        public class MyJsonEntityWithCtor(bool myBool, string name)
        {
            public bool MyBool { get; set; } = myBool;
            public string Name { get; set; } = name;

            public MyJsonEntityWithCtorNested NestedReference { get; set; }
            public List<MyJsonEntityWithCtorNested> NestedCollection { get; set; }
        }

        public class MyJsonEntityWithCtorNested(DateTime doB)
        {
            public DateTime DoB { get; set; } = doB;
        }
    }

    #endregion

    #region TrickyBuffering

    [ConditionalFact]
    public virtual async Task Tricky_buffering_basic()
    {
        var contextFactory = await InitializeNonSharedTest<ContextTrickyBuffering>(
            onModelCreating: OnModelCreatingTrickyBuffering,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedTrickyBuffering);

        using var context = contextFactory.CreateDbContext();
        var query = context.Set<ContextTrickyBuffering.MyEntity>();
        var result = await query.ToListAsync();

        Assert.Equal(1, result.Count);
        Assert.Equal("r1", result[0].Reference.Name);
        Assert.Equal(7, result[0].Reference.Number);
        Assert.Equal(new DateTime(2000, 1, 1), result[0].Reference.NestedReference.DoB);
        Assert.Equal(2, result[0].Reference.NestedCollection.Count);
    }

    protected abstract Task SeedTrickyBuffering(DbContext ctx);

    protected virtual void OnModelCreatingTrickyBuffering(ModelBuilder modelBuilder)
        => modelBuilder.Entity<ContextTrickyBuffering.MyEntity>(b =>
        {
            b.Property(x => x.Id).ValueGeneratedNever();
            b.OwnsOne(
                x => x.Reference, b =>
                {
                    b.OwnsOne(x => x.NestedReference);
                    b.OwnsMany(x => x.NestedCollection);
                });
        });

    protected class ContextTrickyBuffering(DbContextOptions options) : DbContext(options)
    {
        public class MyEntity
        {
            public int Id { get; set; }
            public MyJsonEntity Reference { get; set; }
        }

        public class MyJsonEntity
        {
            public string Name { get; set; }
            public int Number { get; set; }
            public MyJsonEntityNested NestedReference { get; set; }
            public List<MyJsonEntityNested> NestedCollection { get; set; }
        }

        public class MyJsonEntityNested
        {
            public DateTime DoB { get; set; }
        }
    }

    #endregion

    #region BadJsonProperties

    [ConditionalTheory, InlineData(true), InlineData(false)]
    public virtual async Task Bad_json_properties_duplicated_navigations(bool noTracking)
    {
        var contextFactory = await InitializeNonSharedTest<ContextBadJsonProperties>(
            onModelCreating: OnModelCreatingBadJsonProperties,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedBadJsonProperties);

        using var context = contextFactory.CreateDbContext();
        var query = noTracking ? context.Entities.AsNoTracking() : context.Entities;
        var baseline = await query.SingleAsync(x => x.Scenario == "baseline");
        var dupNavs = await query.SingleAsync(x => x.Scenario == "duplicated navigations");

        // for no tracking, last one wins
        Assert.Equal(baseline.RequiredReference.NestedOptional.Text + " dupnav", dupNavs.RequiredReference.NestedOptional.Text);
        Assert.Equal(baseline.RequiredReference.NestedRequired.Text + " dupnav", dupNavs.RequiredReference.NestedRequired.Text);
        Assert.Equal(baseline.RequiredReference.NestedCollection[0].Text + " dupnav", dupNavs.RequiredReference.NestedCollection[0].Text);
        Assert.Equal(baseline.RequiredReference.NestedCollection[1].Text + " dupnav", dupNavs.RequiredReference.NestedCollection[1].Text);

        Assert.Equal(baseline.OptionalReference.NestedOptional.Text + " dupnav", dupNavs.OptionalReference.NestedOptional.Text);
        Assert.Equal(baseline.OptionalReference.NestedRequired.Text + " dupnav", dupNavs.OptionalReference.NestedRequired.Text);
        Assert.Equal(baseline.OptionalReference.NestedCollection[0].Text + " dupnav", dupNavs.OptionalReference.NestedCollection[0].Text);
        Assert.Equal(baseline.OptionalReference.NestedCollection[1].Text + " dupnav", dupNavs.OptionalReference.NestedCollection[1].Text);

        Assert.Equal(baseline.Collection[0].NestedOptional.Text + " dupnav", dupNavs.Collection[0].NestedOptional.Text);
        Assert.Equal(baseline.Collection[0].NestedRequired.Text + " dupnav", dupNavs.Collection[0].NestedRequired.Text);
        Assert.Equal(baseline.Collection[0].NestedCollection[0].Text + " dupnav", dupNavs.Collection[0].NestedCollection[0].Text);
        Assert.Equal(baseline.Collection[0].NestedCollection[1].Text + " dupnav", dupNavs.Collection[0].NestedCollection[1].Text);

        Assert.Equal(baseline.Collection[1].NestedOptional.Text + " dupnav", dupNavs.Collection[1].NestedOptional.Text);
        Assert.Equal(baseline.Collection[1].NestedRequired.Text + " dupnav", dupNavs.Collection[1].NestedRequired.Text);
        Assert.Equal(baseline.Collection[1].NestedCollection[0].Text + " dupnav", dupNavs.Collection[1].NestedCollection[0].Text);
        Assert.Equal(baseline.Collection[1].NestedCollection[1].Text + " dupnav", dupNavs.Collection[1].NestedCollection[1].Text);
    }

    [ConditionalTheory, InlineData(true), InlineData(false)]
    public virtual async Task Bad_json_properties_duplicated_scalars(bool noTracking)
    {
        var contextFactory = await InitializeNonSharedTest<ContextBadJsonProperties>(
            onModelCreating: OnModelCreatingBadJsonProperties,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedBadJsonProperties);

        using var context = contextFactory.CreateDbContext();
        var query = noTracking ? context.Entities.AsNoTracking() : context.Entities;

        var baseline = await query.SingleAsync(x => x.Scenario == "baseline");
        var dupProps = await query.SingleAsync(x => x.Scenario == "duplicated scalars");

        Assert.Equal(baseline.RequiredReference.NestedOptional.Text + " dupprop", dupProps.RequiredReference.NestedOptional.Text);
        Assert.Equal(baseline.RequiredReference.NestedRequired.Text + " dupprop", dupProps.RequiredReference.NestedRequired.Text);
        Assert.Equal(baseline.RequiredReference.NestedCollection[0].Text + " dupprop", dupProps.RequiredReference.NestedCollection[0].Text);
        Assert.Equal(baseline.RequiredReference.NestedCollection[1].Text + " dupprop", dupProps.RequiredReference.NestedCollection[1].Text);

        Assert.Equal(baseline.OptionalReference.NestedOptional.Text + " dupprop", dupProps.OptionalReference.NestedOptional.Text);
        Assert.Equal(baseline.OptionalReference.NestedRequired.Text + " dupprop", dupProps.OptionalReference.NestedRequired.Text);
        Assert.Equal(baseline.OptionalReference.NestedCollection[0].Text + " dupprop", dupProps.OptionalReference.NestedCollection[0].Text);
        Assert.Equal(baseline.OptionalReference.NestedCollection[1].Text + " dupprop", dupProps.OptionalReference.NestedCollection[1].Text);

        Assert.Equal(baseline.Collection[0].NestedOptional.Text + " dupprop", dupProps.Collection[0].NestedOptional.Text);
        Assert.Equal(baseline.Collection[0].NestedRequired.Text + " dupprop", dupProps.Collection[0].NestedRequired.Text);
        Assert.Equal(baseline.Collection[0].NestedCollection[0].Text + " dupprop", dupProps.Collection[0].NestedCollection[0].Text);
        Assert.Equal(baseline.Collection[0].NestedCollection[1].Text + " dupprop", dupProps.Collection[0].NestedCollection[1].Text);

        Assert.Equal(baseline.Collection[1].NestedOptional.Text + " dupprop", dupProps.Collection[1].NestedOptional.Text);
        Assert.Equal(baseline.Collection[1].NestedRequired.Text + " dupprop", dupProps.Collection[1].NestedRequired.Text);
        Assert.Equal(baseline.Collection[1].NestedCollection[0].Text + " dupprop", dupProps.Collection[1].NestedCollection[0].Text);
        Assert.Equal(baseline.Collection[1].NestedCollection[1].Text + " dupprop", dupProps.Collection[1].NestedCollection[1].Text);
    }

    [ConditionalTheory, InlineData(true), InlineData(false)]
    public virtual async Task Bad_json_properties_empty_navigations(bool noTracking)
    {
        var contextFactory = await InitializeNonSharedTest<ContextBadJsonProperties>(
            onModelCreating: OnModelCreatingBadJsonProperties,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedBadJsonProperties);

        using var context = contextFactory.CreateDbContext();
        var query = noTracking ? context.Entities.AsNoTracking() : context.Entities;
        var emptyNavs = await query.SingleAsync(x => x.Scenario == "empty navigation property names");

        Assert.Null(emptyNavs.RequiredReference.NestedOptional);
        Assert.Null(emptyNavs.RequiredReference.NestedRequired);
        Assert.Null(emptyNavs.RequiredReference.NestedCollection);

        Assert.Null(emptyNavs.OptionalReference.NestedOptional);
        Assert.Null(emptyNavs.OptionalReference.NestedRequired);
        Assert.Null(emptyNavs.OptionalReference.NestedCollection);

        Assert.Null(emptyNavs.Collection[0].NestedOptional);
        Assert.Null(emptyNavs.Collection[0].NestedRequired);
        Assert.Null(emptyNavs.Collection[0].NestedCollection);

        Assert.Null(emptyNavs.Collection[1].NestedOptional);
        Assert.Null(emptyNavs.Collection[1].NestedRequired);
        Assert.Null(emptyNavs.Collection[1].NestedCollection);
    }

    [ConditionalTheory, InlineData(true), InlineData(false)]
    public virtual async Task Bad_json_properties_empty_scalars(bool noTracking)
    {
        var contextFactory = await InitializeNonSharedTest<ContextBadJsonProperties>(
            onModelCreating: OnModelCreatingBadJsonProperties,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedBadJsonProperties);

        using var context = contextFactory.CreateDbContext();
        var query = noTracking ? context.Entities.AsNoTracking() : context.Entities;
        var emptyNavs = await query.SingleAsync(x => x.Scenario == "empty scalar property names");

        Assert.Null(emptyNavs.RequiredReference.NestedOptional.Text);
        Assert.Null(emptyNavs.RequiredReference.NestedRequired.Text);
        Assert.Null(emptyNavs.RequiredReference.NestedCollection[0].Text);
        Assert.Null(emptyNavs.RequiredReference.NestedCollection[1].Text);

        Assert.Null(emptyNavs.OptionalReference.NestedOptional.Text);
        Assert.Null(emptyNavs.OptionalReference.NestedRequired.Text);
        Assert.Null(emptyNavs.OptionalReference.NestedCollection[0].Text);
        Assert.Null(emptyNavs.OptionalReference.NestedCollection[1].Text);

        Assert.Null(emptyNavs.Collection[0].NestedOptional.Text);
        Assert.Null(emptyNavs.Collection[0].NestedRequired.Text);
        Assert.Null(emptyNavs.Collection[0].NestedCollection[0].Text);
        Assert.Null(emptyNavs.Collection[0].NestedCollection[1].Text);

        Assert.Null(emptyNavs.Collection[1].NestedOptional.Text);
        Assert.Null(emptyNavs.Collection[1].NestedRequired.Text);
        Assert.Null(emptyNavs.Collection[1].NestedCollection[0].Text);
        Assert.Null(emptyNavs.Collection[1].NestedCollection[1].Text);
    }

    [ConditionalTheory, InlineData(true), InlineData(false)]
    public virtual async Task Bad_json_properties_null_navigations(bool noTracking)
    {
        var contextFactory = await InitializeNonSharedTest<ContextBadJsonProperties>(
            onModelCreating: OnModelCreatingBadJsonProperties,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedBadJsonProperties);

        using var context = contextFactory.CreateDbContext();
        var query = noTracking ? context.Entities.AsNoTracking() : context.Entities;
        var _ = await query.SingleAsync(x => x.Scenario == "null navigation property names");
    }

    [ConditionalTheory, InlineData(true), InlineData(false)]
    public virtual async Task Bad_json_properties_null_scalars(bool noTracking)
    {
        var contextFactory = await InitializeNonSharedTest<ContextBadJsonProperties>(
            onModelCreating: OnModelCreatingBadJsonProperties,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedBadJsonProperties);

        using var context = contextFactory.CreateDbContext();
        var query = noTracking ? context.Entities.AsNoTracking() : context.Entities;
        var _ = await query.SingleAsync(x => x.Scenario == "null scalar property names");
    }

    protected abstract Task SeedBadJsonProperties(ContextBadJsonProperties ctx);

    protected virtual void OnModelCreatingBadJsonProperties(ModelBuilder modelBuilder)
        => modelBuilder.Entity<ContextBadJsonProperties.Entity>(b =>
        {
            b.Property(x => x.Id).ValueGeneratedNever();

            b.OwnsOne(
                x => x.RequiredReference, b =>
                {
                    b.OwnsOne(x => x.NestedOptional);
                    b.OwnsOne(x => x.NestedRequired);
                    b.OwnsMany(x => x.NestedCollection);
                });

            b.OwnsOne(
                x => x.OptionalReference, b =>
                {
                    b.OwnsOne(x => x.NestedOptional);
                    b.OwnsOne(x => x.NestedRequired);
                    b.OwnsMany(x => x.NestedCollection);
                });

            b.OwnsMany(
                x => x.Collection, b =>
                {
                    b.OwnsOne(x => x.NestedOptional);
                    b.OwnsOne(x => x.NestedRequired);
                    b.OwnsMany(x => x.NestedCollection);
                });
        });

    protected class ContextBadJsonProperties(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity> Entities { get; set; }

        public class Entity
        {
            public int Id { get; set; }
            public string Scenario { get; set; }
            public JsonRoot OptionalReference { get; set; }
            public JsonRoot RequiredReference { get; set; }
            public List<JsonRoot> Collection { get; set; }
        }

        public class JsonRoot
        {
            public JsonBranch NestedRequired { get; set; }
            public JsonBranch NestedOptional { get; set; }
            public List<JsonBranch> NestedCollection { get; set; }
        }

        public class JsonBranch
        {
            public string Text { get; set; }
        }
    }

    #endregion


    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Project_json_reference_in_tracking_query_fails(bool async)
        // verify exception on the provider level, relational and core throw different exceptions
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedReferenceRoot));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Project_json_collection_in_tracking_query_fails(bool async)
        // verify exception on the provider level, relational and core throw different exceptions
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Project_json_entity_in_tracking_query_fails_even_when_owner_is_present(bool async)
        // verify exception on the provider level, relational and core throw different exceptions
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new
            {
                x,
                x.OwnedReferenceRoot,
                x.OwnedCollectionRoot
            }));

    protected virtual void ClearLog()
    {
    }

    protected virtual void ConfigureWarnings(WarningsConfigurationBuilder builder)
    {
    }

}
