// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Transactions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

public class LoggingSqliteTest : LoggingRelationalTestBase<SqliteDbContextOptionsBuilder, SqliteOptionsExtension>
{
    [Fact]
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
            Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Commands_are_logged_after_a_command_fails(bool async)
    {
        var log = new List<string>();

        using var context = new DbContext(
            new DbContextOptionsBuilder()
                .UseSqlite("Data Source=:memory:")
                .LogTo(log.Add, LogLevel.Information)
                .ConfigureLoggingCacheTime(TimeSpan.FromHours(1))
                .Options);

        if (async)
        {
            await Assert.ThrowsAsync<SqliteException>(() => context.Database.ExecuteSqlRawAsync("SELECT * FROM Missing"));
            await context.Database.ExecuteSqlRawAsync("SELECT 2");
        }
        else
        {
            Assert.Throws<SqliteException>(() => context.Database.ExecuteSqlRaw("SELECT * FROM Missing"));
            context.Database.ExecuteSqlRaw("SELECT 2");
        }

        Assert.Contains(log, m => m.Contains("SELECT 2"));
    }

    protected class AmbientTransactionWarningContext(DbContextOptionsBuilder optionsBuilder) : DbContext(optionsBuilder.Options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Animal>();
    }

    protected override DbContextOptionsBuilder CreateOptionsBuilder(
        IServiceCollection services,
        Action<RelationalDbContextOptionsBuilder<SqliteDbContextOptionsBuilder, SqliteOptionsExtension>>? relationalAction)
        => new DbContextOptionsBuilder()
            .UseInternalServiceProvider(services.AddEntityFrameworkSqlite().BuildServiceProvider(validateScopes: true))
            .UseSqlite("Data Source=LoggingSqliteTest.db", relationalAction);

    protected override TestLogger CreateTestLogger()
        => new TestLogger<SqliteLoggingDefinitions>();

    protected override string ProviderName
        => "Microsoft.EntityFrameworkCore.Sqlite";

    protected override string ProviderVersion
        => typeof(SqliteOptionsExtension).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
}
