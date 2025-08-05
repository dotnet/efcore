// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships;

public abstract class RelationshipsQueryFixtureBase : SharedStoreFixtureBase<PoolableDbContext>, IQueryFixtureBase
{
    public virtual bool AreCollectionsOrdered => true;

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

    private void AssertRootEntity(RootEntity e, RootEntity a)
    {
        Assert.Equal(e.Id, a.Id);
        Assert.Equal(e.Name, a.Name);

        NullSafeAssert<RelatedType>(e.RequiredRelated, a.RequiredRelated, AssertRelatedType);
        NullSafeAssert<RelatedType>(e.OptionalRelated, a.OptionalRelated, AssertRelatedType);

        // TODO: Complete for collection, mind ordering (how is this done elsewhere?)
    }

    private void AssertRelatedType(RelatedType e, RelatedType a)
    {
        Assert.Equal(e.Id, a.Id);
        Assert.Equal(e.Name, a.Name);

        Assert.Equal(e.Int, a.Int);
        Assert.Equal(e.String, a.String);

        NullSafeAssert<NestedType>(e.RequiredNested, a.RequiredNested, AssertNestedType);
        NullSafeAssert<NestedType>(e.OptionalNested, a.OptionalNested, AssertNestedType);

        // TODO: Complete for collection, mind ordering (how is this done elsewhere?)
    }

    private void AssertNestedType(NestedType e, NestedType a)
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

    protected virtual void NullSafeAssert<T>(object? e, object? a, Action<T, T> assertAction)
    {
        if (e is T ee && a is T aa)
        {
            assertAction(ee, aa);
            return;
        }

        Assert.Equal(e, a);
    }
}
