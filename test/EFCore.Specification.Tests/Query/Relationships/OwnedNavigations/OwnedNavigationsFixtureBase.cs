// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.OwnedNavigations;

public abstract class OwnedNavigationsFixtureBase : RelationshipsQueryFixtureBase
{
    protected override string StoreName => "OwnedNavigationsQueryTest";

    protected override Task SeedAsync(RelationshipsContext context)
    {
        var rootEntitiesWithOwnerships = RelationshipsData.CreateRootEntitiesWithOwnerships();
        context.Set<RelationshipsRoot>().AddRange(rootEntitiesWithOwnerships);

        return context.SaveChangesAsync();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<RelationshipsRoot>()
            .OwnsOne(x => x.OptionalReferenceTrunk, b =>
            {
                b.Ignore(x => x.Id);
                b.Ignore(x => x.OptionalReferenceInverseRoot);
                b.Ignore(x => x.OptionalReferenceBranchId);
                b.OwnsOne(x => x.OptionalReferenceBranch, bb =>
                {
                    bb.Ignore(x => x.Id);
                    bb.Ignore(x => x.OptionalReferenceInverseTrunk);
                    bb.Ignore(x => x.OptionalReferenceLeafId);
                    bb.OwnsOne(x => x.OptionalReferenceLeaf, bbb =>
                    {
                        bbb.Ignore(x => x.Id);
                        bbb.Ignore(x => x.OptionalReferenceInverseBranch);
                        bbb.Ignore(x => x.RequiredReferenceInverseBranch);
                        bbb.Ignore(x => x.CollectionInverseBranch);
                        bbb.Ignore(x => x.CollectionBranchId);
                    });
                    bb.Navigation(x => x.OptionalReferenceLeaf).IsRequired(false);

                    bb.Ignore(x => x.RequiredReferenceInverseTrunk);
                    bb.Ignore(x => x.RequiredReferenceLeafId);
                    bb.OwnsOne(x => x.RequiredReferenceLeaf, bbb =>
                    {
                        bbb.Ignore(x => x.Id);
                        bbb.Ignore(x => x.OptionalReferenceInverseBranch);
                        bbb.Ignore(x => x.RequiredReferenceInverseBranch);
                        bbb.Ignore(x => x.CollectionInverseBranch);
                        bbb.Ignore(x => x.CollectionBranchId);
                    });
                    bb.Navigation(x => x.RequiredReferenceLeaf).IsRequired(true);

                    bb.Ignore(x => x.CollectionInverseTrunk);
                    bb.Ignore(x => x.CollectionTrunkId);
                    bb.OwnsMany(x => x.CollectionLeaf, bbb =>
                    {
                        bbb.Ignore(x => x.Id);
                        bbb.Ignore(x => x.OptionalReferenceInverseBranch);
                        bbb.Ignore(x => x.RequiredReferenceInverseBranch);
                        bbb.Ignore(x => x.CollectionInverseBranch);
                        bbb.Ignore(x => x.CollectionBranchId);
                    });
                });
                b.Navigation(x => x.OptionalReferenceBranch).IsRequired(false);

                b.Ignore(x => x.RequiredReferenceInverseRoot);
                b.Ignore(x => x.RequiredReferenceBranchId);
                b.OwnsOne(x => x.RequiredReferenceBranch, bb =>
                {
                    bb.Ignore(x => x.Id);
                    bb.Ignore(x => x.OptionalReferenceInverseTrunk);
                    bb.Ignore(x => x.OptionalReferenceLeafId);
                    bb.OwnsOne(x => x.OptionalReferenceLeaf, bbb =>
                    {
                        bbb.Ignore(x => x.Id);
                        bbb.Ignore(x => x.OptionalReferenceInverseBranch);
                        bbb.Ignore(x => x.RequiredReferenceInverseBranch);
                        bbb.Ignore(x => x.CollectionInverseBranch);
                        bbb.Ignore(x => x.CollectionBranchId);
                    });
                    bb.Navigation(x => x.OptionalReferenceLeaf).IsRequired(false);

                    bb.Ignore(x => x.RequiredReferenceInverseTrunk);
                    bb.Ignore(x => x.RequiredReferenceLeafId);
                    bb.OwnsOne(x => x.RequiredReferenceLeaf, bbb =>
                    {
                        bbb.Ignore(x => x.Id);
                        bbb.Ignore(x => x.OptionalReferenceInverseBranch);
                        bbb.Ignore(x => x.RequiredReferenceInverseBranch);
                        bbb.Ignore(x => x.CollectionInverseBranch);
                        bbb.Ignore(x => x.CollectionBranchId);
                    });
                    bb.Navigation(x => x.RequiredReferenceLeaf).IsRequired(true);

                    bb.Ignore(x => x.CollectionInverseTrunk);
                    bb.Ignore(x => x.CollectionTrunkId);
                    bb.OwnsMany(x => x.CollectionLeaf, bbb =>
                    {
                        bbb.Ignore(x => x.Id);
                        bbb.Ignore(x => x.OptionalReferenceInverseBranch);
                        bbb.Ignore(x => x.RequiredReferenceInverseBranch);
                        bbb.Ignore(x => x.CollectionInverseBranch);
                        bbb.Ignore(x => x.CollectionBranchId);
                    });
                });
                b.Navigation(x => x.RequiredReferenceBranch).IsRequired(true);

                b.Ignore(x => x.CollectionInverseRoot);
                b.Ignore(x => x.CollectionRootId);
                b.OwnsMany(x => x.CollectionBranch, bb =>
                {
                    bb.Ignore(x => x.Id);
                    bb.Ignore(x => x.OptionalReferenceInverseTrunk);
                    bb.Ignore(x => x.OptionalReferenceLeafId);
                    bb.OwnsOne(x => x.OptionalReferenceLeaf, bbb =>
                    {
                        bbb.Ignore(x => x.Id);
                        bbb.Ignore(x => x.OptionalReferenceInverseBranch);
                        bbb.Ignore(x => x.RequiredReferenceInverseBranch);
                        bbb.Ignore(x => x.CollectionInverseBranch);
                        bbb.Ignore(x => x.CollectionBranchId);
                    });
                    bb.Navigation(x => x.OptionalReferenceLeaf).IsRequired(false);

                    bb.Ignore(x => x.RequiredReferenceInverseTrunk);
                    bb.Ignore(x => x.RequiredReferenceLeafId);
                    bb.OwnsOne(x => x.RequiredReferenceLeaf, bbb =>
                    {
                        bbb.Ignore(x => x.Id);
                        bbb.Ignore(x => x.OptionalReferenceInverseBranch);
                        bbb.Ignore(x => x.RequiredReferenceInverseBranch);
                        bbb.Ignore(x => x.CollectionInverseBranch);
                        bbb.Ignore(x => x.CollectionBranchId);
                    });
                    bb.Navigation(x => x.RequiredReferenceLeaf).IsRequired(true);

                    bb.Ignore(x => x.CollectionInverseTrunk);
                    bb.Ignore(x => x.CollectionTrunkId);
                    bb.OwnsMany(x => x.CollectionLeaf, bbb =>
                    {
                        bbb.Ignore(x => x.Id);
                        bbb.Ignore(x => x.OptionalReferenceInverseBranch);
                        bbb.Ignore(x => x.RequiredReferenceInverseBranch);
                        bbb.Ignore(x => x.CollectionInverseBranch);
                        bbb.Ignore(x => x.CollectionBranchId);
                    });
                });
            });
        modelBuilder.Entity<RelationshipsRoot>().Navigation(x => x.OptionalReferenceTrunk).IsRequired(false);

        modelBuilder.Entity<RelationshipsRoot>()
            .OwnsOne(x => x.RequiredReferenceTrunk, b =>
            {
                b.Ignore(x => x.Id);
                b.Ignore(x => x.OptionalReferenceInverseRoot);
                b.Ignore(x => x.OptionalReferenceBranchId);
                b.OwnsOne(x => x.OptionalReferenceBranch, bb =>
                {
                    bb.Ignore(x => x.Id);
                    bb.Ignore(x => x.OptionalReferenceInverseTrunk);
                    bb.Ignore(x => x.OptionalReferenceLeafId);
                    bb.OwnsOne(x => x.OptionalReferenceLeaf, bbb =>
                    {
                        bbb.Ignore(x => x.Id);
                        bbb.Ignore(x => x.OptionalReferenceInverseBranch);
                        bbb.Ignore(x => x.RequiredReferenceInverseBranch);
                        bbb.Ignore(x => x.CollectionInverseBranch);
                        bbb.Ignore(x => x.CollectionBranchId);
                    });
                    bb.Navigation(x => x.OptionalReferenceLeaf).IsRequired(false);

                    bb.Ignore(x => x.RequiredReferenceInverseTrunk);
                    bb.Ignore(x => x.RequiredReferenceLeafId);
                    bb.OwnsOne(x => x.RequiredReferenceLeaf, bbb =>
                    {
                        bbb.Ignore(x => x.Id);
                        bbb.Ignore(x => x.OptionalReferenceInverseBranch);
                        bbb.Ignore(x => x.RequiredReferenceInverseBranch);
                        bbb.Ignore(x => x.CollectionInverseBranch);
                        bbb.Ignore(x => x.CollectionBranchId);
                    });
                    bb.Navigation(x => x.RequiredReferenceLeaf).IsRequired(true);

                    bb.Ignore(x => x.CollectionInverseTrunk);
                    bb.Ignore(x => x.CollectionTrunkId);
                    bb.OwnsMany(x => x.CollectionLeaf, bbb =>
                    {
                        bbb.Ignore(x => x.Id);
                        bbb.Ignore(x => x.OptionalReferenceInverseBranch);
                        bbb.Ignore(x => x.RequiredReferenceInverseBranch);
                        bbb.Ignore(x => x.CollectionInverseBranch);
                        bbb.Ignore(x => x.CollectionBranchId);
                    });
                });
                b.Navigation(x => x.OptionalReferenceBranch).IsRequired(false);

                b.Ignore(x => x.RequiredReferenceInverseRoot);
                b.Ignore(x => x.RequiredReferenceBranchId);
                b.OwnsOne(x => x.RequiredReferenceBranch, bb =>
                {
                    bb.Ignore(x => x.Id);
                    bb.Ignore(x => x.OptionalReferenceInverseTrunk);
                    bb.Ignore(x => x.OptionalReferenceLeafId);
                    bb.OwnsOne(x => x.OptionalReferenceLeaf, bbb =>
                    {
                        bbb.Ignore(x => x.Id);
                        bbb.Ignore(x => x.OptionalReferenceInverseBranch);
                        bbb.Ignore(x => x.RequiredReferenceInverseBranch);
                        bbb.Ignore(x => x.CollectionInverseBranch);
                        bbb.Ignore(x => x.CollectionBranchId);
                    });
                    bb.Navigation(x => x.OptionalReferenceLeaf).IsRequired(false);

                    bb.Ignore(x => x.RequiredReferenceInverseTrunk);
                    bb.Ignore(x => x.RequiredReferenceLeafId);
                    bb.OwnsOne(x => x.RequiredReferenceLeaf, bbb =>
                    {
                        bbb.Ignore(x => x.Id);
                        bbb.Ignore(x => x.OptionalReferenceInverseBranch);
                        bbb.Ignore(x => x.RequiredReferenceInverseBranch);
                        bbb.Ignore(x => x.CollectionInverseBranch);
                        bbb.Ignore(x => x.CollectionBranchId);
                    });
                    bb.Navigation(x => x.RequiredReferenceLeaf).IsRequired(true);

                    bb.Ignore(x => x.CollectionInverseTrunk);
                    bb.Ignore(x => x.CollectionTrunkId);
                    bb.OwnsMany(x => x.CollectionLeaf, bbb =>
                    {
                        bbb.Ignore(x => x.Id);
                        bbb.Ignore(x => x.OptionalReferenceInverseBranch);
                        bbb.Ignore(x => x.RequiredReferenceInverseBranch);
                        bbb.Ignore(x => x.CollectionInverseBranch);
                        bbb.Ignore(x => x.CollectionBranchId);
                    });
                });
                b.Navigation(x => x.RequiredReferenceBranch).IsRequired(true);

                b.Ignore(x => x.CollectionInverseRoot);
                b.Ignore(x => x.CollectionRootId);
                b.OwnsMany(x => x.CollectionBranch, bb =>
                {
                    bb.Ignore(x => x.Id);
                    bb.Ignore(x => x.OptionalReferenceInverseTrunk);
                    bb.Ignore(x => x.OptionalReferenceLeafId);
                    bb.OwnsOne(x => x.OptionalReferenceLeaf, bbb =>
                    {
                        bbb.Ignore(x => x.Id);
                        bbb.Ignore(x => x.OptionalReferenceInverseBranch);
                        bbb.Ignore(x => x.RequiredReferenceInverseBranch);
                        bbb.Ignore(x => x.CollectionInverseBranch);
                        bbb.Ignore(x => x.CollectionBranchId);
                    });
                    bb.Navigation(x => x.OptionalReferenceLeaf).IsRequired(false);

                    bb.Ignore(x => x.RequiredReferenceInverseTrunk);
                    bb.Ignore(x => x.RequiredReferenceLeafId);
                    bb.OwnsOne(x => x.RequiredReferenceLeaf, bbb =>
                    {
                        bbb.Ignore(x => x.Id);
                        bbb.Ignore(x => x.OptionalReferenceInverseBranch);
                        bbb.Ignore(x => x.RequiredReferenceInverseBranch);
                        bbb.Ignore(x => x.CollectionInverseBranch);
                        bbb.Ignore(x => x.CollectionBranchId);
                    });
                    bb.Navigation(x => x.RequiredReferenceLeaf).IsRequired(true);

                    bb.Ignore(x => x.CollectionInverseTrunk);
                    bb.Ignore(x => x.CollectionTrunkId);
                    bb.OwnsMany(x => x.CollectionLeaf, bbb =>
                    {
                        bbb.Ignore(x => x.Id);
                        bbb.Ignore(x => x.OptionalReferenceInverseBranch);
                        bbb.Ignore(x => x.RequiredReferenceInverseBranch);
                        bbb.Ignore(x => x.CollectionInverseBranch);
                        bbb.Ignore(x => x.CollectionBranchId);
                    });
                });
            });
        modelBuilder.Entity<RelationshipsRoot>().Navigation(x => x.RequiredReferenceTrunk).IsRequired(true);

        modelBuilder.Entity<RelationshipsRoot>()
            .OwnsMany(x => x.CollectionTrunk, b =>
            {
                b.Ignore(x => x.Id);
                b.Ignore(x => x.OptionalReferenceInverseRoot);
                b.Ignore(x => x.OptionalReferenceBranchId);
                b.OwnsOne(x => x.OptionalReferenceBranch, bb =>
                {
                    bb.Ignore(x => x.Id);
                    bb.Ignore(x => x.OptionalReferenceInverseTrunk);
                    bb.Ignore(x => x.OptionalReferenceLeafId);
                    bb.OwnsOne(x => x.OptionalReferenceLeaf, bbb =>
                    {
                        bbb.Ignore(x => x.Id);
                        bbb.Ignore(x => x.OptionalReferenceInverseBranch);
                        bbb.Ignore(x => x.RequiredReferenceInverseBranch);
                        bbb.Ignore(x => x.CollectionInverseBranch);
                        bbb.Ignore(x => x.CollectionBranchId);
                    });
                    bb.Navigation(x => x.OptionalReferenceLeaf).IsRequired(false);

                    bb.Ignore(x => x.RequiredReferenceInverseTrunk);
                    bb.Ignore(x => x.RequiredReferenceLeafId);
                    bb.OwnsOne(x => x.RequiredReferenceLeaf, bbb =>
                    {
                        bbb.Ignore(x => x.Id);
                        bbb.Ignore(x => x.OptionalReferenceInverseBranch);
                        bbb.Ignore(x => x.RequiredReferenceInverseBranch);
                        bbb.Ignore(x => x.CollectionInverseBranch);
                        bbb.Ignore(x => x.CollectionBranchId);
                    });
                    bb.Navigation(x => x.RequiredReferenceLeaf).IsRequired(true);

                    bb.Ignore(x => x.CollectionInverseTrunk);
                    bb.Ignore(x => x.CollectionTrunkId);
                    bb.OwnsMany(x => x.CollectionLeaf, bbb =>
                    {
                        bbb.Ignore(x => x.Id);
                        bbb.Ignore(x => x.OptionalReferenceInverseBranch);
                        bbb.Ignore(x => x.RequiredReferenceInverseBranch);
                        bbb.Ignore(x => x.CollectionInverseBranch);
                        bbb.Ignore(x => x.CollectionBranchId);
                    });
                });
                b.Navigation(x => x.OptionalReferenceBranch).IsRequired(false);

                b.Ignore(x => x.RequiredReferenceInverseRoot);
                b.Ignore(x => x.RequiredReferenceBranchId);
                b.OwnsOne(x => x.RequiredReferenceBranch, bb =>
                {
                    bb.Ignore(x => x.Id);
                    bb.Ignore(x => x.OptionalReferenceInverseTrunk);
                    bb.Ignore(x => x.OptionalReferenceLeafId);
                    bb.OwnsOne(x => x.OptionalReferenceLeaf, bbb =>
                    {
                        bbb.Ignore(x => x.Id);
                        bbb.Ignore(x => x.OptionalReferenceInverseBranch);
                        bbb.Ignore(x => x.RequiredReferenceInverseBranch);
                        bbb.Ignore(x => x.CollectionInverseBranch);
                        bbb.Ignore(x => x.CollectionBranchId);
                    });
                    bb.Navigation(x => x.OptionalReferenceLeaf).IsRequired(false);

                    bb.Ignore(x => x.RequiredReferenceInverseTrunk);
                    bb.Ignore(x => x.RequiredReferenceLeafId);
                    bb.OwnsOne(x => x.RequiredReferenceLeaf, bbb =>
                    {
                        bbb.Ignore(x => x.Id);
                        bbb.Ignore(x => x.OptionalReferenceInverseBranch);
                        bbb.Ignore(x => x.RequiredReferenceInverseBranch);
                        bbb.Ignore(x => x.CollectionInverseBranch);
                        bbb.Ignore(x => x.CollectionBranchId);
                    });
                    bb.Navigation(x => x.RequiredReferenceLeaf).IsRequired(true);

                    bb.Ignore(x => x.CollectionInverseTrunk);
                    bb.Ignore(x => x.CollectionTrunkId);
                    bb.OwnsMany(x => x.CollectionLeaf, bbb =>
                    {
                        bbb.Ignore(x => x.Id);
                        bbb.Ignore(x => x.OptionalReferenceInverseBranch);
                        bbb.Ignore(x => x.RequiredReferenceInverseBranch);
                        bbb.Ignore(x => x.CollectionInverseBranch);
                        bbb.Ignore(x => x.CollectionBranchId);
                    });
                });
                b.Navigation(x => x.RequiredReferenceBranch).IsRequired(true);

                b.Ignore(x => x.CollectionInverseRoot);
                b.Ignore(x => x.CollectionRootId);
                b.OwnsMany(x => x.CollectionBranch, bb =>
                {
                    bb.Ignore(x => x.Id);
                    bb.Ignore(x => x.OptionalReferenceInverseTrunk);
                    bb.Ignore(x => x.OptionalReferenceLeafId);
                    bb.OwnsOne(x => x.OptionalReferenceLeaf, bbb =>
                    {
                        bbb.Ignore(x => x.Id);
                        bbb.Ignore(x => x.OptionalReferenceInverseBranch);
                        bbb.Ignore(x => x.RequiredReferenceInverseBranch);
                        bbb.Ignore(x => x.CollectionInverseBranch);
                        bbb.Ignore(x => x.CollectionBranchId);
                    });
                    bb.Navigation(x => x.OptionalReferenceLeaf).IsRequired(false);

                    bb.Ignore(x => x.RequiredReferenceInverseTrunk);
                    bb.Ignore(x => x.RequiredReferenceLeafId);
                    bb.OwnsOne(x => x.RequiredReferenceLeaf, bbb =>
                    {
                        bbb.Ignore(x => x.Id);
                        bbb.Ignore(x => x.OptionalReferenceInverseBranch);
                        bbb.Ignore(x => x.RequiredReferenceInverseBranch);
                        bbb.Ignore(x => x.CollectionInverseBranch);
                        bbb.Ignore(x => x.CollectionBranchId);
                    });
                    bb.Navigation(x => x.RequiredReferenceLeaf).IsRequired(true);

                    bb.Ignore(x => x.CollectionInverseTrunk);
                    bb.Ignore(x => x.CollectionTrunkId);
                    bb.OwnsMany(x => x.CollectionLeaf, bbb =>
                    {
                        bbb.Ignore(x => x.Id);
                        bbb.Ignore(x => x.OptionalReferenceInverseBranch);
                        bbb.Ignore(x => x.RequiredReferenceInverseBranch);
                        bbb.Ignore(x => x.CollectionInverseBranch);
                        bbb.Ignore(x => x.CollectionBranchId);
                    });
                });
            });
    }

