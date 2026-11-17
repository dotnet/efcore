// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;

namespace Microsoft.EntityFrameworkCore.Storage.Internal;

public class SqlServerConnectionTest
{
    [Fact]
    public void Creates_SQL_Server_connection_string()
    {
        using var connection = new SqlServerConnection(CreateDependencies());
        var dbConnection = Assert.IsType<SqlConnection>(connection.DbConnection);
        Assert.Equal(RegisteredApplication.EntityFrameworkCore, dbConnection.RegisteredApplication);
    }

    [Theory]
    [InlineData(RegisteredApplication.Unknown)]
    [InlineData(RegisteredApplication.EntityFrameworkCore)]
    public void Does_not_change_registered_application_on_user_connection(RegisteredApplication registeredApplication)
    {
        using var dbConnection = new SqlConnection { RegisteredApplication = registeredApplication };
        var options = new DbContextOptionsBuilder().UseSqlServer(dbConnection).Options;

        using var connection = new SqlServerConnection(CreateDependencies(options));

        Assert.Same(dbConnection, connection.DbConnection);
        Assert.Equal(registeredApplication, dbConnection.RegisteredApplication);
    }

    #region Master connection

    [Fact]
    public void Can_create_master_connection()
    {
        using var connection = new SqlServerConnection(CreateDependencies());
        using var master = connection.CreateMasterConnection();
        Assert.Equal(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master", master.ConnectionString);
        Assert.Equal(60, master.CommandTimeout);
        Assert.Equal(RegisteredApplication.EntityFrameworkCore, Assert.IsType<SqlConnection>(master.DbConnection).RegisteredApplication);
    }

    [Fact]
    public void Master_connection_string_contains_filename()
    {
        var options = new DbContextOptionsBuilder()
            .UseSqlServer(
                @"Server=(localdb)\MSSQLLocalDB;Database=SqlServerConnectionTest;AttachDBFilename=C:\Narf.mdf",
                b => b.CommandTimeout(55))
            .Options;

        using var connection = new SqlServerConnection(CreateDependencies(options));
        using var master = connection.CreateMasterConnection();
        Assert.Equal(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master", master.ConnectionString);
    }

    [Fact]
    public void Master_connection_string_none_default_command_timeout()
    {
        var options = new DbContextOptionsBuilder()
            .UseSqlServer(
                @"Server=(localdb)\MSSQLLocalDB;Database=SqlServerConnectionTest",
                b => b.CommandTimeout(55))
            .Options;

        using var connection = new SqlServerConnection(CreateDependencies(options));
        using var master = connection.CreateMasterConnection();
        Assert.Equal(55, master.CommandTimeout);
    }

    #endregion Master connection

    #region Application Name

    [Theory]
    [InlineData("")]
    [InlineData(";Application Name=")]
    [InlineData(";Application Name=Core Microsoft SqlClient Data Provider")]
    [InlineData(";Application Name=foo")]
    public void Does_not_rewrite_connection_string(string applicationName)
    {
        var connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=SqlServerConnectionTest" + applicationName;
        var options = new DbContextOptionsBuilder()
            .UseSqlServer(connectionString)
            .Options;

        using var connection = new SqlServerConnection(CreateDependencies(options));
        Assert.Equal(connectionString, connection.ConnectionString);

        var dbConnection = Assert.IsType<SqlConnection>(connection.DbConnection);
        Assert.Equal(connectionString, dbConnection.ConnectionString);
        Assert.Equal(RegisteredApplication.EntityFrameworkCore, dbConnection.RegisteredApplication);

        connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=SomeOtherDatabase" + applicationName;
        connection.ConnectionString = connectionString;

        Assert.Equal(connectionString, connection.ConnectionString);
        Assert.Equal(connectionString, dbConnection.ConnectionString);
        Assert.Equal(RegisteredApplication.EntityFrameworkCore, dbConnection.RegisteredApplication);
    }

    [Fact]
    public void ApplicationName_is_not_injected_with_connection()
    {
        var dbConnection1 = new SqlConnection("""Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SqlServerConnectionTest""");
        var options = new DbContextOptionsBuilder().UseSqlServer(dbConnection1).Options;

        using var connection = new SqlServerConnection(CreateDependencies(options));
        Assert.Equal("""Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SqlServerConnectionTest""", connection.ConnectionString);

        var dbConnection2 = new SqlConnection("""Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SomeOtherDatabase""");
        connection.DbConnection = dbConnection2;
        Assert.Equal("""Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SomeOtherDatabase""", connection.ConnectionString);
    }

    #endregion Application Name

    public static RelationalConnectionDependencies CreateDependencies(DbContextOptions? options = null)
    {
        options ??= new DbContextOptionsBuilder()
            .UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=SqlServerConnectionTest")
            .Options;

        return new RelationalConnectionDependencies(
            options,
            new DiagnosticsLogger<DbLoggerCategory.Database.Transaction>(
                new LoggerFactory(),
                new LoggingOptions(),
                new DiagnosticListener("FakeDiagnosticListener"),
                new SqlServerLoggingDefinitions(),
                new NullDbContextLogger()),
            new RelationalConnectionDiagnosticsLogger(
                new LoggerFactory(),
                new LoggingOptions(),
                new DiagnosticListener("FakeDiagnosticListener"),
                new SqlServerLoggingDefinitions(),
                new NullDbContextLogger(),
                CreateOptions()),
            new NamedConnectionStringResolver(options),
            new RelationalTransactionFactory(
                new RelationalTransactionFactoryDependencies(
                    new RelationalSqlGenerationHelper(
                        new RelationalSqlGenerationHelperDependencies()))),
            new CurrentDbContext(new FakeDbContext()),
            new RelationalCommandBuilderFactory(
                new RelationalCommandBuilderDependencies(
                    new TestRelationalTypeMappingSource(
                        TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                        TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>()),
                    new SqlServerExceptionDetector(),
                    new LoggingOptions())),
            new SqlServerExceptionDetector());
    }

    private const string ConnectionString = "Fake Connection String";

    private static IDbContextOptions CreateOptions(
        RelationalOptionsExtension? optionsExtension = null)
    {
        var optionsBuilder = new DbContextOptionsBuilder();

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder)
            .AddOrUpdateExtension(
                optionsExtension
                ?? new FakeRelationalOptionsExtension().WithConnectionString(ConnectionString));

        return optionsBuilder.Options;
    }

    private class FakeDbContext : DbContext;
}
