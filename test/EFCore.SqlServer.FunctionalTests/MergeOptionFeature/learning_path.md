# Entity Framework Core Learning Path Guide

This guide provides a structured learning path to understand Entity Framework Core concepts, from database fundamentals to advanced EF Core features and test contexts.

---

## Phase 1: Database Fundamentals (Prerequisites)

### 1.1 Core Database Concepts You Must Know

#### **Relational Database Basics**
- **Tables, Rows, and Columns**: Basic storage structure
- **Primary Keys**: Unique identifier for each row
- **Foreign Keys**: References to primary keys in other tables
- **Indexes**: Performance optimization structures
- **Constraints**: Rules that ensure data integrity (NOT NULL, CHECK, UNIQUE)

#### **Relationships**
- **One-to-One**: Each record in table A relates to exactly one record in table B
- **One-to-Many**: One record in table A can relate to many records in table B
- **Many-to-Many**: Records in table A can relate to many records in table B and vice versa

#### **Advanced Database Features**
- **Computed Columns**: Database-calculated values (SQL Server, PostgreSQL support this)
- **JSON Columns**: Store and query JSON data (SQL Server 2016+, PostgreSQL, MySQL 5.7+)
- **Shadow Properties**: Columns that exist in database but not in application model
- **Table Sharing**: Multiple entities mapping to the same physical table
- **Views**: Virtual tables based on queries
- **Stored Procedures and Functions**: Server-side code execution
- **Triggers**: Automatic code execution on data changes

#### **Database-Specific Features**
- **SQL Server**: Computed columns, JSON support, HierarchyId, spatial data
- **PostgreSQL**: Arrays, JSONB, custom types, advanced indexing
- **SQLite**: Limited but lightweight, good for development/testing
- **MySQL**: JSON columns, full-text search
- **Oracle**: Advanced enterprise features
- **Cosmos DB**: Document database, NoSQL concepts

### 1.2 Which Databases Support Which Features

| Feature | SQL Server | PostgreSQL | SQLite | MySQL | Oracle | Cosmos DB |
|----------|-------------|-------------|---------|--------|-----------|------------|
| **Computed Columns** | ✅ Yes (since 2005) | ✅ Yes (via generated columns, since 12) | ✅ Yes (since 3.31.0) | ✅ Yes (since 5.7) | ✅ Yes (always supported) | ⚠️ Limited (via expressions or calculated fields in containers) |
| **JSON Columns** | ✅ Since 2016 (`JSON_VALUE`, `JSON_QUERY`) | ✅ Native JSON type (since 9.2, improved in 9.4+) | ⚠️ No native type, but JSON functions since 3.9 | ✅ Native JSON type (since 5.7, JSON_TABLE in 8.0) | ✅ Since 12.1 (`IS JSON`, JSON data type) | ✅ Native JSON storage |
| **Table Sharing** | ⚙️ Supported via inheritance or views (manual) | ⚙️ Supported via table inheritance (native) | ⚙️ Manual (via views/triggers) | ⚙️ Manual (via views/triggers) | ⚙️ Supported via table partitioning or views | ⚙️ Logical containers, but not relational tables |
| **Global Filters** | 🧩 App-level (via ORM, e.g., EF Core) | 🧩 App-level (ORM feature) | 🧩 App-level (ORM feature) | 🧩 App-level (ORM feature) | 🧩 App-level (ORM feature) | 🧩 App-level (ORM feature) |
| **Complex Types** | ⚙️ Supported via JSON or `XML` | ✅ Native composite types + JSON (since 8.0+) | ⚙️ Simulated via JSON | ⚙️ Simulated via JSON | ✅ Object types (native) | ✅ Native complex types via embedded documents |
| **Primitive Collections** | ⚙️ Via JSON arrays | ✅ Native arrays (since 8.4) and JSON arrays | ⚙️ Via JSON arrays | ⚙️ Via JSON arrays | ✅ Nested tables & VARRAY (native) | ✅ Arrays and collections (native) |


