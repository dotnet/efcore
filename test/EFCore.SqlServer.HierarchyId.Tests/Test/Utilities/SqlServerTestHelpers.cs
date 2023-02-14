using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.SqlServer.Diagnostics.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

#pragma warning disable EF1001

public class SqlServerTestHelpers : TestHelpers
{
    private SqlServerTestHelpers()
    {
    }

    public static SqlServerTestHelpers Instance { get; } = new();

    public override IServiceCollection AddProviderServices(IServiceCollection services)
        => services.AddEntityFrameworkSqlServer();

    public override DbContextOptionsBuilder UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(new SqlConnection("Database=DummyDatabase"));

    public override LoggingDefinitions LoggingDefinitions { get; } = new SqlServerLoggingDefinitions();
}
