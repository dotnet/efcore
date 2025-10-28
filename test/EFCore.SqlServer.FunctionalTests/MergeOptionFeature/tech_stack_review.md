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

### 2.3 EF Core Model Definition Examples

#### **Example 1: Using Data Annotations**

Data Annotations provide a declarative way to configure your model directly on entity classes using attributes.

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Blog entity with Data Annotations
[Table("Blogs")] // Override default table name
[Index(nameof(Url), IsUnique = true)] // Create unique index on Url
public class Blog
{
    [Key] // Explicitly mark as primary key
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int BlogId { get; set; }
    
    [Required] // NOT NULL constraint
    [MaxLength(200)] // VARCHAR(200)
    [Column("BlogTitle")] // Override column name
    public string Title { get; set; } = "";
    
    [Required]
    [Url] // URL validation attribute
    [MaxLength(500)]
    public string Url { get; set; } = "";
    
    [Column(TypeName = "decimal(18,2)")] // Specify exact SQL type
    public decimal Rating { get; set; }
    
    [NotMapped] // Exclude from database
    public string DisplayName => $"{Title} ({Url})";
    
    // Navigation property for one-to-many relationship
    public List<Post> Posts { get; set; } = [];
    
    // Complex type (EF Core 8+)
    public BlogSettings Settings { get; set; } = new();
}

// Post entity with foreign key configuration
[Table("Posts")]
public class Post
{
    [Key]
    public int PostId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = "";
    
    [Column(TypeName = "ntext")] // Large text field
    public string Content { get; set; } = "";
    
    [DataType(DataType.Date)]
    public DateTime PublishedDate { get; set; }
    
    // Foreign key property
    [ForeignKey(nameof(Blog))]
    public int BlogId { get; set; }
    
    // Navigation property back to Blog
    public Blog Blog { get; set; } = null!;
    
    // Many-to-many navigation
    public List<Tag> Tags { get; set; } = [];
}

// Tag entity for many-to-many relationship
public class Tag
{
    [Key]
    public int TagId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = "";
    
    // Many-to-many navigation
    public List<Post> Posts { get; set; } = [];
}

// Complex type for blog settings
[ComplexType] // EF Core 8+ complex type
public class BlogSettings
{
    [Required]
    [MaxLength(50)]
    public string Theme { get; set; } = "Default";
    
    public bool AllowComments { get; set; } = true;
    
    [Range(1, 100)]
    public int PostsPerPage { get; set; } = 10;
    
    // Primitive collection (stored as JSON)
    public List<string> Categories { get; set; } = [];
}

// User entity with owned type
public class User
{
    [Key]
    public int UserId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = "";
    
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = "";
    
    // Owned type - will be stored in same table
    [Owned]
    public Address Address { get; set; } = new();
}

// Owned type (no separate table)
[Owned]
public class Address
{
    [MaxLength(100)]
    public string Street { get; set; } = "";
    
    [MaxLength(50)]
    public string City { get; set; } = "";
    
    [MaxLength(20)]
    public string ZipCode { get; set; } = "";
    
    [MaxLength(50)]
    public string Country { get; set; } = "";
}

// DbContext with Data Annotations approach
public class BloggingContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; } = null!;
    public DbSet<Post> Posts { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=.;Database=BloggingDB;Trusted_Connection=true;");
    }
    
    // Minimal configuration - most done via annotations
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure many-to-many relationship join table
        modelBuilder.Entity<Post>()
            .HasMany(p => p.Tags)
            .WithMany(t => t.Posts)
            .UsingEntity(j => j.ToTable("PostTags"));
    }
}
```

#### **Example 2: Using OnModelCreating Fluent API**

The Fluent API provides more comprehensive configuration options and is preferred for complex scenarios.

```csharp
// Clean entity classes without attributes
public class Blog
{
    public int BlogId { get; set; }
    public string Title { get; set; } = "";
    public string Url { get; set; } = "";
    public decimal Rating { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastUpdated { get; set; }
    
    // Shadow property will be configured in OnModelCreating
    // public string CreatedBy { get; set; } // This will be a shadow property
    
    // Navigation properties
    public List<Post> Posts { get; set; } = [];
    public BlogMetadata Metadata { get; set; } = null!; // One-to-one
    public User Owner { get; set; } = null!; // Many-to-one
    
    // Complex type
    public BlogSettings Settings { get; set; } = new();
}

public class Post
{
    public int PostId { get; set; }
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public DateTime PublishedDate { get; set; }
    public PostStatus Status { get; set; }
    
