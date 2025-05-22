# Entity Framework Core - GitHub Copilot Instructions

This document provides guidance for GitHub Copilot when generating code for the Entity Framework Core project. Follow these guidelines to ensure that generated code aligns with the project's coding standards and architectural principles.

If you are not sure, do not guess, just tell that you don't know or ask clarifying questions. Don't copy code that follows the same pattern in a different context. Don't rely just on names, evaluate the code based on the implementation and usage. Verify that the generated code is correct and compilable.

## Code Style

### General Guidelines

- Follow the [.NET coding guidelines](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md) unless explicitly overridden below
- Use the rules defined in the .editorconfig file in the root of the repository for any ambiguous cases
- Write code that is clean, maintainable, and easy to understand
- Favor readability over brevity, but keep methods focused and concise
- Only add comments to explain why something is done, not how it is done. The code should be self-explanatory
- Add license header to all files:
```
    // Licensed to the .NET Foundation under one or more agreements.
    // The .NET Foundation licenses this file to you under the MIT license.
```
- Don't add the UTF-8 BOM to files unless they have non-ASCII characters
- All types should be public. Types in .Internal namespaces should have this comment on all members:
```
/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
```
- Avoid breaking public APIs. If you need to break a public API, add a new API instead and mark the old one as obsolete. Use `ObsoleteAttribute` with the message pointing to the new API

### Formatting

- Use spaces for indentation (4 spaces)
- Use braces for all blocks including single-line blocks
- Place braces on new lines
- Limit line length to 140 characters
- Trim trailing whitespace
- Insert a final newline at the end of files

### C# Specific Guidelines

- File scoped namespace declarations
- Use `var` for local variables
- Use expression-bodied members where appropriate
- Use collection initializers when possible
- Use `is` pattern matching instead of `as` and null checks
- Prefer `switch` expressions over `switch` statements when appropriate
- Prefer field-backed property declarations using field contextual keyword instead of an explicit field.
- Prefer range and index from end operators for indexer access

### Naming Conventions

- Use PascalCase for:
  - Classes, structs, enums, properties, methods, events, namespaces, delegates
  - Public fields
  - Static private fields
  - Constants
- Use camelCase for:
  - Parameters
  - Local variables
- Use `_camelCase` for instance private fields
- Prefix interfaces with `I`
- Prefix type parameters with `T`
- Use meaningful and descriptive names

### Nullability

- Use nullable reference types
- Use proper null-checking patterns
- Use the null-conditional operator (`?.`) and null-coalescing operator (`??`) when appropriate

## Architecture and Design Patterns

### Dependency Injection

- Design services to be registered and resolved through the DI container for functionality that could be replaced or extended by users, providers or plug-ins
- Create sealed records with names ending in `Dependencies` containing the service dependencies

### Testing

- Follow the existing test patterns in the corresponding test projects
- Create both unit tests and functional tests where appropriate
- For database provider implementations, create tests that derive from the appropriate specification test base classes
- Add or modify `SQL` and `C#` baselines for tests when necessary

## Documentation

- Include XML documentation for all public APIs
- Add proper `<remarks>` tags with links to relevant documentation where helpful
- For keywords like `null`, `true` or `false` use `<see langword="*" />` tags
- Use `<see href="https://aka.ms/efcore-docs-*">` redirects for doc links instead of hardcoded `https://learn.microsoft.com/` links
- Include code examples in documentation where appropriate
- Overriding members should inherit the XML documentation from the base type via `/// <inheritdoc />`

## Error Handling

- Use appropriate exception types. 
- Include helpful error messages stored in the .resx file corresponding to the project
- Avoid catching exceptions without rethrowing them

## Asynchronous Programming

- Provide both synchronous and asynchronous versions of methods where appropriate
- Use the `Async` suffix for asynchronous methods
- Return `Task` or `ValueTask` from asynchronous methods
- Use `CancellationToken` parameters to support cancellation
- Avoid async void methods except for event handlers
- Call `ConfigureAwait(false)` on awaited calls to avoid deadlocks

## Performance Considerations

- Be mindful of performance implications, especially for database operations
- Avoid unnecessary allocations
- Consider using more efficient code that is expected to be on the hot path, even if it is less readable

## Implementation Guidelines

- Write code that is secure by default. Avoid exposing potentially private or sensitive data
- Make code NativeAOT compatible when possible. This means avoiding dynamic code generation, reflection, and other features that are not compatible with NativeAOT. If not possible, mark the code with an appropriate annotation or throw an exception

### Entity Framework Core Specific guidelines

- Follow the provider pattern when extending EF Core's capabilities for specific databases
- Follow the existing model building design patterns
  - Add corresponding methods to the `*Builder`, `*Configuration`, `*Extensions`, `IConvention*Builder`, `IReadOnly*`, `IMutable*`and `IConvention*` types as needed
  - Make corresponding changes to the `Runtime*` types as needed.
  - For Relational-specific model changes also modify the `RelationalModel` and `*AnnotationProvider` types.
  - Make corresponding changes to the `CSharpRuntimeModelCodeGenerator`, `*CSharpRuntimeAnnotationCodeGenerator` and `CSharpDbContextGenerator` types as needed
- Use the logging infrastructure for diagnostics
- Prefer using `Check.DebugAssert` to `Debug.Assert` or comments for assertions in the codebase
- Use `Check.NotNull` and `Check.NotEmpty` for preconditions in public APIs

## Overview of Entity Framework Core

Entity Framework Core (EF Core) is an object-database mapper. This overview covers the major parts of EF Core's architecture, explaining how each part functions and how they interrelate:

### DbContext Configuration and Pooling

Definition: The `DbContext` is the main class for interacting with EF Core. Configuring a DbContext involves specifying options like the database provider, connection string, and certain behaviors (like lazy loading, query tracking behavior, etc.). DbContext pooling is a performance feature that allows reusing `DbContext` instances to reduce the cost of context initialization in high-throughput scenarios.

DbContext configuration:

-   Typically done in `OnConfiguring` method of the context or via external configuration (like in ASP.NET Core `AddDbContext`).
-   The primary configuration is specifying the provider and connection: e.g., `optionsBuilder.UseSqlServer("...connection...")` or `UseSqlite`, etc. This registers the appropriate provider and connection info.
-   Other common configuration settings:
    -   Lazy Loading: By default EF Core doesn't use lazy-loading proxies unless you enable them. You can enable lazy loading by using the package Microsoft.EntityFrameworkCore.Proxies and calling `optionsBuilder.UseLazyLoadingProxies()`. Then navigation properties marked virtual can be loaded on demand.
    -   Query tracking behavior: You can set `optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)` as a default if you want all queries to be no-tracking by default (you can override per query with `.AsTracking()`).
    -   Sensitive data logging: For logging SQL with parameters, including sensitive data, call `EnableSensitiveDataLogging` (useful for debugging but be careful in production logs).
    -   Command Timeout, EnableDetailedErrors, etc., are other options.
-   Dependency Injection: In modern .NET apps, you typically don't call `OnConfiguring`; instead, you configure the context in the `Startup` or `Program` class using services. For example, `services.AddDbContext<YourContext>(options => options.UseSqlServer(...))`. This is the recommended approach for ASP.NET Core apps, and it allows the DI container to supply the context where needed with the configuration pre-set.
-   The context configuration object `DbContextOptions` is built once and reused for each context instance (especially with DI). It contains all the immutable settings for that context type.

DbContext lifetime: A `DbContext` is meant to be a short-lived, lightweight object. You create one, use it for a unit of work, then dispose. In web apps, that often means per request. In a desktop app, maybe per form or per operation as needed. Keeping a context open for a very long time (like a global static context) is usually not recommended because the change tracker can accumulate stale entities and memory usage can grow, and it may hold onto a database connection longer than necessary.

