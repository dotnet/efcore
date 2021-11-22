// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore;

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

    protected abstract DbContextOptionsBuilder CreateOptionsBuilder(
        IServiceCollection services,
        Action<RelationalDbContextOptionsBuilder<TBuilder, TExtension>> relationalAction);

    protected override DbContextOptionsBuilder CreateOptionsBuilder(IServiceCollection services)
        => CreateOptionsBuilder(services, null);
}
