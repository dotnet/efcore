// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.MergeOptionFeature;

public class NorthwindMergeOptionFeatureContext(DbContextOptions options) : NorthwindSqlServerContext(options)
{
    // DbSets for ComplexTypes
    public DbSet<ComplexProductEntity> ComplexProducts { get; set; } = null!;
    public DbSet<ComplexCustomerEntity> ComplexCustomers { get; set; } = null!;

    // DbSets for ComputedColumns
    public DbSet<ComputedProductEntity> ComputedProducts { get; set; } = null!;
    public DbSet<ComputedOrderEntity> ComputedOrders { get; set; } = null!;

    // DbSets for GlobalFilters
    public DbSet<GlobalProductEntity> GlobalProducts { get; set; } = null!;
    public DbSet<GlobalOrderEntity> GlobalOrders { get; set; } = null!;

    // DbSets for ManyToMany
    public DbSet<StudentEntity> Students { get; set; } = null!;
    public DbSet<CourseEntity> Courses { get; set; } = null!;
    public DbSet<AuthorEntity> Authors { get; set; } = null!;
    public DbSet<BookEntity> Books { get; set; } = null!;

    // DbSets for PrimitiveCollections
    public DbSet<PrimitiveProductEntity> PrimitiveProducts { get; set; } = null!;
    public DbSet<PrimitiveBlogEntity> PrimitiveBlogs { get; set; } = null!;
    public DbSet<PrimitiveUserEntity> PrimitiveUsers { get; set; } = null!;

    // DbSets for ShadowProperties
    public DbSet<ShadowProductEntity> ShadowProducts { get; set; } = null!;
    public DbSet<ShadowCustomerEntity> ShadowCustomers { get; set; } = null!;
    public DbSet<ShadowOrderEntity> ShadowOrders { get; set; } = null!;

    // DbSets for TableSharing
    public DbSet<TablePersonEntity> TablePeople { get; set; } = null!;
    public DbSet<TableEmployeeEntity> TableEmployees { get; set; } = null!;
    public DbSet<TableBlogEntity> TableBlogs { get; set; } = null!;
    public DbSet<TableBlogMetadataEntity> TableBlogMetadata { get; set; } = null!;
    public DbSet<TableVehicleEntity> TableVehicles { get; set; } = null!;
    public DbSet<TableCarEntity> TableCars { get; set; } = null!;

    // DbSets for ValueConverters
    public DbSet<ConverterProductEntity> ConverterProducts { get; set; } = null!;
    public DbSet<ConverterUserEntity> ConverterUsers { get; set; } = null!;
    public DbSet<ConverterOrderEntity> ConverterOrders { get; set; } = null!;

