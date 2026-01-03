# Entity Framework Core - MergeOptionFeature Test Guide

This guide provides a comprehensive overview of all tests in the `Microsoft.EntityFrameworkCore.MergeOptionFeature` namespace. These tests are designed to validate the refresh functionality (merge options) across various Entity Framework Core features when entities are refreshed from the database after external changes.

## Background

The MergeOptionFeature tests simulate scenarios where database data changes externally (outside of the current EF Core context), and then test how EF Core handles refreshing entities to reflect these external changes. This is crucial for applications that need to stay synchronized with database changes made by other processes, users, or systems.

---

## Test 1: RefreshFromDb_Northwind_SqlServer_Test

### 1.1 EF Core Features Tested
This test focuses on **basic entity refresh functionality** using the standard Northwind database model. It tests core scenarios like:
- Entity states (Unchanged, Modified, Added, Deleted)  
- Query terminating operators (ToList, FirstOrDefault, etc.)
- Include operations for loading related data
- Lazy loading with proxies
- Non-tracking queries
- Streaming vs buffering query consumption

### 1.2 Test Context and Model
- **Context**: Uses `NorthwindQuerySqlServerFixture<NoopModelCustomizer>`
- **Model**: Standard Northwind database (Customers, Orders, Products, etc.)
- **Elements**: Real-world entities with established relationships
- **Database**: SQL Server with pre-seeded Northwind data

### 1.3 Global Test Purpose  
Validates that EF Core's basic refresh mechanisms work correctly across different entity states and query patterns. Ensures that external database changes are properly reflected when entities are refreshed, and that the change tracking system maintains consistency.

### 1.4 Test Methods Overview
- **Entity State Tests**: Verify refresh behavior for entities in different states (Added, Modified, Deleted, Unchanged)
- **Query Pattern Tests**: Test refresh with different LINQ terminating operators and consumption patterns  
- **Include Tests**: Validate refresh behavior when related entities are loaded via Include operations
- **Proxy Tests**: Test lazy loading proxy behavior during refresh operations
- **Error Condition Tests**: Ensure proper exceptions are thrown for invalid refresh scenarios (e.g., non-tracking queries)

---

## Test 2: RefreshFromDb_ComplexTypes_SqlServer_Test

### 2.1 EF Core Features Tested
This test focuses on **Complex Types** - value objects that are embedded within entities but don't have their own identity. Features tested:
- Owned types (both collection and non-collection)
- Complex properties of value and reference types  
- Refresh behavior for nested value objects
- JSON serialization of complex data

### 2.2 Test Context and Model
- **Context**: Custom `ComplexTypesContext` with specially designed entities
- **Model**: 
  - `Product` with owned `ProductDetails` and owned collection `Reviews`
  - `Customer` with complex property `ContactInfo` and owned collection `Addresses`
- **Elements**: Demonstrates embedding complex objects within main entities
- **Database**: Custom tables with JSON columns and nested object storage

### 2.3 Global Test Purpose
Validates that EF Core correctly refreshes complex types and owned entities when the underlying database data changes. Ensures that nested objects maintain their structure and relationships during refresh operations.

### 2.4 Test Methods Overview
- **Collection Owned Types**: Tests refreshing when owned collections (like Reviews) change externally
- **Non-Collection Owned Types**: Tests refreshing simple owned objects (like ProductDetails)
- **Collection Complex Properties**: Tests refreshing complex collections (like Addresses)  
- **Non-Collection Complex Properties**: Tests refreshing simple complex objects (like ContactInfo)

---

## Test 3: RefreshFromDb_ComputedColumns_SqlServer_Test

### 3.1 EF Core Features Tested
This test focuses on **Computed Columns** - database columns whose values are calculated by the database engine rather than stored directly. Features tested:
- Properties mapped to computed columns
- Mathematical computations (Price * Quantity)
- String concatenation formulas
- Date formatting expressions
- Database-generated calculated values

### 3.2 Test Context and Model
- **Context**: Custom `ComputedColumnsContext` with computed column definitions
- **Model**:
  - `Product` with `TotalValue` (Price * Quantity) and `Description` (Name + Price formatted)
  - `Order` with `FormattedOrderDate` (formatted date string)
- **Elements**: SQL Server computed column expressions using T-SQL functions
- **Database**: Tables with computed columns using SQL Server-specific syntax

### 3.3 Global Test Purpose
Ensures that computed columns are correctly refreshed when their underlying base columns change. Validates that database-calculated values are properly materialized into .NET properties during refresh operations.

### 3.4 Test Methods Overview
- **Basic Computed Columns**: Tests refreshing entities where computed values depend on other columns
- **Query Integration**: Validates that computed columns work correctly in LINQ queries
- **Database-Generated Updates**: Tests refresh when computed columns use database functions like date formatting

