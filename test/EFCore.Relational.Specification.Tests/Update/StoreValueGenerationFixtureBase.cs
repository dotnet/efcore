// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.StoreValueGenerationModel;

namespace Microsoft.EntityFrameworkCore.Update;

#nullable disable

public abstract class StoreValueGenerationFixtureBase : SharedStoreFixtureBase<StoreValueGenerationContext>
{
    protected override string StoreName
        => "StoreValueGenerationTest";

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        var sqlGenerationHelper = context.GetService<ISqlGenerationHelper>();

        foreach (var name in new[]
                 {
                     nameof(StoreValueGenerationContext.WithNoDatabaseGenerated),
                     nameof(StoreValueGenerationContext.WithNoDatabaseGenerated2)
                 })
        {
            modelBuilder
                .SharedTypeEntity<StoreValueGenerationData>(name)
                .Property(w => w.Id)
                .ValueGeneratedNever();
        }

        foreach (var name in new[]
                 {
                     nameof(StoreValueGenerationContext.WithSomeDatabaseGenerated),
                     nameof(StoreValueGenerationContext.WithSomeDatabaseGenerated2)
                 })
        {
            modelBuilder
                .SharedTypeEntity<StoreValueGenerationData>(name)
                .Property(w => w.Data1)
                .HasComputedColumnSql(sqlGenerationHelper.DelimitIdentifier(nameof(StoreValueGenerationData.Data2)) + " + 1");
        }

        foreach (var name in new[]
                 {
                     nameof(StoreValueGenerationContext.WithAllDatabaseGenerated),
                     nameof(StoreValueGenerationContext.WithAllDatabaseGenerated2)
                 })
        {
            modelBuilder
                .SharedTypeEntity<StoreValueGenerationData>(name)
                .Property(w => w.Data1)
                .HasComputedColumnSql("80");

            modelBuilder
                .SharedTypeEntity<StoreValueGenerationData>(name)
                .Property(w => w.Data2)
                .HasComputedColumnSql("81");
        }
    }

    protected override Task SeedAsync(StoreValueGenerationContext context)
    {
        context.WithSomeDatabaseGenerated.AddRange(new StoreValueGenerationData { Data2 = 1 }, new StoreValueGenerationData { Data2 = 2 });
        context.WithSomeDatabaseGenerated2.AddRange(new StoreValueGenerationData { Data2 = 1 }, new StoreValueGenerationData { Data2 = 2 });

        context.WithNoDatabaseGenerated.AddRange(
            new StoreValueGenerationData
            {
                Id = 1,
                Data1 = 10,
                Data2 = 20
            }, new StoreValueGenerationData
            {
                Id = 2,
                Data1 = 11,
                Data2 = 21
            });
        context.WithNoDatabaseGenerated2.AddRange(
            new StoreValueGenerationData
            {
                Id = 1,
                Data1 = 10,
                Data2 = 20
            }, new StoreValueGenerationData
            {
                Id = 2,
                Data1 = 11,
                Data2 = 21
            });

        context.WithAllDatabaseGenerated.AddRange(new StoreValueGenerationData(), new StoreValueGenerationData());
        context.WithAllDatabaseGenerated2.AddRange(new StoreValueGenerationData(), new StoreValueGenerationData());

        return context.SaveChangesAsync();
    }

    public async Task SeedAsync()
    {
        using var context = CreateContext();
        await SeedAsync(context);
    }

    public virtual void CleanData()
    {
        using var context = CreateContext();

        context.WithSomeDatabaseGenerated.RemoveRange(context.WithSomeDatabaseGenerated);
        context.WithSomeDatabaseGenerated2.RemoveRange(context.WithSomeDatabaseGenerated2);

        context.WithNoDatabaseGenerated.RemoveRange(context.WithNoDatabaseGenerated);
        context.WithNoDatabaseGenerated2.RemoveRange(context.WithNoDatabaseGenerated2);

        context.WithAllDatabaseGenerated.RemoveRange(context.WithAllDatabaseGenerated);
        context.WithAllDatabaseGenerated2.RemoveRange(context.WithAllDatabaseGenerated2);

        context.SaveChanges();
    }

    protected override bool ShouldLogCategory(string logCategory)
        => logCategory == DbLoggerCategory.Database.Transaction.Name
            || logCategory == DbLoggerCategory.Database.Command.Name;

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;
}
