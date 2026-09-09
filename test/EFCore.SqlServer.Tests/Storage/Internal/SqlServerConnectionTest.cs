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
    [ConditionalFact]
    public void Creates_SQL_Server_connection_string()
    {
        using var connection = new SqlServerConnection(CreateDependencies());
        Assert.IsType<SqlConnection>(connection.DbConnection);
    }

    [ConditionalFact]
    public void Can_create_master_connection()
    {
        using var connection = new SqlServerConnection(CreateDependencies());
        using var master = connection.CreateMasterConnection();
        Assert.Equal(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master", master.ConnectionString);
        Assert.Equal(60, master.CommandTimeout);
    }

    [ConditionalFact]
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

    [ConditionalFact]
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
                    new SqlServerExceptionDetector())));
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
