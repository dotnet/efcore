// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

public class LoggingSqlServerTest : LoggingRelationalTestBase<SqlServerDbContextOptionsBuilder, SqlServerOptionsExtension>
{
    protected override DbContextOptionsBuilder CreateOptionsBuilder(
        IServiceCollection services,
        Action<RelationalDbContextOptionsBuilder<SqlServerDbContextOptionsBuilder, SqlServerOptionsExtension>> relationalAction)
        => new DbContextOptionsBuilder()
            .UseInternalServiceProvider(services.AddEntityFrameworkSqlServer().BuildServiceProvider(validateScopes: true))
            .UseSqlServer("Data Source=LoggingSqlServerTest.db", relationalAction);

    protected override string ProviderName
        => "Microsoft.EntityFrameworkCore.SqlServer";

    protected override string ProviderVersion
        => typeof(SqlServerOptionsExtension).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
}
