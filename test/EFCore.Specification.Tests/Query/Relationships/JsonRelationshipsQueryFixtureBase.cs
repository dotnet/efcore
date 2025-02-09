// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query.Relationships;

public abstract class JsonRelationshipsQueryFixtureBase : OwnedRelationshipsQueryFixtureBase
{
    protected override string StoreName => "JsonRelationshipsQueryTest";

    protected override Task SeedAsync(RelationshipsContext context)
    {
        var rootEntitiesWithOwnerships = RelationshipsData.CreateRootEntitiesWithOwnerships();
        context.Set<RelationshipsRootEntity>().AddRange(rootEntitiesWithOwnerships);

        return context.SaveChangesAsync();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        // TODO: consider creating model explicitly (and derive from base fixture rather than owned) once we agree on the shape etc.
        base.OnModelCreating(modelBuilder, context);
    }
}
