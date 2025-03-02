// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query.Relationships;

public abstract class NavigationRelationshipsRelationalFixtureBase : NavigationRelationshipsFixtureBase
{
    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<RelationshipsRootEntity>().ToTable("RootEntities");
        modelBuilder.Entity<RelationshipsTrunkEntity>().ToTable("TrunkEntities");
        modelBuilder.Entity<RelationshipsBranchEntity>().ToTable("BranchEntities");
        modelBuilder.Entity<RelationshipsLeafEntity>().ToTable("LeafEntities");
    }
}
