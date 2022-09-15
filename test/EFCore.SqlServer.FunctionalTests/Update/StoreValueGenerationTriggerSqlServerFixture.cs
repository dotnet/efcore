// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.StoreValueGenerationModel;

namespace Microsoft.EntityFrameworkCore.Update;

public abstract class StoreValueGenerationTriggerSqlServerFixture : StoreValueGenerationSqlServerFixtureBase
{
    protected override void Seed(StoreValueGenerationContext context)
    {
        base.Seed(context);

        // Add triggers to all tables
        foreach (var table in context.Model.GetEntityTypes().Select(e => e.GetTableName()))
        {
            context.Database.ExecuteSqlRaw(
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
            modelBuilder.Entity(entity.Name).ToTable(b => b.HasTrigger(entity.GetTableName() + "_Trigger"));
        }
    }
}
