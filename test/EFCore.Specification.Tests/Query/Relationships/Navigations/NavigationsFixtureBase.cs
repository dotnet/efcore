// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Navigations;

public abstract class NavigationsFixtureBase : RelationshipsQueryFixtureBase
{
    protected override string StoreName => "NavigationsQueryTest";

    protected override Task SeedAsync(RelationshipsContext context)
    {
        var rootEntities = RelationshipsData.CreateRootEntities();
        var trunkEntities = RelationshipsData.CreateTrunkEntities();
        var branchEntities = RelationshipsData.CreateBranchEntities();
        var leafEntities = RelationshipsData.CreateLeafEntities();

        RelationshipsData.WireUp(rootEntities, trunkEntities, branchEntities, leafEntities, wireUpRootToTrunkOnly: false);

        context.Set<RelationshipsRoot>().AddRange(rootEntities);
        context.Set<RelationshipsTrunk>().AddRange(trunkEntities);
        context.Set<RelationshipsBranch>().AddRange(branchEntities);
        context.Set<RelationshipsLeaf>().AddRange(leafEntities);

        return context.SaveChangesAsync();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<RelationshipsTrunk>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<RelationshipsBranch>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<RelationshipsLeaf>().Property(x => x.Id).ValueGeneratedNever();

        modelBuilder.Entity<RelationshipsRoot>()
            .HasOne(x => x.OptionalReferenceTrunk)
            .WithOne(x => x.OptionalReferenceInverseRoot)
            .HasForeignKey<RelationshipsRoot>(x => x.OptionalReferenceTrunkId)
            .IsRequired(false);

        modelBuilder.Entity<RelationshipsRoot>()
            .HasOne(x => x.RequiredReferenceTrunk)
            .WithOne(x => x.RequiredReferenceInverseRoot)
            .HasForeignKey<RelationshipsRoot>(x => x.RequiredReferenceTrunkId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(true);

        modelBuilder.Entity<RelationshipsRoot>()
            .HasMany(x => x.CollectionTrunk)
            .WithOne(x => x.CollectionInverseRoot)
            .HasForeignKey(x => x.CollectionRootId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RelationshipsTrunk>()
            .HasOne(x => x.OptionalReferenceBranch)
            .WithOne(x => x.OptionalReferenceInverseTrunk)
            .HasForeignKey<RelationshipsTrunk>(x => x.OptionalReferenceBranchId)
            .IsRequired(false);

        modelBuilder.Entity<RelationshipsTrunk>()
            .HasOne(x => x.RequiredReferenceBranch)
            .WithOne(x => x.RequiredReferenceInverseTrunk)
            .HasForeignKey<RelationshipsTrunk>(x => x.RequiredReferenceBranchId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(true);

        modelBuilder.Entity<RelationshipsTrunk>()
            .HasMany(x => x.CollectionBranch)
            .WithOne(x => x.CollectionInverseTrunk)
            .HasForeignKey(x => x.CollectionTrunkId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RelationshipsBranch>()
            .HasOne(x => x.OptionalReferenceLeaf)
            .WithOne(x => x.OptionalReferenceInverseBranch)
            .HasForeignKey<RelationshipsBranch>(x => x.OptionalReferenceLeafId)
            .IsRequired(false);

        modelBuilder.Entity<RelationshipsBranch>()
            .HasOne(x => x.RequiredReferenceLeaf)
            .WithOne(x => x.RequiredReferenceInverseBranch)
            .HasForeignKey<RelationshipsBranch>(x => x.RequiredReferenceLeafId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(true);

        modelBuilder.Entity<RelationshipsBranch>()
            .HasMany(x => x.CollectionLeaf)
            .WithOne(x => x.CollectionInverseBranch)
            .HasForeignKey(x => x.CollectionBranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public override IReadOnlyDictionary<Type, object> EntitySorters { get; } = new Dictionary<Type, Func<object?, object?>>
    {
        { typeof(RelationshipsRoot), e => ((RelationshipsRoot?)e)?.Id },
        { typeof(RelationshipsTrunk), e => ((RelationshipsTrunk?)e)?.Id },
        { typeof(RelationshipsBranch), e => ((RelationshipsBranch?)e)?.Id },
        { typeof(RelationshipsLeaf), e => ((RelationshipsLeaf?)e)?.Id }
    }.ToDictionary(e => e.Key, e => (object)e.Value);

    public override IReadOnlyDictionary<Type, object> EntityAsserters { get; } = new Dictionary<Type, Action<object?, object?>>
    {
        {
            typeof(RelationshipsRoot), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (RelationshipsRoot)e!;
                    var aa = (RelationshipsRoot)a;

                    Assert.Equal(ee.Id, aa.Id);

                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.RequiredReferenceTrunkId, aa.RequiredReferenceTrunkId);
                    Assert.Equal(ee.OptionalReferenceTrunkId, aa.OptionalReferenceTrunkId);
                }
            }
        },
        {
            typeof(RelationshipsTrunk), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (RelationshipsTrunk)e!;
                    var aa = (RelationshipsTrunk)a;

                    Assert.Equal(ee.Id, aa.Id);

                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.RequiredReferenceBranchId, aa.RequiredReferenceBranchId);
                    Assert.Equal(ee.OptionalReferenceBranchId, aa.OptionalReferenceBranchId);
                    Assert.Equal(ee.CollectionRootId, aa.CollectionRootId);
                }
            }
        },
        {
            typeof(RelationshipsBranch), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (RelationshipsBranch)e!;
                    var aa = (RelationshipsBranch)a;

                    Assert.Equal(ee.Id, aa.Id);

                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.RequiredReferenceLeafId, aa.RequiredReferenceLeafId);
                    Assert.Equal(ee.OptionalReferenceLeafId, aa.OptionalReferenceLeafId);
                    Assert.Equal(ee.CollectionTrunkId, aa.CollectionTrunkId);
                }
            }
        },
        {
            typeof(RelationshipsLeaf), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (RelationshipsLeaf)e!;
                    var aa = (RelationshipsLeaf)a;

                    Assert.Equal(ee.Id, aa.Id);

                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.CollectionBranchId, aa.CollectionBranchId);
                }
            }
        },
    }.ToDictionary(e => e.Key, e => (object)e.Value);
}
