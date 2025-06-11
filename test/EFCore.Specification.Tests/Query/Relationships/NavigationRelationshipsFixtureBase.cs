// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query.Relationships;

public abstract class NavigationRelationshipsFixtureBase : RelationshipsQueryFixtureBase
{
    protected override string StoreName => "NavigationRelationshipsQueryTest";

    protected override Task SeedAsync(RelationshipsContext context)
    {
        var rootEntities = RelationshipsData.CreateRootEntities();
        var trunkEntities = RelationshipsData.CreateTrunkEntities();
        var branchEntities = RelationshipsData.CreateBranchEntities();
        var leafEntities = RelationshipsData.CreateLeafEntities();

        RelationshipsData.WireUp(rootEntities, trunkEntities, branchEntities, leafEntities, wireUpRootToTrunkOnly: false);

        context.Set<RelationshipsRootEntity>().AddRange(rootEntities);
        context.Set<RelationshipsTrunkEntity>().AddRange(trunkEntities);
        context.Set<RelationshipsBranchEntity>().AddRange(branchEntities);
        context.Set<RelationshipsLeafEntity>().AddRange(leafEntities);

        return context.SaveChangesAsync();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<RelationshipsTrunkEntity>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<RelationshipsBranchEntity>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<RelationshipsLeafEntity>().Property(x => x.Id).ValueGeneratedNever();

        modelBuilder.Entity<RelationshipsRootEntity>()
            .HasOne(x => x.OptionalReferenceTrunk)
            .WithOne(x => x.OptionalReferenceInverseRoot)
            .HasForeignKey<RelationshipsRootEntity>(x => x.OptionalReferenceTrunkId)
            .IsRequired(false);

        modelBuilder.Entity<RelationshipsRootEntity>()
            .HasOne(x => x.RequiredReferenceTrunk)
            .WithOne(x => x.RequiredReferenceInverseRoot)
            .HasForeignKey<RelationshipsRootEntity>(x => x.RequiredReferenceTrunkId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(true);

        modelBuilder.Entity<RelationshipsRootEntity>()
            .HasMany(x => x.CollectionTrunk)
            .WithOne(x => x.CollectionInverseRoot)
            .HasForeignKey(x => x.CollectionRootId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RelationshipsTrunkEntity>()
            .HasOne(x => x.OptionalReferenceBranch)
            .WithOne(x => x.OptionalReferenceInverseTrunk)
            .HasForeignKey<RelationshipsTrunkEntity>(x => x.OptionalReferenceBranchId)
            .IsRequired(false);

        modelBuilder.Entity<RelationshipsTrunkEntity>()
            .HasOne(x => x.RequiredReferenceBranch)
            .WithOne(x => x.RequiredReferenceInverseTrunk)
            .HasForeignKey<RelationshipsTrunkEntity>(x => x.RequiredReferenceBranchId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(true);

        modelBuilder.Entity<RelationshipsTrunkEntity>()
            .HasMany(x => x.CollectionBranch)
            .WithOne(x => x.CollectionInverseTrunk)
            .HasForeignKey(x => x.CollectionTrunkId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RelationshipsBranchEntity>()
            .HasOne(x => x.OptionalReferenceLeaf)
            .WithOne(x => x.OptionalReferenceInverseBranch)
            .HasForeignKey<RelationshipsBranchEntity>(x => x.OptionalReferenceLeafId)
            .IsRequired(false);

        modelBuilder.Entity<RelationshipsBranchEntity>()
            .HasOne(x => x.RequiredReferenceLeaf)
            .WithOne(x => x.RequiredReferenceInverseBranch)
            .HasForeignKey<RelationshipsBranchEntity>(x => x.RequiredReferenceLeafId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(true);

        modelBuilder.Entity<RelationshipsBranchEntity>()
            .HasMany(x => x.CollectionLeaf)
            .WithOne(x => x.CollectionInverseBranch)
            .HasForeignKey(x => x.CollectionBranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public override IReadOnlyDictionary<Type, object> EntitySorters { get; } = new Dictionary<Type, Func<object?, object?>>
    {
        { typeof(RelationshipsRootEntity), e => ((RelationshipsRootEntity?)e)?.Id },
        { typeof(RelationshipsTrunkEntity), e => ((RelationshipsTrunkEntity?)e)?.Id },
        { typeof(RelationshipsBranchEntity), e => ((RelationshipsBranchEntity?)e)?.Id },
        { typeof(RelationshipsLeafEntity), e => ((RelationshipsLeafEntity?)e)?.Id }
    }.ToDictionary(e => e.Key, e => (object)e.Value);

    public override IReadOnlyDictionary<Type, object> EntityAsserters { get; } = new Dictionary<Type, Action<object?, object?>>
    {
        {
            typeof(RelationshipsRootEntity), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (RelationshipsRootEntity)e!;
                    var aa = (RelationshipsRootEntity)a;

                    Assert.Equal(ee.Id, aa.Id);

                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.RequiredReferenceTrunkId, aa.RequiredReferenceTrunkId);
                    Assert.Equal(ee.OptionalReferenceTrunkId, aa.OptionalReferenceTrunkId);
                }
            }
        },
        {
            typeof(RelationshipsTrunkEntity), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (RelationshipsTrunkEntity)e!;
                    var aa = (RelationshipsTrunkEntity)a;

                    Assert.Equal(ee.Id, aa.Id);

                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.RequiredReferenceBranchId, aa.RequiredReferenceBranchId);
                    Assert.Equal(ee.OptionalReferenceBranchId, aa.OptionalReferenceBranchId);
                    Assert.Equal(ee.CollectionRootId, aa.CollectionRootId);
                }
            }
        },
        {
            typeof(RelationshipsBranchEntity), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (RelationshipsBranchEntity)e!;
                    var aa = (RelationshipsBranchEntity)a;

                    Assert.Equal(ee.Id, aa.Id);

                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.RequiredReferenceLeafId, aa.RequiredReferenceLeafId);
                    Assert.Equal(ee.OptionalReferenceLeafId, aa.OptionalReferenceLeafId);
                    Assert.Equal(ee.CollectionTrunkId, aa.CollectionTrunkId);
                }
            }
        },
        {
            typeof(RelationshipsLeafEntity), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (RelationshipsLeafEntity)e!;
                    var aa = (RelationshipsLeafEntity)a;

                    Assert.Equal(ee.Id, aa.Id);

                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.CollectionBranchId, aa.CollectionBranchId);
                }
            }
        },
    }.ToDictionary(e => e.Key, e => (object)e.Value);
}
