// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

public class JsonQueryContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<EntityBasic> EntitiesBasic { get; set; } = null!;
    public DbSet<JsonEntityBasic> JsonEntitiesBasic { get; set; } = null!;
    public DbSet<JsonEntityBasicForReference> JsonEntitiesBasicForReference { get; set; } = null!;
    public DbSet<JsonEntityBasicForCollection> JsonEntitiesBasicForCollection { get; set; } = null!;
    public DbSet<JsonEntityCustomNaming> JsonEntitiesCustomNaming { get; set; } = null!;
    public DbSet<JsonEntitySingleOwned> JsonEntitiesSingleOwned { get; set; } = null!;
    public DbSet<JsonEntityInheritanceBase> JsonEntitiesInheritance { get; set; } = null!;
    public DbSet<JsonEntityAllTypes> JsonEntitiesAllTypes { get; set; } = null!;
    public DbSet<JsonEntityConverters> JsonEntitiesConverters { get; set; } = null!;

    public static Task SeedAsync(JsonQueryContext context)
    {
        var jsonEntitiesBasic = JsonQueryData.CreateJsonEntitiesBasic();
        var entitiesBasic = JsonQueryData.CreateEntitiesBasic();
        var jsonEntitiesBasicForReference = JsonQueryData.CreateJsonEntitiesBasicForReference();
        var jsonEntitiesBasicForCollection = JsonQueryData.CreateJsonEntitiesBasicForCollection();
        JsonQueryData.WireUp(jsonEntitiesBasic, entitiesBasic, jsonEntitiesBasicForReference, jsonEntitiesBasicForCollection);

        var jsonEntitiesCustomNaming = JsonQueryData.CreateJsonEntitiesCustomNaming();
        var jsonEntitiesSingleOwned = JsonQueryData.CreateJsonEntitiesSingleOwned();
        var jsonEntitiesInheritance = JsonQueryData.CreateJsonEntitiesInheritance();
        var jsonEntitiesAllTypes = JsonQueryData.CreateJsonEntitiesAllTypes();
        var jsonEntitiesConverters = JsonQueryData.CreateJsonEntitiesConverters();

        context.JsonEntitiesBasic.AddRange(jsonEntitiesBasic);
        context.EntitiesBasic.AddRange(entitiesBasic);
        context.JsonEntitiesBasicForReference.AddRange(jsonEntitiesBasicForReference);
        context.JsonEntitiesBasicForCollection.AddRange(jsonEntitiesBasicForCollection);
        context.JsonEntitiesCustomNaming.AddRange(jsonEntitiesCustomNaming);
        context.JsonEntitiesSingleOwned.AddRange(jsonEntitiesSingleOwned);
        context.JsonEntitiesInheritance.AddRange(jsonEntitiesInheritance);
        context.JsonEntitiesAllTypes.AddRange(jsonEntitiesAllTypes);
        context.JsonEntitiesConverters.AddRange(jsonEntitiesConverters);
        return context.SaveChangesAsync();
    }
}
