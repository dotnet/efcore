// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class SqlServerValueGenerationStrategyThrowTest(
    SqlServerValueGenerationStrategyFixture<SqlServerValueGenerationStrategyThrowTest.ThrowContext> fixture) :
    SqlServerValueGenerationConflictTest<SqlServerValueGenerationStrategyThrowTest.ThrowContext>(fixture)
{
    [ConditionalFact]
    public virtual void SqlServerValueGeneration_conflicting_with_existing_ValueGeneration_strategy_throws()
    {
        var modelBuilder = CreateModelBuilder();
        modelBuilder.Entity<Fred>()
            .Property(e => e.Id)
            .HasDefaultValueSql("2")
            .UseHiLo();

        Assert.Equal(
            CoreStrings.WarningAsErrorTemplate(
                SqlServerEventId.ConflictingValueGenerationStrategiesWarning,
                SqlServerResources.LogConflictingValueGenerationStrategies(
                        new TestLogger<SqlServerLoggingDefinitions>())
                    .GenerateMessage(SqlServerValueGenerationStrategy.SequenceHiLo.ToString(), "DefaultValueSql", "Id", nameof(Fred)),
                "SqlServerEventId.ConflictingValueGenerationStrategiesWarning"),
            Assert.Throws<InvalidOperationException>(() => Validate(modelBuilder)).Message);
    }

    [ConditionalFact]
    public virtual void SqlServerValueGeneration_conflicting_with_existing_default_value_strategy_throws()
    {
        var modelBuilder = CreateModelBuilder();
        modelBuilder.Entity<Fred>()
            .Property(e => e.Id)
            .HasDefaultValueSql("2")
            .UseSequence();

        Assert.Equal(
            CoreStrings.WarningAsErrorTemplate(
                SqlServerEventId.ConflictingValueGenerationStrategiesWarning,
                SqlServerResources.LogConflictingValueGenerationStrategies(
                        new TestLogger<SqlServerLoggingDefinitions>())
                    .GenerateMessage(SqlServerValueGenerationStrategy.Sequence.ToString(), "DefaultValueSql", "Id", nameof(Fred)),
                "SqlServerEventId.ConflictingValueGenerationStrategiesWarning"),
            Assert.Throws<InvalidOperationException>(() => Validate(modelBuilder)).Message);
    }

    public class ThrowContext(DbContextOptions options) : DbContext(options)
    {
        public virtual DbSet<Fred> Freds { get; set; }

        // use the normal behavior of ConflictingValueGenerationStrategiesWarning
        // defined in UseSqlServer()
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer();
    }
}

public class SqlServerValueGenerationStrategyNoThrowTest(
    SqlServerValueGenerationStrategyFixture<SqlServerValueGenerationStrategyNoThrowTest.NoThrowContext> fixture) :
    SqlServerValueGenerationConflictTest<SqlServerValueGenerationStrategyNoThrowTest.NoThrowContext>(fixture)
{
    [ConditionalFact]
    public virtual void SqlServerValueGeneration_conflicting_with_existing_ValueGeneration_strategy_warns()
    {
        var modelBuilder = CreateModelBuilder();
        modelBuilder.Entity<Fred>()
            .Property(e => e.Id)
            .HasDefaultValueSql("2")
            .UseHiLo();

        // Assert - this does not throw
        Validate(modelBuilder);

        var logEntry = Fixture.ListLoggerFactory.Log.Single(
            l => l.Level == LogLevel.Warning && l.Id == SqlServerEventId.ConflictingValueGenerationStrategiesWarning);
        Assert.Equal(
            SqlServerResources.LogConflictingValueGenerationStrategies(
                    new TestLogger<SqlServerLoggingDefinitions>())
                .GenerateMessage(SqlServerValueGenerationStrategy.SequenceHiLo.ToString(), "DefaultValueSql", "Id", nameof(Fred)),
            logEntry.Message);
    }

    public class NoThrowContext(DbContextOptions options) : DbContext(options)
    {
        public virtual DbSet<Fred> Freds { get; set; }

        // override the normal behavior of ConflictingValueGenerationStrategiesWarning
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseSqlServer()
                .ConfigureWarnings(
                    b => b.Log(SqlServerEventId.ConflictingValueGenerationStrategiesWarning));
    }
}

public class SqlServerValueGenerationConflictTest<TContext>(SqlServerValueGenerationStrategyFixture<TContext> fixture)
    : IClassFixture<SqlServerValueGenerationStrategyFixture<TContext>>
    where TContext : DbContext
{
    protected SqlServerValueGenerationStrategyFixture<TContext> Fixture { get; } = fixture;

    public TContext CreateContext()
        => (TContext)Fixture.CreateContext();

    protected virtual ModelBuilder CreateModelBuilder()
    {
        var context = CreateContext();
        return new ModelBuilder(
            context.GetService<IConventionSetBuilder>().CreateConventionSet(),
            context.GetService<ModelDependencies>());
    }

    protected virtual IModel Validate(ModelBuilder modelBuilder)
        => modelBuilder.FinalizeModel();

    public class Fred
    {
        public int Id { get; set; }
    }
}

public class SqlServerValueGenerationStrategyFixture<TContext> : SharedStoreFixtureBase<DbContext>
    where TContext : DbContext
{
    protected override string StoreName
        => "SqlServerValueGenerationStrategy";

    protected override Type ContextType { get; } = typeof(TContext);

    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    protected override bool ShouldLogCategory(string logCategory)
        => logCategory == DbLoggerCategory.Model.Validation.Name;

    protected override bool UsePooling
        => false;
}