---

## Test 4: RefreshFromDb_GlobalFilters_SqlServer_Test

### 4.1 EF Core Features Tested
This test focuses on **Global Query Filters** - automatically applied WHERE clauses that filter entities globally across all queries. Features tested:
- Multi-tenancy filtering (filtering by TenantId)
- Soft delete patterns (filtering out deleted records)
- Dynamic filter contexts (changing filter values at runtime)
- Filter bypass using `IgnoreQueryFilters()`

### 4.2 Test Context and Model
- **Context**: Custom `GlobalFiltersContext` with tenant-aware filtering
- **Model**:
  - `Product` with multi-tenancy filter (`TenantId == CurrentTenantId`)
  - `Order` with both multi-tenancy and soft delete filters (`TenantId == CurrentTenantId && !IsDeleted`)
- **Elements**: Context property `TenantId` that controls filtering behavior
- **Database**: Tables with tenant and soft delete columns

### 4.3 Global Test Purpose
Validates that global query filters work correctly during refresh operations, ensuring entities remain properly filtered even when refreshed from the database. Tests that filter context changes affect entity visibility as expected.

### 4.4 Test Methods Overview
- **Basic Filter Tests**: Ensures filtered entities refresh correctly within their filter scope
- **Filter Bypass Tests**: Tests `IgnoreQueryFilters()` to access normally filtered entities
- **Dynamic Context Tests**: Validates changing filter context (tenant switching) affects entity visibility
- **Soft Delete Tests**: Tests the common soft delete pattern with global filters

---

## Test 5: RefreshFromDb_ManyToMany_SqlServer_Test

### 5.1 EF Core Features Tested  
This test focuses on **Many-to-Many Relationships** without explicit join entities. Features tested:
- Modern EF Core many-to-many configuration
- Automatic join table management
- Bidirectional relationship updates
- Collection navigation refresh
- Multiple relationship manipulation

### 5.2 Test Context and Model
- **Context**: Custom `ManyToManyContext` with modern many-to-many setup
- **Model**:
  - `Student` ? `Course` (StudentCourse join table)
  - `Author` ? `Book` (AuthorBook join table)  
- **Elements**: Uses `HasMany().WithMany().UsingEntity()` configuration
- **Database**: Automatic join tables managed by EF Core

### 5.3 Global Test Purpose
Ensures that many-to-many relationships are correctly refreshed when join table records change externally. Validates that both sides of the relationship reflect changes when navigation collections are refreshed.

### 5.4 Test Methods Overview
- **Add Relationships**: Tests adding new many-to-many connections externally and refreshing
- **Remove Relationships**: Tests removing connections and refreshing to reflect removal
- **Bidirectional Updates**: Ensures both sides of relationships update when refreshed
- **Multiple Operations**: Tests handling multiple relationship changes simultaneously

---

## Test 6: RefreshFromDb_PrimitiveCollections_SqlServer_Test

### 6.1 EF Core Features Tested
This test focuses on **Primitive Collections** - collections of primitive types (string, int, Guid) stored as JSON in database columns. Features tested:
- JSON column mapping for collections
- Primitive collection configuration
- Collection serialization/deserialization
- Empty collection handling
- Various primitive types (strings, integers, GUIDs)

### 6.2 Test Context and Model  
- **Context**: Custom `PrimitiveCollectionsContext` with JSON column storage
- **Model**:
  - `Product` with `List<string> Tags` stored as JSON
  - `Blog` with `List<int> Ratings` stored as JSON
  - `User` with `List<Guid> RelatedIds` stored as JSON
- **Elements**: Uses `PrimitiveCollection()` configuration method
- **Database**: JSON columns in SQL Server for collection storage

### 6.3 Global Test Purpose
Validates that primitive collections stored as JSON are correctly refreshed when the underlying JSON data changes. Ensures proper serialization/deserialization during refresh operations.

### 6.4 Test Methods Overview
- **String Collections**: Tests refreshing collections of strings (tags, categories)
- **Number Collections**: Tests refreshing collections of integers (ratings, scores)  
- **GUID Collections**: Tests refreshing collections of GUIDs (identifiers)
- **Empty Collections**: Tests edge cases with empty collections and null handling

---

## Test 7: RefreshFromDb_ShadowProperties_SqlServer_Test

### 7.1 EF Core Features Tested
This test focuses on **Shadow Properties** - properties that exist in the EF model and database but not as .NET class properties. Features tested:
- Shadow property configuration
- Accessing shadow properties via `Entry().Property()`
- Shadow foreign keys for relationships
- Querying using `EF.Property<T>()` method
- Mixed shadow and regular properties

