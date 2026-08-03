// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations;

public abstract class AssociationsQueryFixtureBase : QueryFixtureBase<PoolableDbContext>
{
    public virtual bool AreCollectionsOrdered
        => true;

    public override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => throw new NotSupportedException();

    public AssociationsData Data { get; private set; }

    public AssociationsQueryFixtureBase()
    {
        Data = CreateData();

        EntityAsserters = new Dictionary<Type, object>
        {
            [typeof(RootEntity)] = (RootEntity e, RootEntity a)
                => NullSafeAssert<RootEntity>(e, a, AssertRootEntity),
            [typeof(AssociateType)] = (AssociateType e, AssociateType a)
                => NullSafeAssert<AssociateType>(e, a, AssertAssociate),
            [typeof(NestedAssociateType)] = (NestedAssociateType e, NestedAssociateType a)
                => NullSafeAssert<NestedAssociateType>(e, a, AssertNestedAssociate),
            [typeof(RootReferencingEntity)] = (RootReferencingEntity e, RootReferencingEntity a)
                => NullSafeAssert<RootReferencingEntity>(e, a, AssertPreRootEntity),

            [typeof(ValueRootEntity)] = (ValueRootEntity e, ValueRootEntity a)
                => NullSafeAssert<ValueRootEntity>(e, a, AssertValueRootEntity),
            [typeof(ValueAssociateType)] = (ValueAssociateType e, ValueAssociateType a)
                => NullSafeAssert<ValueAssociateType>(e, a, AssertValueAssociate),
            [typeof(ValueAssociateType?)] = (ValueAssociateType? e, ValueAssociateType? a)
                => NullSafeAssert<ValueAssociateType>(e, a, AssertValueAssociate),
            [typeof(ValueNestedType)] = (ValueNestedType e, ValueNestedType a)
                => NullSafeAssert<ValueNestedType>(e, a, AssertValueNestedAssociate),
            [typeof(ValueNestedType?)] = (ValueNestedType? e, ValueNestedType? a)
                => NullSafeAssert<ValueNestedType>(e, a, AssertValueNestedAssociate)
        }.ToDictionary(e => e.Key, e => e.Value);
    }

    public override Func<DbContext> GetContextCreator()
        => CreateContext;

    public override ISetSource GetExpectedData()
        => Data;

    protected virtual AssociationsData CreateData()
        => new();

