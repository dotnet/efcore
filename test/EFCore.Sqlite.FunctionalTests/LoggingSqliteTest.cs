// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

public class LoggingSqliteTest : LoggingRelationalTestBase<SqliteDbContextOptionsBuilder, SqliteOptionsExtension>
{
    protected override DbContextOptionsBuilder CreateOptionsBuilder(
        IServiceCollection services,
        Action<RelationalDbContextOptionsBuilder<SqliteDbContextOptionsBuilder, SqliteOptionsExtension>> relationalAction)
        => new DbContextOptionsBuilder()
            .UseInternalServiceProvider(services.AddEntityFrameworkSqlite().BuildServiceProvider(validateScopes: true))
            .UseSqlite("Data Source=LoggingSqliteTest.db", relationalAction);

    protected override string ProviderName
        => "Microsoft.EntityFrameworkCore.Sqlite";

    protected override string ProviderVersion
        => typeof(SqliteOptionsExtension).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
}