### 7.2 Test Context and Model
- **Context**: Custom `ShadowPropertiesContext` with shadow property definitions
- **Model**:
  - `Product` with shadow properties `CreatedBy`, `CreatedAt`, `LastModified`
  - `Order` with shadow foreign key `CustomerId`
- **Elements**: Properties defined in model but not in .NET classes
- **Database**: Regular database columns mapped to shadow properties

### 7.3 Global Test Purpose
Ensures that shadow properties are correctly refreshed even though they don't exist as .NET properties. Validates that the entity framework properly manages these "invisible" properties during refresh operations.

### 7.4 Test Methods Overview
- **Basic Shadow Properties**: Tests refreshing shadow properties like audit fields
- **Mixed Property Types**: Tests refreshing entities with both regular and shadow properties
- **Shadow Foreign Keys**: Tests refreshing shadow properties used as foreign keys
- **Query Integration**: Tests using shadow properties in LINQ queries with `EF.Property<T>()`

---

## Test 8: RefreshFromDb_TableSharing_SqlServer_Test

### 8.1 EF Core Features Tested
This test focuses on **Table Sharing** - multiple entity types mapping to the same database table. Features tested:
- Multiple entities sharing table storage
- Shared non-key columns between entities
- Table Per Type (TPT) inheritance patterns
- One-to-one relationships with shared storage
- Independent entity updates on shared tables

### 8.2 Test Context and Model
- **Context**: Custom `TableSharingContext` with multiple entities per table
- **Model**:
  - `Person` and `Employee` sharing "People" table
  - `Blog` and `BlogMetadata` sharing "Blogs" table  
  - `Vehicle` and `Car` (inheritance) sharing "Vehicles" table
- **Elements**: Uses `ToTable()` to map multiple entities to same table
- **Database**: Single tables storing data for multiple entity types

### 8.3 Global Test Purpose
Validates that entities sharing table storage are correctly refreshed when shared columns change. Ensures that updates to shared data are visible to all entity types that map to the same table.

### 8.4 Test Methods Overview
- **Shared Column Updates**: Tests when shared columns (like Name) are updated externally
- **Independent Entity Updates**: Tests entity-specific columns on shared tables
- **Inheritance Scenarios**: Tests table sharing with inheritance hierarchies
- **Relationship Sharing**: Tests entities in relationships that share storage

---

## Test 9: RefreshFromDb_ValueConverters_SqlServer_Test

### 9.1 EF Core Features Tested
This test focuses on **Value Converters** - custom conversion logic between .NET types and database storage types. Features tested:
- Enum to string conversion
- Collection to JSON conversion  
- DateTime to string formatting
- GUID to string representation
- Custom value object conversion
- Built-in and custom converter patterns

### 9.2 Test Context and Model
- **Context**: Custom `ValueConvertersContext` with various converter types
- **Model**:
  - `Product` with enum-to-string and collection-to-JSON converters
  - `User` with DateTime-to-string and GUID-to-string converters
  - `Order` with custom `Money` value object converter
- **Elements**: Uses `HasConversion()` method with custom conversion logic
- **Database**: Storage types different from .NET types (strings, JSON, decimals)

### 9.3 Global Test Purpose  
Ensures that value converters work correctly during refresh operations, properly converting between database storage formats and .NET types. Validates that external changes in the database format are correctly converted back to .NET objects.

### 9.4 Test Methods Overview
- **Enum Converters**: Tests enum values stored as strings in database
- **JSON Converters**: Tests complex objects serialized as JSON
- **DateTime Converters**: Tests custom date formatting patterns
- **GUID Converters**: Tests GUID-to-string conversion patterns  
- **Value Object Converters**: Tests custom value objects with conversion logic

---

## Key Concepts for Understanding These Tests

### Entity Refresh (ReloadAsync)
The core operation being tested - refreshing an entity from the database to pick up external changes.

### External Changes Simulation
Tests use `ExecuteSqlRawAsync()` to simulate changes made outside the current EF context, representing real-world scenarios like other applications, users, or processes modifying data.

### Change Tracking Integration
All tests validate that refresh operations work correctly with EF's change tracking system, maintaining consistency between tracked entities and database state.

### Provider-Specific Testing
These are SQL Server-specific tests, ensuring that refresh functionality works with SQL Server's specific features and data types.

### Comprehensive Coverage
Together, these tests cover the full spectrum of EF Core features to ensure refresh functionality works across all scenarios that applications might encounter.

---

## Running the Tests

To run these tests:
1. Ensure SQL Server is available (LocalDB is sufficient)
2. Run `restore.cmd` to restore dependencies  
3. Run `. .\activate.ps1` to setup the development environment
4. Execute tests using `dotnet test` or Visual Studio Test Explorer
5. Tests will create temporary databases and clean up automatically

Each test is designed to be independent and can be run individually or as a suite to validate the entire refresh functionality across all EF Core features.