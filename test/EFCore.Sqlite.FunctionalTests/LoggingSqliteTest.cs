// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Transactions;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

public class LoggingSqliteTest : LoggingRelationalTestBase<SqliteDbContextOptionsBuilder, SqliteOptionsExtension>
{
    [ConditionalFact]
    public void AmbientTransactionWarning_throws_by_default()
    {
        using var context = new AmbientTransactionWarningContext(CreateOptionsBuilder(new ServiceCollection()));

        context.Add(new Animal());

        using var transactionScope = new TransactionScope();

        Assert.Equal(
            CoreStrings.WarningAsErrorTemplate(
                RelationalEventId.AmbientTransactionWarning.ToString(),
                RelationalResources.LogAmbientTransaction(CreateTestLogger()).GenerateMessage(),
                "RelationalEventId.AmbientTransactionWarning"),
            Assert.Throws<InvalidOperationException>(
                () => context.SaveChanges()).Message);
    }

    protected class AmbientTransactionWarningContext : DbContext
    {
        public AmbientTransactionWarningContext(DbContextOptionsBuilder optionsBuilder)
            : base(optionsBuilder.Options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Animal>();
    }

    protected override DbContextOptionsBuilder CreateOptionsBuilder(
        IServiceCollection services,
        Action<RelationalDbContextOptionsBuilder<SqliteDbContextOptionsBuilder, SqliteOptionsExtension>> relationalAction)
        => new DbContextOptionsBuilder()
            .UseInternalServiceProvider(services.AddEntityFrameworkSqlite().BuildServiceProvider(validateScopes: true))
            .UseSqlite("Data Source=LoggingSqliteTest.db", relationalAction);

    protected override TestLogger CreateTestLogger()
        => new TestLogger<SqliteLoggingDefinitions>();

    protected override string ProviderName
        => "Microsoft.EntityFrameworkCore.Sqlite";

    protected override string ProviderVersion
        => typeof(SqliteOptionsExtension).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
}
