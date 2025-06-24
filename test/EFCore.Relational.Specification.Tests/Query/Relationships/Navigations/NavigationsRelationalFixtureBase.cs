// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Navigations;

public abstract class NavigationsRelationalFixtureBase : NavigationsFixtureBase
{
    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<RelationshipsRoot>().ToTable("RootEntities");
        modelBuilder.Entity<RelationshipsTrunk>().ToTable("TrunkEntities");
        modelBuilder.Entity<RelationshipsBranch>().ToTable("BranchEntities");
        modelBuilder.Entity<RelationshipsLeaf>().ToTable("LeafEntities");
    }
}
