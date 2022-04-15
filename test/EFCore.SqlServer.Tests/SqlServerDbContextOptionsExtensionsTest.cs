// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

public class SqlServerDbContextOptionsExtensionsTest
{
    [ConditionalFact]
    public void Can_add_extension_with_max_batch_size()
    {
        var optionsBuilder = new DbContextOptionsBuilder();
        optionsBuilder.UseSqlServer("Database=Crunchie", b => b.MaxBatchSize(123));

        var extension = optionsBuilder.Options.Extensions.OfType<SqlServerOptionsExtension>().Single();

        Assert.Equal(123, extension.MaxBatchSize);
    }

    [ConditionalFact]
    public void Can_add_extension_with_command_timeout()
    {
        var optionsBuilder = new DbContextOptionsBuilder();
        optionsBuilder.UseSqlServer("Database=Crunchie", b => b.CommandTimeout(30));

        var extension = optionsBuilder.Options.Extensions.OfType<SqlServerOptionsExtension>().Single();

        Assert.Equal(30, extension.CommandTimeout);
    }

    [ConditionalFact]
    public void Can_add_extension_with_connection_string()
    {
        var optionsBuilder = new DbContextOptionsBuilder();
        optionsBuilder.UseSqlServer("Database=Crunchie");

        var extension = optionsBuilder.Options.Extensions.OfType<SqlServerOptionsExtension>().Single();

        Assert.Equal("Database=Crunchie", extension.ConnectionString);
        Assert.Null(extension.Connection);
    }

    [ConditionalFact]
    public void Can_add_extension_with_connection_string_using_generic_options()
    {
        var optionsBuilder = new DbContextOptionsBuilder<DbContext>();
        optionsBuilder.UseSqlServer("Database=Whisper");

        var extension = optionsBuilder.Options.Extensions.OfType<SqlServerOptionsExtension>().Single();

        Assert.Equal("Database=Whisper", extension.ConnectionString);
        Assert.Null(extension.Connection);
    }

    [ConditionalFact]
    public void Can_add_extension_with_connection()
    {
        var optionsBuilder = new DbContextOptionsBuilder();
        var connection = new SqlConnection();

        optionsBuilder.UseSqlServer(connection);

        var extension = optionsBuilder.Options.Extensions.OfType<SqlServerOptionsExtension>().Single();

        Assert.Same(connection, extension.Connection);
        Assert.Null(extension.ConnectionString);
    }

    [ConditionalFact]
    public void Can_add_extension_with_connection_using_generic_options()
    {
        var optionsBuilder = new DbContextOptionsBuilder<DbContext>();
        var connection = new SqlConnection();

        optionsBuilder.UseSqlServer(connection);

        var extension = optionsBuilder.Options.Extensions.OfType<SqlServerOptionsExtension>().Single();

        Assert.Same(connection, extension.Connection);
        Assert.Null(extension.ConnectionString);
    }

    [ConditionalFact]
    public void Service_collection_extension_method_can_configure_sqlserver_options()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSqlServer<ApplicationDbContext>(
            "Database=Crunchie",
            sqlServerOption =>
            {
                sqlServerOption.MaxBatchSize(123);
                sqlServerOption.CommandTimeout(30);
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

            var sqlServerOptions = serviceScope.ServiceProvider
                .GetRequiredService<DbContextOptions<ApplicationDbContext>>().GetExtension<SqlServerOptionsExtension>();

            Assert.Equal(123, sqlServerOptions.MaxBatchSize);
            Assert.Equal(30, sqlServerOptions.CommandTimeout);
            Assert.Equal("Database=Crunchie", sqlServerOptions.ConnectionString);
        }
    }

    private class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }
    }
}
