// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query.Relationships;

// collections are not supported for non-json complex types
// so only use this for reference test case
public abstract class ComplexRelationshipsFixtureBase : RelationshipsQueryFixtureBase
{
    protected override string StoreName => "ComplexRelationshipsQueryTest";

    protected override Task SeedAsync(RelationshipsContext context)
    {
        var rootEntities = RelationshipsData.CreateRootEntities();
        var trunkEntities = RelationshipsData.CreateTrunkEntitiesWithOwnerships();

        RelationshipsData.WireUp(rootEntities, trunkEntities, [], [], wireUpRootToTrunkOnly: true);

        context.Set<RelationshipsRootEntity>().AddRange(rootEntities);
        context.Set<RelationshipsTrunkEntity>().AddRange(trunkEntities);

        return context.SaveChangesAsync();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

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

        modelBuilder.Entity<RelationshipsRootEntity>().Ignore(x => x.CollectionTrunk);
        modelBuilder.Entity<RelationshipsTrunkEntity>().Ignore(x => x.CollectionInverseRoot);

        // TODO: issue #31376 - complex optional references
        modelBuilder.Entity<RelationshipsTrunkEntity>()
            .Ignore(x => x.OptionalReferenceBranch);

        modelBuilder.Entity<RelationshipsTrunkEntity>()
            .ComplexProperty(x => x.RequiredReferenceBranch, bb =>
            {
                bb.IsRequired(true);
                bb.Ignore(x => x.Id);
                bb.Ignore(x => x.OptionalReferenceInverseTrunk);
                bb.Ignore(x => x.OptionalReferenceLeafId);

                // TODO: issue #31376 - complex optional references
                bb.Ignore(x => x.OptionalReferenceLeaf);

                bb.Ignore(x => x.RequiredReferenceInverseTrunk);
                bb.Ignore(x => x.RequiredReferenceLeafId);
                bb.ComplexProperty(x => x.RequiredReferenceLeaf, bbb =>
                {
                    bbb.IsRequired(true);
                    bbb.Ignore(x => x.Id);
                    bbb.Ignore(x => x.OptionalReferenceInverseBranch);
                    bbb.Ignore(x => x.RequiredReferenceInverseBranch);
                    bbb.Ignore(x => x.CollectionInverseBranch);
                    bbb.Ignore(x => x.CollectionBranchId);
                });

                bb.Ignore(x => x.CollectionInverseTrunk);
                bb.Ignore(x => x.CollectionTrunkId);
                bb.Ignore(x => x.CollectionLeaf);
            });

        //  collections are not supported for non-json compex types 
        modelBuilder.Entity<RelationshipsTrunkEntity>().Ignore(x => x.CollectionBranch);
    }

    public override IReadOnlyDictionary<Type, object> EntitySorters { get; } = new Dictionary<Type, Func<object?, object?>>
    {
        { typeof(RelationshipsRootEntity), e => ((RelationshipsRootEntity?)e)?.Id },
        { typeof(RelationshipsTrunkEntity), e => ((RelationshipsTrunkEntity?)e)?.Id },
        { typeof(RelationshipsBranchEntity), e => ((RelationshipsBranchEntity?)e)?.Name },
        { typeof(RelationshipsLeafEntity), e => ((RelationshipsLeafEntity?)e)?.Name }
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

                    AssertOwnedBranch(ee.RequiredReferenceBranch, aa.RequiredReferenceBranch);

                    // TODO: issue #31376 - complex optional references
                    //Assert.Equal(ee.OptionalReferenceBranch == null, aa.OptionalReferenceBranch == null);

                    if (ee.OptionalReferenceBranch != null && aa.OptionalReferenceBranch != null)
                    {
                        AssertOwnedBranch(ee.OptionalReferenceBranch, aa.OptionalReferenceBranch);
                    }
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
                    AssertOwnedBranch(ee, aa);
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
                    AssertOwnedLeaf(ee, aa);
                }
            }
        },
    }.ToDictionary(e => e.Key, e => (object)e.Value);

    public static void AssertOwnedBranch(RelationshipsBranchEntity expected, RelationshipsBranchEntity actual)
    {
        Assert.Equal(expected.Name, actual.Name);

        AssertOwnedLeaf(expected.RequiredReferenceLeaf, actual.RequiredReferenceLeaf);

        // TODO: issue #31376 - complex optional references
        //Assert.Equal(expected.OptionalReferenceLeaf == null, actual.OptionalReferenceLeaf == null);
        //if (expected.OptionalReferenceLeaf != null && actual.OptionalReferenceLeaf != null)
        //{
        //    AssertOwnedLeaf(expected.OptionalReferenceLeaf, actual.OptionalReferenceLeaf);
        //}
    }

    public static void AssertOwnedLeaf(RelationshipsLeafEntity expected, RelationshipsLeafEntity actual)
    {
        Assert.Equal(expected.Name, actual.Name);
    }
}
