// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class LoggingSqlServerTest : LoggingRelationalTestBase<SqlServerDbContextOptionsBuilder, SqlServerOptionsExtension>
{
    [ConditionalFact]
    public virtual void StoredProcedureConcurrencyTokenNotMapped_throws_by_default()
    {
        using var context = new StoredProcedureConcurrencyTokenNotMappedContext(CreateOptionsBuilder(new ServiceCollection()));

        var definition = RelationalResources.LogStoredProcedureConcurrencyTokenNotMapped(CreateTestLogger());
        Assert.Equal(
            CoreStrings.WarningAsErrorTemplate(
                RelationalEventId.StoredProcedureConcurrencyTokenNotMapped.ToString(),
                definition.GenerateMessage(nameof(Animal), "Animal_Update", nameof(Animal.Name)),
                "RelationalEventId.StoredProcedureConcurrencyTokenNotMapped"),
            Assert.Throws<InvalidOperationException>(
                () => context.Model).Message);
    }

    protected class StoredProcedureConcurrencyTokenNotMappedContext(DbContextOptionsBuilder optionsBuilder) : DbContext(optionsBuilder.Options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Animal>(
                b =>
                {
                    b.Ignore(a => a.FavoritePerson);
                    b.Property(e => e.Name).IsRowVersion();
                    b.UpdateUsingStoredProcedure(
                        b =>
                        {
                            b.HasOriginalValueParameter(e => e.Id);
                            b.HasParameter(e => e.Name, p => p.IsOutput());
                            b.HasRowsAffectedReturnValue();
                        });
                });
    }

    protected override DbContextOptionsBuilder CreateOptionsBuilder(
        IServiceCollection services,
        Action<RelationalDbContextOptionsBuilder<SqlServerDbContextOptionsBuilder, SqlServerOptionsExtension>> relationalAction)
        => new DbContextOptionsBuilder()
            .UseInternalServiceProvider(services.AddEntityFrameworkSqlServer().BuildServiceProvider(validateScopes: true))
            .UseSqlServer("Data Source=LoggingSqlServerTest.db", relationalAction);

    protected override TestLogger CreateTestLogger()
        => new TestLogger<SqlServerLoggingDefinitions>();

    protected override string ProviderName
        => "Microsoft.EntityFrameworkCore.SqlServer";

    protected override string ProviderVersion
        => typeof(SqlServerOptionsExtension).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
}