## Phase 2: Entity Framework Core 9 Concepts

### 2.1 Foundational EF Core Concepts

#### **DbContext and Entity Management**
- **DbContext**: The main class for database operations
- **DbSet**: Represents a table in the database
- **Entity States**: Added, Modified, Deleted, Unchanged, Detached
- **Change Tracking**: How EF monitors entity changes
- **SaveChanges**: Persisting changes to the database

#### **Model Configuration**
- **Conventions**: Default rules EF applies to build the model
- **Data Annotations**: Attributes on classes/properties for configuration
- **Fluent API**: Code-based configuration in OnModelCreating
- **Model Builder**: The API for configuring entities and relationships

### 2.2 Entity and Property Types

#### **Regular Entities**
- **Entity Types**: Classes that map to database tables
- **Properties**: Scalar values (int, string, DateTime, etc.)
- **Navigation Properties**: References to related entities
- **Keys**: Primary and alternate keys for entity identity

#### **Complex Types (EF Core 8+)**
```csharp
public class Customer
{
    public int Id { get; set; }
    public Address Address { get; set; } // Complex type - no separate table
}

public class Address // Complex type
{
    public string Street { get; set; }
    public string City { get; set; }
}
```
- **What**: Value objects embedded within entities
- **Database**: Stored as columns in the same table or as JSON
- **Identity**: No independent identity, part of the owning entity

#### **Owned Types**
```csharp
public class Blog
{
    public int Id { get; set; }
    public BlogMetadata Metadata { get; set; } // Owned type
}

modelBuilder.Entity<Blog>().OwnsOne(b => b.Metadata);
```
- **What**: Entities that belong to another entity
- **Database**: Can be stored in same table or separate table
- **Identity**: No independent identity, always accessed through owner

#### **Primitive Collections (EF Core 8+)**
```csharp
public class Product
{
    public int Id { get; set; }
    public List<string> Tags { get; set; } // Primitive collection
    public int[] Ratings { get; set; } // Primitive collection
}
```
- **What**: Collections of primitive types (string, int, etc.)
- **Database**: Usually stored as JSON columns
- **Use Case**: Tags, categories, simple lists

### 2.3 Relationship Patterns

#### **One-to-Many**
```csharp
public class Blog { public List<Post> Posts { get; set; } }
public class Post { public Blog Blog { get; set; } }
```

#### **Many-to-Many (EF Core 5+)**
```csharp
public class Student { public List<Course> Courses { get; set; } }
public class Course { public List<Student> Students { get; set; } }
// EF automatically creates join table
```

#### **One-to-One**
```csharp
public class User { public Profile Profile { get; set; } }
public class Profile { public User User { get; set; } }
```

### 2.4 Advanced EF Core Features

#### **Shadow Properties**
- Properties that exist in the EF model but not in .NET classes
- Accessed via `context.Entry(entity).Property("PropertyName")`
- Useful for audit fields, foreign keys, etc.

#### **Global Query Filters**
```csharp
modelBuilder.Entity<Blog>()
    .HasQueryFilter(b => b.TenantId == CurrentTenantId);
```
- Automatically applied WHERE clauses
- Common for multi-tenancy and soft deletes

#### **Value Converters**
```csharp
modelBuilder.Entity<User>()
    .Property(e => e.Status)
    .HasConversion<string>(); // Enum to string
```
- Convert between .NET types and database types
- Custom serialization, enum handling, etc.

#### **Table Sharing**
```csharp
modelBuilder.Entity<Person>().ToTable("People");
modelBuilder.Entity<Employee>().ToTable("People"); // Same table
```
- Multiple entity types mapping to the same table
- Useful for different views of the same data

#### **Computed Columns**
```csharp
modelBuilder.Entity<Product>()
    .Property(p => p.TotalValue)
    .HasComputedColumnSql("[Price] * [Quantity]");
```
- Database-calculated values
- Read-only from EF perspective

### 2.5 Querying and Data Operations

