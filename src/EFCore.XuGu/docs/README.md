## About

_Microsoft.EntityFrameworkCore.XuGu_ is the Entity Framework Core (EF Core) provider for [MySQL](https://www.mysql.com), [MariaDB](https://mariadb.org), [Amazon Aurora](https://aws.amazon.com/rds/aurora), [Azure Database for MySQL](https://azure.microsoft.com/en-us/services/mysql) and other MySQL-compatible databases.

It is build on top of [XuguClient](https://github.com/mysql-net/XuguClient).

## How to Use

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Replace with your connection string.
        var connectionString = "server=localhost;user=root;password=1234;database=ef";

        // Replace with your server version and type.
        // Use 'MariaDbServerVersion' for MariaDB.
        // Alternatively, use 'ServerVersion.AutoDetect(connectionString)'.
        // For common usages, see pull request #1233.
        var serverVersion = new XGServerVersion(new Version(8, 0, 29));

        // Replace 'YourDbContext' with the name of your own DbContext derived class.
        services.AddDbContext<YourDbContext>(
            dbContextOptions => dbContextOptions
                .UseXG(connectionString, serverVersion)
                // The following three options help with debugging, but should
                // be changed or removed for production.
                .LogTo(Console.WriteLine, LogLevel.Information)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
        );
    }
}
```

## Key Features

* JSON support (both `Newtonsoft.Json` and `System.Text.Json`)
* Spatial support
* High performance

## Related Packages

* JSON support
  * [Microsoft.EntityFrameworkCore.XuGu.Json.Microsoft](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.XuGu.Json.Microsoft)
  * [Microsoft.EntityFrameworkCore.XuGu.Json.Newtonsoft](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.XuGu.Json.Newtonsoft)
* Spatial support
  * [Microsoft.EntityFrameworkCore.XuGu.NetTopologySuite](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.XuGu.NetTopologySuite)
* Other Packages
  * [XuguClient](https://www.nuget.org/packages/XuguClient)
  * [Microsoft.EntityFrameworkCore](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore)

## License

_Microsoft.EntityFrameworkCore.XuGu_ is released as open source under the [MIT license](https://github.com/PomeloFoundation/Microsoft.EntityFrameworkCore.XuGu/blob/main/LICENSE).

## Feedback

Bug reports and contributions are welcome at our [GitHub repository](https://github.com/PomeloFoundation/Microsoft.EntityFrameworkCore.XuGu).