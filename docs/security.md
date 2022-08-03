This document records the security guarantees that we make in EF Core (and later releases) and the security related guidance that developers should follow when using Entity Framework.

The document serves two purposes:

* **Designing, reviewing, and verifying Entity Framework** to ensure it meets the agreed upon security principles.
* **Helping developers write secure applications** by raising awareness of the security concerns that EF handles and guidance that should be followed when using EF.

***
**NOTE:** THIS DOCUMENT CAPTURES THE SECURITY DESIGN FOR EF CORE. SINCE EF CORE IS STILL PRE-RELEASE, NOT ALL GUARANTEES LISTED HERE MAY BE IMPLEMENTED AND/OR VERIFIED YET.
***

## User Input

### Security Note: Validate user input

While EF does provide protection from SQL injection attacks (covered later in this document), it does not do any general validation of input.

**Guidance:** If values being passed to APIs, used in LINQ queries, assigned to entity properties, etc., come from an untrusted source then appropriate validation, per your application requirements, should be performed.

**Note:** This includes any user input used to dynamically construct queries. Even when using LINQ, if you are accepting user input to build expressions you need to verify that only intended expressions can be constructed.

## Data Stores

### Security Note: EF relies on data store security

Entity Framework does not implement any security functionality and relies on the underlying data store to provide the appropriate controls based on the connection information supplied.

**Guidance:** The connection information provided to EF should only have permissions to perform the operations intended by the application.

**Guidance:** If some users of the application should not be able to perform operations that are allowed by the connection supplied to EF, this restriction must be implemented in the application logic.

### Security Note: Protect data store connection information

EF requires connection information to be able to connect to the data store. This information should be stored in a secure location.

**Guidance:** Sensitive parts of the connection information that will be passed to EF (such as username and password) should be stored in a secure location.

## Microsoft.AspNet.Diagnostics.Entity

### Security Note: Microsoft.AspNet.Diagnostics.Entity only intended for development

The components in Microsoft.AspNet.Diagnostics.Entity are only intended for use during development of an application and should not be enabled when the application is deployed. This includes the Database Error Page and the Migration Endpoint. These components do not implement any security controls.

The Database Error Page displays exception details which may contain sensitive information, such as database object names. The Database Error Page also displays details of the context and migration classes in your source code (or the libraries that it references) and allows migrations to be applied to the database. The Migrations Endpoint allows the migration process to be initiated for any context within your source code (or the libraries that it references).

**Guidance:** Only register the middleware from Microsoft.AspNet.Diagnostics.Entity when developing an application. This is best achieved by making use of the functionality in Startup.cs that allows additional configuration for development mode.

**Guidance:** If you chose to enable these components in a deployed application, you are responsible for ensuring the appropriate security controls are implemented to prevent unauthorized access.

## Logging, Exceptions, Etc

Definitions:

* **Logging:** Refers to data sent to the Write method of an instance `ILogger` that is registered with EF. This may include `ILogger` instances that were registered with an external `IServiceProvider` that was passed to EF (such as `ILogger` instances registered in `Startup.cs` of ASP.NET applications).
* **Message(s):** Refers to a string that would be logged and/or displayed or recorded. This includes the `Exception.Message` property, the `Exception.ToString` method, and the string returned from the `message` Action sent to `ILogger.Write`. 
* **Application data:** Refers to data that comes from the data store or may be supplied by an end user of the application. Depending on the application, this data may contain sensitive information (usernames, credit card numbers, etc.). Examples include, results from queries, values stored in entity instances, and constant values used in LINQ expressions.

### Security Guarantee: Messages do not contain data store credentials

The credentials from a connection string (or other means of specifying a connection) are never included in a message. Other information (such as the database name) may be logged.

### Security Guarantee: Messages do not contain application data

By default, messages will not contain any application data. This helps protect data since these messages may be displayed to an end user or be logged. Displaying exceptions to an end user is not recommended, but is not an uncommon occurrence (both intentionally and unintentionally). This also helps protect data that may be logged to a location that does not have the same permissions as the database.