DbContext pooling: By default, when you use `AddDbContext`, each injection (each request, etc.) results in a new context instance. Creating a context isn't extremely heavy, but it does involve setting up internal services and the model (though the model is cached, remember). In high-performance scenarios (hundreds or thousands of operations per second), this small overhead can add up.

DbContext pooling addresses that by keeping a pool of pre-created DbContext instances. When one is requested, if a previously used instance is available, it will be reset and given out, instead of creating a new one from scratch. When it's disposed, instead of actually disposing it, EF Core resets its state and returns it to the pool for reuse.

To use it, you call `services.AddDbContextPool<YourContext>(options => { ... });` instead of AddDbContext. You can specify a pool size if desired (default is quite large, 1024). In practice, the number of concurrent context instances rarely exceeds the number of concurrent requests in your app, so a pool of that size is sufficient to basically always reuse contexts.

What resetting means: EF Core will clear the change tracker, ensure any tracked entities are detached and the state is clean, and it will *not* dispose the underlying services. One thing to note is if you have any stateful aspects in your context (e.g., if you have a custom property in your context for some reason, or you rely on `OnConfiguring` to set things like a tenant ID per instance), those may persist incorrectly. Also, one must be careful not to share a pooled context across threads simultaneously. But when using DI correctly (scoped or transient per request), you typically won't hit that.

When to use pooling: If your application frequently creates and disposes contexts (like an API or web app) and you need extra performance, pooling can help reduce CPU and GC overhead. If your app uses a long-lived context or just a few contexts overall, pooling doesn't add much value. Also, if you do need unique configuration per context instance (like different connection string per tenant dynamically), pooling is trickier since the context's configuration is fixed on first creation.

Other config aspects:
-   `DbContextOptionsBuilder` allows chaining configuration like `.UseModel(compiledModel)` if you want to use a compiled pre-generated model.
-   Logging and interceptors can be configured via optionsBuilder as well (like `optionsBuilder.LogTo(Console.WriteLine)` for simple logging or adding interceptors to catch commands).
-   Enable/disable certain features: e.g., `.UseChangeTrackingProxies()`, `.UseSnakeCaseNamingConvention()` (to apply a naming convention plugin), etc.

Key points: Configuring the context properly sets the stage for how EF Core behaves. Pooling is a transparent performance optimization but requires awareness. For new contributors, it's important to understand context should be used and disposed and not kept as a static instance. Also, realize that some patterns (like storing data in the context class between calls) won't work with pooling since the same instance might be reused for a completely different scope later on. If you do use pooling, treat the context instance as if it could be reused (don't stash things in it that aren't cleared on dispose).

### Query Pipeline

Definition: The query pipeline is the mechanism that processes LINQ queries (`IQueryable` LINQ expressions) into database-specific queries (such as SQL) and executes them to retrieve data.

How it works: When you write a LINQ query against your `DbSet<T>` (for example, `context.Students.Where(s => s.Age > 18)`), EF Core doesn't execute it immediately. Instead, it builds an expression tree representing the query. The EF Core query pipeline goes through multiple stages to transform this expression tree into an executable query:

1.  Linking to IQueryable Provider: The `DbSet` implements `IQueryable<T>` using EF Core's query provider. Each LINQ operation (Where, Select, Join, etc.) builds up the expression tree. The final call (like enumerating the query or calling `ToListAsync`) triggers EF Core to start processing this tree.
2.  Translation and Optimization: EF Core walks the expression tree through a series of visitors and translators. Each visitor inspects or rewrites the tree to handle various aspects:
    -   Navigation expansion: Includes (e.g. `Include()`) and navigation property accesses are translated into joins or separate queries as needed.
    -   Query simplification: Certain patterns are normalized or optimized. For example, LINQ operators that are not directly translatable to SQL may be reformulated. EF Core might rewrite complex subqueries, flatten group joins, remove redundant null-checks, etc., to better align with what the database can handle.
    -   Predicate and projection translation: The LINQ `Where` filters and `Select` projections (including selecting entity types) are analyzed and converted into a form that can be represented in the target query language.
3.  Provider-specific translation: After a series of provider-agnostic transformations, the pipeline generates an intermediate representation of the query. For relational providers, this often manifests as a `SelectExpression` (an abstraction representing an SQL SELECT statement with tables, columns, filters, etc.). The provider's query generator then turns this into the actual SQL query text (or API call for non-relational databases). This stage is handled by a component often called the SQL generator or query compiler for that provider.
4.  Query Execution Preparation: EF Core prepares the final query for execution. It identifies parameters (captured variables in the expression become SQL parameters) to avoid SQL injection and allow efficient plan caching on the database side. It may also apply query caching on the EF Core side: the translated form of the query (and the associated function to materialize results) can be cached so that identical LINQ queries don't need to be reprocessed every time. This makes subsequent executions of the same query much faster.
5.  Execution and Data Reader: EF Core executes the database command (e.g., sends the SQL to the database via ADO.NET) and obtains a `DbDataReader` with the results. At this point, the raw data is ready, and EF Core needs to materialize it into entity objects.

Key points: The query pipeline essentially compiles LINQ to database queries. It is highly optimized and uses techniques like expression tree compilation, intelligent caching, and translation rules to produce efficient queries. It also defers execution until necessary (LINQ's deferred execution) and supports both synchronous and asynchronous query execution. The output of the query pipeline is not only the database result, but also a prepared function (the shaper, described next) to convert each record into the appropriate entity or object structure.

### Shaper/Materializer

Definition: The shaper (or materializer) is the component responsible for taking the raw data from the database (rows/columns in the `DbDataReader`) and constructing entity instances (and related data objects) out of it. Materialization is the process of turning database results into .NET objects.

After the query pipeline executes a query and has a `DbDataReader`, EF Core needs to map each record to your entity types. This involves:
-   Reading Column Values: The shaper knows the expected type and position of each column for the result. It reads values from the data reader for each column in the result set.
-   Creating Entity Instances: For each row, the shaper creates instances of your entity classes (or uses existing ones if the entity is already being tracked---see Change Tracking below). EF Core uses constructors or parameterless instantiation as needed to create the objects. Complex object graphs (for example, including related entities from a join or include) are handled by coordinating multiple readers or by joining data into a single row that is then split into multiple objects.
-   Property Assignment: The raw values are assigned to the corresponding properties of the entity. EF Core uses the metadata from the model (the IModel, IEntityType, IProperty configurations) to know which property corresponds to each column (including handling column name translation, column type conversions, etc. as needed).
-   Relationship Fix-up: If multiple related entities are being materialized (e.g., due to an `.Include` or a projection that brings in related data), the materializer will set up the navigation properties. For example, if an `Order` and its `Customer` are retrieved in one query, EF Core will ensure that the `Order.Customer` property is set to the materialized `Customer` object, and if the `Customer.Orders` collection is being tracked, it adds the order to that collection. This process is called relationship fix-up, and it can also occur after materialization by the change tracker.
-   Tracking vs. No-Tracking: If the query is executed in tracking mode (the default for LINQ queries without `.AsNoTracking()`), EF Core will check its Change Tracker to see if an entity with the same primary key is already in the context.
    -   If it is already being tracked, by default EF Core will not create a new instance but rather populate the existing instance or skip adding it (to avoid duplicates in the context).
    -   If it's not being tracked yet, EF Core will begin tracking the new entity (attach it to the context's change tracker).
    -   In No-Tracking mode, EF Core simply creates new instances every time and does not keep track of them after the query.
-   Materializer generation: EF Core often generates a specialized function (as part of the query compilation) to handle materialization. This function is essentially a compiled lambda or method that reads a result row and produces the object graph. By generating and caching this function, EF Core can materialize results very quickly on subsequent uses of the same query shape.

