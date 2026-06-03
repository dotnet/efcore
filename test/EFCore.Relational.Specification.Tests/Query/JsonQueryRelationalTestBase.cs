// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class JsonQueryRelationalTestBase<TFixture>(TFixture fixture) : JsonQueryTestBase<TFixture>(fixture)
    where TFixture : JsonQueryRelationalFixture, new()
{
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override async Task Project_json_reference_in_tracking_query_fails(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Project_json_reference_in_tracking_query_fails(async))).Message;

        Assert.Equal(
            RelationalStrings.JsonEntityOrCollectionProjectedAtRootLevelInTrackingQuery(
                nameof(EntityFrameworkQueryableExtensions.AsNoTracking)), message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override async Task Project_json_collection_in_tracking_query_fails(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Project_json_collection_in_tracking_query_fails(async))).Message;

        Assert.Equal(
            RelationalStrings.JsonEntityOrCollectionProjectedAtRootLevelInTrackingQuery(
                nameof(EntityFrameworkQueryableExtensions.AsNoTracking)), message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override async Task Project_json_entity_in_tracking_query_fails_even_when_owner_is_present(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Project_json_entity_in_tracking_query_fails_even_when_owner_is_present(async))).Message;

        Assert.Equal(
            RelationalStrings.JsonEntityOrCollectionProjectedAtRootLevelInTrackingQuery(
                nameof(EntityFrameworkQueryableExtensions.AsNoTracking)), message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSql_on_entity_with_json_basic(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<JsonEntityBasic>)ss.Set<JsonEntityBasic>()).FromSqlRaw(
                Fixture.TestStore.NormalizeDelimitersInRawString("SELECT * FROM [JsonEntitiesBasic] AS j")),
            ss => ss.Set<JsonEntityBasic>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSql_on_entity_with_json_project_json_reference(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<JsonEntityBasic>)ss.Set<JsonEntityBasic>()).FromSqlRaw(
                    Fixture.TestStore.NormalizeDelimitersInRawString("SELECT * FROM [JsonEntitiesBasic] AS j"))
                .AsNoTracking()
                .Select(x => x.OwnedReferenceRoot.OwnedReferenceBranch),
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedReferenceRoot.OwnedReferenceBranch));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSql_on_entity_with_json_project_json_collection(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<JsonEntityBasic>)ss.Set<JsonEntityBasic>()).FromSqlRaw(
                    Fixture.TestStore.NormalizeDelimitersInRawString("SELECT * FROM [JsonEntitiesBasic] AS j"))
                .AsNoTracking()
                .Select(x => x.OwnedReferenceRoot.OwnedCollectionBranch),
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedReferenceRoot.OwnedCollectionBranch),
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: ee => (ee.Date, ee.Enum, ee.Fraction)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSql_on_entity_with_json_inheritance_on_base(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<JsonEntityInheritanceBase>)ss.Set<JsonEntityInheritanceBase>()).FromSqlRaw(
                Fixture.TestStore.NormalizeDelimitersInRawString("SELECT * FROM [JsonEntitiesInheritance] AS j")),
            ss => ss.Set<JsonEntityInheritanceBase>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSql_on_entity_with_json_inheritance_on_derived(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<JsonEntityInheritanceDerived>)ss.Set<JsonEntityInheritanceDerived>()).FromSqlRaw(
                Fixture.TestStore.NormalizeDelimitersInRawString("SELECT * FROM [JsonEntitiesInheritance] AS j")),
            ss => ss.Set<JsonEntityInheritanceDerived>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_projection_using_queryable_methods_on_top_of_JSON_collection_AsNoTrackingWithIdentityResolution(
        bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () =>
                AssertQuery(
                    async,
                    ss => ss.Set<JsonEntityBasic>().Select(
                        x => new
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_nested_collection_anonymous_projection_in_projection_NoTrackingWithIdentityResolution(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () =>
                AssertQuery(
                    async,
                    ss => ss.Set<JsonEntityBasic>()
                        .OrderBy(x => x.Id)
                        .Select(
                            x => x.OwnedCollectionRoot
                                .Select(
                                    xx => xx.OwnedCollectionBranch.Select(
                                        xxx => new
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_projection_nested_collection_and_element_using_parameter_AsNoTrackingWithIdentityResolution(bool async)
    {
        var prm = 0;
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () =>
                AssertQuery(
                    async,
                    ss => ss.Set<JsonEntityBasic>().Select(
                        x => new
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_projection_nested_collection_and_element_using_parameter_AsNoTrackingWithIdentityResolution2(bool async)
    {
        var prm1 = 0;
        var prm2 = 0;
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () =>
                AssertQuery(
                    async,
                    ss => ss.Set<JsonEntityBasic>().Select(
                        x => new
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task
        Json_projection_second_element_through_collection_element_parameter_different_values_projected_before_owner_nested_AsNoTrackingWithIdentityResolution(
            bool async)
    {
        var prm1 = 0;
        var prm2 = 1;

        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () =>
                AssertQuery(
                    async,
                    ss => ss.Set<JsonEntityBasic>().Select(
                        x => new
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task
        Json_projection_second_element_through_collection_element_parameter_projected_before_owner_nested_AsNoTrackingWithIdentityResolution(
            bool async)
    {
        var prm = 0;

        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () =>
                AssertQuery(
                    async,
                    ss => ss.Set<JsonEntityBasic>().Select(
                        x => new
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task
        Json_projection_second_element_through_collection_element_parameter_projected_before_owner_nested_AsNoTrackingWithIdentityResolution2(
            bool async)
    {
        var prm1 = 0;
        var prm2 = 0;

        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () =>
                AssertQuery(
                    async,
                    ss => ss.Set<JsonEntityBasic>().Select(
                        x => new
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task
        Json_projection_second_element_through_collection_element_parameter_projected_after_owner_nested_AsNoTrackingWithIdentityResolution(
            bool async)
    {
        var prm = 0;

        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () =>
                AssertQuery(
                    async,
                    ss => ss.Set<JsonEntityBasic>().Select(
                        x => new
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task
        Json_projection_second_element_through_collection_element_constant_projected_before_owner_nested_AsNoTrackingWithIdentityResolution(
            bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () =>
                AssertQuery(
                    async,
                    ss => ss.Set<JsonEntityBasic>().Select(
                        x => new
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_branch_collection_distinct_and_other_collection_AsNoTrackingWithIdentityResolution(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () =>
                AssertQuery(
                    async,
                    ss => ss.Set<JsonEntityBasic>()
                        .OrderBy(x => x.Id)
                        .Select(
                            x => new
                            {
                                First = x.EntityCollection.ToList(),
                                Second = x.OwnedReferenceRoot.OwnedCollectionBranch.Distinct().ToList()
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_collection_SelectMany_AsNoTrackingWithIdentityResolution(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () =>
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_projection_deduplication_with_collection_indexer_in_target_AsNoTrackingWithIdentityResolution(bool async)
    {
        var prm = 1;
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () =>
                AssertQuery(
                    async,
                    ss => ss.Set<JsonEntityBasic>().Select(
                        x => new
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_projection_nested_collection_and_element_wrong_order_AsNoTrackingWithIdentityResolution(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () =>
                AssertQuery(
                    async,
                    ss => ss.Set<JsonEntityBasic>().Select(
                        x => new
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_projection_second_element_projected_before_entire_collection_AsNoTrackingWithIdentityResolution(
        bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () =>
                AssertQuery(
                    async,
                    ss => ss.Set<JsonEntityBasic>().Select(
                        x => new
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_projection_second_element_projected_before_owner_AsNoTrackingWithIdentityResolution(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () =>
                AssertQuery(
                    async,
                    ss => ss.Set<JsonEntityBasic>().Select(
                        x => new
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_projection_second_element_projected_before_owner_nested_AsNoTrackingWithIdentityResolution(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () =>
                AssertQuery(
                    async,
                    ss => ss.Set<JsonEntityBasic>().Select(
                        x => new
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
}
