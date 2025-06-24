// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Relationships.OwnedNavigations;
using Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.JsonOwnedNavigations;

public abstract class JsonOwnedNavigationsRelationalFixtureBase : OwnedNavigationsFixtureBase
{
    protected override string StoreName => "OwnedNavigationsJsonQueryTest";

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<RelationshipsRoot>().ToTable("RootEntities");
        modelBuilder.Entity<RelationshipsRoot>().OwnsOne(x => x.OptionalReferenceTrunk).ToJson();
        modelBuilder.Entity<RelationshipsRoot>().OwnsOne(x => x.RequiredReferenceTrunk).ToJson();
        modelBuilder.Entity<RelationshipsRoot>().OwnsMany(x => x.CollectionTrunk).ToJson();
    }
}
