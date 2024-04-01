// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.InheritanceRelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class InheritanceRelationshipsQueryRelationalTestBase<TFixture>(TFixture fixture) : InheritanceRelationshipsQueryTestBase<TFixture>(fixture)
    where TFixture : InheritanceRelationshipsQueryRelationalFixture, new()
{
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_with_inheritance_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BaseInheritanceRelationshipEntity>().Include(e => e.BaseCollectionOnBase).AsSplitQuery(),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<BaseInheritanceRelationshipEntity>(x => x.BaseCollectionOnBase)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_with_inheritance_reverse_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BaseCollectionOnBase>().Include(e => e.BaseParent).AsSplitQuery(),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<BaseCollectionOnBase>(x => x.BaseParent)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_with_inheritance_with_filter_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BaseInheritanceRelationshipEntity>().Include(e => e.BaseCollectionOnBase).Where(e => e.Name != "Bar")
                .AsSplitQuery(),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<BaseInheritanceRelationshipEntity>(x => x.BaseCollectionOnBase)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_with_inheritance_with_filter_reverse_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BaseCollectionOnBase>().Include(e => e.BaseParent).Where(e => e.Name != "Bar").AsSplitQuery(),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<BaseCollectionOnBase>(x => x.BaseParent)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_without_inheritance_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BaseInheritanceRelationshipEntity>().Include(e => e.CollectionOnBase).AsSplitQuery(),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<BaseInheritanceRelationshipEntity>(x => x.CollectionOnBase)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_without_inheritance_reverse_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CollectionOnBase>().Include(e => e.Parent).AsSplitQuery(),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<CollectionOnBase>(x => x.Parent)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_without_inheritance_with_filter_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BaseInheritanceRelationshipEntity>().Include(e => e.CollectionOnBase).Where(e => e.Name != "Bar")
                .AsSplitQuery(),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<BaseInheritanceRelationshipEntity>(x => x.CollectionOnBase)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_without_inheritance_with_filter_reverse_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CollectionOnBase>().Include(e => e.Parent).Where(e => e.Name != "Bar").AsSplitQuery(),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<CollectionOnBase>(x => x.Parent)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_with_inheritance_on_derived1_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<DerivedInheritanceRelationshipEntity>().Include(e => e.BaseCollectionOnBase).AsSplitQuery(),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<DerivedInheritanceRelationshipEntity>(x => x.BaseCollectionOnBase)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_with_inheritance_on_derived2_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<DerivedInheritanceRelationshipEntity>().Include(e => e.BaseCollectionOnDerived).AsSplitQuery(),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<DerivedInheritanceRelationshipEntity>(x => x.BaseCollectionOnDerived)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_with_inheritance_on_derived3_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<DerivedInheritanceRelationshipEntity>().Include(e => e.DerivedCollectionOnDerived).AsSplitQuery(),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<DerivedInheritanceRelationshipEntity>(x => x.DerivedCollectionOnDerived)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_with_inheritance_on_derived_reverse_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BaseCollectionOnDerived>().Include(e => e.BaseParent).AsSplitQuery(),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<BaseCollectionOnDerived>(x => x.BaseParent)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nested_include_with_inheritance_reference_collection_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BaseInheritanceRelationshipEntity>().Include(e => e.BaseReferenceOnBase.NestedCollection).AsSplitQuery(),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<BaseInheritanceRelationshipEntity>(x => x.BaseReferenceOnBase),
                new ExpectedInclude<BaseReferenceOnBase>(x => x.NestedCollection)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nested_include_with_inheritance_reference_collection_on_base_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<DerivedInheritanceRelationshipEntity>().Include(e => e.BaseReferenceOnBase.NestedCollection).AsSplitQuery(),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<DerivedInheritanceRelationshipEntity>(x => x.BaseReferenceOnBase),
                new ExpectedInclude<BaseReferenceOnBase>(x => x.NestedCollection)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nested_include_with_inheritance_reference_collection_reverse_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<NestedCollectionBase>().Include(e => e.ParentReference.BaseParent).AsSplitQuery(),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<NestedCollectionBase>(x => x.ParentReference),
                new ExpectedInclude<BaseReferenceOnBase>(x => x.BaseParent)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nested_include_with_inheritance_collection_reference_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BaseInheritanceRelationshipEntity>().Include(e => e.BaseCollectionOnBase)
                .ThenInclude(e => e.NestedReference).AsSplitQuery(),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<BaseInheritanceRelationshipEntity>(x => x.BaseCollectionOnBase),
                new ExpectedInclude<BaseCollectionOnBase>(x => x.NestedReference)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nested_include_with_inheritance_collection_reference_reverse_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<NestedReferenceBase>().Include(e => e.ParentCollection.BaseParent).AsSplitQuery(),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<NestedReferenceBase>(x => x.ParentCollection),
                new ExpectedInclude<BaseCollectionOnBase>(x => x.BaseParent)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nested_include_with_inheritance_collection_collection_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BaseInheritanceRelationshipEntity>().Include(e => e.BaseCollectionOnBase)
                .ThenInclude(e => e.NestedCollection).AsSplitQuery(),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<BaseInheritanceRelationshipEntity>(x => x.BaseCollectionOnBase),
                new ExpectedInclude<BaseCollectionOnBase>(x => x.NestedCollection)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nested_include_with_inheritance_collection_collection_reverse_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<NestedCollectionBase>().Include(e => e.ParentCollection.BaseParent).AsSplitQuery(),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<NestedCollectionBase>(x => x.ParentCollection),
                new ExpectedInclude<BaseCollectionOnBase>(x => x.BaseParent)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nested_include_collection_reference_on_non_entity_base_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ReferencedEntity>().Include(e => e.Principals).ThenInclude(e => e.Reference).AsSplitQuery(),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<ReferencedEntity>(x => x.Principals),
                new ExpectedInclude<PrincipalEntity>(x => x.Reference)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_projection_on_base_type_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BaseInheritanceRelationshipEntity>().AsSplitQuery().Select(
                e =>
                    new { e.Id, e.BaseCollectionOnBase }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertCollection(e.BaseCollectionOnBase, a.BaseCollectionOnBase);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_on_derived_type_with_queryable_Cast_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BaseInheritanceRelationshipEntity>()
                .AsSplitQuery()
                .Where(b => b.Id >= 4)
                .Cast<DerivedInheritanceRelationshipEntity>()
                .Include(e => e.DerivedCollectionOnDerived),
            elementAsserter: (e, a) =>
            {
                AssertInclude(e, a, new ExpectedInclude<DerivedInheritanceRelationshipEntity>(i => i.DerivedCollectionOnDerived));
            });
}