    protected override Task SeedAsync(PoolableDbContext context)
    {
        context.Set<RootEntity>().AddRange(Data.RootEntities);
        if (context.Model.FindEntityType(typeof(RootReferencingEntity)) is not null)
        {
            context.Set<RootReferencingEntity>().AddRange(Data.RootReferencingEntities);
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

    public override IReadOnlyDictionary<Type, object> EntitySorters { get; } = new Dictionary<Type, object>
    {
        { typeof(RootEntity), object? (RootEntity e) => ((RootEntity?)e)?.Id },
        { typeof(AssociateType), object? (AssociateType e) => ((AssociateType?)e)?.Id },
        { typeof(NestedAssociateType), object? (NestedAssociateType e) => ((NestedAssociateType?)e)?.Id },
        { typeof(RootReferencingEntity), object? (RootReferencingEntity e) => ((RootReferencingEntity?)e)?.Id },

        { typeof(ValueRootEntity), object? (ValueRootEntity e) => ((ValueRootEntity?)e)?.Id },
        { typeof(ValueAssociateType), object? (ValueAssociateType e) => e.Id },
        { typeof(ValueAssociateType?), object? (ValueAssociateType? e) => e?.Id },
        { typeof(ValueNestedType), object? (ValueNestedType e) => e.Id },
        { typeof(ValueNestedType?), object? (ValueNestedType? e) => e?.Id }
    }.ToDictionary(e => e.Key, e => e.Value);

    public override IReadOnlyDictionary<Type, object> EntityAsserters { get; }

    protected virtual void AssertRootEntity(RootEntity e, RootEntity a)
    {
        Assert.Equal(e.Id, a.Id);
        Assert.Equal(e.Name, a.Name);

        NullSafeAssert<AssociateType>(e.RequiredAssociate, a.RequiredAssociate, AssertAssociate);
        NullSafeAssert<AssociateType>(e.OptionalAssociate, a.OptionalAssociate, AssertAssociate);

        if (e.AssociateCollection is not null && a.AssociateCollection is not null)
        {
            Assert.Equal(e.AssociateCollection.Count, a.AssociateCollection.Count);

            var (orderedExpected, orderedActual) = AreCollectionsOrdered
                ? (e.AssociateCollection, a.AssociateCollection)
                : (e.AssociateCollection.OrderBy(n => n.Id).ToList(), a.AssociateCollection.OrderBy(n => n.Id).ToList());

            for (var i = 0; i < e.AssociateCollection.Count; i++)
            {
                AssertAssociate(orderedExpected[i], orderedActual[i]);
            }
        }
        else
        {
            Assert.Equal(e.AssociateCollection, a.AssociateCollection);
        }
    }

    protected virtual void AssertAssociate(AssociateType e, AssociateType a)
    {
        Assert.Equal(e.Id, a.Id);
        Assert.Equal(e.Name, a.Name);

        Assert.Equal(e.Int, a.Int);
        Assert.Equal(e.String, a.String);

        NullSafeAssert<NestedAssociateType>(e.RequiredNestedAssociate, a.RequiredNestedAssociate, AssertNestedAssociate);
        NullSafeAssert<NestedAssociateType>(e.OptionalNestedAssociate, a.OptionalNestedAssociate, AssertNestedAssociate);

        if (e.NestedCollection is not null && a.NestedCollection != null)
        {
            Assert.Equal(e.NestedCollection.Count, a.NestedCollection.Count);

            var (orderedExpected, orderedActual) = AreCollectionsOrdered
                ? (e.NestedCollection, a.NestedCollection)
                : (e.NestedCollection.OrderBy(n => n.Id).ToList(), a.NestedCollection.OrderBy(n => n.Id).ToList());

            for (var i = 0; i < e.NestedCollection.Count; i++)
            {
                AssertNestedAssociate(orderedExpected[i], orderedActual[i]);
            }
        }
        else
        {
            Assert.Equal(e.NestedCollection, a.NestedCollection);
        }
    }

    protected virtual void AssertNestedAssociate(NestedAssociateType e, NestedAssociateType a)
    {
        Assert.Equal(e.Id, a.Id);
        Assert.Equal(e.Name, a.Name);

        Assert.Equal(e.Int, a.Int);
        Assert.Equal(e.String, a.String);
        Assert.Equal(e.Ints, a.Ints);
    }

    private void AssertPreRootEntity(RootReferencingEntity e, RootReferencingEntity a)
    {
        Assert.Equal(e.Id, a.Id);

        NullSafeAssert<RootEntity>(e.Root, a.Root, AssertRootEntity);
    }

    private void AssertValueRootEntity(ValueRootEntity e, ValueRootEntity a)
    {
        Assert.Equal(e.Id, a.Id);
        Assert.Equal(e.Name, a.Name);

        AssertValueAssociate(e.RequiredAssociate, a.RequiredAssociate);
        NullSafeAssert<ValueAssociateType>(e.OptionalAssociate, a.OptionalAssociate, AssertValueAssociate);

        // TODO: Complete for collection, mind ordering (how is this done elsewhere?)
    }

    private void AssertValueAssociate(ValueAssociateType e, ValueAssociateType a)
    {
        Assert.Equal(e.Id, a.Id);
        Assert.Equal(e.Name, a.Name);

        Assert.Equal(e.Int, a.Int);
        Assert.Equal(e.String, a.String);

        AssertValueNestedAssociate(e.RequiredNested, a.RequiredNested);
        NullSafeAssert<ValueNestedType>(e.OptionalNested, a.OptionalNested, AssertValueNestedAssociate);

        // TODO: Complete for collection, mind ordering (how is this done elsewhere?)
    }

    private void AssertValueNestedAssociate(ValueNestedType e, ValueNestedType a)
    {
        Assert.Equal(e.Id, a.Id);
        Assert.Equal(e.Name, a.Name);

        Assert.Equal(e.Int, a.Int);
        Assert.Equal(e.String, a.String);
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