**WARNING:** This guarantee is no longer applicable to logging messages if the [IncludeSensitiveDataInLog](https://github.com/aspnet/EntityFramework/issues/1374) flag is enabled.

### Security Guarantee: Logging state does not contain application data

State passed to `ILogger.Write` (such as the state and exception parameter) will not contain references to application data or objects from which application data can be obtained. Since some logging frameworks may store the state information to a log, it could inadvertently end up in an insecure location.

**WARNING:** This guarantee is no longer applicable to logging state if the [IncludeSensitiveDataInLog](https://github.com/aspnet/EntityFramework/issues/1374) flag is enabled.

### Security Note: [IncludeSensitiveDataInLog](https://github.com/aspnet/EntityFramework/issues/1374) flag results in application data in logging state and messages

If the [IncludeSensitiveDataInLog](https://github.com/aspnet/EntityFramework/issues/1374) flag is enabled, logging messages may contain application data and the state passed to `ILogger` may contain references to application data or objects from which application data can be obtained.

**Guidance:** Only enable the [IncludeSensitiveDataInLog](https://github.com/aspnet/EntityFramework/issues/1374) flag if application data is non-sensitive, or you have appropriately secured the location that log data is sent to.

**Example**

When the [IncludeSensitiveDataInLog](https://github.com/aspnet/EntityFramework/issues/1374) flag is enabled and EF logs that a query is about to be sent to the database, EF includes the query model as part of the state passed to `ILogger.Write`. This state can include references to parameters that will be included in the query, which could come from a user supplied value that was used in the Where clause of a LINQ query.

### Security Note: Exception properties may contain application data

While exception messages will not contain application data, the exception object may provide references to entities, or other objects, that allow access to application data.

**Guidance:** When processing exceptions you should ensure that sensitive data is appropriately secured.

**Example**

When a database concurrency violation occurs, EF will throw a `DbUpdateConcurrencyException`. This exception type has a `StateEntries` property that provides access to the change tracking information for the entities involved in the concurrency violation. 

### Security Note: Messages may contain model/schema information

Messages may contain information about the shape of your model and/or data store schema. This includes exceptions explicitly thrown by EF as well as exceptions from lower level components that EF uses (such as SqlClient).

**Guidance:** Always use a `try`/`catch` around EF operations and implement appropriate logic to handle the failure without displaying the exception message to the end user of the application.

**Examples**

An entity type is included in the model but does not have a key property configured and does not have a property that will be detected as a key by convention. The exception message includes the name of the entity type in question.

> ModelItemNotFoundException: The entity type 'MusicStore.Models.Album' requires a key to be defined.

The model is configured to map an entity to a table that does not exist in the database. SQL Server throws an exception indicating that the table does not exist. This exception is not handled by EF and returns to the code that was performing an operation using EF. 

> SqlException: Invalid object name 'Customer'.



## Relational Providers

### Security Guarantee: LINQ queries use parameterization and escaping
Any values supplied in a LINQ query will be appropriately parameterized or escaped to protect from SQL injection attacks. This is important because values may come from an end user of the application.

**Example**

For example, the following method looks up customers with a given last name in the database.

```cs
public IEnumerable<Customer> FindCustomers(string lastName)
{
    using (var context = new CustomerContext())
    {
        var customers = context.Customers
            .Where(c => c.LastName == lastName)
            .ToList();
    }
}
```

The last name value is passed as a parameter because it may come from an end user of the application and be subject to malicious input.

```sql
SELECT [c].[CustomerId], [c].[Name]
FROM [Customer] AS [c]
WHERE [c].[LastName] = @p0
```

### Security Guarantee: Update pipeline uses parameterization and escaping

Any values that come from instance data (i.e. values stored in entity properties ) will be parameterized or escaped. This protects against SQL injection, especially important when values come from an end user of the application.

**Example**
For example, the following method creates a new customer in the database based on a supplied first and last name.

```cs
public Customer CreateCustomer(string firstName, string lastName)
{
    using (var context = new CustomerContext())
    {
        var customer = new Customer 
        {
            FirstName = firstName,
            LastName = lastName
        };

        context.Customers.Add(customer);
        context.SaveChanges();

        return customer;
    }
}
```

The names values are passed as a parameter because they may come from an end user of the application and be subject to malicious input.

```sql
INSERT INTO [Customer] ([FirstName], [LastName])
OUTPUT INSERTED.[CustomerId]
VALUES (@p0, @p1)
```

### Security Note: Use parameterization for raw SQL queries

When using APIs that accept a raw SQL string the API allows values to be easily passed as parameters. 

**Note:** There are no APIs in EF that currently accept raw SQL strings. However, this guidance applies to any code that drops down to the underlying ADO.NET components that the relational EF providers build on. 

**Guidance:** Always use parameterization for any values used in a raw SQL query/command.

**Guidance:** If you are using string concatenation to dynamically build any part of the query string then you are responsible for validating any input to protect against SQL injection attacks.

**Example**

For example, the following code makes use of parameters for some end-user supplied strings when executing a raw SQL command against a database. The command is executed by dropping down to the ADO.NET `DbCommand` for the underlying data store.

```cs
public void MoveClients(string oldOwner, string newOwner)
{
    using (var context = new OrdersContext())
    {
        var connection = context.Database.AsRelational().Connection.DbConnection;
        var cmd = connection.CreateCommand();
        cmd.CommandText = "UPDATE [dbo].[Customer] SET [Owner] = @p0 WHERE [Owner] = @p1";
        cmd.Parameters.Add(new SqlParameter("p0", newOwner));
        cmd.Parameters.Add(new SqlParameter("p1", oldOwner));
        connection.Open();
        cmd.ExecuteNonQuery();
        connection.Close();
    }
}
```
	
### Security Note: Migration APIs not designed for untrusted input

The migrations APIs are designed to accept trusted values that are compiled into your application as part of migration code files. Most of the values are appropriately escaped, but if you are accepting untrusted input you should ensure appropriate validation is performed. There are some APIs (such as the `Sql(string)` method and the `defaultValueSql` parameter) that are pass-thru in nature and do not perform and validation or escaping. 

**Guidance:** The migrations APIs are not designed to accept input from an untrusted source. If input from an untrusted source (e.g. the end user of an application) is passed to the migrations APIs then it should be validated to protect against SQL injection attacks.

## Assumptions

The following assumptions are made and should be reviewed when updating this document:
* EF assemblies do not do anything special with security and just execute in the security context of the code that calls into them. EF cannot be used to do anything the calling code could not already do.
* EF uses existing .NET APIs for all I/O operations and does not implement any protocols, file parsing, etc.
* Status output from command line interfaces (such as migrations) use standard `ILogger` functionality and conform to the contents of this document.
