// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class JsonQueryTestBase<TFixture> : QueryTestBase<TFixture>
    where TFixture : JsonQueryFixtureBase, new()
{
    protected JsonQueryTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Basic_json_projection_owner_entity(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Basic_json_projection_owner_entity_NoTracking(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().AsNoTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Basic_json_projection_owner_entity_duplicated(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new { First = x, Second = x }),
            elementSorter: e => e.First.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.First, a.First);
                AssertEqual(e.Second, a.Second);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Basic_json_projection_owner_entity_duplicated_NoTracking(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntitySingleOwned>().Select(x => new { First = x, Second = x }).AsNoTracking(),
            elementSorter: e => e.First.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.First, a.First);
                AssertEqual(e.Second, a.Second);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Basic_json_projection_owner_entity_twice(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new { First = x, Second = x }),
            elementSorter: e => e.First.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.First, a.First);
                AssertEqual(e.Second, a.Second);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Basic_json_projection_owner_entity_twice_NoTracking(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new { First = x, Second = x }).AsNoTracking(),
            elementSorter: e => e.First.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.First, a.First);
                AssertEqual(e.Second, a.Second);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Project_json_reference_in_tracking_query_fails(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () =>
                AssertQuery(
                    async,
                    ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedReferenceRoot)))).Message;

        Assert.Equal(
            RelationalStrings.JsonEntityOrCollectionProjectedAtRootLevelInTrackingQuery(
                nameof(EntityFrameworkQueryableExtensions.AsNoTracking)), message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Project_json_collection_in_tracking_query_fails(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () =>
                AssertQuery(
                    async,
                    ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot)))).Message;

        Assert.Equal(
            RelationalStrings.JsonEntityOrCollectionProjectedAtRootLevelInTrackingQuery(
                nameof(EntityFrameworkQueryableExtensions.AsNoTracking)), message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Project_json_entity_in_tracking_query_fails_even_when_owner_is_present(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () =>
                AssertQuery(
                    async,
                    ss => ss.Set<JsonEntityBasic>().Select(
                        x => new
                        {
                            x,
                            x.OwnedReferenceRoot,
                            x.OwnedCollectionRoot
                        })))).Message;

        Assert.Equal(
            RelationalStrings.JsonEntityOrCollectionProjectedAtRootLevelInTrackingQuery(
                nameof(EntityFrameworkQueryableExtensions.AsNoTracking)), message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Basic_json_projection_owned_reference_root(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedReferenceRoot).AsNoTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Basic_json_projection_owned_reference_duplicated2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(
                    x => new
                    {
                        Root1 = x.OwnedReferenceRoot,
                        Leaf1 = x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf,
                        Root2 = x.OwnedReferenceRoot,
                        Leaf2 = x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf,
                    }).AsNoTracking(),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Root1, a.Root1);
                AssertEqual(e.Root2, a.Root2);
                AssertEqual(e.Leaf1, a.Leaf1);
                AssertEqual(e.Leaf2, a.Leaf2);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Basic_json_projection_owned_reference_duplicated(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(
                    x => new
                    {
                        Root1 = x.OwnedReferenceRoot,
                        Branch1 = x.OwnedReferenceRoot.OwnedReferenceBranch,
                        Root2 = x.OwnedReferenceRoot,
                        Branch2 = x.OwnedReferenceRoot.OwnedReferenceBranch,
                    }).AsNoTracking(),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Root1, a.Root1);
                AssertEqual(e.Root2, a.Root2);
                AssertEqual(e.Branch1, a.Branch1);
                AssertEqual(e.Branch2, a.Branch2);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Basic_json_projection_owned_collection_root(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot).AsNoTracking(),
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Basic_json_projection_owned_reference_branch(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedReferenceRoot.OwnedReferenceBranch).AsNoTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Basic_json_projection_owned_collection_branch(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedReferenceRoot.OwnedCollectionBranch).AsNoTracking(),
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Basic_json_projection_owned_reference_leaf(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf).AsNoTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Basic_json_projection_owned_collection_leaf(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf).AsNoTracking(),
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Basic_json_projection_scalar(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedReferenceRoot.Name));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_scalar_length(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Where(x => x.OwnedReferenceRoot.Name.Length > 2).Select(x => x.Name));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Basic_json_projection_enum_inside_json_entity(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(
                x => new
                {
                    x.Id, x.OwnedReferenceRoot.OwnedReferenceBranch.Enum,
                }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Enum, a.Enum);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_projection_enum_with_custom_conversion(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityCustomNaming>().Select(
                x => new
                {
                    x.Id, x.OwnedReferenceRoot.Enum,
                }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Enum, a.Enum);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_projection_with_deduplication(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(
                x => new
                {
                    x.Id,
                    x.OwnedReferenceRoot.OwnedReferenceBranch,
                    x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf,
                    x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf,
                    x.OwnedReferenceRoot.OwnedCollectionBranch,
                    x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf.SomethingSomething
                }).AsNoTracking(),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                AssertEqual(e.OwnedReferenceBranch, a.OwnedReferenceBranch);
                AssertEqual(e.OwnedReferenceLeaf, a.OwnedReferenceLeaf);
                AssertCollection(e.OwnedCollectionLeaf, a.OwnedCollectionLeaf, ordered: true);
                AssertCollection(e.OwnedCollectionBranch, a.OwnedCollectionBranch, ordered: true);
                Assert.Equal(e.SomethingSomething, a.SomethingSomething);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_projection_with_deduplication_reverse_order(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .Select(
                    x => new
                    {
                        x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf,
                        x.OwnedReferenceRoot,
                        x
                    }).AsNoTracking(),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.OwnedReferenceLeaf, a.OwnedReferenceLeaf);
                AssertEqual(e.OwnedReferenceRoot, a.OwnedReferenceRoot);
                AssertEqual(e.x, a.x);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_property_in_predicate(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .Where(x => x.OwnedReferenceRoot.OwnedReferenceBranch.Fraction < 20.5M).Select(x => x.Id));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_subquery_property_pushdown_length(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf.SomethingSomething)
                .Take(3)
                .Distinct()
                .Select(x => x.Length));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_subquery_reference_pushdown_reference(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => x.OwnedReferenceRoot)
                .Take(10)
                .Distinct()
                .Select(x => x.OwnedReferenceBranch).AsNoTracking());

    [ConditionalTheory(Skip = "issue #24263")]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_subquery_reference_pushdown_reference_anonymous_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(
                    x => new
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

    [ConditionalTheory(Skip = "issue #24263")]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_subquery_reference_pushdown_reference_pushdown_anonymous_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(
                    x => new
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
                .Select(
                    x => new
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_subquery_reference_pushdown_property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf)
                .Take(10)
                .Distinct()
                .Select(x => x.SomethingSomething));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Custom_naming_projection_owner_entity(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityCustomNaming>().Select(x => x));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Custom_naming_projection_owned_reference(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityCustomNaming>().Select(x => x.OwnedReferenceRoot.OwnedReferenceBranch).AsNoTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Custom_naming_projection_owned_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityCustomNaming>().OrderBy(x => x.Id).Select(x => x.OwnedCollectionRoot).AsNoTracking(),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Custom_naming_projection_owned_scalar(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityCustomNaming>().Select(x => x.OwnedReferenceRoot.OwnedReferenceBranch.Fraction));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Custom_naming_projection_everything(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityCustomNaming>().Select(
                x => new
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_entity_with_single_owned(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntitySingleOwned>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Left_join_json_entities(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<JsonEntitySingleOwned>()
                  join e2 in ss.Set<JsonEntityBasic>() on e1.Id equals e2.Id into g
                  from e2 in g.DefaultIfEmpty()
                  select new { e1, e2 },
            elementSorter: e => (e.e1.Id, e.e2?.Id),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.e1, a.e1);
                AssertEqual(e.e2, a.e2);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_json_entity_FirstOrDefault_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(
                    x => ss.Set<JsonEntityBasic>()
                        .OrderBy(xx => xx.Id)
                        .Select(xx => xx.OwnedReferenceRoot)
                        .FirstOrDefault().OwnedReferenceBranch)
                .AsNoTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_json_entity_FirstOrDefault_subquery_with_binding_on_top(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(
                    x => ss.Set<JsonEntityBasic>()
                        .OrderBy(xx => xx.Id)
                        .Select(xx => xx.OwnedReferenceRoot)
                        .FirstOrDefault().OwnedReferenceBranch.Date));

    [ConditionalTheory(Skip = "issue #28733")]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_json_entity_FirstOrDefault_subquery_with_entity_comparison_on_top(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(
                    x => ss.Set<JsonEntityBasic>()
                            .OrderBy(xx => xx.Id)
                            .Select(xx => xx.OwnedReferenceRoot)
                            .FirstOrDefault().OwnedReferenceBranch
                        == ss.Set<JsonEntityBasic>()
                            .OrderByDescending(x => x.Id)
                            .Select(
                                x => ss.Set<JsonEntityBasic>()
                                    .OrderBy(xx => xx.Id)
                                    .Select(xx => xx.OwnedReferenceRoot)
                                    .FirstOrDefault().OwnedReferenceBranch)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_json_entity_FirstOrDefault_subquery_deduplication(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(
                    x => ss.Set<JsonEntityBasic>()
                        .OrderBy(xx => xx.Id)
                        .Select(
                            xx => new
                            {
                                x.OwnedReferenceRoot.OwnedCollectionBranch,
                                xx.OwnedReferenceRoot,
                                xx.OwnedReferenceRoot.OwnedReferenceBranch,
                                xx.OwnedReferenceRoot.Name,
                                x.OwnedReferenceRoot.OwnedReferenceBranch.Enum
                            }).FirstOrDefault()).AsNoTracking(),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.OwnedReferenceRoot, a.OwnedReferenceRoot);
                AssertEqual(e.OwnedReferenceBranch, a.OwnedReferenceBranch);
                AssertEqual(e.Name, a.Name);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_json_entity_FirstOrDefault_subquery_deduplication_and_outer_reference(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(
                    x => ss.Set<JsonEntityBasic>()
                        .OrderBy(xx => xx.Id)
                        .Select(
                            xx => new
                            {
                                x.OwnedReferenceRoot.OwnedCollectionBranch,
                                xx.OwnedReferenceRoot,
                                xx.OwnedReferenceRoot.OwnedReferenceBranch,
                                xx.OwnedReferenceRoot.Name,
                                x.OwnedReferenceRoot.OwnedReferenceBranch.Enum
                            }).FirstOrDefault()).AsNoTracking(),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertCollection(e.OwnedCollectionBranch, a.OwnedCollectionBranch, ordered: true);
                AssertEqual(e.OwnedReferenceRoot, a.OwnedReferenceRoot);
                AssertEqual(e.OwnedReferenceBranch, a.OwnedReferenceBranch);
                AssertEqual(e.Name, a.Name);
                AssertEqual(e.Enum, a.Enum);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_json_entity_FirstOrDefault_subquery_deduplication_outer_reference_and_pruning(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(
                    x => ss.Set<JsonEntityBasic>()
                        .OrderBy(xx => xx.Id)
                        .Select(
                            xx => new
                            {
                                x.OwnedReferenceRoot.OwnedCollectionBranch,
                                xx.OwnedReferenceRoot,
                                xx.OwnedReferenceRoot.OwnedReferenceBranch,
                                xx.OwnedReferenceRoot.Name,
                                x.OwnedReferenceRoot.OwnedReferenceBranch.Enum
                            }).FirstOrDefault().OwnedCollectionBranch).AsNoTracking(),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_entity_with_inheritance_basic_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityInheritanceBase>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_entity_with_inheritance_project_derived(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityInheritanceBase>().OfType<JsonEntityInheritanceDerived>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_entity_with_inheritance_project_navigations(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityInheritanceBase>().Select(
                x => new
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_entity_with_inheritance_project_navigations_on_derived(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityInheritanceBase>().OfType<JsonEntityInheritanceDerived>().Select(
                x => new
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

    [ConditionalTheory(Skip = "issue #28645")]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_entity_backtracking(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf.Parent.Date));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_projection_basic(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot[1]).AsNoTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_ElementAt_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot.AsQueryable().ElementAt(1)).AsNoTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_ElementAtOrDefault_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot.AsQueryable().ElementAtOrDefault(1)).AsNoTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_projection_project_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot[1].OwnedCollectionBranch).AsNoTracking(),
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_ElementAt_project_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .Select(x => x.OwnedCollectionRoot.AsQueryable().ElementAt(1).OwnedCollectionBranch)
                .AsNoTracking(),
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_ElementAtOrDefault_project_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .Select(x => x.OwnedCollectionRoot.AsQueryable().ElementAtOrDefault(1).OwnedCollectionBranch)
                .AsNoTracking(),
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_projection_using_parameter(bool async)
    {
        var prm = 0;

        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot[prm]).AsNoTracking());
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_projection_using_column(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot[x.Id]).AsNoTracking());

    private static int MyMethod(int value)
        => value;

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_projection_using_untranslatable_client_method(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot[MyMethod(x.Id)]).AsNoTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_projection_using_untranslatable_client_method2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot[0].OwnedReferenceBranch.OwnedCollectionLeaf[MyMethod(x.Id)])
                .AsNoTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_outside_bounds(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot[25]).AsNoTracking(),
            ss => ss.Set<JsonEntityBasic>().Select(x => (JsonOwnedRoot)null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_outside_bounds2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf[25]).AsNoTracking(),
            ss => ss.Set<JsonEntityBasic>().Select(x => (JsonOwnedLeaf)null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_outside_bounds_with_property_access(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityBasic>().OrderBy(x => x.Id).Select(x => (int?)x.OwnedCollectionRoot[25].Number),
            ss => ss.Set<JsonEntityBasic>().Select(x => (int?)null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_projection_nested(bool async)
    {
        var prm = 1;

        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot[0].OwnedCollectionBranch[prm]).AsNoTracking());
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_projection_nested_project_scalar(bool async)
    {
        var prm = 1;

        return AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot[0].OwnedCollectionBranch[prm].Date));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_projection_nested_project_reference(bool async)
    {
        var prm = 1;

        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot[0].OwnedCollectionBranch[prm].OwnedReferenceLeaf)
                .AsNoTracking());
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_predicate_using_constant(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityBasic>().Where(x => x.OwnedCollectionRoot[0].Name != "Foo").Select(x => x.Id));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_predicate_using_variable(bool async)
    {
        var prm = 1;

        return AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityBasic>().Where(x => x.OwnedCollectionRoot[prm].Name != "Foo").Select(x => x.Id));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_ElementAt_in_predicate(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityBasic>().Where(x => x.OwnedCollectionRoot.AsQueryable().ElementAt(1).Name != "Foo").Select(x => x.Id));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_predicate_nested_mix(bool async)
    {
        var prm = 0;

        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Where(
                x => x.OwnedCollectionRoot[1].OwnedCollectionBranch[prm].OwnedCollectionLeaf[x.Id - 1].SomethingSomething == "e1_c2_c1_c1"));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_ElementAt_and_pushdown(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(
                x => new { x.Id, CollectionElement = x.OwnedCollectionRoot.Select(xx => xx.Number).ElementAt(0) }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_Any_with_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Where(
                j => j.OwnedReferenceRoot.OwnedCollectionBranch.Any(b => b.OwnedReferenceLeaf.SomethingSomething == "e1_r_c1_r")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_Where_ElementAt(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Where(
                j =>
                    j.OwnedReferenceRoot.OwnedCollectionBranch
                        .Where(o => o.Enum == JsonEnum.Three)
                        .ElementAt(0).OwnedReferenceLeaf.SomethingSomething
                    == "e1_r_c2_r"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_Skip(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .Where(
                    j => j.OwnedReferenceRoot.OwnedCollectionBranch
                            .Skip(1)
                            .ElementAt(0).OwnedReferenceLeaf.SomethingSomething
                        == "e1_r_c2_r"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_OrderByDescending_Skip_ElementAt(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .Where(
                    j => j.OwnedReferenceRoot.OwnedCollectionBranch
                            .OrderByDescending(b => b.Date)
                            .Skip(1)
                            .ElementAt(0).OwnedReferenceLeaf.SomethingSomething
                        == "e1_r_c1_r"));

    // If this test is failing because of DistinctAfterOrderByWithoutRowLimitingOperatorWarning, this is because EF warns/errors by
    // default for Distinct after OrderBy (without Skip/Take); but you likely have a naturally-ordered JSON collection, where the
    // ordering has been added by the provider as part of the collection translation.
    // Consider overriding RelationalQueryableMethodTranslatingExpressionVisitor.IsNaturallyOrdered() to identify such naturally-ordered
    // collections, exempting them from the warning.
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_Distinct_Count_with_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .Where(
                    j => j.OwnedReferenceRoot.OwnedCollectionBranch
                            .Distinct()
                            .Count(b => b.OwnedReferenceLeaf.SomethingSomething == "e1_r_c2_r")
                        == 1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_within_collection_Count(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .Where(j => j.OwnedCollectionRoot.Any(c => c.OwnedCollectionBranch.Count == 2)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_in_projection_with_composition_count(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => x.OwnedCollectionRoot.Count));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_in_projection_with_anonymous_projection_of_scalars(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(
                    x => x.OwnedCollectionRoot
                        .Select(xx => new { xx.Name, xx.Number })
                        .ToList()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_in_projection_with_composition_where_and_anonymous_projection_of_scalars(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(
                    x => x.OwnedCollectionRoot
                        .Where(xx => xx.Name == "Foo")
                        .Select(xx => new { xx.Name, xx.Number })
                        .ToList()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_in_projection_with_composition_where_and_anonymous_projection_of_primitive_arrays(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(
                    x => x.OwnedCollectionRoot
                        .Where(xx => xx.Name == "Foo")
                        .Select(xx => new { xx.Names, xx.Numbers })
                        .ToList()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_nested_collection_filter_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(
                    x => x.OwnedCollectionRoot
                        .Select(xx => xx.OwnedCollectionBranch.Where(xxx => xxx.Date != new DateTime(2000, 1, 1)).ToList()))
                .AsNoTracking(),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(
                e, a, ordered: true, elementAsserter: (ee, aa) => AssertCollection(ee, aa, ordered: true)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_nested_collection_anonymous_projection_in_projection(bool async)
        => AssertQuery(
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_skip_take_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => x.OwnedCollectionRoot.OrderBy(xx => xx.Name).Skip(1).Take(5).ToList())
                .AsNoTracking(),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_skip_take_in_projection_project_into_anonymous_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(
                    x => x.OwnedCollectionRoot
                        .OrderBy(xx => xx.Name)
                        .Skip(1)
                        .Take(5)
                        .Select(
                            xx => new
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_skip_take_in_projection_with_json_reference_access_as_final_operation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(
                    x => x.OwnedCollectionRoot
                        .OrderBy(xx => xx.Name)
                        .Skip(1)
                        .Take(5)
                        .Select(xx => xx.OwnedReferenceBranch).ToList())
                .AsNoTracking(),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_distinct_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => x.OwnedCollectionRoot.Distinct())
                .AsNoTracking(),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: ee => (ee.Name, ee.Number)));

    [ConditionalTheory(Skip = "issue #31397")]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_anonymous_projection_distinct_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(x => x.OwnedCollectionRoot.Select(xx => xx.Name).Distinct().ToList())
                .AsNoTracking(),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: ee => ee));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_leaf_filter_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(
                    x => x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf
                        .Where(xx => xx.SomethingSomething != "Baz").ToList())
                .AsNoTracking(),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_multiple_collection_projections(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(
                    x => new
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_branch_collection_distinct_and_other_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(
                    x => new
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_leaf_collection_distinct_and_other_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .OrderBy(x => x.Id)
                .Select(
                    x => new
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_SelectMany(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .SelectMany(x => x.OwnedCollectionRoot)
                .AsNoTracking(),
            elementSorter: e => (e.Number, e.Name));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_nested_collection_SelectMany(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .SelectMany(x => x.OwnedReferenceRoot.OwnedCollectionBranch)
                .AsNoTracking(),
            elementSorter: e => (e.Enum, e.Date, e.NullableEnum, e.Fraction));

    [ConditionalTheory(Skip = "issue #31364")]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_of_primitives_SelectMany(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .SelectMany(x => x.OwnedReferenceRoot.Names));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_of_primitives_index_used_in_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Where(x => x.OwnedReferenceRoot.Names[0] == "e1_r1"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_of_primitives_index_used_in_projection(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityBasic>().OrderBy(x => x.Id).Select(x => x.OwnedReferenceRoot.OwnedReferenceBranch.Enums[0]),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_of_primitives_index_used_in_orderby(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().OrderBy(x => x.OwnedReferenceRoot.Numbers[0]),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_of_primitives_contains_in_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Where(x => x.OwnedReferenceRoot.Names.Contains("e1_r1")),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_collection_index_with_parameter_Select_ElementAt(bool async)
    {
        var prm = 0;

        await AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(
                x => new { x.Id, CollectionElement = x.OwnedCollectionRoot[prm].OwnedCollectionBranch.Select(xx => "Foo").ElementAt(0) }));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_collection_index_with_expression_Select_ElementAt(bool async)
    {
        var prm = 0;

        await AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(
                j => j.OwnedCollectionRoot[prm + j.Id].OwnedCollectionBranch
                    .Select(b => b.OwnedReferenceLeaf.SomethingSomething)
                    .ElementAt(0)),
            ss => ss.Set<JsonEntityBasic>().Select(
                j => j.OwnedCollectionRoot.Count > prm + j.Id
                    ? j.OwnedCollectionRoot[prm + j.Id].OwnedCollectionBranch
                        .Select(b => b.OwnedReferenceLeaf.SomethingSomething)
                        .ElementAt(0)
                    : null));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_collection_Select_entity_ElementAt(bool async)
        => await AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().AsNoTracking().Select(
                x =>
                    x.OwnedCollectionRoot.Select(xx => xx.OwnedReferenceBranch).ElementAt(0)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_collection_Select_entity_in_anonymous_object_ElementAt(bool async)
        => await AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().AsNoTracking().OrderBy(x => x.Id).Select(
                x =>
                    x.OwnedCollectionRoot.Select(xx => new { xx.OwnedReferenceBranch }).ElementAt(0)),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.OwnedReferenceBranch, a.OwnedReferenceBranch);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_collection_Select_entity_with_initializer_ElementAt(bool async)
        => await AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(
                x => x.OwnedCollectionRoot.Select(xx => new JsonEntityBasic { Id = x.Id }).ElementAt(0)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_projection_deduplication_with_collection_indexer_in_original(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(
                x => new
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_projection_deduplication_with_collection_indexer_in_target(bool async)
    {
        var prm = 1;

        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(
                x => new
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_projection_deduplication_with_collection_in_original_and_collection_indexer_in_target(bool async)
    {
        var prm = 1;

        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(
                x => new
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_projection_when_owner_is_present_misc1(bool async)
    {
        var prm = 1;

        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(
                x => new
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_projection_when_owner_is_not_present_misc1(bool async)
    {
        var prm = 1;

        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(
                x => new
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_projection_when_owner_is_present_multiple(bool async)
    {
        var prm = 1;

        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(
                x => new
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_index_in_projection_when_owner_is_not_present_multiple(bool async)
    {
        var prm = 1;

        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(
                x => new
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_scalar_required_null_semantics(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .Where(x => x.OwnedReferenceRoot.Number != x.OwnedReferenceRoot.Name.Length)
                .Select(x => x.Name));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_scalar_optional_null_semantics(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .Where(x => x.OwnedReferenceRoot.Name != x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf.SomethingSomething)
                .Select(x => x.Name));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Group_by_on_json_scalar(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .GroupBy(x => x.OwnedReferenceRoot.Name).Select(x => new { x.Key, Count = x.Count() }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Group_by_on_json_scalar_using_collection_indexer(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .GroupBy(x => x.OwnedCollectionRoot[0].Name).Select(x => new { x.Key, Count = x.Count() }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Group_by_First_on_json_scalar(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .GroupBy(x => x.OwnedReferenceRoot.Name).Select(g => g.OrderBy(x => x.Id).First()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Group_by_FirstOrDefault_on_json_scalar(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .GroupBy(x => x.OwnedReferenceRoot.Name).Select(g => g.OrderBy(x => x.Id).FirstOrDefault()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Group_by_Skip_Take_on_json_scalar(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .GroupBy(x => x.OwnedReferenceRoot.Name).Select(g => g.OrderBy(x => x.Id).Skip(1).Take(5)));

    [ConditionalTheory(Skip = "issue #29287")]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Group_by_json_scalar_Orderby_json_scalar_FirstOrDefault(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .GroupBy(x => x.OwnedReferenceRoot.OwnedReferenceBranch.Enum)
                .Select(g => g.OrderBy(x => x.OwnedReferenceRoot.Number).FirstOrDefault()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Group_by_json_scalar_Skip_First_project_json_scalar(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .GroupBy(x => x.OwnedReferenceRoot.Name).Select(g => g.First().OwnedReferenceRoot.OwnedReferenceBranch.Enum));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_with_include_on_json_entity(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Include(x => x.OwnedReferenceRoot));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_with_include_on_entity_reference(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Include(x => x.EntityReference),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<JsonEntityBasic>(x => x.EntityReference)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_with_include_on_entity_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Include(x => x.EntityCollection),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<JsonEntityBasic>(x => x.EntityCollection)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Entity_including_collection_with_json(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityBasic>().Include(e => e.JsonEntityBasics),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<EntityBasic>(x => x.JsonEntityBasics)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_with_include_on_entity_collection_and_reference(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Include(x => x.EntityReference).Include(x => x.EntityCollection),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<JsonEntityBasic>(x => x.EntityReference),
                new ExpectedInclude<JsonEntityBasic>(x => x.EntityCollection)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_with_projection_of_json_reference_and_entity_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new { x.OwnedReferenceRoot, x.EntityCollection }).AsNoTracking(),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.OwnedReferenceRoot, a.OwnedReferenceRoot);
                AssertCollection(e.EntityCollection, a.EntityCollection);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_with_projection_of_multiple_json_references_and_entity_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(
                x => new
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_with_projection_of_json_collection_and_entity_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new { x.OwnedCollectionRoot, x.EntityCollection }).AsNoTracking(),
            elementAsserter: (e, a) =>
            {
                AssertCollection(e.OwnedCollectionRoot, a.OwnedCollectionRoot, ordered: true);
                AssertCollection(e.EntityCollection, a.EntityCollection);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_with_projection_of_json_collection_element_and_entity_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(
                x => new
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_with_projection_of_mix_of_json_collections_json_references_and_entity_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(
                x => new
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_all_types_entity_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_all_types_projection_from_owned_entity_reference(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Select(x => x.Reference).AsNoTracking(),
            elementSorter: e => e.TestInt32,
            elementAsserter: (e, a) => AssertEqual(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_all_types_projection_individual_properties(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Select(
                x => new
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_boolean_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestBoolean));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_boolean_predicate_negated(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => !x.Reference.TestBoolean),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_boolean_projection(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Select(x => x.Reference.TestBoolean));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_boolean_projection_negated(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Select(x => !x.Reference.TestBoolean));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_default_string(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestDefaultString != "MyDefaultStringInReference1"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_max_length_string(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestMaxLengthString != "Foo"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_string_condition(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(
                x => (!x.Reference.TestBoolean ? x.Reference.TestMaxLengthString : x.Reference.TestDefaultString)
                    == "MyDefaultStringInReference1"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_byte(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestByte != 3));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_character(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestCharacter != 'z'));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_datetime(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestDateTime != new DateTime(2000, 1, 3)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_datetimeoffset(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(
                x => x.Reference.TestDateTimeOffset != new DateTimeOffset(new DateTime(2000, 1, 4), new TimeSpan(3, 2, 0))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_decimal(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestDecimal != 1.35M));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_double(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestDouble != 33.25));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_guid(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestGuid != new Guid()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_int16(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestInt16 != 3));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_int32(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestInt32 != 33));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_int64(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestInt64 != 333));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_signedbyte(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestSignedByte != 100));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_single(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestSingle != 10.4f));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_timespan(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestTimeSpan != new TimeSpan(3, 2, 0)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_dateonly(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestDateOnly != new DateOnly(3, 2, 1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_timeonly(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestTimeOnly != new TimeOnly(3, 2, 0)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_unisgnedint16(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestUnsignedInt16 != 100));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_unsignedint32(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestUnsignedInt32 != 1000));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_unsignedint64(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestUnsignedInt64 != 10000));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_enum(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestEnum != JsonEnum.Two));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_enumwithintconverter(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestEnumWithIntConverter != JsonEnum.Three));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_nullableenum1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestNullableEnum != JsonEnum.One));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_nullableenum2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestNullableEnum != null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_nullableenumwithconverterthathandlesnulls1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestNullableEnumWithConverterThatHandlesNulls != JsonEnum.One));

    [ConditionalTheory(Skip = "issue #29416")]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_nullableenumwithconverterthathandlesnulls2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestNullableEnumWithConverterThatHandlesNulls != null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_nullableenumwithconverter1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestNullableEnumWithIntConverter != JsonEnum.Two));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_nullableenumwithconverter2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestNullableEnumWithIntConverter != null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_nullableint321(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestNullableInt32 != 100));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_nullableint322(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestNullableInt32 != null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_bool_converted_to_int_zero_one(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityConverters>().Where(x => x.Reference.BoolConvertedToIntZeroOne));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_bool_converted_to_int_zero_one_with_explicit_comparison(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityConverters>().Where(x => x.Reference.BoolConvertedToIntZeroOne == false));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_bool_converted_to_string_True_False(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityConverters>().Where(x => x.Reference.BoolConvertedToStringTrueFalse));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_bool_converted_to_string_True_False_with_explicit_comparison(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityConverters>().Where(x => x.Reference.BoolConvertedToStringTrueFalse == true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_bool_converted_to_string_Y_N(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityConverters>().Where(x => x.Reference.BoolConvertedToStringYN));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_bool_converted_to_string_Y_N_with_explicit_comparison(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityConverters>().Where(x => x.Reference.BoolConvertedToStringYN == false));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_int_zero_one_converted_to_bool(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityConverters>().Where(x => x.Reference.IntZeroOneConvertedToBool == 1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_string_True_False_converted_to_bool(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityConverters>().Where(x => x.Reference.StringTrueFalseConvertedToBool == "False"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_string_Y_N_converted_to_bool(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityConverters>().Where(x => x.Reference.StringYNConvertedToBool == "N"));

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
}
