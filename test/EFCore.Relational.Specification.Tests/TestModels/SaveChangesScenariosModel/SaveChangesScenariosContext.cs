// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.SaveChangesScenariosModel;

#nullable enable

public class SaveChangesScenariosContext : PoolableDbContext
{
    public SaveChangesScenariosContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<SaveChangesData> WithSomeDatabaseGenerated
        => Set<SaveChangesData>(nameof(WithSomeDatabaseGenerated));
    public DbSet<SaveChangesData> WithSomeDatabaseGenerated2
        => Set<SaveChangesData>(nameof(WithSomeDatabaseGenerated2));

    public DbSet<SaveChangesData> WithNoDatabaseGenerated
        => Set<SaveChangesData>(nameof(WithNoDatabaseGenerated));
    public DbSet<SaveChangesData> WithNoDatabaseGenerated2
        => Set<SaveChangesData>(nameof(WithNoDatabaseGenerated2));

    public DbSet<SaveChangesData> WithAllDatabaseGenerated
        => Set<SaveChangesData>(nameof(WithAllDatabaseGenerated));
    public DbSet<SaveChangesData> WithAllDatabaseGenerated2
        => Set<SaveChangesData>(nameof(WithAllDatabaseGenerated2));
}
