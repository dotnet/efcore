﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class TPCManyToManyQueryRelationalFixture : ManyToManyQueryRelationalFixture
{
    protected override string StoreName { get; } = "TPCManyToManyQueryTest";

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).ConfigureWarnings(
            w =>
                w.Log(RelationalEventId.ForeignKeyTpcPrincipalWarning));

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<EntityRoot>().UseTpcMappingStrategy();

        modelBuilder.Entity<EntityRoot>().ToTable("Roots");
        modelBuilder.Entity<EntityBranch>().ToTable("Branches");
        modelBuilder.Entity<EntityLeaf>().ToTable("Leaves");
    }
}
