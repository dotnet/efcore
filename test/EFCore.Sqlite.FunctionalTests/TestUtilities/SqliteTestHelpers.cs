// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Sqlite.Diagnostics.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class SqliteTestHelpers : RelationalTestHelpers
{
    protected SqliteTestHelpers()
    {
    }

    public static SqliteTestHelpers Instance { get; } = new();

    public override IServiceCollection AddProviderServices(IServiceCollection services)
        => services.AddEntityFrameworkSqlite();

    public override DbContextOptionsBuilder UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite(new SqliteConnection("Data Source=:memory:"));

    public override LoggingDefinitions LoggingDefinitions { get; } = new SqliteLoggingDefinitions();
}
