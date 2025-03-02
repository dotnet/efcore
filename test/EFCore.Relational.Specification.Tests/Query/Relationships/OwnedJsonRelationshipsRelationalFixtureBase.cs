// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query.Relationships;

public abstract class OwnedJsonRelationshipsRelationalFixtureBase : OwnedJsonRelationshipsFixtureBase
{
    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<RelationshipsRootEntity>().ToTable("RootEntities");
        modelBuilder.Entity<RelationshipsRootEntity>().OwnsOne(x => x.OptionalReferenceTrunk).ToJson();
        modelBuilder.Entity<RelationshipsRootEntity>().OwnsOne(x => x.RequiredReferenceTrunk).ToJson();
        modelBuilder.Entity<RelationshipsRootEntity>().OwnsMany(x => x.CollectionTrunk).ToJson();
    }
}
