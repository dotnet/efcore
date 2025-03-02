// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query.Relationships;

public abstract class OwnedRelationshipsRelationalFixtureBase : OwnedRelationshipsFixtureBase
{
    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<RelationshipsRootEntity>().ToTable("RootEntities");
        modelBuilder.Entity<RelationshipsRootEntity>()
            .OwnsOne(x => x.OptionalReferenceTrunk, b =>
            {
                b.OwnsOne(x => x.OptionalReferenceBranch, bb =>
                {
                    bb.OwnsMany(x => x.CollectionLeaf, bbb =>
                    {
                        bbb.ToTable("Root_OptionalReferenceTrunk_OptionalReferenceBranch_CollectionLeaf");
                    });
                });

                b.OwnsOne(x => x.RequiredReferenceBranch, bb =>
                {
                    bb.OwnsMany(x => x.CollectionLeaf, bbb =>
                    {
                        bbb.ToTable("Root_OptionalReferenceTrunk_RequiredReferenceBranch_CollectionLeaf");
                    });
                });

                b.OwnsMany(x => x.CollectionBranch, bb =>
                {
                    bb.ToTable("Root_OptionalReferenceTrunk_CollectionBranch");

                    bb.OwnsMany(x => x.CollectionLeaf, bbb =>
                    {
                        bbb.ToTable("Root_OptionalReferenceTrunk_CollectionBranch_CollectionLeaf");
                    });
                });
            });

        modelBuilder.Entity<RelationshipsRootEntity>()
            .OwnsOne(x => x.RequiredReferenceTrunk, b =>
            {
                b.OwnsOne(x => x.OptionalReferenceBranch, bb =>
                {
                    bb.OwnsMany(x => x.CollectionLeaf, bbb =>
                    {
                        bbb.ToTable("Root_RequiredReferenceTrunk_OptionalReferenceBranch_CollectionLeaf");
                    });
                });

                b.OwnsOne(x => x.RequiredReferenceBranch, bb =>
                {
                    bb.OwnsMany(x => x.CollectionLeaf, bbb =>
                    {
                        bbb.ToTable("Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf");
                    });
                });

                b.OwnsMany(x => x.CollectionBranch, bb =>
                {
                    bb.ToTable("Root_RequiredReferenceTrunk_CollectionBranch");

                    bb.OwnsMany(x => x.CollectionLeaf, bbb =>
                    {
                        bbb.ToTable("Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf");
                    });
                });
            });
        modelBuilder.Entity<RelationshipsRootEntity>().Navigation(x => x.RequiredReferenceTrunk).IsRequired(true);

        modelBuilder.Entity<RelationshipsRootEntity>()
            .OwnsMany(x => x.CollectionTrunk, b =>
            {
                b.ToTable("Root_CollectionTrunk");

                b.OwnsOne(x => x.OptionalReferenceBranch, bb =>
                {
                    bb.OwnsMany(x => x.CollectionLeaf, bbb =>
                    {
                        bbb.ToTable("Root_CollectionTrunk_OptionalReferenceBranch_CollectionLeaf");
                    });
                });

                b.OwnsOne(x => x.RequiredReferenceBranch, bb =>
                {
                    bb.OwnsMany(x => x.CollectionLeaf, bbb =>
                    {
                        bbb.ToTable("Root_CollectionTrunk_RequiredReferenceBranch_CollectionLeaf");
                    });
                });

                b.OwnsMany(x => x.CollectionBranch, bb =>
                {
                    bb.ToTable("Root_CollectionTrunk_CollectionBranch");

                    bb.OwnsMany(x => x.CollectionLeaf, bbb =>
                    {
                        bbb.ToTable("Root_CollectionTrunk_CollectionBranch_CollectionLeaf");
                    });
                });
            });
    }
}
