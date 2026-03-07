// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

using System.Text.Json;

#pragma warning disable EF8001 // Owned JSON entities are obsolete

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class JsonQueryRelationalTestBase<TFixture>(TFixture fixture) : JsonQueryTestBase<TFixture>(fixture)
    where TFixture : JsonQueryRelationalFixture, new()
{
    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public override async Task Project_json_reference_in_tracking_query_fails(bool async)
    {
        var message =
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Project_json_reference_in_tracking_query_fails(async))).Message;

        Assert.Equal(CoreStrings.OwnedEntitiesCannotBeTrackedWithoutTheirOwner, message);
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public override async Task Project_json_collection_in_tracking_query_fails(bool async)
    {
        var message =
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Project_json_collection_in_tracking_query_fails(async)))
            .Message;

        Assert.Equal(CoreStrings.OwnedEntitiesCannotBeTrackedWithoutTheirOwner, message);
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public override async Task Project_json_entity_in_tracking_query_fails_even_when_owner_is_present(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(()
            => base.Project_json_entity_in_tracking_query_fails_even_when_owner_is_present(async))).Message;

        Assert.Equal(CoreStrings.OwnedEntitiesCannotBeTrackedWithoutTheirOwner, message);
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task FromSql_on_entity_with_json_basic(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<JsonEntityBasic>)ss.Set<JsonEntityBasic>()).FromSqlRaw(
                Fixture.TestStore.NormalizeDelimitersInRawString("SELECT * FROM [JsonEntitiesBasic] AS j")),
            ss => ss.Set<JsonEntityBasic>());

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task FromSql_on_entity_with_json_project_json_reference(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<JsonEntityBasic>)ss.Set<JsonEntityBasic>()).FromSqlRaw(
                    Fixture.TestStore.NormalizeDelimitersInRawString("SELECT * FROM [JsonEntitiesBasic] AS j"))
                .AsNoTracking()
                .Select(x => x.OwnedReferenceRoot.OwnedReferenceBranch),
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedReferenceRoot.OwnedReferenceBranch));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task FromSql_on_entity_with_json_project_json_collection(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<JsonEntityBasic>)ss.Set<JsonEntityBasic>()).FromSqlRaw(
                    Fixture.TestStore.NormalizeDelimitersInRawString("SELECT * FROM [JsonEntitiesBasic] AS j"))
                .AsNoTracking()
                .Select(x => x.OwnedReferenceRoot.OwnedCollectionBranch),
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedReferenceRoot.OwnedCollectionBranch),
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: ee => (ee.Date, ee.Enum, ee.Fraction)));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task FromSql_on_entity_with_json_inheritance_on_base(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<JsonEntityInheritanceBase>)ss.Set<JsonEntityInheritanceBase>()).FromSqlRaw(
                Fixture.TestStore.NormalizeDelimitersInRawString("SELECT * FROM [JsonEntitiesInheritance] AS j")),
            ss => ss.Set<JsonEntityInheritanceBase>());

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task FromSql_on_entity_with_json_inheritance_on_derived(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<JsonEntityInheritanceDerived>)ss.Set<JsonEntityInheritanceDerived>()).FromSqlRaw(
                Fixture.TestStore.NormalizeDelimitersInRawString("SELECT * FROM [JsonEntitiesInheritance] AS j")),
            ss => ss.Set<JsonEntityInheritanceDerived>());

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task FromSql_on_entity_with_json_inheritance_project_reference_on_base(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<JsonEntityInheritanceBase>)ss.Set<JsonEntityInheritanceBase>()).FromSqlRaw(
                    Fixture.TestStore.NormalizeDelimitersInRawString("SELECT * FROM [JsonEntitiesInheritance] AS j"))
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Select(x => x.ReferenceOnBase),
            ss => ss.Set<JsonEntityInheritanceBase>().OrderBy(x => x.Id).Select(x => x.ReferenceOnBase),
            assertOrder: true);

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task FromSql_on_entity_with_json_inheritance_project_reference_on_derived(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<JsonEntityInheritanceDerived>)ss.Set<JsonEntityInheritanceDerived>()).FromSqlRaw(
                    Fixture.TestStore.NormalizeDelimitersInRawString("SELECT * FROM [JsonEntitiesInheritance] AS j"))
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Select(x => x.CollectionOnDerived),
            ss => ss.Set<JsonEntityInheritanceDerived>().OrderBy(x => x.Id).Select(x => x.CollectionOnDerived),
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: ee => (ee.Date, ee.Enum, ee.Fraction)),
            assertOrder: true);

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]

    #region 32939

    protected override void OnModelCreating32939(ModelBuilder modelBuilder)
    {
        base.OnModelCreating32939(modelBuilder);

        modelBuilder.Entity<Context32939.Entity>().OwnsOne(x => x.Empty, b => b.ToJson().HasColumnType(JsonColumnType));
        modelBuilder.Entity<Context32939.Entity>().OwnsOne(x => x.FieldOnly, b => b.ToJson().HasColumnType(JsonColumnType));
    }

    #endregion

    #region 21006

    public override async Task Project_missing_required_navigation(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Project_missing_required_navigation(async))).Message;

        Assert.Equal(RelationalStrings.JsonRequiredEntityWithNullJson(typeof(Context21006.JsonEntityNested).Name), message);
    }

    public override async Task Project_null_required_navigation(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Project_null_required_navigation(async))).Message;

        Assert.Equal(RelationalStrings.JsonRequiredEntityWithNullJson(typeof(Context21006.JsonEntityNested).Name), message);
    }

    public override async Task Project_top_level_entity_with_null_value_required_scalars(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(()
            => base.Project_top_level_entity_with_null_value_required_scalars(async))).Message;

        Assert.Equal("Cannot get the value of a token type 'Null' as a number.", message);
    }

    protected override void OnModelCreating21006(ModelBuilder modelBuilder)
    {
        base.OnModelCreating21006(modelBuilder);

        modelBuilder.Entity<Context21006.Entity>(b =>
        {
            b.ToTable("Entities");
            b.OwnsOne(x => x.OptionalReference).ToJson().HasColumnType(JsonColumnType);
            b.OwnsOne(x => x.RequiredReference).ToJson().HasColumnType(JsonColumnType);
            b.OwnsMany(x => x.Collection).ToJson().HasColumnType(JsonColumnType);
        });
    }

    #endregion

    #region 34293

    [ConditionalFact]
    public virtual async Task Project_entity_with_optional_json_entity_owned_by_required_json()
    {
        var contextFactory = await InitializeNonSharedTest<Context34293>(
            onModelCreating: OnModelCreating34293,
            seed: ctx => ctx.Seed());

        using var context = contextFactory.CreateDbContext();
        var entityProjection = await context.Set<Context34293.Entity>().ToListAsync();

        Assert.Equal(3, entityProjection.Count);
    }

    [ConditionalFact]
    public virtual async Task Project_required_json_entity()
    {
        var contextFactory = await InitializeNonSharedTest<Context34293>(
            onModelCreating: OnModelCreating34293,
            seed: ctx => ctx.Seed());

        using var context = contextFactory.CreateDbContext();

        var rootProjection =
            await context.Set<Context34293.Entity>().AsNoTracking().Where(x => x.Id != 3).Select(x => x.Json).ToListAsync();
        Assert.Equal(2, rootProjection.Count);

        var branchProjection = await context.Set<Context34293.Entity>().AsNoTracking().Where(x => x.Id != 3).Select(x => x.Json.Required)
            .ToListAsync();
        Assert.Equal(2, rootProjection.Count);

        var badRootProjectionMessage = (await Assert.ThrowsAsync<InvalidOperationException>(()
            => context.Set<Context34293.Entity>().AsNoTracking().Where(x => x.Id == 3).Select(x => x.Json).ToListAsync())).Message;
        Assert.Equal(RelationalStrings.JsonRequiredEntityWithNullJson(nameof(Context34293.JsonBranch)), badRootProjectionMessage);

        var badBranchProjectionMessage = (await Assert.ThrowsAsync<InvalidOperationException>(()
            => context.Set<Context34293.Entity>().AsNoTracking().Where(x => x.Id == 3).Select(x => x.Json.Required).ToListAsync())).Message;
        Assert.Equal(RelationalStrings.JsonRequiredEntityWithNullJson(nameof(Context34293.JsonBranch)), badBranchProjectionMessage);
    }

    [ConditionalFact]
    public virtual async Task Project_optional_json_entity_owned_by_required_json_entity()
    {
        var contextFactory = await InitializeNonSharedTest<Context34293>(
            onModelCreating: OnModelCreating34293,
            seed: ctx => ctx.Seed());

        using var context = contextFactory.CreateDbContext();
        var leafProjection = await context.Set<Context34293.Entity>().AsNoTracking().Select(x => x.Json.Required.Optional).ToListAsync();
        Assert.Equal(3, leafProjection.Count);
    }

    protected class Context34293(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity> Entities { get; set; } = null!;

        public class Entity
        {
            public int Id { get; set; }
            public JsonRoot Json { get; set; } = null!;
        }

        public class JsonRoot
        {
            public DateTime Date { get; set; }

            public JsonBranch Required { get; set; } = null!;
        }

        public class JsonBranch
        {
            public int Number { get; set; }
            public JsonLeaf? Optional { get; set; }
        }

        public class JsonLeaf
        {
            public string? Name { get; set; }
        }

        public async Task Seed()
        {
            // everything - ok
            var e1 = new Entity
            {
                Id = 1,
                Json = new JsonRoot
                {
                    Date = new DateTime(2001, 1, 1),
                    Required = new JsonBranch { Number = 1, Optional = new JsonLeaf { Name = "optional 1" } }
                }
            };

            // null leaf - ok (optional nav)
            var e2 = new Entity
            {
                Id = 2,
                Json = new JsonRoot { Date = new DateTime(2002, 2, 2), Required = new JsonBranch { Number = 2, Optional = null } }
            };

            // null branch - invalid (required nav)
            var e3 = new Entity
            {
                Id = 3,
                Json = new JsonRoot
                {
                    Date = new DateTime(2003, 3, 3), Required = null!,
                }
            };

            Entities.AddRange(e1, e2, e3);
            await SaveChangesAsync();
        }
    }

    protected virtual void OnModelCreating34293(ModelBuilder modelBuilder)
        => modelBuilder.Entity<Context34293.Entity>(b =>
        {
            b.Property(x => x.Id).ValueGeneratedNever();
            b.OwnsOne(
                x => x.Json, b =>
                {
                    b.ToJson().HasColumnType(JsonColumnType);
                    b.OwnsOne(
                        x => x.Required, bb =>
                        {
                            bb.OwnsOne(x => x.Optional);
                            bb.Navigation(x => x.Optional).IsRequired(false);
                        });
                    b.Navigation(x => x.Required).IsRequired();
                });
            b.Navigation(x => x.Json).IsRequired();
        });

    #endregion

    #region 34960

    public override async Task Try_project_collection_but_JSON_is_entity()
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Try_project_collection_but_JSON_is_entity())).Message;

        Assert.Equal(
            CoreStrings.JsonReaderInvalidTokenType(nameof(JsonTokenType.StartObject)),
            message);
    }

    public override async Task Try_project_reference_but_JSON_is_collection()
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Try_project_reference_but_JSON_is_collection()))
            .Message;

        Assert.Equal(
            CoreStrings.JsonReaderInvalidTokenType(nameof(JsonTokenType.StartArray)),
            message);
    }

    protected override void OnModelCreating34960(ModelBuilder modelBuilder)
    {
        base.OnModelCreating34960(modelBuilder);

        modelBuilder.Entity<Context34960.Entity>(b =>
        {
            b.ToTable("Entities");

            b.OwnsOne(
                x => x.Reference, b =>
                {
                    b.ToJson().HasColumnType(JsonColumnType);
                });

            b.OwnsMany(
                x => x.Collection, b =>
                {
                    b.ToJson().HasColumnType(JsonColumnType);
                });
        });

        modelBuilder.Entity<Context34960.JunkEntity>(b =>
        {
            b.ToTable("Junk");

            b.OwnsOne(
                x => x.Reference, b =>
                {
                    b.ToJson().HasColumnType(JsonColumnType);
                });

            b.OwnsMany(
                x => x.Collection, b =>
                {
                    b.ToJson().HasColumnType(JsonColumnType);
                });
        });
    }

    #endregion

    public virtual async Task Json_projection_using_queryable_methods_on_top_of_JSON_collection_AsNoTrackingWithIdentityResolution(
        bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AssertQuery(
                async,
                ss => ss.Set<JsonEntityBasic>().Select(x => new
                {
                    x.Id,
                    Skip = x.OwnedCollectionRoot.Skip(1).ToList(),
                    Take = x.OwnedCollectionRoot.Take(2).ToList(),
                }).AsNoTrackingWithIdentityResolution(),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.Id, a.Id);
                    AssertCollection(e.Skip, a.Skip);
                    AssertCollection(e.Take, a.Take);
                }))).Message;

        Assert.Equal(
            RelationalStrings.JsonProjectingQueryableOperationNoTrackingWithIdentityResolution(
                nameof(QueryTrackingBehavior.NoTrackingWithIdentityResolution)),
            message);
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_nested_collection_anonymous_projection_in_projection_NoTrackingWithIdentityResolution(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AssertQuery(
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
                            AssertEqual(eee.OwnedReferenceLeaf, aaa.OwnedReferenceLeaf);
                            AssertCollection(eee.OwnedCollectionLeaf, aaa.OwnedCollectionLeaf, ordered: true);
                        }))))).Message;

        Assert.Equal(
            RelationalStrings.JsonProjectingQueryableOperationNoTrackingWithIdentityResolution(
                nameof(QueryTrackingBehavior.NoTrackingWithIdentityResolution)),
            message);
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_projection_nested_collection_and_element_using_parameter_AsNoTrackingWithIdentityResolution(bool async)
    {
        var prm = 0;
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AssertQuery(
                async,
                ss => ss.Set<JsonEntityBasic>().Select(x => new
                {
                    x.Id,
                    Original = x.OwnedReferenceRoot.OwnedCollectionBranch[prm].OwnedCollectionLeaf,
                    Duplicate = x.OwnedReferenceRoot.OwnedCollectionBranch[prm].OwnedCollectionLeaf[1],
                }).AsNoTrackingWithIdentityResolution(),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.Id, a.Id);
                    AssertEqual(e.Duplicate, a.Duplicate);
                    AssertCollection(e.Original, a.Original, ordered: true);
                }))).Message;

        Assert.Equal(
            RelationalStrings.JsonProjectingCollectionElementAccessedUsingParmeterNoTrackingWithIdentityResolution(
                "JsonEntityBasic.OwnedReferenceRoot#JsonOwnedRoot.OwnedCollectionBranch#JsonOwnedBranch.OwnedCollectionLeaf#JsonOwnedLeaf",
                nameof(QueryTrackingBehavior.NoTrackingWithIdentityResolution)),
            message);
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_projection_nested_collection_and_element_using_parameter_AsNoTrackingWithIdentityResolution2(bool async)
    {
        var prm1 = 0;
        var prm2 = 0;
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AssertQuery(
                async,
                ss => ss.Set<JsonEntityBasic>().Select(x => new
                {
                    x.Id,
                    Duplicate = x.OwnedReferenceRoot.OwnedCollectionBranch[prm1].OwnedCollectionLeaf[1],
                    Original = x.OwnedReferenceRoot.OwnedCollectionBranch[prm2].OwnedCollectionLeaf,
                }).AsNoTrackingWithIdentityResolution(),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.Id, a.Id);
                    AssertEqual(e.Duplicate, a.Duplicate);
                    AssertCollection(e.Original, a.Original, ordered: true);
                }))).Message;

        Assert.Equal(
            RelationalStrings.JsonProjectingCollectionElementAccessedUsingParmeterNoTrackingWithIdentityResolution(
                "JsonEntityBasic.OwnedReferenceRoot#JsonOwnedRoot.OwnedCollectionBranch#JsonOwnedBranch.OwnedCollectionLeaf#JsonOwnedLeaf",
                nameof(QueryTrackingBehavior.NoTrackingWithIdentityResolution)),
            message);
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task
        Json_projection_second_element_through_collection_element_parameter_different_values_projected_before_owner_nested_AsNoTrackingWithIdentityResolution(
            bool async)
    {
        var prm1 = 0;
        var prm2 = 1;

        var message = (await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AssertQuery(
                async,
                ss => ss.Set<JsonEntityBasic>().Select(x => new
                {
                    x.Id,
                    Duplicate = x.OwnedReferenceRoot.OwnedCollectionBranch[prm1].OwnedCollectionLeaf[1],
                    Original = x.OwnedReferenceRoot.OwnedCollectionBranch[prm2].OwnedCollectionLeaf,
                }).AsNoTrackingWithIdentityResolution(),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.Id, a.Id);
                    AssertCollection(e.Original, a.Original, ordered: true);
                    AssertEqual(e.Duplicate, a.Duplicate);
                }))).Message;

        Assert.Equal(
            RelationalStrings.JsonProjectingCollectionElementAccessedUsingParmeterNoTrackingWithIdentityResolution(
                "JsonEntityBasic.OwnedReferenceRoot#JsonOwnedRoot.OwnedCollectionBranch#JsonOwnedBranch.OwnedCollectionLeaf#JsonOwnedLeaf",
                nameof(QueryTrackingBehavior.NoTrackingWithIdentityResolution)),
            message);
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task
        Json_projection_second_element_through_collection_element_parameter_projected_before_owner_nested_AsNoTrackingWithIdentityResolution(
            bool async)
    {
        var prm = 0;

        var message = (await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AssertQuery(
                async,
                ss => ss.Set<JsonEntityBasic>().Select(x => new
                {
                    x.Id,
                    Duplicate = x.OwnedReferenceRoot.OwnedCollectionBranch[prm].OwnedCollectionLeaf[1],
                    Original = x.OwnedReferenceRoot.OwnedCollectionBranch[prm].OwnedCollectionLeaf,
                }).AsNoTrackingWithIdentityResolution(),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.Id, a.Id);
                    AssertCollection(e.Original, a.Original, ordered: true);
                    AssertEqual(e.Duplicate, a.Duplicate);
                }))).Message;

        Assert.Equal(
            RelationalStrings.JsonProjectingCollectionElementAccessedUsingParmeterNoTrackingWithIdentityResolution(
                "JsonEntityBasic.OwnedReferenceRoot#JsonOwnedRoot.OwnedCollectionBranch#JsonOwnedBranch.OwnedCollectionLeaf#JsonOwnedLeaf",
                nameof(QueryTrackingBehavior.NoTrackingWithIdentityResolution)),
            message);
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task
        Json_projection_second_element_through_collection_element_parameter_projected_before_owner_nested_AsNoTrackingWithIdentityResolution2(
            bool async)
    {
        var prm1 = 0;
        var prm2 = 0;

        var message = (await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AssertQuery(
                async,
                ss => ss.Set<JsonEntityBasic>().Select(x => new
                {
                    x.Id,
                    Duplicate = x.OwnedReferenceRoot.OwnedCollectionBranch[prm1].OwnedCollectionLeaf[1],
                    Original = x.OwnedReferenceRoot.OwnedCollectionBranch[prm2].OwnedCollectionLeaf,
                }).AsNoTrackingWithIdentityResolution(),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.Id, a.Id);
                    AssertEqual(e.Original, a.Original);
                    AssertEqual(e.Duplicate, a.Duplicate);
                }))).Message;

        Assert.Equal(
            RelationalStrings.JsonProjectingCollectionElementAccessedUsingParmeterNoTrackingWithIdentityResolution(
                "JsonEntityBasic.OwnedReferenceRoot#JsonOwnedRoot.OwnedCollectionBranch#JsonOwnedBranch.OwnedCollectionLeaf#JsonOwnedLeaf",
                nameof(QueryTrackingBehavior.NoTrackingWithIdentityResolution)),
            message);
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task
        Json_projection_second_element_through_collection_element_parameter_projected_after_owner_nested_AsNoTrackingWithIdentityResolution(
            bool async)
    {
        var prm = 0;

        var message = (await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AssertQuery(
                async,
                ss => ss.Set<JsonEntityBasic>().Select(x => new
                {
                    x.Id,
                    Original = x.OwnedReferenceRoot.OwnedCollectionBranch[prm].OwnedCollectionLeaf,
                    Duplicate = x.OwnedReferenceRoot.OwnedCollectionBranch[prm].OwnedCollectionLeaf[1],
                }).AsNoTrackingWithIdentityResolution(),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.Id, a.Id);
                    AssertCollection(e.Original, a.Original, ordered: true);
                    AssertEqual(e.Duplicate, a.Duplicate);
                }))).Message;

        Assert.Equal(
            RelationalStrings.JsonProjectingCollectionElementAccessedUsingParmeterNoTrackingWithIdentityResolution(
                "JsonEntityBasic.OwnedReferenceRoot#JsonOwnedRoot.OwnedCollectionBranch#JsonOwnedBranch.OwnedCollectionLeaf#JsonOwnedLeaf",
                nameof(QueryTrackingBehavior.NoTrackingWithIdentityResolution)),
            message);
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task
        Json_projection_second_element_through_collection_element_constant_projected_before_owner_nested_AsNoTrackingWithIdentityResolution(
            bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AssertQuery(
                async,
                ss => ss.Set<JsonEntityBasic>().Select(x => new
                {
                    x.Id,
                    Duplicate = x.OwnedReferenceRoot.OwnedCollectionBranch[0].OwnedCollectionLeaf[1],
                    Original = x.OwnedReferenceRoot.OwnedCollectionBranch[0].OwnedCollectionLeaf,
                }).AsNoTrackingWithIdentityResolution(),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.Id, a.Id);
                    AssertEqual(e.Original, a.Original);
                    AssertEqual(e.Duplicate, a.Duplicate);
                }))).Message;

        Assert.Equal(
            RelationalStrings.JsonProjectingEntitiesIncorrectOrderNoTrackingWithIdentityResolution(
                "JsonEntityBasic.OwnedReferenceRoot#JsonOwnedRoot.OwnedCollectionBranch#JsonOwnedBranch.OwnedCollectionLeaf#JsonOwnedLeaf",
                nameof(QueryTrackingBehavior.NoTrackingWithIdentityResolution)),
            message);
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_branch_collection_distinct_and_other_collection_AsNoTrackingWithIdentityResolution(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AssertQuery(
                async,
                ss => ss.Set<JsonEntityBasic>()
                    .OrderBy(x => x.Id)
                    .Select(x => new
                    {
                        First = x.EntityCollection.ToList(), Second = x.OwnedReferenceRoot.OwnedCollectionBranch.Distinct().ToList()
                    })
                    .AsNoTrackingWithIdentityResolution(),
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    AssertCollection(e.First, a.First, ordered: true);
                    AssertCollection(e.Second, a.Second, elementSorter: ee => ee.Fraction);
                }))).Message;

        Assert.Equal(
            RelationalStrings.JsonProjectingQueryableOperationNoTrackingWithIdentityResolution(
                nameof(QueryTrackingBehavior.NoTrackingWithIdentityResolution)),
            message);
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_collection_SelectMany_AsNoTrackingWithIdentityResolution(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AssertQuery(
                async,
                ss => ss.Set<JsonEntityBasic>()
                    .SelectMany(x => x.OwnedCollectionRoot)
                    .AsNoTrackingWithIdentityResolution(),
                elementSorter: e => (e.Number, e.Name)))).Message;

        Assert.Equal(
            RelationalStrings.JsonProjectingQueryableOperationNoTrackingWithIdentityResolution(
                nameof(QueryTrackingBehavior.NoTrackingWithIdentityResolution)),
            message);
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_projection_deduplication_with_collection_indexer_in_target_AsNoTrackingWithIdentityResolution(bool async)
    {
        var prm = 1;
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AssertQuery(
                async,
                ss => ss.Set<JsonEntityBasic>().Select(x => new
                {
                    x.Id,
                    Duplicate1 = x.OwnedReferenceRoot.OwnedCollectionBranch[1],
                    Original = x.OwnedReferenceRoot,
                    Duplicate2 = x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf[prm]
                }).AsNoTrackingWithIdentityResolution(),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.Id, a.Id);
                    AssertEqual(e.Original, a.Original);
                    AssertEqual(e.Duplicate1, a.Duplicate1);
                    AssertEqual(e.Duplicate2, a.Duplicate2);
                }))).Message;

        Assert.Equal(
            RelationalStrings.JsonProjectingEntitiesIncorrectOrderNoTrackingWithIdentityResolution(
                "JsonEntityBasic.OwnedReferenceRoot#JsonOwnedRoot.OwnedCollectionBranch#JsonOwnedBranch",
                nameof(QueryTrackingBehavior.NoTrackingWithIdentityResolution)),
            message);
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_projection_nested_collection_and_element_wrong_order_AsNoTrackingWithIdentityResolution(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AssertQuery(
                async,
                ss => ss.Set<JsonEntityBasic>().Select(x => new
                {
                    x.Id,
                    Duplicate = x.OwnedReferenceRoot.OwnedCollectionBranch[0].OwnedCollectionLeaf[1],
                    Original = x.OwnedReferenceRoot.OwnedCollectionBranch[0].OwnedCollectionLeaf,
                }).AsNoTrackingWithIdentityResolution(),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.Id, a.Id);
                    AssertEqual(e.Duplicate, a.Duplicate);
                    AssertCollection(e.Original, a.Original, ordered: true);
                }))).Message;

        Assert.Equal(
            RelationalStrings.JsonProjectingEntitiesIncorrectOrderNoTrackingWithIdentityResolution(
                "JsonEntityBasic.OwnedReferenceRoot#JsonOwnedRoot.OwnedCollectionBranch#JsonOwnedBranch.OwnedCollectionLeaf#JsonOwnedLeaf",
                nameof(QueryTrackingBehavior.NoTrackingWithIdentityResolution)),
            message);
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_projection_second_element_projected_before_entire_collection_AsNoTrackingWithIdentityResolution(
        bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AssertQuery(
                async,
                ss => ss.Set<JsonEntityBasic>().Select(x => new
                {
                    x.Id,
                    Duplicate = x.OwnedReferenceRoot.OwnedCollectionBranch[1],
                    Original = x.OwnedReferenceRoot.OwnedCollectionBranch,
                }).AsNoTrackingWithIdentityResolution(),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.Id, a.Id);
                    AssertEqual(e.Original, a.Original);
                    AssertEqual(e.Duplicate, a.Duplicate);
                }))).Message;

        Assert.Equal(
            RelationalStrings.JsonProjectingEntitiesIncorrectOrderNoTrackingWithIdentityResolution(
                "JsonEntityBasic.OwnedReferenceRoot#JsonOwnedRoot.OwnedCollectionBranch#JsonOwnedBranch",
                nameof(QueryTrackingBehavior.NoTrackingWithIdentityResolution)),
            message);
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_projection_second_element_projected_before_owner_AsNoTrackingWithIdentityResolution(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AssertQuery(
                async,
                ss => ss.Set<JsonEntityBasic>().Select(x => new
                {
                    x.Id,
                    Duplicate = x.OwnedReferenceRoot.OwnedCollectionBranch[1],
                    Original = x.OwnedReferenceRoot,
                }).AsNoTrackingWithIdentityResolution(),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.Id, a.Id);
                    AssertEqual(e.Original, a.Original);
                    AssertEqual(e.Duplicate, a.Duplicate);
                }))).Message;

        Assert.Equal(
            RelationalStrings.JsonProjectingEntitiesIncorrectOrderNoTrackingWithIdentityResolution(
                "JsonEntityBasic.OwnedReferenceRoot#JsonOwnedRoot.OwnedCollectionBranch#JsonOwnedBranch",
                nameof(QueryTrackingBehavior.NoTrackingWithIdentityResolution)),
            message);
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_projection_second_element_projected_before_owner_nested_AsNoTrackingWithIdentityResolution(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AssertQuery(
                async,
                ss => ss.Set<JsonEntityBasic>().Select(x => new
                {
                    x.Id,
                    Duplicate = x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf[1],
                    Original = x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf,
                    Parent = x.OwnedReferenceRoot.OwnedReferenceBranch,
                }).AsNoTrackingWithIdentityResolution(),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.Id, a.Id);
                    AssertEqual(e.Original, a.Original);
                    AssertEqual(e.Duplicate, a.Duplicate);
                }))).Message;

        Assert.Equal(
            RelationalStrings.JsonProjectingEntitiesIncorrectOrderNoTrackingWithIdentityResolution(
                "JsonEntityBasic.OwnedReferenceRoot#JsonOwnedRoot.OwnedReferenceBranch#JsonOwnedBranch.OwnedCollectionLeaf#JsonOwnedLeaf",
                nameof(QueryTrackingBehavior.NoTrackingWithIdentityResolution)),
            message);
    }

    #region 32310

    protected override void OnModelCreating32310(ModelBuilder modelBuilder)
    {
        base.OnModelCreating32310(modelBuilder);

        modelBuilder.Entity<Context32310.Pub>().OwnsOne(e => e.Visits).ToJson().HasColumnType(JsonColumnType);
    }

    #endregion

    #region 33046

    protected override void OnModelCreating33046(ModelBuilder modelBuilder)
    {
        base.OnModelCreating33046(modelBuilder);

        modelBuilder.Entity<Context33046.Review>(b =>
        {
            b.ToTable("Reviews");
            b.OwnsMany(
                x => x.Rounds, ownedBuilder =>
                {
                    ownedBuilder.ToJson().HasColumnType(JsonColumnType);
                });
        });
    }

    #endregion

    #region ArrayOfPrimitives

    protected override void OnModelCreatingArrayOfPrimitives(ModelBuilder modelBuilder)
    {
        base.OnModelCreatingArrayOfPrimitives(modelBuilder);

        modelBuilder.Entity<ContextArrayOfPrimitives.MyEntity>().OwnsOne(
            x => x.Reference, b => b.ToJson().HasColumnType(JsonColumnType));

        modelBuilder.Entity<ContextArrayOfPrimitives.MyEntity>().OwnsMany(
            x => x.Collection, b => b.ToJson().HasColumnType(JsonColumnType));
    }

    #endregion

    #region NotICollection

    protected override void OnModelCreatingNotICollection(ModelBuilder modelBuilder)
    {
        base.OnModelCreatingNotICollection(modelBuilder);

        modelBuilder.Entity<ContextNotICollection.MyEntity>(b =>
        {
            b.ToTable("Entities");
            b.OwnsOne(
                cr => cr.Json, nb =>
                {
                    nb.ToJson().HasColumnType(JsonColumnType);
                });
        });
    }

    #endregion

    #region 30028

    protected override void OnModelCreating30028(ModelBuilder modelBuilder)
    {
        base.OnModelCreating30028(modelBuilder);

        modelBuilder.Entity<Context30028.MyEntity>(b =>
        {
            b.ToTable("Entities");
            b.OwnsOne(
                x => x.Json, nb =>
                {
                    nb.ToJson().HasColumnType(JsonColumnType);
                });
        });
    }

    #endregion

    #region 29219

    protected override void OnModelCreating29219(ModelBuilder modelBuilder)
    {
        base.OnModelCreating29219(modelBuilder);

        modelBuilder.Entity<Context29219.MyEntity>(b =>
        {
            b.ToTable("Entities");
            b.OwnsOne(x => x.Reference).ToJson().HasColumnType(JsonColumnType);
            b.OwnsMany(x => x.Collection).ToJson().HasColumnType(JsonColumnType);
        });
    }

    #endregion

    #region LazyLoadingProxies

    protected override void OnModelCreatingLazyLoadingProxies(ModelBuilder modelBuilder)
    {
        base.OnModelCreatingLazyLoadingProxies(modelBuilder);

        modelBuilder.Entity<ContextLazyLoadingProxies.MyEntity>().OwnsOne(x => x.Reference, b => b.ToJson().HasColumnType(JsonColumnType));
        modelBuilder.Entity<ContextLazyLoadingProxies.MyEntity>()
            .OwnsMany(x => x.Collection, b => b.ToJson().HasColumnType(JsonColumnType));
    }

    //protected void OnConfiguringLazyLoadingProxies(DbContextOptionsBuilder optionsBuilder)
    //    => optionsBuilder.UseLazyLoadingProxies();

    //protected IServiceCollection AddServicesLazyLoadingProxies(IServiceCollection addServices)
    //    => addServices.AddEntityFrameworkProxies();

    //private Task SeedLazyLoadingProxies(DbContext ctx)
    //{
    //    var r1 = new MyJsonEntityLazyLoadingProxiesWithCtor("r1", 1);
    //    var c11 = new MyJsonEntityLazyLoadingProxies { Name = "c11", Number = 11 };
    //    var c12 = new MyJsonEntityLazyLoadingProxies { Name = "c12", Number = 12 };
    //    var c13 = new MyJsonEntityLazyLoadingProxies { Name = "c13", Number = 13 };

    //    var r2 = new MyJsonEntityLazyLoadingProxiesWithCtor("r2", 2);
    //    var c21 = new MyJsonEntityLazyLoadingProxies { Name = "c21", Number = 21 };
    //    var c22 = new MyJsonEntityLazyLoadingProxies { Name = "c22", Number = 22 };

    //    var e1 = new MyEntityLazyLoadingProxies
    //    {
    //        Id = 1,
    //        Name = "e1",
    //        Reference = r1,
    //        Collection =
    //        [
    //            c11,
    //            c12,
    //            c13
    //        ]
    //    };

    //    var e2 = new MyEntityLazyLoadingProxies
    //    {
    //        Id = 2,
    //        Name = "e2",
    //        Reference = r2,
    //        Collection = [c21, c22]
    //    };

    //    ctx.Set<MyEntityLazyLoadingProxies>().AddRange(e1, e2);
    //    return ctx.SaveChangesAsync();
    //}

    #endregion

    #region ShadowProperties

    protected override void OnModelCreatingShadowProperties(ModelBuilder modelBuilder)
    {
        base.OnModelCreatingShadowProperties(modelBuilder);

        modelBuilder.Entity<ContextShadowProperties.MyEntity>(b =>
        {
            b.ToTable("Entities");

            b.OwnsOne(
                x => x.Reference, b =>
                {
                    b.ToJson().HasColumnType(JsonColumnType);
                });

            b.OwnsOne(
                x => x.ReferenceWithCtor, b =>
                {
                    b.ToJson().HasColumnType(JsonColumnType);
                    b.Property<int>("Shadow_Int").HasJsonPropertyName("ShadowInt");
                });

            b.OwnsMany(
                x => x.Collection, b =>
                {
                    b.ToJson().HasColumnType(JsonColumnType);
                });

            b.OwnsMany(
                x => x.CollectionWithCtor, b =>
                {
                    b.ToJson().HasColumnType(JsonColumnType);
                });
        });
    }

    #endregion

    #region Entity splitting

    [ConditionalFact] // #36145
    public virtual async Task Entity_splitting_with_owned_json()
    {
        var contextFactory = await InitializeNonSharedTest<ContextEntitySplitting>(
            onModelCreating: OnModelCreatingEntitySplitting,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedEntitySplitting);

        using var context = contextFactory.CreateDbContext();
        var result = await context.Set<ContextEntitySplitting.MyEntity>().SingleAsync();

        Assert.Equal("split content", result.PropertyInOtherTable);
        var json = Assert.Single(result.Json);
        Assert.Equal("JSON content", json.Foo);
    }

    protected virtual void OnModelCreatingEntitySplitting(ModelBuilder modelBuilder)
        => modelBuilder.Entity<ContextEntitySplitting.MyEntity>(b =>
        {
            b.Property(p => p.Id).ValueGeneratedNever();
            b.OwnsMany(p => p.Json, b => b.ToJson());
            b.SplitToTable("OtherTable", b => b.Property(p => p.PropertyInOtherTable));
        });

    protected virtual async Task SeedEntitySplitting(ContextEntitySplitting context)
    {
        var e1 = new ContextEntitySplitting.MyEntity
        {
            Id = 1,
            PropertyInOtherTable = "split content",
            Json = [new ContextEntitySplitting.JsonEntity { Foo = "JSON content" }]
        };

        context.Add(e1);
        await context.SaveChangesAsync();
    }

    protected class ContextEntitySplitting(DbContextOptions options) : DbContext(options)
    {
        public class MyEntity
        {
            public int Id { get; set; }
            public string? PropertyInMainTable { get; set; } // TODO: currently required because of #36171
            public string? PropertyInOtherTable { get; set; }

            public List<JsonEntity> Json { get; set; } = null!;
        }

        public class JsonEntity
        {
            public string? Foo { get; set; }
        }
    }

    #endregion

    #region HasJsonPropertyName

    [ConditionalFact]
    public virtual async Task HasJsonPropertyName()
    {
        var contextFactory = await InitializeNonSharedTest<Context37009>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            onModelCreating: m => m.Entity<Context37009.Entity>().ComplexProperty(e => e.Json, b =>
            {
                b.ToJson();

                b.Property(j => j.String).HasJsonPropertyName("string");

                b.ComplexProperty(j => j.Nested, b =>
                {
                    b.HasJsonPropertyName("nested");
                    b.Property(x => x.Int).HasJsonPropertyName("int");
                });

                b.ComplexCollection(a => a.NestedCollection, b =>
                {
                    b.HasJsonPropertyName("nested_collection");
                    b.Property(x => x.Int).HasJsonPropertyName("int");
                });
            }),
            seed: context =>
            {
                context.Set<Context37009.Entity>().Add(new Context37009.Entity
                {
                    Json = new Context37009.JsonComplexType
                    {
                        String = "foo",
                        Nested = new Context37009.JsonNestedType { Int = 1 },
                        NestedCollection = [new Context37009.JsonNestedType { Int = 2 }]
                    }
                });

                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        Assert.Equal(1, await context.Set<Context37009.Entity>().CountAsync(e => e.Json.String == "foo"));
        Assert.Equal(1, await context.Set<Context37009.Entity>().CountAsync(e => e.Json.Nested.Int == 1));
        Assert.Equal(1, await context.Set<Context37009.Entity>().CountAsync(e => e.Json.NestedCollection.Any(x => x.Int == 2)));
    }

    protected class Context37009(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity> Entities { get; set; } = null!;

        public class Entity
        {
            public int Id { get; set; }
            public JsonComplexType Json { get; set; } = null!;
        }

        public class JsonComplexType
        {
            public string? String { get; set; }

            public JsonNestedType Nested { get; set; } = null!;
            public List<JsonNestedType> NestedCollection { get; set; } = null!;
        }

        public class JsonNestedType
        {
            public int Int { get; set; }
        }
    }

    #endregion HasJsonPropertyName

    #region JunkInJson

    protected override void OnModelCreatingJunkInJson(ModelBuilder modelBuilder)
    {
        base.OnModelCreatingJunkInJson(modelBuilder);

        modelBuilder.Entity<ContextJunkInJson.MyEntity>(b =>
        {
            b.ToTable("Entities");

            b.OwnsOne(
                x => x.Reference, b =>
                {
                    b.ToJson().HasColumnType(JsonColumnType);
                });

            b.OwnsOne(
                x => x.ReferenceWithCtor, b =>
                {
                    b.ToJson().HasColumnType(JsonColumnType);
                });

            b.OwnsMany(
                x => x.Collection, b =>
                {
                    b.ToJson().HasColumnType(JsonColumnType);
                });

            b.OwnsMany(
                x => x.CollectionWithCtor, b =>
                {
                    b.ToJson().HasColumnType(JsonColumnType);
                });
        });
    }

    #endregion

    #region TrickyBuffering

    protected override void OnModelCreatingTrickyBuffering(ModelBuilder modelBuilder)
    {
        base.OnModelCreatingTrickyBuffering(modelBuilder);

        modelBuilder.Entity<ContextTrickyBuffering.MyEntity>(b =>
        {
            b.ToTable("Entities");
            b.OwnsOne(
                x => x.Reference, b =>
                {
                    b.ToJson().HasColumnType(JsonColumnType);
                });
        });
    }

    #endregion

    #region BadJsonProperties

    public override async Task Bad_json_properties_duplicated_navigations(bool noTracking)
    {
        // tracking returns different results - see #35807
        if (noTracking)
        {
            await base.Bad_json_properties_duplicated_navigations(noTracking);
        }
    }

    public override Task Bad_json_properties_null_navigations(bool noTracking)
        => Assert.ThrowsAnyAsync<JsonException>(() => base.Bad_json_properties_null_navigations(noTracking));

    public override async Task Bad_json_properties_null_scalars(bool noTracking)
    {
        var message = (await Assert.ThrowsAnyAsync<JsonException>(() => base.Bad_json_properties_null_scalars(noTracking))).Message;

        Assert.StartsWith("'n' is an invalid start of a property name. Expected a '\"'.", message);
    }

    protected override void OnModelCreatingBadJsonProperties(ModelBuilder modelBuilder)
    {
        base.OnModelCreatingBadJsonProperties(modelBuilder);

        modelBuilder.Entity<ContextBadJsonProperties.Entity>(b =>
        {
            b.ToTable("Entities");

            b.OwnsOne(
                x => x.RequiredReference, b =>
                {
                    b.ToJson().HasColumnType(JsonColumnType);
                });

            b.OwnsOne(
                x => x.OptionalReference, b =>
                {
                    b.ToJson().HasColumnType(JsonColumnType);
                });

            b.OwnsMany(
                x => x.Collection, b =>
                {
                    b.ToJson().HasColumnType(JsonColumnType);
                });
        });
    }

    #endregion

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected virtual string? JsonColumnType
        => null;

}

