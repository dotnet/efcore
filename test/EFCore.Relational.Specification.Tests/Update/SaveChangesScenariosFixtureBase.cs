// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.SaveChangesScenariosModel;

namespace Microsoft.EntityFrameworkCore.Update;

#nullable enable

public abstract class SaveChangesScenariosFixtureBase : SharedStoreFixtureBase<SaveChangesScenariosContext>
{
    protected override string StoreName { get; } = "SaveChangesScenariosTest";

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        foreach (var name in new[]
                 {
                     nameof(SaveChangesScenariosContext.WithNoDatabaseGenerated),
                     nameof(SaveChangesScenariosContext.WithNoDatabaseGenerated2)
                 })
        {
            modelBuilder
                .SharedTypeEntity<SaveChangesData>(name)
                .Property(w => w.Id)
                .ValueGeneratedNever();
        }

        foreach (var name in new[]
                 {
                     nameof(SaveChangesScenariosContext.WithSomeDatabaseGenerated),
                     nameof(SaveChangesScenariosContext.WithSomeDatabaseGenerated2),
                     nameof(SaveChangesScenariosContext.WithAllDatabaseGenerated),
                     nameof(SaveChangesScenariosContext.WithAllDatabaseGenerated2)
                 })
        {
            modelBuilder
                .SharedTypeEntity<SaveChangesData>(name)
                .Property(w => w.Data1)
                .HasComputedColumnSql("80");
        }

        foreach (var name in new[]
                 {
                     nameof(SaveChangesScenariosContext.WithAllDatabaseGenerated),
                     nameof(SaveChangesScenariosContext.WithAllDatabaseGenerated2)
                 })
        {
            modelBuilder
                .SharedTypeEntity<SaveChangesData>(name)
                .Property(w => w.Data2)
                .HasComputedColumnSql("81");
        }
    }

    protected override void Seed(SaveChangesScenariosContext context)
    {
        context.WithSomeDatabaseGenerated.AddRange(new() { Data2 = 1 }, new() { Data2 = 2 });
        context.WithSomeDatabaseGenerated2.AddRange(new() { Data2 = 1 }, new() { Data2 = 2 });

        context.WithNoDatabaseGenerated.AddRange(new() { Id = 1, Data1 = 10, Data2 = 20 }, new() { Id = 2, Data1 = 11, Data2 = 21 });
        context.WithNoDatabaseGenerated2.AddRange(new() { Id = 1, Data1 = 10, Data2 = 20 }, new() { Id = 2, Data1 = 11, Data2 = 21 });

        context.WithAllDatabaseGenerated.AddRange(new(), new());
        context.WithAllDatabaseGenerated2.AddRange(new(), new());

        context.SaveChanges();
    }

    protected override void Clean(DbContext context)
    {
        var saveChangesScenariosContext = CreateContext();

        saveChangesScenariosContext.WithSomeDatabaseGenerated.RemoveRange(saveChangesScenariosContext.WithSomeDatabaseGenerated);
        saveChangesScenariosContext.WithSomeDatabaseGenerated2.RemoveRange(saveChangesScenariosContext.WithSomeDatabaseGenerated2);

        saveChangesScenariosContext.WithNoDatabaseGenerated.RemoveRange(saveChangesScenariosContext.WithNoDatabaseGenerated);
        saveChangesScenariosContext.WithNoDatabaseGenerated2.RemoveRange(saveChangesScenariosContext.WithNoDatabaseGenerated2);

        saveChangesScenariosContext.WithAllDatabaseGenerated.RemoveRange(saveChangesScenariosContext.WithAllDatabaseGenerated);
        saveChangesScenariosContext.WithAllDatabaseGenerated2.RemoveRange(saveChangesScenariosContext.WithAllDatabaseGenerated2);

        saveChangesScenariosContext.SaveChanges();
    }

    protected override bool ShouldLogCategory(string logCategory)
        => logCategory == DbLoggerCategory.Database.Transaction.Name
            || logCategory == DbLoggerCategory.Database.Command.Name;

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;
}
