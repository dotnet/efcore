// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query.Relationships;

public abstract class ComplexRelationsjipsRelationalFixtureBase : ComplexRelationshipsFixtureBase
{
    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<RelationshipsRootEntity>().ToTable("RootEntities");
        modelBuilder.Entity<RelationshipsTrunkEntity>().ToTable("TrunkEntities");
        modelBuilder.Entity<RelationshipsTrunkEntity>().Property(x => x.Id).ValueGeneratedNever();
    }
}
