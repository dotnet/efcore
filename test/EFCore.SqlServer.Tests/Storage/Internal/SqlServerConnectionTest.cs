// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
        Assert.IsType<SqlConnection>(connection.DbConnection);
    }

    #region Master connection

    [Fact]
    public void Can_create_master_connection()
    {
        using var connection = new SqlServerConnection(CreateDependencies());
        using var master = connection.CreateMasterConnection();
        Assert.Equal(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master", master.ConnectionString);
        Assert.Equal(60, master.CommandTimeout);
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

    #region Connection string not modified

    [Fact]
    public void Connection_string_is_not_modified()
    {
        var options = new DbContextOptionsBuilder()
            .UseSqlServer("""Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SqlServerConnectionTest""")
            .Options;

        using var connection = new SqlServerConnection(CreateDependencies(options));
        Assert.Equal(
            """Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SqlServerConnectionTest""",
            connection.ConnectionString);
    }

    [Fact]
    public void DbConnection_sets_application_name_to_ef_version_when_unspecified()
    {
        using var connection = new SqlServerConnection(CreateDependencies());

        var connectionStringBuilder = new SqlConnectionStringBuilder(connection.DbConnection.ConnectionString);

        Assert.Equal($"EFCore/{ProductInfo.GetVersion()}", connectionStringBuilder.ApplicationName);
    }

    [Fact]
    public void DbConnection_does_not_modify_specified_application_name()
    {
        var options = new DbContextOptionsBuilder()
            .UseSqlServer("""Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SqlServerConnectionTest;Application Name=CustomApp""")
            .Options;

        using var connection = new SqlServerConnection(CreateDependencies(options));

        var connectionStringBuilder = new SqlConnectionStringBuilder(connection.DbConnection.ConnectionString);

        Assert.Equal("CustomApp", connectionStringBuilder.ApplicationName);
    }

    #endregion Connection string not modified

    public static RelationalConnectionDependencies CreateDependencies(DbContextOptions options = null)
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
        RelationalOptionsExtension optionsExtension = null)
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
