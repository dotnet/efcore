﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships;

public abstract class RelationshipsQueryFixtureBase : SharedStoreFixtureBase<PoolableDbContext>, IQueryFixtureBase
{
    public virtual bool AreCollectionsOrdered
        => true;

    private readonly RelationshipsData _data;

    public RelationshipsQueryFixtureBase()
    {
        _data = CreateData();

        EntityAsserters = new Dictionary<Type, Action<object?, object?>>
        {
            [typeof(RootEntity)] = (e, a) => NullSafeAssert<RootEntity>(e, a, AssertRootEntity),
            [typeof(RelatedType)] = (e, a) => NullSafeAssert<RelatedType>(e, a, AssertRelatedType),
            [typeof(NestedType)] = (e, a) => NullSafeAssert<NestedType>(e, a, AssertNestedType),
            [typeof(RootReferencingEntity)] = (e, a) => NullSafeAssert<RootReferencingEntity>(e, a, AssertPreRootEntity)
        }.ToDictionary(e => e.Key, e => (object)e.Value);
    }

    public Func<DbContext> GetContextCreator()
        => CreateContext;

    public virtual ISetSource GetExpectedData()
        => _data;

    protected virtual RelationshipsData CreateData()
        => new();

    protected override Task SeedAsync(PoolableDbContext context)
    {
        context.Set<RootEntity>().AddRange(_data.RootEntities);
        if (context.Model.FindEntityType(typeof(RootReferencingEntity)) is not null)
        {
            context.Set<RootReferencingEntity>().AddRange(_data.RootReferencingEntities);
        }

        return context.SaveChangesAsync();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        // Don't use database value generation since e.g. Cosmos doesn't support it.
        modelBuilder.Entity<RootEntity>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<RootReferencingEntity>().Property(x => x.Id).ValueGeneratedNever();

        modelBuilder.Entity<RootReferencingEntity>()
            .HasOne(r => r.Root)
            .WithOne(r => r.RootReferencingEntity)
            .HasForeignKey<RootReferencingEntity>("RootEntityId") // TODO: possibly make a CLR property
            .IsRequired(false);
    }

    public virtual IReadOnlyDictionary<Type, object> EntitySorters { get; } = new Dictionary<Type, Func<object?, object?>>
    {
        { typeof(RootEntity), e => ((RootEntity?)e)?.Id },
        { typeof(RelatedType), e => ((RelatedType?)e)?.Id },
        { typeof(NestedType), e => ((NestedType?)e)?.Id },
        { typeof(RootReferencingEntity), e => ((RootReferencingEntity?)e)?.Id }
    }.ToDictionary(e => e.Key, e => (object)e.Value);

    public virtual IReadOnlyDictionary<Type, object> EntityAsserters { get; }

    protected virtual void AssertRootEntity(RootEntity e, RootEntity a)
    {
        Assert.Equal(e.Id, a.Id);
        Assert.Equal(e.Name, a.Name);

        NullSafeAssert<RelatedType>(e.RequiredRelated, a.RequiredRelated, AssertRelatedType);
        NullSafeAssert<RelatedType>(e.OptionalRelated, a.OptionalRelated, AssertRelatedType);

        if (e.RelatedCollection is not null && a.RelatedCollection is not null)
        {
            Assert.Equal(e.RelatedCollection.Count, a.RelatedCollection.Count);

            var (orderedExpected, orderedActual) = AreCollectionsOrdered
                ? (e.RelatedCollection, a.RelatedCollection)
                : (e.RelatedCollection.OrderBy(n => n.Id).ToList(), a.RelatedCollection.OrderBy(n => n.Id).ToList());

            for (var i = 0; i < e.RelatedCollection.Count; i++)
            {
                AssertRelatedType(orderedExpected[i], orderedActual[i]);
            }
        }
        else
        {
            Assert.Equal(e.RelatedCollection, a.RelatedCollection);
        }
    }

    protected virtual void AssertRelatedType(RelatedType e, RelatedType a)
    {
        Assert.Equal(e.Id, a.Id);
        Assert.Equal(e.Name, a.Name);

        Assert.Equal(e.Int, a.Int);
        Assert.Equal(e.String, a.String);

        NullSafeAssert<NestedType>(e.RequiredNested, a.RequiredNested, AssertNestedType);
        NullSafeAssert<NestedType>(e.OptionalNested, a.OptionalNested, AssertNestedType);

        if (e.NestedCollection is not null && a.NestedCollection != null)
        {
            Assert.Equal(e.NestedCollection.Count, a.NestedCollection.Count);

            var (orderedExpected, orderedActual) = AreCollectionsOrdered
                ? (e.NestedCollection, a.NestedCollection)
                : (e.NestedCollection.OrderBy(n => n.Id).ToList(), a.NestedCollection.OrderBy(n => n.Id).ToList());

            for (var i = 0; i < e.NestedCollection.Count; i++)
            {
                AssertNestedType(orderedExpected[i], orderedActual[i]);
            }
        }
        else
        {
            Assert.Equal(e.NestedCollection, a.NestedCollection);
        }
    }

    protected virtual void AssertNestedType(NestedType e, NestedType a)
    {
        Assert.Equal(e.Id, a.Id);
        Assert.Equal(e.Name, a.Name);

        Assert.Equal(e.Int, a.Int);
        Assert.Equal(e.String, a.String);
    }

    private void AssertPreRootEntity(RootReferencingEntity e, RootReferencingEntity a)
    {
        Assert.Equal(e.Id, a.Id);

        NullSafeAssert<RootEntity>(e.Root, a.Root, AssertRootEntity);
    }

    private void NullSafeAssert<T>(object? e, object? a, Action<T, T> assertAction)
    {
        if (e is T ee && a is T aa)
        {
            assertAction(ee, aa);
            return;
        }

        Assert.Equal(e, a);
    }
}
