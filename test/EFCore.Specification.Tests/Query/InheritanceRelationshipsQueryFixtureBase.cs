// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.InheritanceRelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class InheritanceRelationshipsQueryFixtureBase : SharedStoreFixtureBase<InheritanceRelationshipsContext>,
    IQueryFixtureBase
{
    protected override string StoreName
        => "InheritanceRelationships";

    public Func<DbContext> GetContextCreator()
        => () => CreateContext();

    public virtual ISetSource GetExpectedData()
        => InheritanceRelationshipsData.Instance;

    public IReadOnlyDictionary<Type, object> EntitySorters { get; } = new Dictionary<Type, Func<object, object>>
    {
        { typeof(BaseCollectionOnBase), e => ((BaseCollectionOnBase)e)?.Id },
        { typeof(DerivedCollectionOnBase), e => ((DerivedCollectionOnBase)e)?.Id },
        { typeof(BaseCollectionOnDerived), e => ((BaseCollectionOnDerived)e)?.Id },
        { typeof(DerivedCollectionOnDerived), e => ((DerivedCollectionOnDerived)e)?.Id },
        { typeof(BaseInheritanceRelationshipEntity), e => ((BaseInheritanceRelationshipEntity)e)?.Id },
        { typeof(DerivedInheritanceRelationshipEntity), e => ((DerivedInheritanceRelationshipEntity)e)?.Id },
        { typeof(BaseReferenceOnBase), e => ((BaseReferenceOnBase)e)?.Id },
        { typeof(DerivedReferenceOnBase), e => ((DerivedReferenceOnBase)e)?.Id },
        { typeof(BaseReferenceOnDerived), e => ((BaseReferenceOnDerived)e)?.Id },
        { typeof(DerivedReferenceOnDerived), e => ((DerivedReferenceOnDerived)e)?.Id },
        { typeof(CollectionOnBase), e => ((CollectionOnBase)e)?.Id },
        { typeof(CollectionOnDerived), e => ((CollectionOnDerived)e)?.Id },
        { typeof(NestedCollectionBase), e => ((NestedCollectionBase)e)?.Id },
        { typeof(NestedCollectionDerived), e => ((NestedCollectionDerived)e)?.Id },
        { typeof(NestedReferenceBase), e => ((NestedReferenceBase)e)?.Id },
        { typeof(NonEntityBase), e => ((NonEntityBase)e)?.Id },
        { typeof(PrincipalEntity), e => ((PrincipalEntity)e)?.Id },
        { typeof(OwnedEntity), e => ((OwnedEntity)e)?.Id },
        { typeof(ReferencedEntity), e => ((ReferencedEntity)e)?.Id },
        { typeof(ReferenceOnBase), e => ((ReferenceOnBase)e)?.Id },
        { typeof(ReferenceOnDerived), e => ((ReferenceOnDerived)e)?.Id },
    }.ToDictionary(e => e.Key, e => (object)e.Value);

    public IReadOnlyDictionary<Type, object> EntityAsserters { get; } = new Dictionary<Type, Action<object, object>>
    {
        {
            typeof(BaseCollectionOnBase), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (BaseCollectionOnBase)e;
                    var aa = (BaseCollectionOnBase)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.BaseParentId, aa.BaseParentId);
                }
            }
        },
        {
            typeof(DerivedCollectionOnBase), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (DerivedCollectionOnBase)e;
                    var aa = (DerivedCollectionOnBase)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.BaseParentId, aa.BaseParentId);
                    Assert.Equal(ee.DerivedProperty, aa.DerivedProperty);
                }
            }
        },
        {
            typeof(BaseCollectionOnDerived), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (BaseCollectionOnDerived)e;
                    var aa = (BaseCollectionOnDerived)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.ParentId, aa.ParentId);
                }
            }
        },
        {
            typeof(DerivedCollectionOnDerived), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (DerivedCollectionOnDerived)e;
                    var aa = (DerivedCollectionOnDerived)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.ParentId, aa.ParentId);
                }
            }
        },
        {
            typeof(BaseInheritanceRelationshipEntity), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (BaseInheritanceRelationshipEntity)e;
                    var aa = (BaseInheritanceRelationshipEntity)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);

                    Assert.Equal(ee.OwnedReferenceOnBase?.Id, aa.OwnedReferenceOnBase?.Id);
                    Assert.Equal(ee.OwnedReferenceOnBase?.Name, aa.OwnedReferenceOnBase?.Name);

                    Assert.Equal(ee.OwnedCollectionOnBase?.Count, aa.OwnedCollectionOnBase?.Count);
                    if (ee.OwnedCollectionOnBase?.Count > 0)
                    {
                        var orderedExpected = ee.OwnedCollectionOnBase.OrderBy(x => x.Id).ToList();
                        var orderedActual = aa.OwnedCollectionOnBase.OrderBy(x => x.Id).ToList();
                        for (var i = 0; i < orderedExpected.Count; i++)
                        {
                            Assert.Equal(orderedExpected[i].Id, orderedActual[i].Id);
                            Assert.Equal(orderedExpected[i].Name, orderedActual[i].Name);
                        }
                    }
                }
            }
        },
        {
            typeof(DerivedInheritanceRelationshipEntity), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (DerivedInheritanceRelationshipEntity)e;
                    var aa = (DerivedInheritanceRelationshipEntity)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.BaseId, aa.BaseId);

                    Assert.Equal(ee.OwnedReferenceOnBase?.Id, aa.OwnedReferenceOnBase?.Id);
                    Assert.Equal(ee.OwnedReferenceOnBase?.Name, aa.OwnedReferenceOnBase?.Name);

                    Assert.Equal(ee.OwnedReferenceOnDerived?.Id, aa.OwnedReferenceOnDerived?.Id);
                    Assert.Equal(ee.OwnedReferenceOnDerived?.Name, aa.OwnedReferenceOnDerived?.Name);

                    Assert.Equal(ee.OwnedCollectionOnBase?.Count, aa.OwnedCollectionOnBase?.Count);
                    if (ee.OwnedCollectionOnBase?.Count > 0)
                    {
                        var orderedExpected = ee.OwnedCollectionOnBase.OrderBy(x => x.Id).ToList();
                        var orderedActual = aa.OwnedCollectionOnBase.OrderBy(x => x.Id).ToList();
                        for (var i = 0; i < orderedExpected.Count; i++)
                        {
                            Assert.Equal(orderedExpected[i].Id, orderedActual[i].Id);
                            Assert.Equal(orderedExpected[i].Name, orderedActual[i].Name);
                        }
                    }

                    Assert.Equal(ee.OwnedCollectionOnDerived?.Count, aa.OwnedCollectionOnDerived?.Count);
                    if (ee.OwnedCollectionOnDerived?.Count > 0)
                    {
                        var orderedExpected = ee.OwnedCollectionOnDerived.OrderBy(x => x.Id).ToList();
                        var orderedActual = aa.OwnedCollectionOnDerived.OrderBy(x => x.Id).ToList();
                        for (var i = 0; i < orderedExpected.Count; i++)
                        {
                            Assert.Equal(orderedExpected[i].Id, orderedActual[i].Id);
                            Assert.Equal(orderedExpected[i].Name, orderedActual[i].Name);
                        }
                    }
                }
            }
        },
        {
            typeof(BaseReferenceOnBase), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (BaseReferenceOnBase)e;
                    var aa = (BaseReferenceOnBase)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.BaseParentId, aa.BaseParentId);
                }
            }
        },
        {
            typeof(DerivedReferenceOnBase), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (DerivedReferenceOnBase)e;
                    var aa = (DerivedReferenceOnBase)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.BaseParentId, aa.BaseParentId);
                }
            }
        },
        {
            typeof(BaseReferenceOnDerived), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (BaseReferenceOnDerived)e;
                    var aa = (BaseReferenceOnDerived)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.BaseParentId, aa.BaseParentId);
                }
            }
        },
        {
            typeof(DerivedReferenceOnDerived), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (DerivedReferenceOnDerived)e;
                    var aa = (DerivedReferenceOnDerived)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.BaseParentId, aa.BaseParentId);
                }
            }
        },
        {
            typeof(CollectionOnBase), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (CollectionOnBase)e;
                    var aa = (CollectionOnBase)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.ParentId, aa.ParentId);
                }
            }
        },
        {
            typeof(CollectionOnDerived), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (CollectionOnDerived)e;
                    var aa = (CollectionOnDerived)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.ParentId, aa.ParentId);
                }
            }
        },
        {
            typeof(NestedCollectionBase), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (NestedCollectionBase)e;
                    var aa = (NestedCollectionBase)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.ParentReferenceId, aa.ParentReferenceId);
                    Assert.Equal(ee.ParentCollectionId, aa.ParentCollectionId);
                }
            }
        },
        {
            typeof(NestedCollectionDerived), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (NestedCollectionDerived)e;
                    var aa = (NestedCollectionDerived)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.ParentReferenceId, aa.ParentReferenceId);
                    Assert.Equal(ee.ParentCollectionId, aa.ParentCollectionId);
                }
            }
        },
        {
            typeof(NestedReferenceBase), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (NestedReferenceBase)e;
                    var aa = (NestedReferenceBase)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.ParentReferenceId, aa.ParentReferenceId);
                    Assert.Equal(ee.ParentCollectionId, aa.ParentCollectionId);
                }
            }
        },
        {
            typeof(NestedReferenceDerived), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (NestedReferenceDerived)e;
                    var aa = (NestedReferenceDerived)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.ParentReferenceId, aa.ParentReferenceId);
                    Assert.Equal(ee.ParentCollectionId, aa.ParentCollectionId);
                }
            }
        },
        {
            typeof(NonEntityBase), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (NonEntityBase)e;
                    var aa = (NonEntityBase)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                }
            }
        },
        {
            typeof(PrincipalEntity), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (PrincipalEntity)e;
                    var aa = (PrincipalEntity)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                }
            }
        },
        {
            typeof(ReferencedEntity), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (ReferencedEntity)e;
                    var aa = (ReferencedEntity)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                }
            }
        },
        {
            typeof(ReferenceOnBase), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (ReferenceOnBase)e;
                    var aa = (ReferenceOnBase)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.ParentId, aa.ParentId);
                }
            }
        },
        {
            typeof(ReferenceOnDerived), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (ReferenceOnDerived)e;
                    var aa = (ReferenceOnDerived)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.ParentId, aa.ParentId);
                }
            }
        },
    }.ToDictionary(e => e.Key, e => (object)e.Value);

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.Owned<OwnedEntity>();
        modelBuilder.Entity<NestedReferenceDerived>();
        modelBuilder.Entity<NestedCollectionDerived>();
        modelBuilder.Entity<DerivedReferenceOnBase>();
        modelBuilder.Entity<DerivedCollectionOnBase>();
        modelBuilder.Entity<DerivedReferenceOnDerived>();
        modelBuilder.Entity<DerivedCollectionOnDerived>();

        modelBuilder.Entity<BaseCollectionOnBase>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<BaseCollectionOnDerived>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<BaseInheritanceRelationshipEntity>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<BaseReferenceOnBase>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<BaseReferenceOnDerived>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<CollectionOnBase>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<CollectionOnDerived>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<NestedCollectionBase>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<NestedReferenceBase>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<PrincipalEntity>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<ReferencedEntity>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<ReferenceOnBase>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<ReferenceOnDerived>().Property(e => e.Id).ValueGeneratedNever();

        modelBuilder.Entity<BaseInheritanceRelationshipEntity>()
            .HasOne(e => e.DerivedSefReferenceOnBase)
            .WithOne(e => e.BaseSelfReferenceOnDerived)
            .HasForeignKey<DerivedInheritanceRelationshipEntity>(e => e.BaseId)
            .IsRequired(false);

        modelBuilder.Entity<BaseInheritanceRelationshipEntity>()
            .HasOne(e => e.BaseReferenceOnBase)
            .WithOne(e => e.BaseParent)
            .HasForeignKey<BaseReferenceOnBase>(e => e.BaseParentId)
            .IsRequired(false);

        modelBuilder.Entity<BaseInheritanceRelationshipEntity>()
            .HasOne(e => e.ReferenceOnBase)
            .WithOne(e => e.Parent)
            .HasForeignKey<ReferenceOnBase>(e => e.ParentId)
            .IsRequired(false);

        modelBuilder.Entity<BaseInheritanceRelationshipEntity>()
            .HasMany(e => e.BaseCollectionOnBase)
            .WithOne(e => e.BaseParent)
            .HasForeignKey(e => e.BaseParentId)
            .IsRequired(false);

        modelBuilder.Entity<BaseInheritanceRelationshipEntity>()
            .HasMany(e => e.CollectionOnBase)
            .WithOne(e => e.Parent)
            .HasForeignKey(e => e.ParentId)
            .IsRequired(false);

        modelBuilder.Entity<BaseInheritanceRelationshipEntity>()
            .OwnsMany(e => e.OwnedCollectionOnBase)
            .Property(e => e.Id).ValueGeneratedNever();

        modelBuilder.Entity<DerivedInheritanceRelationshipEntity>()
            .HasOne(e => e.DerivedReferenceOnDerived)
            .WithOne()
            .HasForeignKey<DerivedReferenceOnDerived>("DerivedInheritanceRelationshipEntityId")
            .IsRequired(false);

        modelBuilder.Entity<DerivedInheritanceRelationshipEntity>()
            .HasOne(e => e.ReferenceOnDerived)
            .WithOne(e => e.Parent)
            .HasForeignKey<ReferenceOnDerived>(e => e.ParentId)
            .IsRequired(false);

        modelBuilder.Entity<DerivedInheritanceRelationshipEntity>()
            .HasMany(e => e.BaseCollectionOnDerived)
            .WithOne(e => e.BaseParent)
            .HasForeignKey(e => e.ParentId)
            .IsRequired(false);

        modelBuilder.Entity<DerivedInheritanceRelationshipEntity>()
            .HasMany(e => e.CollectionOnDerived)
            .WithOne(e => e.Parent)
            .HasForeignKey(e => e.ParentId)
            .IsRequired(false);

        modelBuilder.Entity<DerivedInheritanceRelationshipEntity>()
            .HasMany(e => e.DerivedCollectionOnDerived)
            .WithOne()
            .HasForeignKey("DerivedInheritanceRelationshipEntityId")
            .IsRequired(false);

        modelBuilder.Entity<DerivedInheritanceRelationshipEntity>()
            .HasOne(e => e.BaseReferenceOnDerived)
            .WithOne(e => e.BaseParent)
            .HasForeignKey<BaseReferenceOnDerived>(e => e.BaseParentId)
            .IsRequired(false);

        modelBuilder.Entity<DerivedInheritanceRelationshipEntity>()
            .OwnsMany(e => e.OwnedCollectionOnDerived)
            .Property(e => e.Id).ValueGeneratedNever();

        modelBuilder.Entity<BaseReferenceOnBase>()
            .HasOne(e => e.NestedReference)
            .WithOne(e => e.ParentReference)
            .HasForeignKey<NestedReferenceBase>(e => e.ParentReferenceId)
            .IsRequired(false);

        modelBuilder.Entity<BaseReferenceOnBase>()
            .HasMany(e => e.NestedCollection)
            .WithOne(e => e.ParentReference)
            .HasForeignKey(e => e.ParentReferenceId)
            .IsRequired(false);

        modelBuilder.Entity<BaseCollectionOnBase>()
            .HasOne(e => e.NestedReference)
            .WithOne(e => e.ParentCollection)
            .HasForeignKey<NestedReferenceBase>(e => e.ParentCollectionId)
            .IsRequired(false);

        modelBuilder.Entity<BaseCollectionOnBase>()
            .HasMany(e => e.NestedCollection)
            .WithOne(e => e.ParentCollection)
            .HasForeignKey(e => e.ParentCollectionId)
            .IsRequired(false);

        modelBuilder.Entity<PrincipalEntity>()
            .HasOne(e => e.Reference)
            .WithMany()
            .IsRequired(false);

        modelBuilder.Entity<ReferencedEntity>()
            .HasMany(e => e.Principals)
            .WithOne()
            .IsRequired(false);
    }

    protected override Task SeedAsync(InheritanceRelationshipsContext context)
        => InheritanceRelationshipsContext.SeedAsync(context);

    public override InheritanceRelationshipsContext CreateContext()
    {
        var context = base.CreateContext();
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        return context;
    }
}