#### **LINQ to Entities**
- Write LINQ queries that translate to SQL
- Deferred execution
- Include for loading related data

#### **Raw SQL**
- Execute raw SQL when LINQ isn't sufficient
- Parameterized queries for security

#### **Bulk Operations (EF Core 7+)**
- `ExecuteUpdate` and `ExecuteDelete`
- Set-based operations bypassing change tracking

### 2.6 Performance and Optimization

#### **Change Tracking Options**
- **Tracking**: Default behavior, enables SaveChanges
- **No Tracking**: Read-only queries, better performance
- **Identity Resolution**: Ensures same entity instance for same key

#### **Query Optimization**
- **Compiled Queries**: Pre-compile LINQ for repeated use
- **Split Queries**: Avoid Cartesian explosion in Include queries
- **Projection**: Select only needed data
- **Batching**: Multiple operations in single database round-trip

---

## Phase 3: EF Core Test Contexts and Their Specialties

### 3.1 Standard Test Contexts

#### **Northwind Context (`NorthwindQuerySqlServerFixture`)**
- **Purpose**: Classic business database with realistic relationships
- **Entities**: Customers, Orders, Products, Categories, Suppliers, Employees
- **Specialties**:
  - Real-world relationship patterns
  - Complex queries with multiple joins
  - Performance testing scenarios
  - Standard CRUD operations testing
- **Use Cases**: Basic functionality testing, query translation validation

#### **BlogsContext (Various Blog-related fixtures)**
- **Purpose**: Simple blogging domain for basic testing
- **Entities**: Blogs, Posts, Tags
- **Specialties**:
  - One-to-many relationships (Blog ? Posts)
  - Many-to-many relationships (Posts ? Tags)
  - Inheritance testing (different post types)
- **Use Cases**: Relationship testing, inheritance scenarios

### 3.2 Specialized Feature Test Contexts

#### **ComplexTypesContext (`RefreshFromDb_ComplexTypes_SqlServer_Test`)**
- **Purpose**: Test complex types and owned entities
- **Entities**: 
  - Product with owned ProductDetails and Reviews collection
  - Customer with complex ContactInfo and owned Addresses
- **Specialties**:
  - Complex type embedding
  - Owned type collections and single instances
  - JSON serialization of complex data
- **Database**: Custom tables with nested object storage

#### **ComputedColumnsContext (`RefreshFromDb_ComputedColumns_SqlServer_Test`)**
- **Purpose**: Test computed column functionality
- **Entities**:
  - Product with TotalValue (Price * Quantity) and Description (formatted)
  - Order with FormattedOrderDate
- **Specialties**:
  - SQL Server computed column expressions
  - Database-generated calculated values
  - Read-only property behavior

#### **GlobalFiltersContext (`RefreshFromDb_GlobalFilters_SqlServer_Test`)**
- **Purpose**: Test global query filter functionality
- **Entities**:
  - Product with TenantId filter
  - Order with TenantId and soft delete filters
- **Specialties**:
  - Multi-tenancy patterns
  - Soft delete implementation
  - Dynamic filter context changes
  - Filter bypass scenarios

#### **ManyToManyContext (`RefreshFromDb_ManyToMany_SqlServer_Test`)**
- **Purpose**: Test modern many-to-many relationships
- **Entities**:
  - Student ? Course (academic scenario)
  - Author ? Book (publishing scenario)
- **Specialties**:
  - Skip navigation properties
  - Automatic join table management
  - Bidirectional relationship updates
  - Collection navigation refresh

#### **PrimitiveCollectionsContext (`RefreshFromDb_PrimitiveCollections_SqlServer_Test`)**
- **Purpose**: Test primitive collection storage and retrieval
- **Entities**:
  - Product with List<string> Tags
  - Blog with List<int> Ratings  
  - User with List<Guid> RelatedIds
- **Specialties**:
  - JSON column mapping
  - Various primitive types (string, int, Guid)
  - Collection serialization/deserialization
  - Empty collection handling

