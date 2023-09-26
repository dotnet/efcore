// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.OptionalDependent;

public class OptionalDependentContext : DbContext
{
    public OptionalDependentContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<OptionalDependentEntityAllOptional> EntitiesAllOptional { get; set; }
    public DbSet<OptionalDependentEntitySomeRequired> EntitiesSomeRequired { get; set; }

    public static void Seed(OptionalDependentContext context)
    {
        var entitiesAllOptional = OptionalDependentData.CreateEntitiesAllOptional();
        var entitiesSomeRequired = OptionalDependentData.CreateEntitiesSomeRequired();

        context.EntitiesAllOptional.AddRange(entitiesAllOptional);
        context.EntitiesSomeRequired.AddRange(entitiesSomeRequired);
        context.SaveChanges();
    }
}
