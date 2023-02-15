// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

namespace Microsoft.EntityFrameworkCore.Query;

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
            ss => ss.Set<JsonEntityBasic>(),
            entryCount: 40);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Basic_json_projection_owned_reference_root(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedReferenceRoot).AsNoTracking());

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
                    x.Id,
                    x.OwnedReferenceRoot.OwnedReferenceBranch.Enum,
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
                    x,
                    x.OwnedReferenceRoot.OwnedReferenceBranch,
                    x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf,
                    x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf,
                    x.OwnedReferenceRoot.OwnedCollectionBranch,
                    x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf.SomethingSomething
                }),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.OwnedReferenceBranch, a.OwnedReferenceBranch);
                AssertEqual(e.OwnedReferenceLeaf, a.OwnedReferenceLeaf);
                AssertCollection(e.OwnedCollectionLeaf, a.OwnedCollectionLeaf, ordered: true);
                AssertCollection(e.OwnedCollectionBranch, a.OwnedCollectionBranch, ordered: true);
                Assert.Equal(e.SomethingSomething, a.SomethingSomething);
            },
            entryCount: 40);

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
            ss => ss.Set<JsonEntityCustomNaming>().Select(x => x),
            entryCount: 13);

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
                }),
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
            },
            entryCount: 13);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_entity_with_single_owned(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntitySingleOwned>(),
            entryCount: 8);

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
            },
            entryCount: 48);

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
                       e2,
                       e2.OwnedReferenceRoot,
                       e2.OwnedReferenceRoot.OwnedReferenceBranch,
                       e2.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf,
                       e2.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf
                   }),
            elementSorter: e => (e.Id1, e?.Id2),
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id1, a.Id1);
                Assert.Equal(e.Id2, a.Id2);
                AssertEqual(e.e2, a.e2);
                AssertEqual(e.OwnedReferenceRoot, a.OwnedReferenceRoot);
                AssertEqual(e.OwnedReferenceBranch, a.OwnedReferenceBranch);
                AssertEqual(e.OwnedReferenceLeaf, a.OwnedReferenceLeaf);
                AssertCollection(e.OwnedCollectionLeaf, a.OwnedCollectionLeaf, ordered: true);
            },
            entryCount: 40);

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
            },
            entryCount: 44);

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
                       e1,
                       e1.OwnedReferenceRoot,
                       e1.OwnedReferenceRoot.OwnedReferenceBranch,
                       e1.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf,
                       e1.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf,
                       e2,
                       e2.Name
                   }),
            elementSorter: e => (e.Id1, e?.Id2),
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id1, a.Id1);
                Assert.Equal(e.Id2, a.Id2);
                AssertEqual(e.e1, a.e1);
                AssertEqual(e.OwnedReferenceRoot, a.OwnedReferenceRoot);
                AssertEqual(e.OwnedReferenceBranch, a.OwnedReferenceBranch);
                AssertEqual(e.OwnedReferenceLeaf, a.OwnedReferenceLeaf);
                AssertCollection(e.OwnedCollectionLeaf, a.OwnedCollectionLeaf, ordered: true);
                AssertEqual(e.e2, a.e2);
                AssertEqual(e.Name, a.Name);
            },
            entryCount: 44);

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
            ss => ss.Set<JsonEntityInheritanceBase>(),
            entryCount: 38);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_entity_with_inheritance_project_derived(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityInheritanceBase>().OfType<JsonEntityInheritanceDerived>(),
            entryCount: 25);

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
                    x,
                    x.ReferenceOnBase,
                    x.ReferenceOnDerived,
                    x.CollectionOnBase,
                    x.CollectionOnDerived
                }),
            elementSorter: e => e.x.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.x, a.x);
                AssertEqual(e.ReferenceOnBase, a.ReferenceOnBase);
                AssertEqual(e.ReferenceOnDerived, a.ReferenceOnDerived);
                AssertCollection(e.CollectionOnBase, a.CollectionOnBase, ordered: true);
                AssertCollection(e.CollectionOnDerived, a.CollectionOnDerived, ordered: true);
            },
            entryCount: 25);

    [ConditionalTheory(Skip = "issue #28645")]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_entity_backtracking(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf.Parent.Date));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_element_access_in_projection_basic(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot[1]).AsNoTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_element_access_in_projection_using_ElementAt(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot.AsQueryable().ElementAt(1)).AsNoTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_element_access_in_projection_using_ElementAtOrDefault(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot.AsQueryable().ElementAtOrDefault(1)).AsNoTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_element_access_in_projection_project_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot[1].OwnedCollectionBranch).AsNoTracking(),
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_element_access_in_projection_using_ElementAt_project_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .Select(x => x.OwnedCollectionRoot.AsQueryable().ElementAt(1).OwnedCollectionBranch)
                .AsNoTracking(),
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_element_access_in_projection_using_ElementAtOrDefault_project_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .Select(x => x.OwnedCollectionRoot.AsQueryable().ElementAtOrDefault(1).OwnedCollectionBranch)
                .AsNoTracking(),
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_element_access_in_projection_using_parameter(bool async)
    {
        var prm = 0;

        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot[prm]).AsNoTracking());
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_element_access_in_projection_using_column(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot[x.Id]).AsNoTracking());

    private static int MyMethod(int value)
        => value;

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_collection_element_access_in_projection_using_untranslatable_client_method(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot[MyMethod(x.Id)]).AsNoTracking()))).Message;

        Assert.Equal(
            CoreStrings.TranslationFailed("JsonQueryExpression(j.OwnedCollectionRoot, $)"),
            message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_collection_element_access_in_projection_using_untranslatable_client_method2(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot[0].OwnedReferenceBranch.OwnedCollectionLeaf[MyMethod(x.Id)]).AsNoTracking()))).Message;

        Assert.Equal(
            CoreStrings.TranslationFailed("JsonQueryExpression(j.OwnedCollectionRoot, $.[0].OwnedReferenceBranch.OwnedCollectionLeaf)"),
            message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_element_access_outside_bounds(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot[25]).AsNoTracking(),
            ss => ss.Set<JsonEntityBasic>().Select(x => (JsonOwnedRoot)null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_element_access_outside_bounds_with_property_access(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityBasic>().OrderBy(x => x.Id).Select(x => (int?)x.OwnedCollectionRoot[25].Number),
            ss => ss.Set<JsonEntityBasic>().Select(x => (int?)null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_element_access_in_projection_nested(bool async)
    {
        var prm = 1;

        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot[0].OwnedCollectionBranch[prm]).AsNoTracking());
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_element_access_in_projection_nested_project_scalar(bool async)
    {
        var prm = 1;

        return AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot[0].OwnedCollectionBranch[prm].Date));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_element_access_in_projection_nested_project_reference(bool async)
    {
        var prm = 1;

        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot[0].OwnedCollectionBranch[prm].OwnedReferenceLeaf).AsNoTracking());
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_element_access_in_projection_nested_project_collection(bool async)
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
    public virtual Task Json_collection_element_access_in_projection_nested_project_collection_anonymous_projection(bool async)
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
    public virtual Task Json_collection_element_access_in_predicate_using_constant(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityBasic>().Where(x => x.OwnedCollectionRoot[0].Name != "Foo").Select(x => x.Id));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_element_access_in_predicate_using_variable(bool async)
    {
        var prm = 1;

        return AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityBasic>().Where(x => x.OwnedCollectionRoot[prm].Name != "Foo").Select(x => x.Id));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_element_access_in_predicate_using_column(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Where(x => x.OwnedCollectionRoot[x.Id].Name == "e1_c2").Select(x => new { x.Id, x }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                AssertEqual(e.x, a.x);
            },
            entryCount: 40);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_element_access_in_predicate_using_complex_expression1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Where(x => x.OwnedCollectionRoot[x.Id == 1 ? 0 : 1].Name == "e1_c1").Select(x => new { x.Id, x }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                AssertEqual(e.x, a.x);
            },
            entryCount: 40);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_element_access_in_predicate_using_complex_expression2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Where(x => x.OwnedCollectionRoot[ss.Set<JsonEntityBasic>().Max(x => x.Id)].Name == "e1_c2").Select(x => new { x.Id, x }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                AssertEqual(e.x, a.x);
            },
            entryCount: 40);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_element_access_in_predicate_using_ElementAt(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityBasic>().Where(x => x.OwnedCollectionRoot.AsQueryable().ElementAt(1).Name != "Foo").Select(x => x.Id));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_element_access_in_predicate_nested_mix(bool async)
    {
        var prm = 0;

        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Where(x => x.OwnedCollectionRoot[1].OwnedCollectionBranch[prm].OwnedCollectionLeaf[x.Id - 1].SomethingSomething == "e1_c2_c1_c1"),
            entryCount: 40);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_element_access_manual_Element_at_and_pushdown(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new
            {
                x.Id,
                CollectionElement = x.OwnedCollectionRoot.Select(xx => xx.Number).ElementAt(0)
            }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_collection_element_access_manual_Element_at_and_pushdown_negative(bool async)
    {
        var prm = 0;
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => AssertQuery(
                async,
                ss => ss.Set<JsonEntityBasic>().Select(x => new
                {
                    x.Id,
                    CollectionElement = x.OwnedCollectionRoot[prm].OwnedCollectionBranch.Select(xx => "Foo").ElementAt(0)
                })))).Message;

        Assert.Equal(CoreStrings.TranslationFailed("JsonQueryExpression(j.OwnedCollectionRoot, $.[__prm_0].OwnedCollectionBranch)"), message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_collection_element_access_manual_Element_at_and_pushdown_negative2(bool async)
    {
        var prm = 0;
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => AssertQuery(
                async,
                ss => ss.Set<JsonEntityBasic>().Select(x => new
                {
                    x.Id,
                    CollectionElement = x.OwnedCollectionRoot[prm + x.Id].OwnedCollectionBranch.Select(xx => x.Id).ElementAt(0)
                })))).Message;

        Assert.Equal(CoreStrings.TranslationFailed("JsonQueryExpression(j.OwnedCollectionRoot, $.[(...)].OwnedCollectionBranch)"), message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_collection_element_access_manual_Element_at_and_pushdown_negative3(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => AssertQuery(
                async,
                ss => ss.Set<JsonEntityBasic>().Select(x => new
                {
                    x.Id,
                    CollectionElement = x.OwnedCollectionRoot.Select(xx => x.OwnedReferenceRoot).ElementAt(0)
                })))).Message;

        Assert.Equal(CoreStrings.TranslationFailed("JsonQueryExpression(j.OwnedCollectionRoot, $)"), message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_collection_element_access_manual_Element_at_and_pushdown_negative4(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => AssertQuery(
                async,
                ss => ss.Set<JsonEntityBasic>().Select(x => new
                {
                    x.Id,
                    CollectionElement = x.OwnedCollectionRoot.Select(xx => x.OwnedCollectionRoot).ElementAt(0)
                })))).Message;

        Assert.Equal(CoreStrings.TranslationFailed("JsonQueryExpression(j.OwnedCollectionRoot, $)"), message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_collection_element_access_manual_Element_at_and_pushdown_negative5(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => AssertQuery(
                async,
                ss => ss.Set<JsonEntityBasic>().Select(x => new
                {
                    x.Id,
                    CollectionElement = x.OwnedCollectionRoot.Select(xx => new { xx.OwnedReferenceBranch }).ElementAt(0)
                })))).Message;

        Assert.Equal(CoreStrings.TranslationFailed("JsonQueryExpression(j.OwnedCollectionRoot, $)"), message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_collection_element_access_manual_Element_at_and_pushdown_negative6(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => AssertQuery(
                async,
                ss => ss.Set<JsonEntityBasic>().Select(x => new
                {
                    x.Id,
                    CollectionElement = x.OwnedCollectionRoot.Select(xx => new JsonEntityBasic { Id = x.Id }).ElementAt(0)
                })))).Message;

        Assert.Equal(CoreStrings.TranslationFailed("JsonQueryExpression(j.OwnedCollectionRoot, $)"), message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_element_access_in_projection_using_constant_when_owner_is_present(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new
            {
                x,
                CollectionElement = x.OwnedCollectionRoot[1]
            }),
            elementSorter: e => e.x.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.x, a.x);
                AssertEqual(e.CollectionElement, a.CollectionElement);
            },
            entryCount: 40);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_element_access_in_projection_using_parameter_when_owner_is_present(bool async)
    {
        var prm = 1;

        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new
            {
                x,
                CollectionElement = x.OwnedCollectionRoot[prm]
            }),
            elementSorter: e => e.x.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.x, a.x);
                AssertEqual(e.CollectionElement, a.CollectionElement);
            },
            entryCount: 40);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_after_collection_element_access_in_projection_using_constant_when_owner_is_present(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new
            {
                x,
                Collection = x.OwnedCollectionRoot[1].OwnedCollectionBranch
            }),
            elementSorter: e => e.x.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.x, a.x);
                AssertCollection(e.Collection, a.Collection, ordered: true);
            },
            entryCount: 40);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_after_collection_element_access_in_projection_using_parameter_when_owner_is_present(bool async)
    {
        var prm = 1;

        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new
            {
                x,
                Collection = x.OwnedCollectionRoot[prm].OwnedCollectionBranch
            }),
            elementSorter: e => e.x.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.x, a.x);
                AssertCollection(e.Collection, a.Collection, ordered: true);
            },
            entryCount: 40);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_element_access_in_projection_when_owner_is_present_misc1(bool async)
    {
        var prm = 1;

        return AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new
            {
                x,
                CollectionElement = x.OwnedCollectionRoot[1].OwnedCollectionBranch[prm],
            }),
            elementSorter: e => e.x.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.x, a.x);
                AssertEqual(e.CollectionElement, a.CollectionElement);
            },
            entryCount: 40);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_element_access_in_projection_when_owner_is_present_misc2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Select(x => new
            {
                x,
                CollectionElement = x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf[1]
            }),
            elementSorter: e => e.x.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.x, a.x);
                AssertEqual(e.CollectionElement, a.CollectionElement);
            },
            entryCount: 40);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_element_access_in_projection_when_owner_is_present_multiple(bool async)
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
            }),
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
            },
            entryCount: 40);
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
                .Where(x => x.OwnedReferenceRoot.Name == x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf.SomethingSomething)
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
                .GroupBy(x => x.OwnedReferenceRoot.Name).Select(g => g.OrderBy(x => x.Id).First()),
            entryCount: 40);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Group_by_FirstOrDefault_on_json_scalar(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
                .GroupBy(x => x.OwnedReferenceRoot.Name).Select(g => g.OrderBy(x => x.Id).FirstOrDefault()),
            entryCount: 40);

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
                .GroupBy(x => x.OwnedReferenceRoot.OwnedReferenceBranch.Enum).Select(g => g.OrderBy(x => x.OwnedReferenceRoot.Number).FirstOrDefault()),
            entryCount: 40);

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
            ss => ss.Set<JsonEntityBasic>().Include(x => x.OwnedReferenceRoot),
            entryCount: 40);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_with_include_on_entity_reference(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Include(x => x.EntityReference),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<JsonEntityBasic>(x => x.EntityReference)),
            entryCount: 41);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_with_include_on_entity_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Include(x => x.EntityCollection),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<JsonEntityBasic>(x => x.EntityCollection)),
            entryCount: 43);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Entity_including_collection_with_json(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityBasic>().Include(e => e.JsonEntityBasics),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<EntityBasic>(x => x.JsonEntityBasics)),
            entryCount: 41);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_with_include_on_entity_collection_and_reference(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Include(x => x.EntityReference).Include(x => x.EntityCollection),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<JsonEntityBasic>(x => x.EntityReference),
                new ExpectedInclude<JsonEntityBasic>(x => x.EntityCollection)),
            entryCount: 44);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_all_types_entity_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>(),
            entryCount: 6);

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
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestBoolean),
            entryCount: 6);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_boolean_predicate_negated(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => !x.Reference.TestBoolean),
            entryCount: 0);

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
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestDefaultString != "MyDefaultStringInReference1"),
            entryCount: 3);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_max_length_string(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestMaxLengthString != "Foo"),
            entryCount: 3);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_string_condition(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => (!x.Reference.TestBoolean ? x.Reference.TestMaxLengthString : x.Reference.TestDefaultString) == "MyDefaultStringInReference1"),
            entryCount: 3);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_byte(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestByte != 3),
            entryCount: 6);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_character(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestCharacter != 'z'),
            entryCount: 6);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_datetime(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestDateTime != new DateTime(2000, 1, 3)),
            entryCount: 6);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_datetimeoffset(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestDateTimeOffset != new DateTimeOffset(new DateTime(2000, 1, 4), new TimeSpan(3, 2, 0))),
            entryCount: 6);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_decimal(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestDecimal != 1.35M),
            entryCount: 6);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_double(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestDouble != 33.25),
            entryCount: 6);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_guid(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestGuid != new Guid()),
            entryCount: 6);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_int16(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestInt16 != 3),
            entryCount: 6);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_int32(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestInt32 != 33),
            entryCount: 6);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_int64(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestInt64 != 333),
            entryCount: 6);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_signedbyte(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestSignedByte != 100),
            entryCount: 6);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_single(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestSingle != 10.4f),
            entryCount: 6);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_timespan(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestTimeSpan != new TimeSpan(3, 2, 0)),
            entryCount: 6);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_unisgnedint16(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestUnsignedInt16 != 100),
            entryCount: 6);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_unsignedint32(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestUnsignedInt32 != 1000),
            entryCount: 6);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_unsignedint64(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestUnsignedInt64 != 10000),
            entryCount: 6);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_enum(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestEnum != JsonEnum.Two),
            entryCount: 3);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_enumwithintconverter(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestEnumWithIntConverter != JsonEnum.Three),
            entryCount: 3);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_nullableenum1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestNullableEnum != JsonEnum.One),
            entryCount: 3);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_nullableenum2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestNullableEnum != null),
            entryCount: 3);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_nullableenumwithconverterthathandlesnulls1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestNullableEnumWithConverterThatHandlesNulls != JsonEnum.One),
            entryCount: 6);

    [ConditionalTheory(Skip = "issue #29416")]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_nullableenumwithconverterthathandlesnulls2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestNullableEnumWithConverterThatHandlesNulls != null),
            entryCount: 6);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_nullableenumwithconverter1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestNullableEnumWithIntConverter != JsonEnum.Two),
            entryCount: 3);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_nullableenumwithconverter2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestNullableEnumWithIntConverter != null),
            entryCount: 3);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_nullableint321(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestNullableInt32 != 100),
            entryCount: 6);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_nullableint322(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityAllTypes>().Where(x => x.Reference.TestNullableInt32 != null),
            entryCount: 3);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_bool_converted_to_int_zero_one(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityConverters>().Where(x => x.Reference.BoolConvertedToIntZeroOne),
            entryCount: 2);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_bool_converted_to_int_zero_one_with_explicit_comparison(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityConverters>().Where(x => x.Reference.BoolConvertedToIntZeroOne == false),
            entryCount: 2);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_bool_converted_to_string_True_False(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityConverters>().Where(x => x.Reference.BoolConvertedToStringTrueFalse),
            entryCount: 2);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_bool_converted_to_string_True_False_with_explicit_comparison(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityConverters>().Where(x => x.Reference.BoolConvertedToStringTrueFalse == true),
            entryCount: 2);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_bool_converted_to_string_Y_N(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityConverters>().Where(x => x.Reference.BoolConvertedToStringYN),
            entryCount: 2);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_bool_converted_to_string_Y_N_with_explicit_comparison(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityConverters>().Where(x => x.Reference.BoolConvertedToStringYN == false),
            entryCount: 2);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_int_zero_one_converted_to_bool(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityConverters>().Where(x => x.Reference.IntZeroOneConvertedToBool == 1),
            entryCount: 2);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_string_True_False_converted_to_bool(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityConverters>().Where(x => x.Reference.StringTrueFalseConvertedToBool == "False"),
            entryCount: 2);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_predicate_on_string_Y_N_converted_to_bool(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityConverters>().Where(x => x.Reference.StringYNConvertedToBool == "N"),
            entryCount: 2);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSql_on_entity_with_json_basic(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<JsonEntityBasic>)ss.Set<JsonEntityBasic>()).FromSqlRaw(
                Fixture.TestStore.NormalizeDelimitersInRawString("SELECT * FROM [JsonEntitiesBasic] AS j")),
            ss => ss.Set<JsonEntityBasic>(),
            entryCount: 40);

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
            ss => ss.Set<JsonEntityInheritanceBase>(),
            entryCount: 38);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSql_on_entity_with_json_inheritance_on_derived(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<JsonEntityInheritanceDerived>)ss.Set<JsonEntityInheritanceDerived>()).FromSqlRaw(
                Fixture.TestStore.NormalizeDelimitersInRawString("SELECT * FROM [JsonEntitiesInheritance] AS j")),
            ss => ss.Set<JsonEntityInheritanceDerived>(),
            entryCount: 25);

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