    // Foreign keys (can be shadow properties too)
    public int BlogId { get; set; }
    public int? AuthorId { get; set; }
    
    // Navigation properties
    public Blog Blog { get; set; } = null!;
    public User? Author { get; set; }
    public List<Comment> Comments { get; set; } = [];
    public List<Tag> Tags { get; set; } = [];
    
    // Primitive collections
    public List<string> Keywords { get; set; } = [];
    public List<int> ViewCounts { get; set; } = [];
}

public class Comment
{
    public int CommentId { get; set; }
    public string Content { get; set; } = "";
    public DateTime CreatedDate { get; set; }
    public bool IsApproved { get; set; }
    
    public int PostId { get; set; }
    public Post Post { get; set; } = null!;
    
    // Self-referencing relationship for reply threads
    public int? ParentCommentId { get; set; }
    public Comment? ParentComment { get; set; }
    public List<Comment> Replies { get; set; } = [];
}

public class Tag
{
    public int TagId { get; set; }
    public string Name { get; set; } = "";
    public string Color { get; set; } = "";
    public List<Post> Posts { get; set; } = [];
}

public class User
{
    public int UserId { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public UserRole Role { get; set; }
    public List<Blog> OwnedBlogs { get; set; } = [];
    public List<Post> AuthoredPosts { get; set; } = [];
    
    // Owned type
    public ContactInfo ContactInfo { get; set; } = new();
}

// One-to-one related entity
public class BlogMetadata
{
    public int BlogId { get; set; } // Same as Blog's PK
    public string Description { get; set; } = "";
    public string Keywords { get; set; } = "";
    public string SeoTitle { get; set; } = "";
    public Blog Blog { get; set; } = null!;
}

// Complex type (EF Core 8+)
public class BlogSettings
{
    public string Theme { get; set; } = "Default";
    public bool AllowComments { get; set; } = true;
    public int PostsPerPage { get; set; } = 10;
    public List<string> AllowedFileTypes { get; set; } = [];
    public NotificationSettings Notifications { get; set; } = new();
}

// Nested complex type
public class NotificationSettings
{
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = false;
    public int NotificationFrequency { get; set; } = 1; // Daily
}

// Owned type
public class ContactInfo
{
    public string Phone { get; set; } = "";
    public string Website { get; set; } = "";
    public Address Address { get; set; } = new();
    public List<SocialMediaAccount> SocialAccounts { get; set; } = [];
}

public class Address
{
    public string Street { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string ZipCode { get; set; } = "";
    public string Country { get; set; } = "";
}

public class SocialMediaAccount
{
    public string Platform { get; set; } = "";
    public string Username { get; set; } = "";
    public string Url { get; set; } = "";
}

// Enums
public enum PostStatus
{
    Draft = 0,
    Published = 1,
    Archived = 2,
    Deleted = 3
}

public enum UserRole
{
    Reader = 0,
    Author = 1,
    Editor = 2,
    Admin = 3
}

// DbContext with comprehensive Fluent API configuration
public class BloggingContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; } = null!;
    public DbSet<Post> Posts { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<BlogMetadata> BlogMetadata { get; set; } = null!;
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=.;Database=BloggingFluentDB;Trusted_Connection=true;");
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureBlogEntity(modelBuilder);
        ConfigurePostEntity(modelBuilder);
        ConfigureCommentEntity(modelBuilder);
        ConfigureTagEntity(modelBuilder);
        ConfigureUserEntity(modelBuilder);
        ConfigureBlogMetadataEntity(modelBuilder);
        ConfigureRelationships(modelBuilder);
        ConfigureIndexesAndConstraints(modelBuilder);
        ConfigureGlobalFilters(modelBuilder);
    }
    
