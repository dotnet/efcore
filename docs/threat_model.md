# Entity Framework Core Threat-Model

Entity Framework Core is a lightweight, extensible, open source and cross-platform version of the popular Entity Framework data access technology.

EF Core can serve as an object-relational mapper (O/RM), enabling .NET developers to work with a database using .NET objects, and eliminating the need for most of the data-access code they usually need to write.

## Data flows

A query, usually with data in parameters, is sent to the database, then a response, usually with data, is received from the database.

## Trust boundaries

* Communication with the database
  * EF is a layer on top of the lower-level database driver or SDK (e.g. SqlClient for SQL Server) and relies on it securing the connection to the database.
* Logging, diagnostics
  * EF doesn't offer any built-in sinks for logging or diagnostics. This is only considered a trust boundary as part of the defense in depth strategy.
* Loading migration assembly for migration operations at runtime
  * The name of the migration assembly to load must be specified in the DbContext options:

    ```csharp
    services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
       x => x.MigrationsAssembly("WebApplication1.Migrations")));
    ```

  * An `Assembly` instance can be used instead of the string.
  * When unspecified, the `ApplicationDbContext` assembly will be used.
* Assembly loading by tooling during design and build-time
  * The EF tools build the specified project and load the assembly specified by the `TargetFileName` MSBuild property.
  * The EF tools are distributed in separate NuGet packages - `dotnet-ef`, `Microsoft.EntityFrameworkCore.Tools`, `Microsoft.EntityFrameworkCore.Tasks`. These are meant only to provide design-time functionality and shouldn't be deployed with the app.
* Multi-tenant applications
  * This threat model assumes that all tenants using a given app instance have the same trust level. Developers are responsible for isolating untrusted tenants.

### Untrusted/unknown databases

A trust boundary exists between EF and the database it connects to; that is, EF supports untrusted result sets coming from the database. This means that there are no known exploits **via EF** using malicious database contents.

EF contains a "scaffolding" feature, which examines the metadata of an existing database (tables, columns) and generates .NET code which can be used to access that database. As a specific case of guarantee detailed just above, EF guarantees that such scaffolded code - which is generated based on e.g. table and column names in the scaffolded database - is safe to run, and cannot be used as a vector for e.g. a remote code execution attack via a maliciously crafted table name.

#### Exercise caution with untrusted/unknown databases

However, connecting and interacting with an unknown/untrusted databases inherently and unavoidably brings substantial risks with it, which go beyond attacks specifically against EF itself. For example, an unknown database can contain huge amounts of data as a form of attack: depending on how the application queries the database and what it does with the results, a query against such a maliciously huge dataset could produce a denial of service (this would be a vulnerability in the application, not in EF). SQL JOINs in relational database can exacerbate this problem by causing a relatively small dataset to produce a huge query resultset (via the so-called "cartesian explosion" phenomenon). Special care should be taken when employing JOINs against untrusted databases.

To summarize, while EF narrowly guarantees that its own code shall not be vulnerable to security attacks via data returned from the database, application code using EF may contain such vulnerabilities, which are outside of EF's scope of responsibility. As a result, and regardless of the EF guarantee provided above, we highly recommend proceeding with extreme caution when connecting to databases whose origin and contents are unknown

## Storage of secrets

EF doesn't store any secrets, it only passes through the connection string to the underlying database provider.

