// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;

public class FakeRelationalConnection(IDbContextOptions options = null) : RelationalConnection(
        new RelationalConnectionDependencies(
                options ?? CreateOptions(),
                new DiagnosticsLogger<DbLoggerCategory.Database.Transaction>(
                    new LoggerFactory(),
                    new LoggingOptions(),
                    new DiagnosticListener("FakeDiagnosticListener"),
                    new TestRelationalLoggingDefinitions(),
                    new NullDbContextLogger()),
                new RelationalConnectionDiagnosticsLogger(
                    new LoggerFactory(),
                    new LoggingOptions(),
                    new DiagnosticListener("FakeDiagnosticListener"),
                    new TestRelationalLoggingDefinitions(),
                    new NullDbContextLogger(),
                    CreateOptions()),
                new NamedConnectionStringResolver(options ?? CreateOptions()),
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
                        new ExceptionDetector()))))
{
    private DbConnection _connection;

    private readonly List<FakeDbConnection> _dbConnections = [];

    private class FakeDbContext : DbContext;

    private static IDbContextOptions CreateOptions()
    {
        var optionsBuilder = new DbContextOptionsBuilder();

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder)
            .AddOrUpdateExtension(new FakeRelationalOptionsExtension().WithConnectionString("Database=Dummy"));

        return optionsBuilder.Options;
    }

    public void UseConnection(DbConnection connection)
        => _connection = connection;

    public override DbConnection DbConnection
        => _connection ?? base.DbConnection;

    public IReadOnlyList<FakeDbConnection> DbConnections
        => _dbConnections;

    protected override DbConnection CreateDbConnection()
    {
        var connection = new FakeDbConnection(ConnectionString);

        _dbConnections.Add(connection);

        return connection;
    }
}
