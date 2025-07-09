// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.ComplexTableSplitting;

public abstract class ComplexTableSplittingRelationalFixtureBase : RelationshipsQueryFixtureBase
{
    protected override string StoreName => "ComplexTableSplittingQueryTest";

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<RelationshipsRoot>().ToTable("RootEntities");
        modelBuilder.Entity<RelationshipsTrunk>().ToTable("TrunkEntities");
        modelBuilder.Entity<RelationshipsTrunk>().Property(x => x.Id).ValueGeneratedNever();

        modelBuilder.Entity<RelationshipsRoot>()
            .HasOne(x => x.OptionalReferenceTrunk)
            .WithOne(x => x.OptionalReferenceInverseRoot)
            .HasForeignKey<RelationshipsRoot>(x => x.OptionalReferenceTrunkId)
            .IsRequired(false);

        // TODO: Why is this a navigation and not a complex property?
        modelBuilder.Entity<RelationshipsRoot>()
            .HasOne(x => x.RequiredReferenceTrunk)
            .WithOne(x => x.RequiredReferenceInverseRoot)
            .HasForeignKey<RelationshipsRoot>(x => x.RequiredReferenceTrunkId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(true);

        modelBuilder.Entity<RelationshipsRoot>().Ignore(x => x.CollectionTrunk);
        modelBuilder.Entity<RelationshipsTrunk>().Ignore(x => x.CollectionInverseRoot);

        // TODO: issue #31376 - complex optional references
        modelBuilder.Entity<RelationshipsTrunk>()
            .Ignore(x => x.OptionalReferenceBranch);

        modelBuilder.Entity<RelationshipsTrunk>()
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
        modelBuilder.Entity<RelationshipsTrunk>().Ignore(x => x.CollectionBranch);
    }

    protected override Task SeedAsync(RelationshipsContext context)
    {
        var rootEntities = RelationshipsData.CreateRootEntities();
        var trunkEntities = RelationshipsData.CreateTrunkEntitiesWithOwnerships();

        RelationshipsData.WireUp(rootEntities, trunkEntities, [], [], wireUpRootToTrunkOnly: true);

        context.Set<RelationshipsRoot>().AddRange(rootEntities);
        context.Set<RelationshipsTrunk>().AddRange(trunkEntities);

        return context.SaveChangesAsync();
    }

    public override IReadOnlyDictionary<Type, object> EntitySorters { get; } = new Dictionary<Type, Func<object?, object?>>
    {
        { typeof(RelationshipsRoot), e => ((RelationshipsRoot?)e)?.Id },
        { typeof(RelationshipsTrunk), e => ((RelationshipsTrunk?)e)?.Id },
        { typeof(RelationshipsBranch), e => ((RelationshipsBranch?)e)?.Name },
        { typeof(RelationshipsLeaf), e => ((RelationshipsLeaf?)e)?.Name }
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
            typeof(RelationshipsBranch), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (RelationshipsBranch)e!;
                    var aa = (RelationshipsBranch)a;
                    AssertOwnedBranch(ee, aa);
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
                    AssertOwnedLeaf(ee, aa);
                }
            }
        },
    }.ToDictionary(e => e.Key, e => (object)e.Value);

    public static void AssertOwnedBranch(RelationshipsBranch expected, RelationshipsBranch actual)
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

    public static void AssertOwnedLeaf(RelationshipsLeaf expected, RelationshipsLeaf actual)
    {
        Assert.Equal(expected.Name, actual.Name);
    }
}

