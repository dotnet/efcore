// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.StoreValueGenerationModel;

namespace Microsoft.EntityFrameworkCore.Update;

#nullable disable

public abstract class StoreValueGenerationWithoutOutputSqlServerFixture : StoreValueGenerationSqlServerFixtureBase
{
    protected override async Task SeedAsync(StoreValueGenerationContext context)
    {
        await base.SeedAsync(context);

        // Add triggers to all tables
        foreach (var table in context.Model.GetEntityTypes().Select(e => e.GetTableName()))
        {
            await context.Database.ExecuteSqlRawAsync(
                $@"
CREATE OR ALTER TRIGGER [{table}_Trigger]
ON [{table}]
FOR INSERT, UPDATE, DELETE AS
BEGIN
	IF @@ROWCOUNT = 0
		return
END");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            modelBuilder.Entity(entity.Name).ToTable(b => b.UseSqlOutputClause(false));
        }
    }
}
