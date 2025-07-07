// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query.Relationships;

public abstract class OwnedTableSplittingRelationshipsRelationalFixtureBase : OwnedRelationshipsRelationalFixtureBase
{
    protected override string StoreName => "OwnedTableSplittingRelationshipsQueryTest";

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<RelationshipsRootEntity>()
            .OwnsOne(x => x.OptionalReferenceTrunk, b =>
            {
                b.ToTable("Root_OptionalReferenceTrunk");
                b.OwnsOne(x => x.OptionalReferenceBranch, bb =>
                {
                    bb.ToTable("Root_OptionalReferenceTrunk_OptionalReferenceBranch");
                    bb.OwnsOne(x => x.RequiredReferenceLeaf).ToTable("Root_OptionalReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf");
                    bb.OwnsOne(x => x.OptionalReferenceLeaf).ToTable("Root_OptionalReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf");
                });

                b.OwnsOne(x => x.RequiredReferenceBranch, bb =>
                {
                    bb.ToTable("Root_OptionalReferenceTrunk_RequiredReferenceBranch");
                    bb.OwnsOne(x => x.RequiredReferenceLeaf).ToTable("Root_OptionalReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf");
                    bb.OwnsOne(x => x.OptionalReferenceLeaf).ToTable("Root_OptionalReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf");
                });

                b.OwnsMany(x => x.CollectionBranch, bb =>
                {
                    bb.OwnsOne(x => x.RequiredReferenceLeaf).ToTable("Root_OptionalReferenceTrunk_CollectionBranch_RequiredReferenceLeaf");
                    bb.OwnsOne(x => x.OptionalReferenceLeaf).ToTable("Root_OptionalReferenceTrunk_CollectionBranch_OptionalReferenceLeaf");
                });
            });

        modelBuilder.Entity<RelationshipsRootEntity>()
            .OwnsOne(x => x.RequiredReferenceTrunk, b =>
            {
                b.ToTable("Root_RequiredReferenceTrunk");
                b.OwnsOne(x => x.OptionalReferenceBranch, bb =>
                {
                    bb.ToTable("Root_RequiredReferenceTrunk_OptionalReferenceBranch");
                    bb.OwnsOne(x => x.RequiredReferenceLeaf).ToTable("Root_RequiredReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf");
                    bb.OwnsOne(x => x.OptionalReferenceLeaf).ToTable("Root_RequiredReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf");
                });

                b.OwnsOne(x => x.RequiredReferenceBranch, bb =>
                {
                    bb.ToTable("Root_RequiredReferenceTrunk_RequiredReferenceBranch");
                    bb.OwnsOne(x => x.RequiredReferenceLeaf).ToTable("Root_RequiredReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf");
                    bb.OwnsOne(x => x.OptionalReferenceLeaf).ToTable("Root_RequiredReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf");
                });

                b.OwnsMany(x => x.CollectionBranch, bb =>
                {
                    bb.OwnsOne(x => x.RequiredReferenceLeaf).ToTable("Root_RequiredReferenceTrunk_CollectionBranch_RequiredReferenceLeaf");
                    bb.OwnsOne(x => x.OptionalReferenceLeaf).ToTable("Root_RequiredReferenceTrunk_CollectionBranch_OptionalReferenceLeaf");
                });
            });

        modelBuilder.Entity<RelationshipsRootEntity>()
            .OwnsMany(x => x.CollectionTrunk, b =>
            {
                b.OwnsOne(x => x.OptionalReferenceBranch, bb =>
                {
                    bb.ToTable("Root_CollectionTrunk_OptionalReferenceBranch");
                    bb.OwnsOne(x => x.RequiredReferenceLeaf).ToTable("Root_CollectionTrunk_OptionalReferenceBranch_RequiredReferenceLeaf");
                    bb.OwnsOne(x => x.OptionalReferenceLeaf).ToTable("Root_CollectionTrunk_OptionalReferenceBranch_OptionalReferenceLeaf");
                });

                b.OwnsOne(x => x.RequiredReferenceBranch, bb =>
                {
                    bb.ToTable("Root_CollectionTrunk_RequiredReferenceBranch");
                    bb.OwnsOne(x => x.RequiredReferenceLeaf).ToTable("Root_CollectionTrunk_RequiredReferenceBranch_RequiredReferenceLeaf");
                    bb.OwnsOne(x => x.OptionalReferenceLeaf).ToTable("Root_CollectionTrunk_RequiredReferenceBranch_OptionalReferenceLeaf");
                });

                b.OwnsMany(x => x.CollectionBranch, bb =>
                {
                    bb.OwnsOne(x => x.RequiredReferenceLeaf).ToTable("Root_CollectionTrunk_CollectionBranch_RequiredReferenceLeaf");
                    bb.OwnsOne(x => x.OptionalReferenceLeaf).ToTable("Root_CollectionTrunk_CollectionBranch_OptionalReferenceLeaf");
                });
            });
    }
}