However, if the supplied connection string is of the format `name=MyConnection` EF will use the `IConfiguration` service from the service provider where `DbContext` was registered to resolve `"MyConnection"` or `"ConnectionStrings:MyConnection"`. See [connection string documentation](https://learn.microsoft.com/ef/core/miscellaneous/connection-strings#aspnet-core) for more details.

## Serialization

* JSON (de)serializer of some payloads (Utf8JsonWriter/Utf8JsonReader)
* Composing and receiving data from the database server. EF relies on the database driver for the low-level (de)serialization involved in the database communication protocol.

### Data format

EF doesn't load, construct or execute code dynamically based on the data received from the database. All types are specified in the model in advance. The model is considered trusted.

For JSON EF expects the property names to be the exactly the same as configured in the model, similarly for column names in `DbDataReader`. Extra data will be either ignored or result in an exception. For duplicated JSON properties the last value will take precedence.

EF doesn't assume any particular order for properties when deserializing. During serialization discriminator (`$type`) comes first, then keys, then the rest in a deterministic order.

Only configuration in the EF model will be used to determine the expected data shape and how the entity object will be created, [some attributes on entity type can influence this](https://learn.microsoft.com/ef/core/modeling/#use-data-annotations-to-configure-a-model), but attributes that affect JSON (de)serialization outside EF like `[JsonPropertyName("")]` are ignored.

There is no API that can be used by the developer to provide a JSON string or a `DbDataReader` to be deserialized, this is handled internally by the provider for a specific database.

EF providers and plugins can extend or replace some of the (de)serialization logic and could introduce vulnurabilities.

### Exceptions

By default, all data is censored in exception messages. Schema information like property and column names is not considered sensitive.

### Recursiveness

JSON supports recursive primitive collections. And for relational data EF supports self-joins, but in both cases the developer has to explicitly set the recursion depth in their query either through the query shape or the configuration of the data type.

### Complexity

For JSON EF only does a constant amount of work for each value returned by the `Utf8Reader`, same for each value in `DbDataReader`.

Some queries can produce a [cartesian explosion](https://learn.microsoft.com/en-us/ef/core/querying/single-split-queries#cartesian-explosion) where the number of rows returned is the product of the number of rows the select from 2 or more tables. This can be mitigated by using `AsSplitQuery`. EF Core also produces a warning for this case as it's an issue in the user's code that can be easy to miss.

EF creates objects at linear relation to the data received. The amount of work EF performs is O(n) where the n is the size of the database response.

For particularly large data sets streaming or paging can be used to limit the peak amount of memory used.

## Threats

### Query pipeline/Bulk CUD

#### SQL Injection

Most EF queries are specified using LINQ where user input is sent as parameters which are not vulnarable to SQL injection. Methods like `ExecuteSql` and `FromSql` accept a `FormattableString` which can be parameterized as well. However, the methods `FromSqlRaw` and `ExecuteSqlRaw` accept a `string` and if the developer directly concatenates user input to the supplied SQL command an attacker can provide data containing commands that can be used for Spoofing, Tampering, Repudiation, Information disclosure, Denial of service or Elevation of privilege.

**Mitigation:** These methods are documented as potentially dangerous and the user is responsible for input sanitization.

#### JSON deserializers DoS

An attacker can inject malicious data in the database that causes Denial of service for the client machine.

**Mitigation:** EF relies on the underlying database and driver to avoid injection and the developers are advised to not directly embed untrusted JSON strings in the database. But as defense in depth, EF deserializer implementations using `Utf8JsonReader` are O(n).

#### Query cache DoS

In applications that build queries dynamically an attacker can provide malicious data that causes Denial of service for the client machine by bloating the query cache.

**Mitigation:** Applications [building queries dynamically](https://learn.microsoft.com/ef/core/performance/advanced-performance-topics?%2Cexpression-api-with-constant#dynamically-constructed-queries) should restrict the ability of user input to affect the query shape to a reasonable degree that's appropriate for the application. As defense in depth EF computes the cache key in O(n) and query cache size is limited to 10240. Developers can call `UseMemoryCache` to provide an instance with a different size limit. Currently, the following sizes are used by the caching mechanism:

* Model - 250
* Compiled query - 10
* Relational query command - 10

### Logging, interceptors, diagnostics

#### Information disclosure

If the mechanism used for logging, interceptors or diagnostics crosses a trust boundary an attacker can exploit a vulnerability to cause Information disclosure of the data in queries sent to the database.

**Mitigation:** By default EF won't include sensitive information in the call to logging, interceptors or diagnostics. Here, sensitive information is the data that would be sent as parameters. Names of user types, database objects and constants used in the queries are not considered sensitive. EF provides a stricter sensitivity level that will also censor constants.

### Model building

Model building is based on static code in the loaded assembly and is consider to be trusted.

### Type Mapping

A type mapping is an internal object that provides information on how a CLR type maps to a type natively supported by the target database.

#### Caching considerations

Type mappings can be calculated repeatedly for parameters containing user input of different lengths, so an attacker can use this to artificially bloat the type mapping cache up to a set size and potentially cause Denial of service for the client machine.

For example, if a `string` is provided by the user EF uses the length to determine whether it needs to be mapped to SQL data type of a specific length - `nvarchar(24)` or unbounded - `nvarchar(max)`. By providing strings of different lengths the user will cause a new type mapping to be calculated each time.

**Mitigation:** EF will use a different type mapping caching mechanism for query parameters and group type mappings in the cache based on a limited number of values for the length.

### Migrations

#### Migrations lock DoS

Migration application can happen at runtime and take a database-wide lock that could cause delays and potentially cause Denial of service for the client machine. For example, the attacker could spike the load for the application that would cause it to scale out (if configured) and this could trigger multiple concurrent attempts to apply the migrations.

**Mitigation:** [The guidance](https://learn.microsoft.com/ef/core/managing-schemas/migrations/applying#apply-migrations-at-runtime) is to only perform migration application once per deployment.

#### Migrations elevation of privilige

Migration application requires more permission than required for normal app operation. If an attacker is able gain control of the app or the credentials they would be able to inflict more damage than otherwise.

**Mitigation:** [The guidance](https://learn.microsoft.com/ef/core/managing-schemas/migrations/applying#apply-migrations-at-runtime) is to use separate credentials for applying migrations that allow DDL operations and restrict them for the normal app execution.

### CLI Tools

The tools build and load the developer's project containing the context, model-building code and migrations before running any operation. EF tools rely on MSBuild to build the supply the expected assembly to load.

### Providers, Plug-ins

EF extensibility relies only on static code to explicitly use providers and plug-ins that could modify the runtime behavior.