#### **ShadowPropertiesContext (`RefreshFromDb_ShadowProperties_SqlServer_Test`)**
- **Purpose**: Test shadow property functionality
- **Entities**:
  - Product with shadow audit fields (CreatedBy, CreatedAt, LastModified)
  - Order with shadow foreign key (CustomerId)
- **Specialties**:
  - Properties not in .NET model
  - Shadow foreign key relationships
  - EF.Property<T>() usage in queries
  - Audit field patterns

#### **TableSharingContext (`RefreshFromDb_TableSharing_SqlServer_Test`)**
- **Purpose**: Test multiple entities sharing database tables
- **Entities**:
  - Person and Employee sharing "People" table
  - Blog and BlogMetadata sharing "Blogs" table
  - Vehicle and Car (inheritance) sharing "Vehicles" table
- **Specialties**:
  - Shared column scenarios
  - Independent entity updates on shared tables
  - Inheritance with table sharing
  - One-to-one relationships with shared storage

#### **ValueConvertersContext (`RefreshFromDb_ValueConverters_SqlServer_Test`)**
- **Purpose**: Test value converter functionality
- **Entities**:
  - Product with enum-to-string and collection-to-JSON converters
  - User with DateTime-to-string and Guid-to-string converters
  - Order with custom Money value object converter
- **Specialties**:
  - Built-in converter patterns (enum to string)
  - Custom conversion logic
  - JSON serialization converters
  - Value object mapping

### 3.3 Provider-Specific Test Contexts

#### **SQL Server Contexts**
- **Features**: Computed columns, HierarchyId, spatial data, JSON support
- **Specialties**: SQL Server-specific syntax and features
- **Performance**: Optimized for SQL Server query patterns

#### **SQLite Contexts** 
- **Features**: Lightweight, file-based, limited computed columns
- **Specialties**: Cross-platform testing, simple deployments
- **Limitations**: Fewer advanced features than SQL Server

#### **PostgreSQL Contexts**
- **Features**: Arrays, JSONB, advanced indexing, custom types
- **Specialties**: Open-source alternative with rich feature set

#### **Cosmos DB Contexts**
- **Features**: Document database, JSON-native, NoSQL patterns
- **Specialties**: Cloud-scale, different query patterns, partition keys

#### **InMemory Contexts**
- **Purpose**: Fast testing without real database
- **Specialties**: No database setup, limitations in functionality
- **Use Cases**: Unit testing, simple scenarios

---

## Learning Path Recommendations

### **Beginner Path** (2-3 weeks)
1. Learn basic SQL and relational concepts
2. Understand DbContext, entities, and basic CRUD
3. Practice with simple one-to-many relationships
4. Learn LINQ to Entities basics

### **Intermediate Path** (4-6 weeks)
5. Master all relationship types (1:1, 1:N, N:N)
6. Understand change tracking and entity states
7. Learn Fluent API and model configuration
8. Practice with migrations and scaffolding
9. Study the Northwind and Blog test contexts

### **Advanced Path** (6-8 weeks)
10. Master complex types and owned entities
11. Understand value converters and shadow properties
12. Learn global query filters and table sharing
13. Study primitive collections and computed columns
14. Practice with all specialized test contexts
15. Understand performance optimization techniques

### **Expert Path** (Ongoing)
16. Study EF Core source code and internals
17. Contribute to EF Core tests and features
18. Build custom database providers
19. Implement advanced scenarios like multi-tenancy
20. Performance tuning and production optimization

---

## Practical Study Approach

1. **Hands-on Practice**: Create small projects using each concept
2. **Read Tests**: Study the test contexts mentioned above to see real usage
3. **Documentation**: Use [Microsoft Learn EF Core docs](https://learn.microsoft.com/ef/core/)
4. **Source Code**: Explore the EF Core repository for advanced understanding
5. **Community**: Join EF Core discussions and GitHub issues to learn from real problems

Each phase builds upon the previous, ensuring a solid foundation before moving to advanced concepts. The test contexts provide excellent real-world examples of how these features are implemented and tested in practice.