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
            .Select(x => new
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
            ss => ss.Set<JsonEntityBasic>().Select(x => new
            {
                x.Id,
                Enum = x.OwnedReferenceRoot.OwnedReferenceBranch.Enum,
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
            ss => ss.Set<JsonEntityCustomNaming>().Select(x => new
            {
                x.Id,
                Enum = x.OwnedReferenceRoot.Enum,
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
            ss => ss.Set<JsonEntityBasic>().Select(x => new
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
            ss => ss.Set<JsonEntityBasic>().Select(x => new
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
                .Select(x => new { Entity = x.OwnedReferenceRoot, Scalar = x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf.SomethingSomething })
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
                .Select(x => new { Root = x.OwnedReferenceRoot, Scalar = x.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf.SomethingSomething })
                .Take(10)
                .Distinct()
                .Select(x => new { Branch = x.Root.OwnedReferenceBranch, x.Scalar.Length })
                .OrderBy(x => x.Length)
                .Take(10)
                .Distinct()
                .Select(x => new { x.Branch.OwnedReferenceLeaf, x.Branch.OwnedCollectionLeaf, x.Length })
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
            ss => ss.Set<JsonEntityCustomNaming>().Select(x => new
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
    public virtual Task Project_json_entity_FirstOrDefault_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>()
            .OrderBy(x => x.Id)
            .Select(x => ss.Set<JsonEntityBasic>()
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
            .Select(x => ss.Set<JsonEntityBasic>()
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
            .Select(x => ss.Set<JsonEntityBasic>()
                .OrderBy(xx => xx.Id)
                .Select(xx => xx.OwnedReferenceRoot)
                .FirstOrDefault().OwnedReferenceBranch == ss.Set<JsonEntityBasic>()
            .OrderByDescending(x => x.Id)
            .Select(x => ss.Set<JsonEntityBasic>()
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
                .Select(x => ss.Set<JsonEntityBasic>()
                    .OrderBy(xx => xx.Id)
                    .Select(xx => new
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
                .Select(x => ss.Set<JsonEntityBasic>()
                    .OrderBy(xx => xx.Id)
                    .Select(xx => new
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
                .Select(x => ss.Set<JsonEntityBasic>()
                    .OrderBy(xx => xx.Id)
                    .Select(xx => new
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_entity_with_inheritance_project_navigations_on_derived(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityInheritanceBase>().OfType<JsonEntityInheritanceDerived>().Select(x => new
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
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedCollectionRoot[0]).AsNoTracking());

    [ConditionalTheory(Skip = "issue #28648")]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Json_collection_element_access_in_predicate(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<JsonEntityBasic>().Where(x => x.OwnedCollectionRoot[0].Name != "Foo").Select(x => x.Id));

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


}