    public override IReadOnlyDictionary<Type, object> EntitySorters { get; } = new Dictionary<Type, Func<object?, object?>>
    {
        { typeof(RelationshipsRoot), e => ((RelationshipsRoot?)e)?.Id },
        { typeof(RelationshipsTrunk), e => ((RelationshipsTrunk?)e)?.Name },
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

                    Assert.Equal(ee.Name, aa.Name);
                    AssertOwnedTrunk(ee.RequiredReferenceTrunk, aa.RequiredReferenceTrunk);
                    Assert.Equal(ee.OptionalReferenceTrunk == null, aa.OptionalReferenceTrunk == null);

                    if (ee.OptionalReferenceTrunk != null && aa.OptionalReferenceTrunk != null)
                    {
                        AssertOwnedTrunk(ee.OptionalReferenceTrunk, aa.OptionalReferenceTrunk);
                    }

                    // TODO: cosmos may return null for empty collections, consider having separate asserters for cosmos
                    // once models are established (to avoid multiple copy-paste while we iterate over it initially)
                    Assert.Equal(ee.CollectionTrunk.Count, aa.CollectionTrunk?.Count ?? 0);
                    for (var i = 0; i < ee.CollectionTrunk.Count; i++)
                    {
                        AssertOwnedTrunk(ee.CollectionTrunk[i], aa.CollectionTrunk![i]);
                    }
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
                    AssertOwnedTrunk(ee, aa);
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

    public static void AssertOwnedTrunk(RelationshipsTrunk expected, RelationshipsTrunk actual)
    {
        Assert.Equal(expected.Name, actual.Name);

        AssertOwnedBranch(expected.RequiredReferenceBranch, actual.RequiredReferenceBranch);

        Assert.Equal(expected.OptionalReferenceBranch == null, actual.OptionalReferenceBranch == null);
        if (expected.OptionalReferenceBranch != null && actual.OptionalReferenceBranch != null)
        {
            AssertOwnedBranch(expected.OptionalReferenceBranch, actual.OptionalReferenceBranch);
        }

        Assert.Equal(expected.CollectionBranch.Count, actual.CollectionBranch?.Count ?? 0);
        var expectedCollection = expected.CollectionBranch.OrderBy(e => e.Name).ToList();
        var actualCollection = actual.CollectionBranch is null ? [] : actual.CollectionBranch.OrderBy(e => e.Name).ToList();
        for (var i = 0; i < expected.CollectionBranch.Count; i++)
        {
            AssertOwnedBranch(expectedCollection[i], actualCollection![i]);
        }
    }

    public static void AssertOwnedBranch(RelationshipsBranch expected, RelationshipsBranch actual)
    {
        Assert.Equal(expected.Name, actual.Name);

        AssertOwnedLeaf(expected.RequiredReferenceLeaf, actual.RequiredReferenceLeaf);

        Assert.Equal(expected.OptionalReferenceLeaf == null, actual.OptionalReferenceLeaf == null);
        if (expected.OptionalReferenceLeaf != null && actual.OptionalReferenceLeaf != null)
        {
            AssertOwnedLeaf(expected.OptionalReferenceLeaf, actual.OptionalReferenceLeaf);
        }

        Assert.Equal(expected.CollectionLeaf.Count, actual.CollectionLeaf?.Count ?? 0);
        var expectedCollection = expected.CollectionLeaf.OrderBy(e => e.Name).ToList();
        var actualCollection = actual.CollectionLeaf is null ? [] : actual.CollectionLeaf.OrderBy(e => e.Name).ToList();
        for (var i = 0; i < expected.CollectionLeaf.Count; i++)
        {
            AssertOwnedLeaf(expectedCollection[i], actualCollection![i]);
        }
    }

    public static void AssertOwnedLeaf(RelationshipsLeaf expected, RelationshipsLeaf actual)
    {
        Assert.Equal(expected.Name, actual.Name);
    }
}
