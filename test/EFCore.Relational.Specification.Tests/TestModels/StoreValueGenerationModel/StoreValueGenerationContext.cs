// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.StoreValueGenerationModel;

#nullable disable

public class StoreValueGenerationContext(DbContextOptions options) : PoolableDbContext(options)
{
    public DbSet<StoreValueGenerationData> WithSomeDatabaseGenerated
        => Set<StoreValueGenerationData>(nameof(WithSomeDatabaseGenerated));

    public DbSet<StoreValueGenerationData> WithSomeDatabaseGenerated2
        => Set<StoreValueGenerationData>(nameof(WithSomeDatabaseGenerated2));

    public DbSet<StoreValueGenerationData> WithNoDatabaseGenerated
        => Set<StoreValueGenerationData>(nameof(WithNoDatabaseGenerated));

    public DbSet<StoreValueGenerationData> WithNoDatabaseGenerated2
        => Set<StoreValueGenerationData>(nameof(WithNoDatabaseGenerated2));

    public DbSet<StoreValueGenerationData> WithAllDatabaseGenerated
        => Set<StoreValueGenerationData>(nameof(WithAllDatabaseGenerated));

    public DbSet<StoreValueGenerationData> WithAllDatabaseGenerated2
        => Set<StoreValueGenerationData>(nameof(WithAllDatabaseGenerated2));
}