    private void ConfigureBlogEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blog>(entity =>
        {
            // Table configuration
            entity.ToTable("Blogs");
            
            // Primary key
            entity.HasKey(b => b.BlogId);
            
            // Property configurations
            entity.Property(b => b.Title)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("BlogTitle");
            
            entity.Property(b => b.Url)
                .IsRequired()
                .HasMaxLength(500);
            
            entity.Property(b => b.Rating)
                .HasColumnType("decimal(3,2)")
                .HasDefaultValue(0.0m);
            
            entity.Property(b => b.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");
            
            entity.Property(b => b.LastUpdated)
                .IsConcurrencyToken(); // Optimistic concurrency
            
            // Shadow property
            entity.Property<string>("CreatedBy")
                .HasMaxLength(100)
                .IsRequired();
            
            // Computed column
            entity.Property<string>("FullDescription")
                .HasComputedColumnSql("[Title] + ' - ' + [Url]");
            
            // Complex type configuration (EF Core 8+)
            entity.ComplexProperty(b => b.Settings, settings =>
            {
                settings.Property(s => s.Theme)
                    .HasMaxLength(50)
                    .HasDefaultValue("Default");
                
                settings.Property(s => s.PostsPerPage)
                    .HasDefaultValue(10);
                
                // Primitive collection as JSON
                settings.Property(s => s.AllowedFileTypes)
                    .HasConversion(
                        v => string.Join(',', v),
                        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                    .HasColumnName("AllowedFileTypesJson");
                
                // Nested complex type
                settings.ComplexProperty(s => s.Notifications, notif =>
                {
                    notif.Property(n => n.EmailNotifications)
                        .HasDefaultValue(true);
                    
                    notif.Property(n => n.NotificationFrequency)
                        .HasDefaultValue(1);
                });
            });
        });
    }
    
    private void ConfigurePostEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>(entity =>
        {
            entity.ToTable("Posts");
            
            entity.HasKey(p => p.PostId);
            
            entity.Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(p => p.Content)
                .HasColumnType("ntext");
            
            entity.Property(p => p.Status)
                .HasConversion<string>() // Store enum as string
                .HasMaxLength(20);
            
            entity.Property(p => p.PublishedDate)
                .HasColumnType("date");
            
            // Primitive collections stored as JSON
            entity.Property(p => p.Keywords)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? [])
                .HasColumnType("nvarchar(max)");
            
            entity.Property(p => p.ViewCounts)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions?)null) ?? [])
                .HasColumnType("nvarchar(max)");
        });
    }
    
    private void ConfigureCommentEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.ToTable("Comments");
            
            entity.HasKey(c => c.CommentId);
            
            entity.Property(c => c.Content)
                .IsRequired()
                .HasMaxLength(1000);
            
            entity.Property(c => c.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");
            
            entity.Property(c => c.IsApproved)
                .HasDefaultValue(false);
            
            // Self-referencing relationship
            entity.HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
    
    private void ConfigureTagEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("Tags");
            
            entity.HasKey(t => t.TagId);
            
            entity.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(50);
            
            entity.Property(t => t.Color)
                .HasMaxLength(7)
                .HasDefaultValue("#000000");
        });
    }
    
    private void ConfigureUserEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            
            entity.HasKey(u => u.UserId);
            
            entity.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(u => u.Role)
                .HasConversion<int>(); // Store enum as int
            
            // Owned type configuration
            entity.OwnsOne(u => u.ContactInfo, contactInfo =>
            {
                contactInfo.Property(ci => ci.Phone)
                    .HasMaxLength(20);
                
                contactInfo.Property(ci => ci.Website)
                    .HasMaxLength(200);
                
                // Nested owned type
                contactInfo.OwnsOne(ci => ci.Address, address =>
                {
                    address.Property(a => a.Street).HasMaxLength(100);
                    address.Property(a => a.City).HasMaxLength(50);
                    address.Property(a => a.State).HasMaxLength(50);
                    address.Property(a => a.ZipCode).HasMaxLength(20);
                    address.Property(a => a.Country).HasMaxLength(50);
                });
                
                // Owned collection
                contactInfo.OwnsMany(ci => ci.SocialAccounts, socialAccount =>
                {
                    socialAccount.WithOwner();
                    socialAccount.Property(sa => sa.Platform).HasMaxLength(50);
                    socialAccount.Property(sa => sa.Username).HasMaxLength(100);
                    socialAccount.Property(sa => sa.Url).HasMaxLength(200);
                });
            });
        });
    }
    
    private void ConfigureBlogMetadataEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BlogMetadata>(entity =>
        {
            entity.ToTable("BlogMetadata");
            
            entity.HasKey(bm => bm.BlogId);
            
            entity.Property(bm => bm.Description)
                .HasMaxLength(500);
            
            entity.Property(bm => bm.Keywords)
                .HasMaxLength(200);
            
            entity.Property(bm => bm.SeoTitle)
                .HasMaxLength(100);
        });
    }
    
    private void ConfigureRelationships(ModelBuilder modelBuilder)
    {
        // Blog -> Posts (One-to-Many)
        modelBuilder.Entity<Blog>()
            .HasMany(b => b.Posts)
            .WithOne(p => p.Blog)
            .HasForeignKey(p => p.BlogId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Blog -> BlogMetadata (One-to-One)
        modelBuilder.Entity<Blog>()
            .HasOne(b => b.Metadata)
            .WithOne(bm => bm.Blog)
            .HasForeignKey<BlogMetadata>(bm => bm.BlogId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // User -> Blogs (One-to-Many)
        modelBuilder.Entity<User>()
            .HasMany(u => u.OwnedBlogs)
            .WithOne(b => b.Owner)
            .HasForeignKey("OwnerId") // Shadow property
            .OnDelete(DeleteBehavior.Restrict);
        
        // User -> Posts (One-to-Many, optional)
        modelBuilder.Entity<User>()
            .HasMany(u => u.AuthoredPosts)
            .WithOne(p => p.Author)
            .HasForeignKey(p => p.AuthorId)
            .OnDelete(DeleteBehavior.SetNull);
        
        // Post -> Comments (One-to-Many)
        modelBuilder.Entity<Post>()
            .HasMany(p => p.Comments)
            .WithOne(c => c.Post)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Post <-> Tags (Many-to-Many)
        modelBuilder.Entity<Post>()
            .HasMany(p => p.Tags)
            .WithMany(t => t.Posts)
            .UsingEntity<Dictionary<string, object>>(
                "PostTag",
                j => j.HasOne<Tag>().WithMany().HasForeignKey("TagId"),
                j => j.HasOne<Post>().WithMany().HasForeignKey("PostId"),
                j =>
                {
                    j.HasKey("PostId", "TagId");
                    j.ToTable("PostTags");
                });
    }
    
    private void ConfigureIndexesAndConstraints(ModelBuilder modelBuilder)
    {
        // Unique constraints
        modelBuilder.Entity<Blog>()
            .HasIndex(b => b.Url)
            .IsUnique()
            .HasDatabaseName("IX_Blogs_Url_Unique");
        
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
        
        modelBuilder.Entity<Tag>()
            .HasIndex(t => t.Name)
            .IsUnique();
        
        // Composite indexes
        modelBuilder.Entity<Post>()
            .HasIndex(p => new { p.BlogId, p.PublishedDate })
            .HasDatabaseName("IX_Posts_Blog_PublishedDate");
        
        modelBuilder.Entity<Comment>()
            .HasIndex(c => new { c.PostId, c.CreatedDate })
            .HasDatabaseName("IX_Comments_Post_Created");
        
        // Filtered index (SQL Server specific)
        modelBuilder.Entity<Post>()
            .HasIndex(p => p.PublishedDate)
            .HasFilter("[Status] = 'Published'")
            .HasDatabaseName("IX_Posts_PublishedDate_Published");
    }
    
    private void ConfigureGlobalFilters(ModelBuilder modelBuilder)
    {
        // Soft delete filter for posts
        modelBuilder.Entity<Post>()
            .HasQueryFilter(p => p.Status != PostStatus.Deleted);
        
        // Approved comments only filter
        modelBuilder.Entity<Comment>()
            .HasQueryFilter(c => c.IsApproved);
    }
}
```

#### **Key Differences Between Approaches**

| Aspect | Data Annotations | Fluent API (OnModelCreating) |
|--------|------------------|------------------------------|
| **Location** | On entity classes | In DbContext.OnModelCreating |
| **Readability** | Easy to see configuration with entities | Configuration separated from entities |
| **Capabilities** | Limited to common scenarios | Full EF Core configuration power |
| **Maintenance** | Can clutter entity classes | Centralized in one location |
| **Complex Scenarios** | Not suitable for advanced configurations | Handles any EF Core feature |
| **Team Preference** | Good for simple models | Preferred for complex applications |

#### **When to Use Which Approach**

**Use Data Annotations when:**
- Working with simple models
- Team prefers seeing configuration near properties
- Using basic features (required, max length, foreign keys)
- Rapid prototyping

**Use Fluent API when:**
- Building complex models with advanced relationships
- Need full control over database schema
- Configuring indexes, constraints, computed columns
- Working with shadow properties, owned types, complex types
- Team prefers separation of concerns

**Hybrid Approach (Recommended):**
- Use Data Annotations for basic property validation
- Use Fluent API for complex relationships and advanced features
- Keep entity classes clean and focused on domain logic

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