Key points: The shaper/materializer ensures that the data coming from the database is transformed into the correct object structure as defined by your entity classes. It handles complexities like inheritance (discriminator columns), related data, and optional values (including converting `DBNull` to null references, etc.). Materialization is also the phase where EF Core's change tracker can start tracking entities, applying any uninitialized default values, and more. Understanding this phase can help in scenarios like optimizing large read operations or reasoning about identity resolution (ensuring the same entity isn't duplicated in the result set).

### Change Tracking

Definition: Change tracking is the ability of EF Core to keep track of the state of each entity (instance of your domain classes) that it is working with. The `ChangeTracker` is a part of the `DbContext` that records which entities have been added, modified, or deleted, and what their original values were. This information is later used to generate appropriate database commands on `SaveChanges()`.

How it works:
-   When you query for data without `AsNoTracking`, EF Core will attach those entities to the context's change tracker in the `Unchanged` state (meaning "this entity's data is the same as in the database as far as we know").
-   If you then make changes to those entities (e.g., `student.Name = "New Name"`), EF Core detects that change. One way it does this is by storing original values of properties when the entity was fetched. It can compare current values to original values to see what changed. In this case, it would mark the `Name` property as modified and the entity's state as Modified.
-   If you call `context.Students.Add(newStudent)`, EF will start tracking that `newStudent` entity as `Added` (meaning it needs to be inserted into the database on save).
-   If you remove an entity via `context.Remove(entity)` or by calling `context.Students.Remove(entity)`, EF marks it as `Deleted`.

Each tracked entity has an associated `EntityEntry` (accessible via `context.Entry(entity)`) that gives information about the tracking, such as the current state (Added, Modified, etc.), the original values, current values, and so on. In addition to the above states the entity can be `Detached` -- Not tracked by any context (initial state if you just `new` up an object, or after you `Detach` it or the context is disposed).

Detecting changes: EF Core can detect changes in a couple of ways:

-   For normal POCO entities without any change tracking proxies, EF Core uses snapshot tracking by default. This means when an entity is tracked, EF stores a snapshot of its original values. When `SaveChanges` is called, or when `DetectChanges()` is triggered (EF calls this automatically at certain times, or you can call it manually), EF compares the current values of each property to the original snapshot. Any differences mean that property has been modified.
-   EF Core also provides an option to use change tracking proxies (by enabling it in your context configuration or using the `ChangeTracking.Proxies` package). In this mode, EF generates a proxy subclass of your entity that intercepts property setters and automatically marks the state as Modified when you set a property. This avoids scanning all properties for changes and can be more efficient for large graphs, at the cost of not being pure POCOs (and requiring virtual properties, etc.).
-   Collections/navigation changes (like adding an entity to a navigation collection) are also tracked by detecting changes in foreign key values or by the correlation of relationships. EF will set the FK property and track the new entity if needed, etc.

Why change tracking matters: When you call `SaveChanges()`, EF Core looks at all the tracked entities and their states:

-   Any entity in Added state results in an INSERT.
-   Modified state results in an UPDATE (with only the changed columns included in the SET clause, thanks to tracking which properties changed).
-   Deleted state results in a DELETE.

If entities are `Unchanged`, EF does nothing with them. If `Detached`, EF doesn't even consider them.

Also, the change tracker handles referential integrity in memory. For example, if you mark a parent entity as `Deleted`, by convention EF might also mark related children as `Deleted` (depending on cascade delete rules defined in the model). Or if you add a new child entity to a parent's collection, it will automatically set the child's foreign key and start tracking the child as `Added`.

No-Tracking scenario: Sometimes you want read-only queries where you don't need change tracking (for performance reasons). With `.AsNoTracking()`, EF will not track those entities at all. This can significantly improve performance for read-heavy scenarios, because it avoids the overhead of tracking and creating snapshots.

Concurrency: The change tracker also plays a role in concurrency handling. If you have concurrency tokens (like a rowversion/timestamp or a property with `[ConcurrencyCheck]`), EF will store the original value of that token. When you SaveChanges, it includes that original value in the WHERE clause of the UPDATE/DELETE. If zero rows are affected (meaning the row was changed by someone else), EF knows a concurrency conflict occurred. It can then raise a `DbUpdateConcurrencyException` which you can handle.

Key points: Change tracking is central to how EF Core manages the unit-of-work pattern. A single `DbContext` represents a unit of work; all changes made to entities in that context will be tracked and can be persisted together. Best practice is to keep the context short-lived so the change tracker set doesn't grow too large (e.g., use a context per business transaction or per web request). Also be mindful that tracked entities are not automatically freed until the context is disposed or they are detached; this can lead to higher memory usage if not managed. Finally, if performance is a concern and you don't need tracking, use no-tracking queries or explicitly detach entities when needed.

### SaveChanges and Update Pipeline

Definition: The update pipeline refers to the process that runs when `SaveChanges()` (or `SaveChangesAsync()`) is called on the `DbContext`. This pipeline takes the information in the change tracker (which entities are added/modified/deleted) and converts it into the appropriate database operations (INSERTs, UPDATEs, DELETEs).

How `SaveChanges` works:

