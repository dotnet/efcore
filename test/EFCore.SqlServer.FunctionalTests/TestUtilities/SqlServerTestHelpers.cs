// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.SqlServer.Diagnostics.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class SqlServerTestHelpers : RelationalTestHelpers
{
    protected SqlServerTestHelpers()
    {
    }

    public static SqlServerTestHelpers Instance { get; } = new();

    public override IServiceCollection AddProviderServices(IServiceCollection services)
        => services.AddEntityFrameworkSqlServer();

    public override DbContextOptionsBuilder UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(new SqlConnection("Database=DummyDatabase"));

    public override LoggingDefinitions LoggingDefinitions { get; } = new SqlServerLoggingDefinitions();
}