    // Add other DbSets as needed for remaining tests

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurations for ComplexTypes
        modelBuilder.Entity<ComplexProductEntity>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.OwnsOne(p => p.Details, details =>
            {
                details.Property(d => d.Price)
                    .HasColumnType("decimal(18,2)");
            });
            entity.OwnsMany(p => p.Reviews, b =>
            {
                b.ToTable("ComplexProductReview");
                b.WithOwner().HasForeignKey("ComplexProductId");
                b.HasKey("ComplexProductId", "Rating", "Comment");
            });
        });

        modelBuilder.Entity<ComplexCustomerEntity>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.ComplexProperty(c => c.Contact);
            entity.OwnsMany(c => c.Addresses, b =>
            {
                b.ToTable("ComplexCustomerAddress");
                b.WithOwner().HasForeignKey("ComplexCustomerId");
                b.HasKey("ComplexCustomerId", "Street", "City");
            });
        });

        // Configurations for ComputedColumns
        modelBuilder.Entity<ComputedProductEntity>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(p => p.Price)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            entity.Property(p => p.Quantity)
                .IsRequired();

            entity.Property(p => p.TotalValue)
                .HasColumnType("decimal(18,2)")
                .HasComputedColumnSql("[Price] * [Quantity]");

            entity.Property(p => p.Description)
                .HasMaxLength(200)
                .HasComputedColumnSql("[Name] + ' - $' + CAST([Price] AS NVARCHAR(20))");
        });

        modelBuilder.Entity<ComputedOrderEntity>(entity =>
        {
            entity.HasKey(o => o.Id);

            entity.Property(o => o.OrderDate)
                .IsRequired();

            entity.Property(o => o.CustomerName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(o => o.FormattedOrderDate)
                .HasMaxLength(100)
                .HasComputedColumnSql("'Order Date: ' + CONVERT(NVARCHAR(10), [OrderDate], 120)");
        });

        // Configurations for GlobalFilters
        modelBuilder.Entity<GlobalProductEntity>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(p => p.Price)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            entity.Property(p => p.TenantId)
                .IsRequired();

            entity.HasQueryFilter(p => p.TenantId == TenantId);
        });

        modelBuilder.Entity<GlobalOrderEntity>(entity =>
        {
            entity.HasKey(o => o.Id);

            entity.Property(o => o.OrderDate)
                .IsRequired();

            entity.Property(o => o.CustomerName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(o => o.TenantId)
                .IsRequired();

            entity.Property(o => o.IsDeleted)
                .IsRequired();

            entity.HasQueryFilter(o => o.TenantId == TenantId && !o.IsDeleted);
        });

        // Configurations for ManyToMany
        modelBuilder.Entity<StudentEntity>(entity =>
        {
            entity.HasKey(s => s.Id);

            entity.Property(s => s.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(s => s.Email)
                .HasMaxLength(255)
                .IsRequired();

            entity.HasMany(s => s.Courses)
                .WithMany(c => c.Students)
                .UsingEntity(j => j.ToTable("StudentCourse"));
        });

        modelBuilder.Entity<CourseEntity>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(c => c.Credits)
                .IsRequired();
        });

        modelBuilder.Entity<AuthorEntity>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.Property(a => a.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasMany(a => a.Books)
                .WithMany(b => b.Authors)
                .UsingEntity(j => j.ToTable("AuthorBook"));
        });

        modelBuilder.Entity<BookEntity>(entity =>
        {
            entity.HasKey(b => b.Id);

            entity.Property(b => b.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(b => b.Genre)
                .HasMaxLength(50);
        });

        // Configurations for PrimitiveCollections
        modelBuilder.Entity<PrimitiveProductEntity>(entity =>
        {
            entity.HasKey(p => p.Id);
            
            entity.Property(p => p.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.PrimitiveCollection(p => p.Tags)
                .ElementType().HasMaxLength(50);
        });

        modelBuilder.Entity<PrimitiveBlogEntity>(entity =>
        {
            entity.HasKey(b => b.Id);
            
            entity.Property(b => b.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.PrimitiveCollection(b => b.Ratings);
        });

        modelBuilder.Entity<PrimitiveUserEntity>(entity =>
        {
            entity.HasKey(u => u.Id);
            
            entity.Property(u => u.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.PrimitiveCollection(u => u.RelatedIds);
        });

        // Configurations for ShadowProperties
        modelBuilder.Entity<ShadowProductEntity>(entity =>
        {
            entity.HasKey(p => p.Id);
            
            entity.Property(p => p.Name)
                .HasMaxLength(100)
                .IsRequired();
            
            entity.Property(p => p.Price)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            entity.Property<string>("CreatedBy")
                .HasMaxLength(50)
                .IsRequired();
            
            entity.Property<DateTime>("CreatedAt")
                .IsRequired();
            
            entity.Property<DateTime?>("LastModified");
        });

        modelBuilder.Entity<ShadowCustomerEntity>(entity =>
        {
            entity.HasKey(c => c.Id);
            
            entity.Property(c => c.Name)
                .HasMaxLength(100)
                .IsRequired();
            
            entity.Property(c => c.Email)
                .HasMaxLength(255)
                .IsRequired();
        });

        modelBuilder.Entity<ShadowOrderEntity>(entity =>
        {
            entity.HasKey(o => o.Id);
            
            entity.Property(o => o.OrderDate)
                .IsRequired();
            
            entity.Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            entity.Property<int>("CustomerId")
                .IsRequired();

            entity.HasOne<ShadowCustomerEntity>()
                .WithMany()
                .HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configurations for TableSharing
        modelBuilder.Entity<TablePersonEntity>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.ToTable("TablePeople");
            
            entity.Property(p => p.Name)
                .HasMaxLength(100)
                .IsRequired();
            
            entity.Property(p => p.DateOfBirth)
                .IsRequired();
        });

        modelBuilder.Entity<TableEmployeeEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("TablePeople");
            
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsRequired();
            
            entity.Property(e => e.DateOfBirth)
                .IsRequired();
            
            entity.Property(e => e.Department)
                .HasMaxLength(50);
            
            entity.Property(e => e.Salary)
                .HasColumnType("decimal(18,2)");

            entity.HasOne<TablePersonEntity>()
                .WithOne()
                .HasForeignKey<TableEmployeeEntity>(e => e.Id)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TableBlogEntity>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.ToTable("TableBlogs");
            
            entity.Property(b => b.Title)
                .HasMaxLength(200)
                .IsRequired();
            
            entity.Property(b => b.Content)
                .IsRequired();
        });

        modelBuilder.Entity<TableBlogMetadataEntity>(entity =>
        {
            entity.HasKey(m => m.BlogId);
            entity.ToTable("TableBlogs");
            
            entity.Property(m => m.MetaDescription)
                .HasMaxLength(500)
                .IsRequired();
            
            entity.Property(m => m.Keywords)
                .HasMaxLength(200);

            entity.HasOne<TableBlogEntity>()
                .WithOne()
                .HasForeignKey<TableBlogMetadataEntity>(m => m.BlogId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TableVehicleEntity>(entity =>
        {
            entity.HasKey(v => v.Id);
            entity.ToTable("TableVehicles");
            
            entity.Property(v => v.Make)
                .HasMaxLength(50)
                .IsRequired();
            
            entity.Property(v => v.Model)
                .HasMaxLength(50)
                .IsRequired();
        });

        modelBuilder.Entity<TableCarEntity>(entity =>
        {
            entity.HasBaseType<TableVehicleEntity>();
            entity.ToTable("TableVehicles");
            
            entity.Property(c => c.NumberOfDoors)
                .IsRequired();
        });

        // Configurations for ValueConverters
        modelBuilder.Entity<ConverterProductEntity>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(p => p.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(p => p.Tags)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
                .HasColumnType("nvarchar(max)")
                .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));
        });

        modelBuilder.Entity<ConverterUserEntity>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(u => u.BirthDate)
                .HasConversion(
                    v => v.ToString("yyyy-MM-dd"),
                    v => DateTime.ParseExact(v, "yyyy-MM-dd", null))
                .HasMaxLength(10);

            entity.Property(u => u.ExternalId)
                .HasConversion(
                    v => v.ToString(),
                    v => Guid.Parse(v))
                .HasMaxLength(36);
        });

        modelBuilder.Entity<ConverterOrderEntity>(entity =>
        {
            entity.HasKey(o => o.Id);

            entity.Property(o => o.OrderNumber)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(o => o.Price)
                .HasConversion(
                    v => v.Value,
                    v => new ConverterMoney(v))
                .HasColumnType("decimal(18,2)");
        });

        // Add configurations for remaining tests as needed
    }

    // Property for GlobalFilters
    public int TenantId { get; set; }

    // Entity classes for ComplexTypes
    public class ComplexProductEntity
    {
        public int Id { get; set; }
        public ComplexProductDetailsEntity Details { get; set; } = new();
        public List<ComplexReviewEntity> Reviews { get; set; } = [];
    }

    public class ComplexProductDetailsEntity
    {
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
    }

    public class ComplexReviewEntity
    {
        public int ComplexProductId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = "";
    }

    public class ComplexCustomerEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public ComplexContactInfoEntity Contact { get; set; } = new();
        public List<ComplexAddressEntity> Addresses { get; set; } = [];
    }

    public class ComplexContactInfoEntity
    {
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
    }

    public class ComplexAddressEntity
    {
        public string Street { get; set; } = "";
        public string City { get; set; } = "";
        public string PostalCode { get; set; } = "";
    }

    // Entity classes for ComputedColumns
    public class ComputedProductEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal TotalValue { get; set; }
        public string Description { get; set; } = "";
    }

    public class ComputedOrderEntity
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; } = "";
        public string FormattedOrderDate { get; set; } = "";
    }

    // Entity classes for GlobalFilters
    public class GlobalProductEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int TenantId { get; set; }
    }

    public class GlobalOrderEntity
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; } = "";
        public int TenantId { get; set; }
        public bool IsDeleted { get; set; }
    }

    // Entity classes for ManyToMany
    public class StudentEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public List<CourseEntity> Courses { get; set; } = [];
    }

    public class CourseEntity
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public int Credits { get; set; }
        public List<StudentEntity> Students { get; set; } = [];
    }

    public class AuthorEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public List<BookEntity> Books { get; set; } = [];
    }

    public class BookEntity
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? Genre { get; set; }
        public List<AuthorEntity> Authors { get; set; } = [];
    }

    // Entity classes for PrimitiveCollections
    public class PrimitiveProductEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public List<string> Tags { get; set; } = [];
    }

    public class PrimitiveBlogEntity
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public List<int> Ratings { get; set; } = [];
    }

    public class PrimitiveUserEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public List<Guid> RelatedIds { get; set; } = [];
    }

    // Entity classes for ShadowProperties
    public class ShadowProductEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
    }

    public class ShadowCustomerEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
    }

    public class ShadowOrderEntity
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
    }

    // Entity classes for TableSharing
    public class TablePersonEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime DateOfBirth { get; set; }
    }

    public class TableEmployeeEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime DateOfBirth { get; set; }
        public string? Department { get; set; }
        public decimal? Salary { get; set; }
    }

    public class TableBlogEntity
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
    }

    public class TableBlogMetadataEntity
    {
        public int BlogId { get; set; }
        public string MetaDescription { get; set; } = "";
        public string? Keywords { get; set; }
    }

    public class TableVehicleEntity
    {
        public int Id { get; set; }
        public string Make { get; set; } = "";
        public string Model { get; set; } = "";
    }

    public class TableCarEntity : TableVehicleEntity
    {
        public int NumberOfDoors { get; set; }
    }

    // Entity classes for ValueConverters
    public class ConverterProductEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public ConverterProductStatus Status { get; set; }
        public List<string> Tags { get; set; } = [];
    }

    public class ConverterUserEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime BirthDate { get; set; }
        public Guid ExternalId { get; set; }
    }

    public class ConverterOrderEntity
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = "";
        public ConverterMoney Price { get; set; } = new(0);
    }

    public enum ConverterProductStatus
    {
        Active,
        Inactive,
        Discontinued
    }

    public readonly record struct ConverterMoney(decimal Value)
    {
        public static implicit operator decimal(ConverterMoney money) => money.Value;
        public static implicit operator ConverterMoney(decimal value) => new(value);

        public override string ToString() => Value.ToString("C");
    }

    // Add entity classes for remaining tests as needed
}
