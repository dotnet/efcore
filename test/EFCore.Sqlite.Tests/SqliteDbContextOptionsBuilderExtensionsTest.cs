// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore;

public class SqliteDbContextOptionsBuilderExtensionsTest
{
    [ConditionalFact]
    public void Can_add_extension_with_max_batch_size()
    {
        var optionsBuilder = new DbContextOptionsBuilder();
        optionsBuilder.UseSqlite("Database=Crunchie", b => b.MaxBatchSize(123));

        var extension = optionsBuilder.Options.Extensions.OfType<SqliteOptionsExtension>().Single();

        Assert.Equal(123, extension.MaxBatchSize);
    }

    [ConditionalFact]
    public void Can_add_extension_with_command_timeout()
    {
        var optionsBuilder = new DbContextOptionsBuilder();
        optionsBuilder.UseSqlite("Database=Crunchie", b => b.CommandTimeout(30));

        var extension = optionsBuilder.Options.Extensions.OfType<SqliteOptionsExtension>().Single();

        Assert.Equal(30, extension.CommandTimeout);
    }

    [ConditionalFact]
    public void Can_add_extension_with_connection_string()
    {
        var optionsBuilder = new DbContextOptionsBuilder();
        optionsBuilder.UseSqlite("Database=Crunchie");

        var extension = optionsBuilder.Options.Extensions.OfType<SqliteOptionsExtension>().Single();

        Assert.Equal("Database=Crunchie", extension.ConnectionString);
        Assert.Null(extension.Connection);
    }

    [ConditionalFact]
    public void Can_add_extension_with_connection_string_using_generic_options()
    {
        var optionsBuilder = new DbContextOptionsBuilder<DbContext>();
        optionsBuilder.UseSqlite("Database=Whisper");

        var extension = optionsBuilder.Options.Extensions.OfType<SqliteOptionsExtension>().Single();

        Assert.Equal("Database=Whisper", extension.ConnectionString);
        Assert.Null(extension.Connection);
    }

    [ConditionalFact]
    public void Can_add_extension_with_connection()
    {
        var optionsBuilder = new DbContextOptionsBuilder();
        var connection = new SqliteConnection();

        optionsBuilder.UseSqlite(connection);

        var extension = optionsBuilder.Options.Extensions.OfType<SqliteOptionsExtension>().Single();

        Assert.Same(connection, extension.Connection);
        Assert.Null(extension.ConnectionString);
    }

    [ConditionalFact]
    public void Can_add_extension_with_no_connection()
    {
        var optionsBuilder = new DbContextOptionsBuilder<DbContext>();

        optionsBuilder.UseSqlite();

        var extension = optionsBuilder.Options.Extensions.OfType<SqliteOptionsExtension>().Single();

        Assert.Null(extension.Connection);
        Assert.Null(extension.ConnectionString);
    }

    [ConditionalFact]
    public void Can_add_extension_with_connection_using_generic_options()
    {
        var optionsBuilder = new DbContextOptionsBuilder<DbContext>();
        var connection = new SqliteConnection();

        optionsBuilder.UseSqlite(connection);

        var extension = optionsBuilder.Options.Extensions.OfType<SqliteOptionsExtension>().Single();

        Assert.Same(connection, extension.Connection);
        Assert.Null(extension.ConnectionString);
    }

    [ConditionalFact]
    public void Service_collection_extension_method_can_configure_sqlite_options()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSqlite<ApplicationDbContext>(
            "Database=Crunchie",
            sqliteOptions =>
            {
                sqliteOptions.MaxBatchSize(123);
                sqliteOptions.CommandTimeout(30);
            },
            dbContextOption =>
            {
                dbContextOption.EnableDetailedErrors();
            });

        var services = serviceCollection.BuildServiceProvider(validateScopes: true);

        using (var serviceScope = services
                   .GetRequiredService<IServiceScopeFactory>()
                   .CreateScope())
        {
            var coreOptions = serviceScope.ServiceProvider
                .GetRequiredService<DbContextOptions<ApplicationDbContext>>().GetExtension<CoreOptionsExtension>();

            Assert.True(coreOptions.DetailedErrorsEnabled);

            var sqliteOptions = serviceScope.ServiceProvider
                .GetRequiredService<DbContextOptions<ApplicationDbContext>>().GetExtension<SqliteOptionsExtension>();

            Assert.Equal(123, sqliteOptions.MaxBatchSize);
            Assert.Equal(30, sqliteOptions.CommandTimeout);
            Assert.Equal("Database=Crunchie", sqliteOptions.ConnectionString);
        }
    }

    private class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
