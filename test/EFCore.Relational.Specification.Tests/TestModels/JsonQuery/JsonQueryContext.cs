// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.JsonQuery
{
    public class JsonQueryContext : DbContext
    {
        public JsonQueryContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<JsonEntityBasic> JsonEntitiesBasic { get; set; }
        public DbSet<JsonEntityBasicForReference> JsonEntitiesBasicForReference { get; set; }
        public DbSet<JsonEntityBasicForCollection> JsonEntitiesBasicForCollection { get; set; }
        public DbSet<JsonEntityCustomNaming> JsonEntitiesCustomNaming { get; set; }
        public DbSet<JsonEntitySingleOwned> JsonEntitiesSingleOwned { get; set; }
        public DbSet<JsonEntityInheritanceBase> JsonEntitiesInheritance { get; set; }

        public static void Seed(JsonQueryContext context)
        {
            var jsonEntitiesBasic = JsonQueryData.CreateJsonEntitiesBasic();
            var jsonEntitiesBasicForReference = JsonQueryData.CreateJsonEntitiesBasicForReference();
            var jsonEntitiesBasicForCollection = JsonQueryData.CreateJsonEntitiesBasicForCollection();
            JsonQueryData.WireUp(jsonEntitiesBasic, jsonEntitiesBasicForReference, jsonEntitiesBasicForCollection);

            var jsonEntitiesCustomNaming = JsonQueryData.CreateJsonEntitiesCustomNaming();
            var jsonEntitiesSingleOwned = JsonQueryData.CreateJsonEntitiesSingleOwned();
            var jsonEntitiesInheritance = JsonQueryData.CreateJsonEntitiesInheritance();

            context.JsonEntitiesBasic.AddRange(jsonEntitiesBasic);
            context.JsonEntitiesBasicForReference.AddRange(jsonEntitiesBasicForReference);
            context.JsonEntitiesBasicForCollection.AddRange(jsonEntitiesBasicForCollection);
            context.JsonEntitiesCustomNaming.AddRange(jsonEntitiesCustomNaming);
            context.JsonEntitiesSingleOwned.AddRange(jsonEntitiesSingleOwned);
            context.JsonEntitiesInheritance.AddRange(jsonEntitiesInheritance);
            context.SaveChanges();
        }
    }
}
