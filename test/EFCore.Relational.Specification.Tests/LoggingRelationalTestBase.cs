// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

using Microsoft.EntityFrameworkCore.Diagnostics.Internal;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class LoggingRelationalTestBase<TBuilder, TExtension> : LoggingTestBase
    where TBuilder : RelationalDbContextOptionsBuilder<TBuilder, TExtension>
    where TExtension : RelationalOptionsExtension, new()
{
    [ConditionalFact]
    public void Logs_context_initialization_max_batch_size()
        => Assert.Equal(
            ExpectedMessage("MaxBatchSize=10 " + DefaultOptions),
            ActualMessage(s => CreateOptionsBuilder(s, b => b.MaxBatchSize(10))));

    [ConditionalFact]
    public void Logs_context_initialization_command_timeout()
        => Assert.Equal(
            ExpectedMessage("CommandTimeout=10 " + DefaultOptions),
            ActualMessage(s => CreateOptionsBuilder(s, b => b.CommandTimeout(10))));

    [ConditionalFact]
    public void Logs_context_initialization_relational_nulls()
        => Assert.Equal(
            ExpectedMessage("UseRelationalNulls " + DefaultOptions),
            ActualMessage(s => CreateOptionsBuilder(s, b => b.UseRelationalNulls())));

    [ConditionalFact]
    public void Logs_context_initialization_migrations_assembly()
        => Assert.Equal(
            ExpectedMessage("MigrationsAssembly=A.B.C " + DefaultOptions),
            ActualMessage(s => CreateOptionsBuilder(s, b => b.MigrationsAssembly("A.B.C"))));

    [ConditionalFact]
    public void Logs_context_initialization_migrations_history_table()
        => Assert.Equal(
            ExpectedMessage("MigrationsHistoryTable=MyHistory " + DefaultOptions),
            ActualMessage(s => CreateOptionsBuilder(s, b => b.MigrationsHistoryTable("MyHistory"))));

    [ConditionalFact]
    public void Logs_context_initialization_migrations_history_table_schema()
        => Assert.Equal(
            ExpectedMessage("MigrationsHistoryTable=mySchema.MyHistory " + DefaultOptions),
            ActualMessage(s => CreateOptionsBuilder(s, b => b.MigrationsHistoryTable("MyHistory", "mySchema"))));

    [ConditionalFact]
    public virtual void IndexPropertiesBothMappedAndNotMappedToTable_throws_by_default()
    {
        using var context = new IndexPropertiesBothMappedAndNotMappedToTableContext(CreateOptionsBuilder(new ServiceCollection()));

        Assert.Equal(
            CoreStrings.WarningAsErrorTemplate(
                RelationalEventId.IndexPropertiesBothMappedAndNotMappedToTable.ToString(),
                RelationalResources.LogUnnamedIndexPropertiesBothMappedAndNotMappedToTable(CreateTestLogger())
                    .GenerateMessage(nameof(Cat), "{'Name', 'Identity'}", nameof(Cat.Identity)),
                "RelationalEventId.IndexPropertiesBothMappedAndNotMappedToTable"),
            Assert.Throws<InvalidOperationException>(
                () => context.Model).Message);
    }

    protected class IndexPropertiesBothMappedAndNotMappedToTableContext(DbContextOptionsBuilder optionsBuilder) : DbContext(optionsBuilder.Options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().ToTable((string)null).HasIndex(nameof(Animal.Name), nameof(Cat.Identity));
        }
    }

    [ConditionalFact]
    public virtual void UnnamedIndexPropertiesMappedToNonOverlappingTables_throws_by_default()
    {
        using var context = new UnnamedIndexPropertiesMappedToNonOverlappingTablesContext(CreateOptionsBuilder(new ServiceCollection()));

        Assert.Equal(
            CoreStrings.WarningAsErrorTemplate(
                RelationalEventId.IndexPropertiesMappedToNonOverlappingTables.ToString(),
                RelationalResources.LogUnnamedIndexPropertiesMappedToNonOverlappingTables(CreateTestLogger())
                    .GenerateMessage(
                        nameof(Cat), "{'Name', 'Identity'}", nameof(Animal.Name), "{'Animals'}", nameof(Cat.Identity), "{'Cats'}"),
                "RelationalEventId.IndexPropertiesMappedToNonOverlappingTables"),
            Assert.Throws<InvalidOperationException>(
                () => context.Model).Message);
    }

    protected class UnnamedIndexPropertiesMappedToNonOverlappingTablesContext(DbContextOptionsBuilder optionsBuilder) : DbContext(optionsBuilder.Options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Animal>().ToTable("Animals");
            modelBuilder.Entity<Cat>().ToTable("Cats");
            modelBuilder.Entity<Cat>().HasIndex(nameof(Animal.Name), nameof(Cat.Identity));
        }
    }

    [ConditionalFact]
    public virtual void ForeignKeyPropertiesMappedToUnrelatedTables_throws_by_default()
    {
        using var context = new ForeignKeyPropertiesMappedToUnrelatedTablesContext(CreateOptionsBuilder(new ServiceCollection()));

        var definition = RelationalResources.LogForeignKeyPropertiesMappedToUnrelatedTables(CreateTestLogger());
        Assert.Equal(
            CoreStrings.WarningAsErrorTemplate(
                RelationalEventId.ForeignKeyPropertiesMappedToUnrelatedTables.ToString(),
                definition
                    .GenerateMessage(
                        l => l.Log(
                            definition.Level,
                            definition.EventId,
                            definition.MessageFormat,
                            "{'FavoritePersonId'}", nameof(Cat), nameof(Person), "{'FavoritePersonId'}", nameof(Cat), "{'Id'}",
                            nameof(Person))),
                "RelationalEventId.ForeignKeyPropertiesMappedToUnrelatedTables"),
            Assert.Throws<InvalidOperationException>(
                () => context.Model).Message);
    }

    protected class ForeignKeyPropertiesMappedToUnrelatedTablesContext(DbContextOptionsBuilder optionsBuilder) : DbContext(optionsBuilder.Options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Animal>()
                .Ignore(a => a.FavoritePerson)
                .Property<int>("FavoritePersonId");
            modelBuilder.Entity<Cat>().ToTable("Cat")
                .HasOne<Person>().WithMany()
                .HasForeignKey("FavoritePersonId");
        }
    }

    protected abstract DbContextOptionsBuilder CreateOptionsBuilder(
        IServiceCollection services,
        Action<RelationalDbContextOptionsBuilder<TBuilder, TExtension>> relationalAction);

    protected override DbContextOptionsBuilder CreateOptionsBuilder(IServiceCollection services)
        => CreateOptionsBuilder(services, null);
}
