// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.InheritanceRelationshipsModel;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class InheritanceRelationshipsQueryTestBase<TFixture> : QueryTestBase<TFixture>
    where TFixture : InheritanceRelationshipsQueryFixtureBase, new()
{
    protected InheritanceRelationshipsQueryTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    [ConditionalFact]
    public virtual void Changes_in_derived_related_entities_are_detected()
    {
        using var context = CreateContext();
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

        var derivedEntity = context.BaseEntities.Include(e => e.BaseCollectionOnBase)
            .Single(e => e.Name == "Derived1(4)") as DerivedInheritanceRelationshipEntity;

        Assert.NotNull(derivedEntity);

        var firstRelatedEntity = derivedEntity.BaseCollectionOnBase.Cast<DerivedCollectionOnBase>().First();

        var originalValue = firstRelatedEntity.DerivedProperty;
        Assert.NotEqual(0, originalValue);

        var entry = context.ChangeTracker.Entries<DerivedCollectionOnBase>()
            .Single(e => e.Entity == firstRelatedEntity);

        Assert.IsType<DerivedCollectionOnBase>(entry.Entity);

        Assert.Equal(
            "Microsoft.EntityFrameworkCore.TestModels.InheritanceRelationshipsModel.DerivedCollectionOnBase",
            entry.Metadata.Name);

        firstRelatedEntity.DerivedProperty = originalValue + 1;
        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.Equal(originalValue, entry.Property(e => e.DerivedProperty).OriginalValue);
        Assert.Equal(originalValue + 1, entry.Property(e => e.DerivedProperty).CurrentValue);

        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    [ConditionalFact]
    public virtual void Entity_can_make_separate_relationships_with_base_type_and_derived_type_both()
    {
        using var context = CreateContext();
        var model = context.Model;
        var principalEntityType = model.FindEntityType(typeof(DerivedInheritanceRelationshipEntity));
        var dependentEntityType = model.FindEntityType(typeof(BaseReferenceOnDerived));
        var derivedDependentEntityType = model.FindEntityType(typeof(DerivedReferenceOnDerived));

        var fkOnBase = dependentEntityType.GetForeignKeys().Single();
        Assert.Equal(principalEntityType, fkOnBase.PrincipalEntityType);
        Assert.Equal(dependentEntityType, fkOnBase.DeclaringEntityType);
        Assert.Equal(nameof(BaseReferenceOnDerived.BaseParent), fkOnBase.DependentToPrincipal.Name);
        Assert.Equal(nameof(DerivedInheritanceRelationshipEntity.BaseReferenceOnDerived), fkOnBase.PrincipalToDependent.Name);

        var fkOnDerived = derivedDependentEntityType.GetDeclaredForeignKeys()
            .Single(fk => fk.PrincipalEntityType != dependentEntityType);
        Assert.NotSame(fkOnBase, fkOnDerived);
        Assert.Equal(principalEntityType, fkOnDerived.PrincipalEntityType);
        Assert.Equal(derivedDependentEntityType, fkOnDerived.DeclaringEntityType);
        Assert.Null(fkOnDerived.DependentToPrincipal);
        Assert.Equal(nameof(DerivedInheritanceRelationshipEntity.DerivedReferenceOnDerived), fkOnDerived.PrincipalToDependent.Name);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_reference_with_inheritance(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BaseInheritanceRelationshipEntity>().Include(e => e.BaseReferenceOnBase),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<BaseInheritanceRelationshipEntity>(x => x.BaseReferenceOnBase)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_reference_with_inheritance_reverse(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BaseReferenceOnBase>().Include(e => e.BaseParent),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<BaseReferenceOnBase>(x => x.BaseParent)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_self_reference_with_inheritance(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BaseInheritanceRelationshipEntity>().Include(e => e.DerivedSefReferenceOnBase),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<BaseInheritanceRelationshipEntity>(x => x.DerivedSefReferenceOnBase)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_self_reference_with_inheritance_reverse(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<DerivedInheritanceRelationshipEntity>().Include(e => e.BaseSelfReferenceOnDerived),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<DerivedInheritanceRelationshipEntity>(x => x.BaseSelfReferenceOnDerived)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_reference_with_inheritance_with_filter(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BaseInheritanceRelationshipEntity>().Include(e => e.BaseReferenceOnBase).Where(e => e.Name != "Bar"),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<BaseInheritanceRelationshipEntity>(x => x.BaseReferenceOnBase)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_reference_with_inheritance_with_filter_reverse(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BaseReferenceOnBase>().Include(e => e.BaseParent).Where(e => e.Name != "Bar"),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<BaseReferenceOnBase>(x => x.BaseParent)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_reference_without_inheritance(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BaseInheritanceRelationshipEntity>().Include(e => e.ReferenceOnBase),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<BaseInheritanceRelationshipEntity>(x => x.ReferenceOnBase)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_reference_without_inheritance_reverse(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ReferenceOnBase>().Include(e => e.Parent),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<ReferenceOnBase>(x => x.Parent)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_reference_without_inheritance_with_filter(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BaseInheritanceRelationshipEntity>().Include(e => e.ReferenceOnBase).Where(e => e.Name != "Bar"),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<BaseInheritanceRelationshipEntity>(x => x.ReferenceOnBase)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_reference_without_inheritance_with_filter_reverse(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ReferenceOnBase>().Include(e => e.Parent).Where(e => e.Name != "Bar"),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<ReferenceOnBase>(x => x.Parent)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_with_inheritance(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BaseInheritanceRelationshipEntity>().Include(e => e.BaseCollectionOnBase),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<BaseInheritanceRelationshipEntity>(x => x.BaseCollectionOnBase)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_with_inheritance_reverse(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BaseCollectionOnBase>().Include(e => e.BaseParent),
            elementAsserter: (e, a) => AssertInclude(
                e,
                a,
                new ExpectedInclude<BaseCollectionOnBase>(x => x.BaseParent)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_with_inheritance_with_filter(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BaseInheritanceRelationshipEntity>().Include(e => e.BaseCollectionOnBase).Where(e => e.Name != "Bar"),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<BaseInheritanceRelationshipEntity>(x => x.BaseCollectionOnBase)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_with_inheritance_with_filter_reverse(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BaseCollectionOnBase>().Include(e => e.BaseParent).Where(e => e.Name != "Bar"),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<BaseCollectionOnBase>(x => x.BaseParent)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_without_inheritance(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BaseInheritanceRelationshipEntity>().Include(e => e.CollectionOnBase),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<BaseInheritanceRelationshipEntity>(x => x.CollectionOnBase)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_without_inheritance_reverse(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CollectionOnBase>().Include(e => e.Parent),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<CollectionOnBase>(x => x.Parent)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_without_inheritance_with_filter(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BaseInheritanceRelationshipEntity>().Include(e => e.CollectionOnBase).Where(e => e.Name != "Bar"),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<BaseInheritanceRelationshipEntity>(x => x.CollectionOnBase)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_without_inheritance_with_filter_reverse(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CollectionOnBase>().Include(e => e.Parent).Where(e => e.Name != "Bar"),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<CollectionOnBase>(x => x.Parent)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_reference_with_inheritance_on_derived1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<DerivedInheritanceRelationshipEntity>().Include(e => e.BaseReferenceOnBase),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<DerivedInheritanceRelationshipEntity>(x => x.BaseReferenceOnBase)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_reference_with_inheritance_on_derived2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<DerivedInheritanceRelationshipEntity>().Include(e => e.BaseReferenceOnDerived),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<DerivedInheritanceRelationshipEntity>(x => x.BaseReferenceOnDerived)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_reference_with_inheritance_on_derived4(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<DerivedInheritanceRelationshipEntity>().Include(e => e.DerivedReferenceOnDerived),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<DerivedInheritanceRelationshipEntity>(x => x.DerivedReferenceOnDerived)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_reference_with_inheritance_on_derived_reverse(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BaseReferenceOnDerived>().Include(e => e.BaseParent),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<BaseReferenceOnDerived>(x => x.BaseParent)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_reference_with_inheritance_on_derived_with_filter1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<DerivedInheritanceRelationshipEntity>().Include(e => e.BaseReferenceOnBase).Where(e => e.Name != "Bar"),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<DerivedInheritanceRelationshipEntity>(x => x.BaseReferenceOnBase)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_reference_with_inheritance_on_derived_with_filter2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<DerivedInheritanceRelationshipEntity>().Include(e => e.BaseReferenceOnDerived).Where(e => e.Name != "Bar"),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<DerivedInheritanceRelationshipEntity>(x => x.BaseReferenceOnDerived)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_reference_with_inheritance_on_derived_with_filter4(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<DerivedInheritanceRelationshipEntity>().Include(e => e.DerivedReferenceOnDerived).Where(e => e.Name != "Bar"),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<DerivedInheritanceRelationshipEntity>(x => x.DerivedReferenceOnDerived)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_reference_with_inheritance_on_derived_with_filter_reverse(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BaseReferenceOnDerived>().Include(e => e.BaseParent).Where(e => e.Name != "Bar"),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<BaseReferenceOnDerived>(x => x.BaseParent)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_reference_without_inheritance_on_derived1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<DerivedInheritanceRelationshipEntity>().Include(e => e.ReferenceOnBase),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<DerivedInheritanceRelationshipEntity>(x => x.ReferenceOnBase)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_reference_without_inheritance_on_derived2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<DerivedInheritanceRelationshipEntity>().Include(e => e.ReferenceOnDerived),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<DerivedInheritanceRelationshipEntity>(x => x.ReferenceOnDerived)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_reference_without_inheritance_on_derived_reverse(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ReferenceOnDerived>().Include(e => e.Parent),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<ReferenceOnDerived>(x => x.Parent)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_with_inheritance_on_derived1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<DerivedInheritanceRelationshipEntity>().Include(e => e.BaseCollectionOnBase),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<DerivedInheritanceRelationshipEntity>(x => x.BaseCollectionOnBase)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_with_inheritance_on_derived2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<DerivedInheritanceRelationshipEntity>().Include(e => e.BaseCollectionOnDerived),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<DerivedInheritanceRelationshipEntity>(x => x.BaseCollectionOnDerived)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_with_inheritance_on_derived3(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<DerivedInheritanceRelationshipEntity>().Include(e => e.DerivedCollectionOnDerived),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<DerivedInheritanceRelationshipEntity>(x => x.DerivedCollectionOnDerived)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_with_inheritance_on_derived_reverse(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BaseCollectionOnDerived>().Include(e => e.BaseParent),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<BaseCollectionOnDerived>(x => x.BaseParent)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nested_include_with_inheritance_reference_reference(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BaseInheritanceRelationshipEntity>().Include(e => e.BaseReferenceOnBase.NestedReference),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<BaseInheritanceRelationshipEntity>(x => x.BaseReferenceOnBase),
                new ExpectedInclude<BaseReferenceOnBase>(x => x.NestedReference)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nested_include_with_inheritance_reference_reference_on_base(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<DerivedInheritanceRelationshipEntity>().Include(e => e.BaseReferenceOnBase.NestedReference),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<DerivedInheritanceRelationshipEntity>(x => x.BaseReferenceOnBase),
                new ExpectedInclude<BaseReferenceOnBase>(x => x.NestedReference)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nested_include_with_inheritance_reference_reference_reverse(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<NestedReferenceBase>().Include(e => e.ParentReference.BaseParent),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<NestedReferenceBase>(x => x.ParentReference),
                new ExpectedInclude<BaseReferenceOnBase>(x => x.BaseParent)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nested_include_with_inheritance_reference_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BaseInheritanceRelationshipEntity>().Include(e => e.BaseReferenceOnBase.NestedCollection),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<BaseInheritanceRelationshipEntity>(x => x.BaseReferenceOnBase),
                new ExpectedInclude<BaseReferenceOnBase>(x => x.NestedCollection)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nested_include_with_inheritance_reference_collection_on_base(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<DerivedInheritanceRelationshipEntity>().Include(e => e.BaseReferenceOnBase.NestedCollection),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<DerivedInheritanceRelationshipEntity>(x => x.BaseReferenceOnBase),
                new ExpectedInclude<BaseReferenceOnBase>(x => x.NestedCollection)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nested_include_with_inheritance_reference_collection_reverse(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<NestedCollectionBase>().Include(e => e.ParentReference.BaseParent),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<NestedCollectionBase>(x => x.ParentReference),
                new ExpectedInclude<BaseReferenceOnBase>(x => x.BaseParent)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nested_include_with_inheritance_collection_reference(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BaseInheritanceRelationshipEntity>().Include(e => e.BaseCollectionOnBase).ThenInclude(e => e.NestedReference),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<BaseInheritanceRelationshipEntity>(x => x.BaseCollectionOnBase),
                new ExpectedInclude<BaseCollectionOnBase>(x => x.NestedReference)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nested_include_with_inheritance_collection_reference_reverse(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<NestedReferenceBase>().Include(e => e.ParentCollection.BaseParent),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<NestedReferenceBase>(x => x.ParentCollection),
                new ExpectedInclude<BaseCollectionOnBase>(x => x.BaseParent)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nested_include_with_inheritance_collection_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BaseInheritanceRelationshipEntity>().Include(e => e.BaseCollectionOnBase).ThenInclude(e => e.NestedCollection),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<BaseInheritanceRelationshipEntity>(x => x.BaseCollectionOnBase),
                new ExpectedInclude<BaseCollectionOnBase>(x => x.NestedCollection)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nested_include_with_inheritance_collection_collection_reverse(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<NestedCollectionBase>().Include(e => e.ParentCollection.BaseParent),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<NestedCollectionBase>(x => x.ParentCollection),
                new ExpectedInclude<BaseCollectionOnBase>(x => x.BaseParent)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nested_include_collection_reference_on_non_entity_base(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ReferencedEntity>().Include(e => e.Principals).ThenInclude(e => e.Reference),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<ReferencedEntity>(x => x.Principals),
                new ExpectedInclude<PrincipalEntity>(x => x.Reference)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_projection_on_base_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BaseInheritanceRelationshipEntity>().Select(
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
    public virtual Task Include_on_derived_type_with_queryable_Cast(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BaseInheritanceRelationshipEntity>()
                .Where(b => b.Id >= 4)
                .Cast<DerivedInheritanceRelationshipEntity>()
                .Include(e => e.DerivedCollectionOnDerived),
            elementAsserter: (e, a) =>
            {
                AssertInclude(e, a, new ExpectedInclude<DerivedInheritanceRelationshipEntity>(i => i.DerivedCollectionOnDerived));
            });

    protected InheritanceRelationshipsContext CreateContext()
        => Fixture.CreateContext();

    protected virtual void ClearLog()
    {
    }
}