1.  Gather changes: EF Core obtains the list of all tracked entities that are in Added, Modified, or Deleted state from the change tracker.
2.  Order the operations: EF Core will sort these operations in an order that respects referential integrity. For example, if you added both a new `Order` and a new `OrderLine` (child of Order), EF will insert the `Order` first to get its primary key (especially if it's an IDENTITY/auto-increment key), then insert `OrderLine` which uses that key. Conversely, if deleting, it will delete dependents (like `OrderLine`) before principals (`Order`) if there are cascading deletes in the database or if not, it might have to handle relationships manually. EF Core by default will include all inserts, updates, deletes in a single transaction, so that the changes either all succeed or all fail together (maintaining consistency).
3.  Generate commands: For each change, EF Core creates a database command. This is again done in a database provider-agnostic way first, then translated by the provider:
    -   For `Added` entities, it creates an INSERT command for that table, including all the scalar property values. If the database generates some values (like identity columns or defaults), EF can fetch those back (it knows which ones by the model configuration).
    -   For `Modified` entities, it creates an UPDATE command for that table. Importantly, EF Core by default only includes the columns that have actually changed (according to tracking) in the SET clause. This minimizes the data sent and avoids overwriting concurrent changes on other columns. The UPDATE's WHERE clause will include the primary key to target the correct row, and also any concurrency token if applicable (for concurrency checking).
    -   For `Deleted` entities, it creates a DELETE command with a WHERE clause for the primary key (and concurrency token if applicable).
4.  Execute commands: EF Core then executes these commands against the database. It may batch them if the provider supports batching. For example, SQL Server provider might send a batch of multiple SQL statements together to reduce round trips. The exact behavior can depend on `DbContextOptionsBuilder.EnableRetryOnFailure` (which might wrap in retry logic), but typically it just executes via ADO.NET.
5.  Handle results: After execution, EF checks the database's results. If an insert had database-generated keys (like identity or GUID defaults, etc.), it retrieves those and populates the entity's property (so your object now has the primary key set). It also accepts all changes (marking entries as Unchanged since now they are in sync with the database).
6.  Concurrency conflicts: If any command resulted in 0 rows affected when EF expected 1 (meaning the row was not found or a concurrency token didn't match), EF interprets that as a concurrency conflict (someone else modified or deleted the data since it was fetched). In such cases, it throws a `DbUpdateConcurrencyException`. It's up to the application to resolve that (by reloading data, merging changes, etc., and trying again or aborting).
7.  After SaveChanges: EF will detach deleted entities (since they no longer exist in the database), and added/modified become Unchanged (now considered baseline synced with DB). The context is ready for another cycle of changes.

Batching and transactions: EF Core, by default, will wrap `SaveChanges` in a transaction if there are multiple operations. If there's only one operation, it may not explicitly create a transaction (the database will implicitly handle it). You can also manage transactions yourself, especially if combining with other operations. EF Core also supports batching multiple statements in one round trip to the database to improve performance (controlled by an option in the DbContext or the provider would handle it by default up to a certain batch size).

Update pipeline extensibility: Advanced: EF Core allows interception of the SaveChanges process via interceptors or the lower-level `ChangeTracker.DetectChanges()` and events. You can, for instance, write logic in the context's `SaveChanges` override to do auditing, etc., or use the `ISaveChangesInterceptor` to intercept operations. But typically, the pipeline is not something you modify heavily as a user, it's internal.

Key points: The update pipeline is the counterpart to the query pipeline. Where query brings data in, the update pipeline pushes data out to the database. The separation of change detection and SQL generation means EF Core can ensure only intended changes are applied. For contributors, understanding this helps with features like bulk updates or custom operations. For example, EF Core 7 introduced `ExecuteUpdate`/`ExecuteDelete` which bypass some of the change tracker process (we cover that next). Also, be aware that calling SaveChanges frequently (e.g., after every single entity change in a loop) is less efficient than accumulating changes and saving in one go, due to transaction overhead and repeated change detection. Ideally, make all the necessary changes in your entities within a context, then call SaveChanges once at the end of a unit-of-work.

### Bulk CUD Operations (ExecuteUpdate and ExecuteDelete)

Definition: Bulk CUD (Create, Update, Delete) refers to performing create/update/delete operations on multiple rows in a set-based manner rather than one by one. In EF Core, bulk operations can be achieved with special methods like `ExecuteUpdate` and `ExecuteDelete` as well as by inserting or updating multiple entities and calling SaveChanges (which is common but not truly a single bulk operation at the SQL level).

Bulk inserts (Create): EF Core does not have a single "bulk insert" command that converts to a multi-row insert in one SQL statement or a bulk copy operation (aside from batching multiple inserts). For truly large bulk inserts (like thousands of rows), EF Core might not be the most efficient approach and one might use raw SQL (like `SqlBulkCopy` for SQL Server or the COPY command for PostgreSQL via a library). However, EF Core can batch inserts: if you add 100 entities and call `SaveChanges`, by default it might send them in smaller batches in multiple insert statements, or as a single combined command if the provider supports concatenating them. This is usually fine for moderate sets.

`ExecuteUpdate` and `ExecuteDelete`: These methods allow you to perform bulk updates or deletes based on a LINQ filter directly in the database, without loading the data as entities. For example: `context.Orders.Where(o => o.IsActive == false).ExecuteDelete();` will translate to a single `DELETE FROM Orders WHERE IsActive = 0` SQL statement that removes all inactive orders. Similarly, `context.Orders.Where(o => o.Status == "Pending").ExecuteUpdate(set => set.SetProperty(o => o.Status, "Archived"));` can translate to something like `UPDATE Orders SET Status = 'Archived' WHERE Status = 'Pending';`. These operations happen completely on the database side.

How ExecuteUpdate/Delete works: Under the hood, when you call `ExecuteUpdate` on a queryable:
-   EF Core essentially uses the query translation part of the pipeline to transform the filter (the `Where` clause, etc.) into a SQL `WHERE` clause.
-   Instead of a SELECT, it formulates an UPDATE statement. The lambda you provide to `SetProperty` is used to know which columns to set and to what values. EF ensures those values or expressions get translated to SQL properly. You can even do things like `SetProperty(o => o.LastUpdated, o => DateTime.UtcNow)` to use current time, which would get translated to the appropriate SQL function or parameter.
-   It then executes that SQL directly. No change tracking is involved (the context does not track the individual entities affected since they were never loaded as entities).
-   For `ExecuteDelete`, EF Core just needs the filter part and creates a DELETE command.
-   These methods return the number of rows affected.

Benefits of these bulk operations: They are much more efficient for mass updates/deletes because:
-   You skip loading data into memory.
-   One SQL statement (or a small number of them) can affect many rows, which is faster than doing many round-trips.
-   It reduces memory usage and can leverage the database's set-based operation optimizations.

Caveats: Since the context isn't tracking those entities, your in-memory state might not know about these changes. For instance, if you have some `Order` entities already loaded and tracked in the context and you do an `ExecuteUpdate` that changes their status in the database, the tracked objects in your context won't be automatically updated. You would need to synchronize or reload them as appropriate. Generally, one uses these bulk operations when needing to do maintenance or cleanup tasks, or applying broad changes where tracking each entity isn't necessary.

Other bulk operations: EF Core still doesn't have a built-in bulk merge/upsert, or bulk copy, etc., as of current versions. Those scenarios either rely on raw SQL or third-party libraries. However, the addition of ExecuteUpdate/Delete covers a lot of the common bulk update/delete needs. Bulk insert is usually handled by just adding multiple entities and saving (which, as mentioned, can be batched). Also, note that if you add thousands of entities to the change tracker, it will consume a lot of memory and slow down ChangeTracking. For extreme cases, some prefer to disable tracking for bulk inserts as well and use manual SQL or specialized APIs.

Key points: Bulk CUD operations highlight EF Core's ability to perform set-based changes. They complement the normal unit-of-work pattern by offering a convenient way to affect many rows at once when needed. As a contributor or advanced user, be aware of these because they drastically improve performance for certain scenarios and are a relatively recent addition to EF Core's feature set.

### Model Building and Conventions

Definition: The EF Core model is the internal representation of your domain types (entities) and their mapping to the database schema. Model building is the process of constructing this model, which includes entity types, relationships, keys, property mappings, constraints, etc. Conventions are default rules that infer model configuration implicitly, to minimize the amount of explicit configuration needed.

How the model is built: EF Core can build the model in four ways, often combined:

1.  Conventions: By default, EF Core infers a lot of information by convention. For example, any property named "Id" or `<TypeName>Id` is assumed to be the primary key. A `string` property without further configuration becomes a column of a suitable length (e.g., nvarchar(max) in SQL Server). Relationships are inferred by matching navigation property types and foreign key property naming conventions (e.g., a property named `CustomerId` in an `Order` entity and a navigation of type `Customer` clues EF Core to a relationship). Conventions also cover things like default table naming (the entity class name becomes the table name, unless overridden).
2.  Data Annotations (Attributes): You can decorate your classes and properties with attributes (like `[Key]`, `[Required]`, `[MaxLength]`, `[Column(...)]`, etc.). These mapping attributes override or augment the conventions. For example, `[Key]` on a property marks it as the primary key if conventions didn't pick it up, or `[Column("ProductName")]` can specify a column name explicitly.
3.  Fluent API (Model Builder): In the `OnModelCreating(ModelBuilder builder)` method of your `DbContext`, you have full control to configure the model using the fluent API. This is the most powerful way to shape the model. Here you can configure things like composite keys (`builder.Entity<Blog>().HasKey(b => new { b.BlogId, b.OtherKey })`), relationships (`builder.Entity<Order>().HasOne(o => o.Customer).WithMany(c => c.Orders).HasForeignKey(o => o.CustomerId)`), table names, indexes, concurrency tokens, and much more.
4.  Pre-convention Configuration: you can globally configure model defaults before the normal conventions run. By overriding `ConfigureConventions(ModelConfigurationBuilder builder)` in your `DbContext`, you can set up rules that apply to all entities or properties of a certain type. For example, you could specify that all `string` properties are non-unicode with a maximum length, or that all `DateTime` properties use a specific value converter, without having to configure each property individually. This pre-convention configuration uses the `ModelConfigurationBuilder` API (e.g., `builder.Properties<string>().HaveMaxLength(256)`), and it influences the model as it is being built by the convention system. In essence, you’re customizing the conventions: you declare defaults (for property types, default mappings, value converters, precision/scale, etc.) once, and EF Core’s model builder will automatically apply those rules wherever applicable. This makes it easier to manage cross-cutting concerns in the model configuration and reduces repetition. Pre-convention configuration is applied at context startup (during model creation), ensuring those settings are in effect for all subsequent operations (migrations, queries, etc.) using that model.

Conventions in depth: EF Core's convention system runs while the model is being built. Essentially, EF Core scans through the entity classes (usually discovered through your `DbSet<T>` properties on the context, or added via `modelBuilder.Entity<T>()` calls) and applies a series of convention rules. These conventions are implemented as small logic components that get called as the model is being created or modified. For example, one convention might say "Every property named 'Id' or ending with 'Id' that is of a numeric type becomes the primary key." Another might infer relationships by matching navigation property and key names. Conventions can also set up default values, generate shadow properties (hidden FK properties that aren't in your classes), and infer join tables for many-to-many relations.

Model validation and finalization: After applying all conventions and configurations (attributes and fluent API), EF Core finalizes the model. This involves validating there are no conflicting configurations, that all needed information is present (keys for each entity, etc.), and then caching the model. By default, EF Core will cache the model for a given context type so that it doesn't need to rebuild it on every instantiation of your `DbContext`. The model is typically built the first time you instantiate a particular `DbContext` subclass (or the first time you run a query on it, depending on implementation), and then reused.

Controlling the model:

-   You can explicitly build a model and cache it using a compiled model (see Compiled Models below) for large models.
-   EF Core allows you to add or replace conventions by overriding the `OnModelCreating` and using `builder.Conventions` (or via plugins) -- for advanced scenarios where the default conventions need tweaking.
-   The model includes metadata types like `IEntityType`, `IProperty`, `IKey`, `IForeignKey`, etc., accessible via the context's `Model` property. These can be inspected at runtime if needed (for dynamic scenarios or generic code).

Key points: Model building is how EF Core knows about your classes and their mapping. Good understanding of conventions helps you know what you get for free versus what must be configured. For new contributors, it's important to know that EF Core will automatically configure most common patterns, and you only need to override when conventions don't match your needs. The combination of conventions, annotations, and fluent configurations gives flexibility in how the model is defined.

### Model Components

In an EF Core model (represented by the `IModel` metadata), several fundamental building blocks describe the shape of your data and how it interconnects. Here are the major components and how they relate to each other:

-   Entity Types: An *entity type* corresponds to a .NET class (usually your entity/domain class) that is included in the model. Each entity type typically maps to a database table or collection. An entity type can participate in an inheritance hierarchy (having a base or derived types), and can be marked as owned (see Complex/Owned Types below). Entity types are identified by their primary key and contain definitions for their properties, relationships, and behaviors in the model.

-   Properties: A *property* represents a piece of data on an entity type (usually a class property on your .NET class). Properties have a CLR type (e.g., `int`, `string`, `DateTime`) and usually map to a database column (or part of a column, in some cases like concurrency tokens or computed columns). EF Core distinguishes between different kinds of properties:
    -   *Scalar properties* hold primitive or enum values (e.g., `Name`, `Price`, `CreatedDate` on an entity).
    -   *Primitive collection properties* can hold collections of scalars.
    -   *Navigation properties* are also properties in the class, but they reference other entity types instead of scalar values.
    -   *Shadow properties* are model properties not represented by an actual field in the .NET class; EF Core can maintain them in the model for foreign keys or audit timestamps if you choose not to have them in your class.
    -   *Service properties* store references to EF Core internal services and are not persisted to the database.
	
-   Keys: A *key* in the model is a set of one or more properties that uniquely identifies an instance of the entity type. The primary key (PK) is a special key that serves as the unique identity of each entity (and often corresponds to a primary key constraint in the database). Keys can be simple (a single property) or composite (multiple properties). EF Core also allows *alternate keys* (unique constraints) for additional unique identifiers. In the model metadata, an entity type will have one primary key defined (accessible via `IEntityType.FindPrimaryKey()` in the API) and zero or more alternate keys. Keys are fundamental for establishing relationships because a foreign key on another entity will point to a key (usually the primary key) on the principal entity.

-   Foreign Keys and Relationships: A *foreign key (FK)* is one or more properties on a dependent entity type that form a relationship to a principal entity type by referencing its primary (or alternate) key. In the EF Core model, a relationship between two entity types is defined by a foreign key object which knows the dependent entity type, the principal entity type, the alignment of properties, and the delete behavior (cascading rules). For example, if `Order` has a foreign key `CustomerId` pointing to `Customer.Id`, then in the model there is a foreign key linking Order (dependent) to Customer (principal) using Order.CustomerId = Customer.Id.
    Every one-to-many or one-to-one relationship is backed by a foreign key. In many-to-many relationships, there isn't an explicit join entity type in your code, but under the hood EF represents it via two foreign key relationships to an implicit join entity type. The model tracks foreign keys not just for the database schema, but also to manage how navigations work at runtime (e.g., fixing up navigation properties based on FK values).

-   Navigation Properties: *Navigation properties* are the object references or collections in your entity classes that allow navigation of the relationships. There are two kinds:
    -   Reference navigation: a property that holds a single related entity (for example, an `Order` class might have a `Customer` navigation property referring to the single `Customer` who placed it).
    -   Collection navigation: a property that holds a collection of related entities (for example, a `Customer` class might have an `Orders` collection property containing all of that customer's orders).\
        Navigations are *mapped on top of foreign keys*. That is, a navigation does not contain any additional data in the database; it's a way to traverse the association defined by a foreign key. In the model, a navigation property is always paired with a corresponding foreign key (and often an inverse navigation on the other side of the relationship). You can have a relationship with just the foreign key defined (no navigation properties in the classes), or with only one navigation, or with both sides navigable. EF Core uses the model's navigation definitions to automatically load related entities (when you `.Include(...)` or lazy-load) and to keep the object graph in sync (setting a navigation also sets the FK value, etc., when tracked by the context).
		
-   Owned Types: An *owned entity type* is a type that does not have its own independent identity and is owned by another entity type. In EF Core, you make a type owned by configuring it with `OwnsOne`/`OwnsMany` in the model builder or [OwnedAttribute]. Owned types are conceptually similar to *aggregates*: they are part of the parent entity. For example, you might have an `Address` class that is always used as a property of a `Customer` entity (and not saved on its own). The owned `Address` might map to the same table as Customer (table splitting) or to a separate table with a foreign key back to Customer, depending on configuration. In the model, owned types are represented as separate entity types, but marked with an Owned flag and tied to the owner via a special kind of relationship. They do not have their own primary key; instead, the primary key of the owner plus possibly some discriminator or position serves to identify instances if needed. You typically don't directly query for an owned type without the owner; they come along with the owner entity.

-   Service Properties: A *service property* is a property in an entity class that is used to store an EF Core service, rather than application data. These are relatively rare, but one common example is the `ILazyLoader` interface for lazy-loading proxies. If you enable lazy loading via proxies, EF Core can inject an `ILazyLoader` into your entities (usually through a constructor or by setting a property) which it uses to load related entities on demand. Such a property is marked in the model as a service property, meaning EF Core will ignore it when mapping to the database (no column for it) but will know to supply the appropriate service instance to it when the entity is materialized. Service properties allow the entity instances to have certain behaviors (like lazy loading) without that data being part of the persisted model state. In summary, service properties exist in the entity class, are captured in the model metadata, but are excluded from data mapping.

-   Primitive Collections: *Primitive collection properties* are collections of scalar types (like a list of strings, numbers, etc.) on an entity. Under the covers, EF might map this to a JSON column to store the array of values (some providers like SQL Server can map collection of primitives to JSON if configured). The important point is that each element in the collection is not an entity with its own key; the collection as a whole is a property of the owner entity. The model records the element type and how it's stored.

####  Complex Types

In EF Core, a *complex type* represents a structured type that is part of an entity but does not have its own identity or primary key. Complex types (often called *value objects* in DDD terminology) are like a grouping of properties that hang off an entity. Key characteristics of complex types in the EF model include:

-   No Identity: A complex type is not identified by a key and is never tracked independently. You cannot have a `DbSet<ComplexType>`; they must be owned by an entity. They are effectively an extension of the entity's data rather than a standalone entity.
-   Embedded in Owner: The properties of a complex type are stored as part of the owner entity's data. By default, this often means their values are stored in the same table as the declaring entity type. For example, if you have a complex type `Address` with properties like `Street`, `City`, `ZipCode` and it's a property on `Customer`, the `Address` fields might be stored in the `Customers` table columns (Street, City, ZipCode).
-   No Separate Existence: Complex types cannot exist without their owner entity. If you delete the owner, its complex type data is deleted as well (cascading naturally as it's part of the same row or dependent storage). Complex types also can't have navigations to independent entities on their own (since they aren't full entities), and typically they don't support their own foreign keys. They are meant to be a self-contained value object.
-   Configuration: In EF Core, complex types can be configured with the `[ComplexType]` attribute or via the Fluent API (e.g., `builder.ComplexProperty(...)` ). Owned entity types are similar in usage but differ in some capabilities -- for instance, owned types internally have a key and can optionally be stored in a separate table, whereas complex types have no key and are always part of the owner's storage. Additionaly complex types can be used to map value types.
-   Shared Instances: A nuance with complex types is that because they are treated as value objects, the same instance of a complex object can be assigned to multiple owners or properties if desired, and EF will treat those as non-related instances (since identity isn't tracked). In contrast, owned entity types could not be shared across different owners because of their key-based identity; EF would clone or treat them as a single instance.
-   Limitations: Because complex types are part of the owner and lack independent identity, you cannot query or manipulate them independently of the owner. For instance, you can't directly query "all Address value objects in the database" as first-class query roots; you'd query the owners (Customers) and include or select the addresses.

Complex Type Collections: EF Core also supports having collections of complex types as properties of an entity. This means an entity can own a list of value objects. For example, a `Customer` might have a collection of `Address` value objects (multiple addresses).
-   Storage: Under the hood, in databases or providers that support JSON or document storage (e.g., SQL Server's JSON columns, Azure Cosmos DB, etc.), the collection of complex objects might be stored as a JSON array in a single column of the owner's table.

### Storage and Type Mapping

Definition: Storage and type mapping in EF Core refer to how .NET types and values are mapped to database types and how values are converted when reading from or writing to the database. EF Core has to bridge the gap between the CLR world and the storage world.

Type mapping: Each property in your model is associated with a type mapping. This mapping knows:

-   The .NET CLR type of the property (e.g., `int`, `string`, `DateTime`, `Guid`, custom enums, etc.).
-   The corresponding database type (e.g., `int` might map to `INTEGER` in SQLite, `int` in SQL Server, etc.; `string` might map to `nvarchar( max )` on SQL Server or `TEXT` on SQLite).
-   Additional facets: length for strings, precision/scale for decimals, whether a type is unicode or fixed-length, etc.
-   How to generate literal values or parameter bindings in SQL for this type.
-   How to read the value back from `DbDataReader` (this is often handled by ADO.NET DataReader directly, but EF ensures using the right accessor, like `GetInt32` for an int column, etc.).

EF Core's provider model includes a service called a `TypeMappingSource`. The provider implements this to supply type mappings for any given property or CLR type. By default, EF Core will choose a mapping based on the CLR type and perhaps data annotations (like `[Column(TypeName="...")]` or fluent API configurations like `.HasColumnType("...")`). If the default isn't what you want, you can override it via the fluent API or attributes.

For example, by convention a `string` without max length might be nvarchar(max) in SQL Server, but you could configure `.HasMaxLength(100)` to make it nvarchar(100). Or use `.HasColumnType("varchar(100)")` to explicitly set the store type. This all goes through the type mapping system.

Value conversions: EF Core also allows configuring value converters for properties. This is part of type mapping as well. For instance, you may have an enum property in your class but want to store it as a string in the database. You can configure a conversion for that property. EF Core will then map the enum to string when saving (and vice versa when querying). Under the hood, EF sets up a type mapping that knows there's a conversion: the CLR type is enum, provider CLR type is string, and then the database type maybe is nvarchar. EF handles applying the conversion logic at the right time.

Similarly, if you have a complex type like a `Point` struct or a value object, you might store it as a single column (like "POINT" in a spatial type or as a JSON string). The type mapping system is flexible to accommodate these via converters or provider-specific types.

Storage aspects: Apart from type mapping, EF Core's storage layer deals with database connections and commands (via ADO.NET) and ensures that parameters are assigned with the correct types. It also deals with ensuring transactions and command timeouts, etc., but those are more infrastructure.

Another part of storage is execution strategy -- e.g., handling transient errors by retrying (which is part of `ExecutionStrategy` typically enabled for Azure SQL or others). While not exactly type mapping, it's another piece of how EF interacts with the underlying database robustly.

Why it matters: Usually, developers don't have to worry about type mapping unless:

-   A default mapping is unsuitable (like maybe a `decimal` default to decimal(18,2) but you need more precision, so you change it).
-   You run into an issue where a .NET type isn't supported by a provider out of the box (for example, `DateOnly` or `TimeOnly` structs introduced in .NET 6 might not have been supported until EF Core 7/8 providers).
-   You want to use a custom conversion (like encrypting a field transparently, or storing a complex object as JSON).
-   Performance considerations like using the appropriate size (not using max if not needed, etc.).

Type mapping and providers: Each database has its own quirks. For instance, SQLite doesn't have a dedicated DateTime type; dates are stored as TEXT or REAL or INTEGER. The SQLite provider has to map .NET DateTime to one of these (by default, as TEXT in a certain format, I believe). SQL Server has unique types like `sql_variant` or spatial geography types; its provider has to handle those. Some databases distinguish Unicode vs ASCII strings, or have fixed vs variable types, etc. The provider's TypeMappingSource has logic to return the best match for each scenario.

Key points: EF Core's type mapping ensures that the data you retrieve is the same as the data you stored (within the limits of what the database types can represent). It's a critical piece for correctness and compatibility. When dealing with raw SQL (via `FromSqlRaw` or `ExecuteSql`) combined with EF, you might sometimes need to be aware of mapping too (like the SQL you write must return column names/types that match your entity). If you're creating a provider or extending one, implementing the mapping correctly is crucial for everything else (migrations, queries) to work well.

### Scaffolding (Reverse Engineering)

Definition: Scaffolding (also known as reverse engineering) is the process by which EF Core can generate code (DbContext and entity classes) from an existing database schema. This is essentially the inverse of the typical code-first approach: it starts from the database and produces the corresponding model in code.

How it works: Using the EF Core command-line tools or Visual Studio tools, you can run a command such as `dotnet ef dbcontext scaffold` (for CLI) or `Scaffold-DbContext` (in Package Manager Console). This process involves several steps:

1.  Database Introspection: EF Core uses the specified database provider to read the schema of the existing database. Each provider has the ability to query the database's metadata (tables, columns, constraints, etc.). For example, for a relational database, it will query the information_schema tables or use database-specific commands to get schema information.
2.  Model Construction: EF Core then constructs an EF Core model in memory that represents that schema. Essentially, it creates an equivalent `IModel` with all the entity types, properties, keys, and relationships that correspond to the database. During this process, it applies some heuristics similar to conventions (for instance, it might infer pluralization/singularization of names, or identify join tables for many-to-many). However, since the database is authoritative, it mostly takes the schema as-is (every table becomes an entity, columns become properties with their database types mapped to .NET types, foreign keys become relationships, etc.).
3.  Code Generation: Once the model is built, EF Core's design-time services generate code files for the model:
    -   An equivalent `DbContext` class with `DbSet<TEntity>` properties for each table. This context class will also include an `OnModelCreating` method call that applies any necessary configuration that cannot be inferred by conventions alone (for example, composite primary keys, table names that don't match the entity class names, etc.). The scaffolder tries to use fluent API here only for things needed, letting conventions handle the rest.
    -   Entity classes for each table (if you didn't specify to use existing ones). These classes will have properties for each column. If keys or relationships are discovered, data annotations may be used (e.g., `[Key]`, `[ForeignKey]`) or fluent API in OnModelCreating to represent them.
    -   Optionally, it can use Data Annotations in the generated classes for simplicity, and in some cases, it might also generate separate configuration classes using the fluent API (if you scaffold with certain options or manually separate configuration).
4.  Preserving customizations: Scaffolded code is meant as a starting point. You can modify it, and you can re-scaffold later if the database changes. EF Core will not overwrite existing files unless told to. A common practice is to scaffold once, then treat the classes as code-first afterwards, using migrations for further changes.

Use cases: Reverse engineering is very useful when you have an existing database and want to create a new application or service using EF Core with that database. It saves a lot of manual coding. New contributors should understand that scaffolded code may not exactly match how they would write code-first classes by hand, but it provides a correct baseline reflecting the database.

Key points: Scaffolding relies on the provider's ability to interpret the database schema and on EF Core's code generation for the model. The generated code may not be the most clean or ideal for long-term maintenance (for example, it may not detect certain desired data types or relationships the way you would model them in code-first), but it should be functionally correct. After scaffolding, developers typically review and refactor the code (e.g., rename classes or properties to better names, remove pluralization if not desired, etc.). It's essentially an automated first draft of the EF Core model.

### Database Providers

Definition: EF Core is database-agnostic at its core; the actual database-specific behaviors are implemented by database providers. A provider is a library (typically a NuGet package) that plugs into EF Core to enable support for a particular database engine. For example, Microsoft provides providers for SQL Server (`Microsoft.EntityFrameworkCore.SqlServer`), SQLite (`Microsoft.EntityFrameworkCore.Sqlite`), and Azure Cosmos DB (`Microsoft.EntityFrameworkCore.Cosmos`), and there are many third-party providers (e.g., Npgsql for PostgreSQL, Pomelo for MySQL, Oracle's provider, etc.).

Role of providers: Providers extend or implement several subsystems of EF Core to translate the abstract EF Core concepts into the specific dialect or API of the target database. Key areas where providers contribute are:

-   Query Translation and SQL Generation: The provider supplies the logic to convert the intermediate expression tree (or the high-level query representation) into the database's query language. For relational databases, this means SQL generation tailored to that database's flavor (for instance, SQL Server vs. SQLite have slightly different SQL capabilities and syntax). Providers can introduce custom LINQ extensions or support for additional functions that only their database has. They also can adjust how certain queries are processed (for example, some database might not support a certain LINQ method, so the provider might translate it differently or throw a clear exception).
-   Connection and Execution: Providers often implement how EF Core opens connections and executes commands. In practice, most providers build on ADO.NET data providers. For example, the SQL Server EF Core provider uses `System.Data.SqlClient` or `Microsoft.Data.SqlClient` under the hood to actually talk to SQL Server. The EF provider wraps that in EF's `Database` and `DbConnection` abstractions. Non-relational providers like Cosmos DB don't use ADO.NET but instead call the appropriate SDK (e.g., the Azure Cosmos DB SDK for .NET).
-   Database Creation/Migration Scripting: The provider knows how to create a database if asked (`context.Database.EnsureCreated()`), and more importantly, how to generate migration scripts (SQL DDL statements) for schema changes. For instance, when you add a new entity and create a migration, the migration uses provider services to generate SQL like `CREATE TABLE` statements or `ALTER TABLE` etc. Each provider defines how to map EF Core's schema operations (create table, add column, etc.) to the correct DDL for that database.
-   Type Mapping: Providers supply a mapping from .NET types to the database types. This includes facets like maximum length, precision, etc. For example, the SQL Server provider will map a .NET `string` (without further configuration) to `nvarchar(max)` by default, whereas SQLite might map it to `TEXT`. Providers handle differences in types (e.g., how GUIDs or DateTime offsets are stored) and ensure that when EF Core generates code or migrations, it uses correct type names.
-   Functions and Method Translations: Providers can implement support for database-specific functions (like JSON functions, full-text search, etc.). They do so by hooking into the query pipeline with custom translators or expression visitors that recognize certain .NET methods and convert them into SQL functions or expressions.
-   Behavioral nuances: Different databases have different capabilities or behaviors (for example, case-sensitivity, identity value generation, default schema, etc.). The provider often needs to adjust EF Core's behavior or default assumptions. For instance, by convention EF Core might use schema "dbo" for SQL Server but schema is not applicable for SQLite; the SQLite provider might ignore the schema part. Another example: Some databases require special handling of batch commands or don't support multiple active result sets --- the provider will indicate those capabilities to EF Core so it can adjust (like using MARS on SQL Server or sequential access on others).

Design of providers: Under the hood, a provider typically extends a base class or uses a set of base services provided by EF Core. Relational providers usually extend from Microsoft.EntityFrameworkCore.Relational, which provides a lot of common functionality (shared SQL generation logic, migrations infrastructure, etc.). Non-relational providers bypass the relational layer and implement the needed interfaces directly. For contributors, understanding that EF Core's core is in the `Microsoft.EntityFrameworkCore` package (which has no knowledge of SQL, for example), and the relational-specific stuff is in `Microsoft.EntityFrameworkCore.Relational` is important. Providers plug in via the `UseXYZ` extension method on `DbContextOptionsBuilder` (like `UseSqlServer()`), which internally registers the provider's services (using EF Core's internal dependency injection). This is how the EF Core context knows which provider's services to use for any given context.

Key points: *Providers are the database-specific layer of EF Core.* When contributing, if you add a feature (say a new LINQ function support), often you have to implement parts of it in the core and possibly each provider's translation layer. For users, the important thing is to choose the correct provider for your database and include it via NuGet and `DbContextOptions`. EF Core cannot function without a provider (except the in-memory provider used for testing or special scenarios).

### Migrations

Definition: Migrations in EF Core are a mechanism to incrementally evolve the database schema over time, keeping it in sync with the application's model definitions. Each migration describes a set of changes (add a table, alter a column, drop a constraint, etc.) and can be applied to the database in sequence.

How migrations work: In a code-first approach, when you modify your entity classes or model configuration, those changes need to reflect in the database schema. Migrations provide a structured way to apply these changes. The workflow typically looks like:

1.  Add Migration (Design-time): You run `dotnet ef migrations add <Name>` or use Package Manager Console `Add-Migration <Name>`. EF Core compares the current model (derived from your code) to the last applied migration (or a snapshot of the model from the last migration). Based on the differences, it scaffolds a new migration class (C# code) that describes the transitions. This class will have an `Up()` method with code (using the `MigrationBuilder`) to perform the schema changes, and a corresponding `Down()` method to revert those changes if needed.
2.  Review & Modify (Optional): The generated migration is often ready to go as-is, but you might customize it. For example, you might want to split a large operation, add a data correction step, or manually write SQL for complex operations that EF Core doesn't handle automatically.
3.  Apply Migration (Runtime or Design-time): To apply migrations to the database, you use `dotnet ef database update` or PMC's `Update-Database`, or call `context.Database.Migrate()` at runtime. This will execute all pending migrations on the target database. EF Core keeps track of applied migrations using a special table in the database (usually `__EFMigrationsHistory`). Each migration, when run, will update this log.
4.  Database Modification: Under the hood, applying a migration runs the commands in the `Up()` method. For relational databases, these are typically translated into SQL `ALTER TABLE`, `CREATE TABLE`, etc., statements by the provider. The operations are executed in a transaction (by default) to ensure all or nothing. Once done, EF updates the migrations history table.
5.  Repeat: Each time the model changes (e.g., you add a new property or entity), a new migration can be added, maintaining versioned changes.

Migrations and model snapshot: EF Core also maintains a snapshot of the model (usually as a designer file alongside your migrations or embedded in the migrations code) to know what the model looked like after the last migration. It uses this snapshot to detect changes for the next migration. It's important to keep these in source control, and if you ever need to, you can recreate migrations from scratch (but that can be complex; better to let EF diff models via migrations).

Key features of migrations:

-   They support renaming of objects without data loss through an API (e.g., `RenameColumn`) to avoid drop/re-add when you just rename a property.
-   They allow adding seed data through the migrations API (`InsertData` etc.) for static data that should go with schema (though large data inserts are usually better handled outside migrations).
-   You can generate a SQL script of migrations (via `migrations script` command) for review or to apply in environments where running the tool directly is not possible.
-   You can skip using migrations entirely and use `EnsureCreated()` to create a database schema directly from the model, but this is recommended only for prototyping or testing, not for evolving a database schema in production (since `EnsureCreated` cannot update an existing schema or handle changes beyond create/drop).

### Command-Line Tools (CLI, Package Manager Console, MSBuild)

EF Core provides a set of design-time tools that help with managing the EF Core model and migrations. These tools come in two flavors:

-   .NET CLI Tools (cross-platform, work in any shell, primarily `dotnet ef` commands).
-   Package Manager Console (PMC) Tools in Visual Studio (PowerShell commands like `Add-Migration`, etc.).

Additionally, there are some MSBuild integration points, though those are mostly behind the scenes for the above tools.

.NET CLI Tools (`dotnet ef`): This is the go-to for many developers especially outside Visual Studio. After installing the `Microsoft.EntityFrameworkCore.Design` package (which contains the necessary bits), you can use commands like:

-   `dotnet ef migrations add <Name>` -- to add a new migration.
-   `dotnet ef migrations list` -- to list pending/applied migrations.
-   `dotnet ef migrations remove` -- to remove last migration (if not applied yet).
-   `dotnet ef database update` -- to apply migrations to the database (or `dotnet ef database update <MigrationName>` to go to a specific migration).
-   `dotnet ef database drop` -- to drop the database (useful in development/testing).
-   `dotnet ef dbcontext scaffold` -- to reverse engineer a database into a model (needs connection string and provider).
-   `dotnet ef dbcontext list` -- to list available DbContext types in the project (helpful if multiple).
-   `dotnet ef dbcontext optimize` -- to generate a compiled model (this generates code for your model for performance, see compiled model section).

These CLI commands are essentially an interface to EF Core's design-time services. When you run them, under the hood the tools will build and run a portion of your application to gather the EF model or apply actions. EF finds the `DbContext` by either looking for it in the assembly or using a specified `IDesignTimeDbContextFactory<T>` if you have one implemented (that factory way is useful if your context requires special configuration at design time).

Package Manager Console (Visual Studio): If you're using Visual Studio, you can use the PMC (from Tools > NuGet Package Manager > Package Manager Console). The commands are similar:

-   `Add-Migration <Name>`
-   `Update-Database`
-   `Remove-Migration`
-   `Scaffold-DbContext`
-   `Script-Migration` (to generate SQL script for the migrations)
-   etc.

Behind the scenes, these do the same things as the CLI. They just run inside Visual Studio so the PMC commands automatically target the startup project or a specific project you can set, and they open the generated files (like the new Migration file) for you after running the command.

MSBuild integration: The EF Core tools integrate with MSBuild primarily to locate and run your code. When you run a design-time command, it will trigger a build of your project (since it needs the latest compiled assembly to reflect over). The tools use MSBuild targets (in the Microsoft.EntityFrameworkCore.Tools package) to perform things like:

-   Detect the target framework and output path.
-   Run your program's assembly with a special entry to execute the desired operation (for example, it might invoke your `Program.CreateHostBuilder()` for ASP.NET Core apps to get the services and context).
-   The `dotnet ef` command is essentially a thin layer that invokes these MSBuild targets behind the scenes to do the work.

So, while you typically don't directly invoke MSBuild for EF operations, it's the mechanism that glues the CLI/PMC to your actual code and context.

Summary of Tools Usage:

-   Always ensure you have the design package installed.
-   The tools must be the same version as your EF Core runtime for compatibility.
-   In VS, there's also GUI for migrations in some extensions (or directly through PMC).
-   As a contributor or user, these tools help maintain the EF model and apply migrations without writing boilerplate code yourself.

Key points: The EF Core tools are an essential part of the development workflow. They help manage Migrations and scaffolding. For new developers: familiarize yourself with at least the CLI commands or the Visual Studio PMC commands, because manually creating migration classes or updating the DB schema is error-prone. The tools ensure consistency and reduce mistakes. They also often catch configuration issues by essentially test-running part of your code (for example, if your DbContext isn't configured correctly, `dotnet ef` will fail to find it or to connect).

### Compiled Models

Definition: A *compiled model* is a pre-generated, optimized representation of your EF Core model (the metadata about your entity types and mappings) that is compiled into your program. The idea is to avoid the cost of model discovery and creation at runtime, thus improving startup performance for applications with large models.

When it matters: In applications with a very large number of entity types (hundreds or more), building the model through conventions and configurations can take noticeable time and CPU. By compiling the model, EF Core can skip that step and just load an already-built model. For small to medium models, the difference is minor and usually not worth the extra complexity.

How to create a compiled model: EF Core provides a command to scaffold a compiled model. Using the CLI, you can run:

```
dotnet ef dbcontext optimize --output-dir <folder> --namespace <name>

```

This will generate a set of C# files in the specified folder containing code that represents the model. Specifically, it creates classes that inherit from `CompiledModel` and a context factory, etc., that when invoked will set up the model exactly as your fluent API/annotations would.

You then compile these into your project. In your `DbContext` configuration, you tell EF to use the compiled model instead of doing the normal `OnModelCreating`. This is done via `.UseModel(CompiledModel.Model)` on the `DbContextOptionsBuilder` (the generated code includes a reference to an `IModel` instance you can use).

What it does under the hood: The generated model code basically hard-codes all the configuration that EF would normally figure out at runtime. Instead of EF scanning assemblies for entities or using reflection heavily, the compiled model code explicitly registers all entities, properties, keys, relationships, etc., by calling the appropriate builder methods. It's like doing manually what the convention and model builder does dynamically, but ahead of time.

Because it's compiled, all the reflection to discover properties becomes direct assignments in code. This typically yields a performance boost in the first query or first use of the context (since normally that's when the model would be built).

Limitations: Compiled models come with a few caveats:

-   If you change your model (e.g., add a new property or entity), you must regenerate the compiled model and recompile your project, otherwise EF will be using an out-of-date model definition.
-   Some features are not supported in compiled models as of now (for example, certain kinds of model customization like global query filters, or the use of dynamic model cache key which might vary the model per connection string; these things might not translate to the compiled model).

Key points: For a typical project with, say, 10-50 entity types, you likely don't need a compiled model. But it's good to know that this exists for high-end performance tuning. As a contributor, be aware that any changes to how the model building works must also be reflected in compiled model generation logic. And if you are troubleshooting performance and see a big chunk of time in model creation, a compiled model might be the solution. Always weigh the complexity cost: the compiled model files are generated code you'll have to include and maintain (regenerate when needed). The compiled model and pre-compiled queries are required if you wish to compile your app with NativeAOT.
