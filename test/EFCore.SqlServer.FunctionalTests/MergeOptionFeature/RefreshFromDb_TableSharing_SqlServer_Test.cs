// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.MergeOptionFeature;

public class RefreshFromDb_TableSharing_SqlServer_Test : IClassFixture<RefreshFromDb_TableSharing_SqlServer_Test.TableSharingFixture>
{
    private readonly TableSharingFixture _fixture;

    public RefreshFromDb_TableSharing_SqlServer_Test(TableSharingFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task Test_TableSharingWithSharedNonKeyColumns()
    {
        using var ctx = _fixture.CreateContext();

        // Get both entities that share the same table
        var person = await ctx.People.FirstAsync();
        var employee = await ctx.Employees.FirstAsync(e => e.Id == person.Id);

        var originalPersonName = person.Name;
        var originalEmployeeDepartment = employee.Department;

        try
        {
            // Simulate external change to shared non-key column
            var newName = "Updated Name";
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [People] SET [Name] = {0} WHERE [Id] = {1}",
                newName, person.Id);

            // Also update employee-specific column
            var newDepartment = "Updated Department";
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [People] SET [Department] = {0} WHERE [Id] = {1}",
                newDepartment, employee.Id);

            // Refresh both entities
            await ctx.Entry(person).ReloadAsync();
            await ctx.Entry(employee).ReloadAsync();

            // Assert that both entities see the updated shared column
            Assert.Equal(newName, person.Name);
            Assert.Equal(newName, employee.Name); // Employee inherits shared column
            Assert.Equal(newDepartment, employee.Department);
        }
        finally
        {
            // Cleanup
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [People] SET [Name] = {0}, [Department] = {1} WHERE [Id] = {2}",
                originalPersonName, originalEmployeeDepartment ?? (object)DBNull.Value, person.Id);
        }
    }

    [Fact]
    public async Task Test_TableSharing_IndependentEntityUpdates()
    {
        using var ctx = _fixture.CreateContext();

        var blog = await ctx.Blogs.FirstAsync();
        var blogMetadata = await ctx.BlogMetadata.FirstAsync(m => m.BlogId == blog.Id);

        var originalTitle = blog.Title;
        var originalMetaDescription = blogMetadata.MetaDescription;

        try
        {
            // Update blog-specific column
            var newTitle = "Updated Blog Title";
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Blogs] SET [Title] = {0} WHERE [Id] = {1}",
                newTitle, blog.Id);

            // Update metadata-specific column
            var newMetaDescription = "Updated Meta Description";
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Blogs] SET [MetaDescription] = {0} WHERE [Id] = {1}",
                newMetaDescription, blogMetadata.BlogId);

            // Refresh both entities
            await ctx.Entry(blog).ReloadAsync();
            await ctx.Entry(blogMetadata).ReloadAsync();

            // Assert changes are reflected in respective entities
            Assert.Equal(newTitle, blog.Title);
            Assert.Equal(newMetaDescription, blogMetadata.MetaDescription);
        }
        finally
        {
            // Cleanup
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Blogs] SET [Title] = {0}, [MetaDescription] = {1} WHERE [Id] = {2}",
                originalTitle, originalMetaDescription ?? (object)DBNull.Value, blog.Id);
        }
    }

    [Fact]
    public async Task Test_TableSharing_ConditionalColumns()
    {
        using var ctx = _fixture.CreateContext();

        // Get entities that share a table but have different discriminator values
        var vehicle = await ctx.Vehicles.FirstAsync();
        var car = await ctx.Cars.FirstAsync(c => c.Id == vehicle.Id);

        var originalMake = vehicle.Make;
        var originalDoors = car.NumberOfDoors;

        try
        {
            // Update shared column
            var newMake = "Updated Make";
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Vehicles] SET [Make] = {0} WHERE [Id] = {1}",
                newMake, vehicle.Id);

            // Update car-specific column
            var newDoors = 5;
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Vehicles] SET [NumberOfDoors] = {0} WHERE [Id] = {1}",
                newDoors, car.Id);

            // Refresh both entities
            await ctx.Entry(vehicle).ReloadAsync();
            await ctx.Entry(car).ReloadAsync();

            // Assert changes are reflected
            Assert.Equal(newMake, vehicle.Make);
            Assert.Equal(newMake, car.Make); // Car inherits shared property
            Assert.Equal(newDoors, car.NumberOfDoors);
        }
        finally
        {
            // Cleanup
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Vehicles] SET [Make] = {0}, [NumberOfDoors] = {1} WHERE [Id] = {2}",
                originalMake, originalDoors, vehicle.Id);
        }
    }

    public class TableSharingFixture : SharedStoreFixtureBase<TableSharingContext>
    {
        protected override string StoreName
            => "TableSharingRefreshFromDb";

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).EnableSensitiveDataLogging();

        protected override Task SeedAsync(TableSharingContext context)
        {
            // Seed Person and Employee (same table, different entity types)
            var person = new Person
            {
                Name = "John Doe",
                DateOfBirth = new DateTime(1980, 1, 1)
            };

            var employee = new Employee
            {
                Name = "Jane Smith",
                DateOfBirth = new DateTime(1985, 5, 15),
                Department = "Engineering",
                Salary = 75000
            };

            // Seed Blog and BlogMetadata (same table)
            var blog = new Blog
            {
                Title = "Tech Blog",
                Content = "This is a technology blog."
            };

            var blogMetadata = new BlogMetadata
            {
                BlogId = blog.Id,
                MetaDescription = "A blog about technology",
                Keywords = "tech, programming, software"
            };

            // Seed Vehicle and Car (TPT inheritance sharing table)
            var vehicle = new Vehicle
            {
                Make = "Generic",
                Model = "Vehicle"
            };

            var car = new Car
            {
                Make = "Toyota",
                Model = "Camry",
                NumberOfDoors = 4
            };

            context.People.Add(person);
            context.Employees.Add(employee);
            context.Blogs.Add(blog);
            context.BlogMetadata.Add(blogMetadata);
            context.Vehicles.Add(vehicle);
            context.Cars.Add(car);

            return context.SaveChangesAsync();
        }
    }

    public class TableSharingContext : DbContext
    {
        public TableSharingContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Person> People { get; set; } = null!;
        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<Blog> Blogs { get; set; } = null!;
        public DbSet<BlogMetadata> BlogMetadata { get; set; } = null!;
        public DbSet<Vehicle> Vehicles { get; set; } = null!;
        public DbSet<Car> Cars { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Person and Employee to share the same table
            modelBuilder.Entity<Person>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.ToTable("People");
                
                entity.Property(p => p.Name)
                    .HasMaxLength(100)
                    .IsRequired();
                
                entity.Property(p => p.DateOfBirth)
                    .IsRequired();
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("People"); // Share table with Person
                
                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .IsRequired();
                
                entity.Property(e => e.DateOfBirth)
                    .IsRequired();
                
                entity.Property(e => e.Department)
                    .HasMaxLength(50);
                
                entity.Property(e => e.Salary)
                    .HasColumnType("decimal(18,2)");
            });

            // Configure Blog and BlogMetadata to share the same table
            modelBuilder.Entity<Blog>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.ToTable("Blogs");
                
                entity.Property(b => b.Title)
                    .HasMaxLength(200)
                    .IsRequired();
                
                entity.Property(b => b.Content)
                    .IsRequired();
            });

            modelBuilder.Entity<BlogMetadata>(entity =>
            {
                entity.HasKey(m => m.BlogId);
                entity.ToTable("Blogs"); // Share table with Blog
                
                entity.Property(m => m.MetaDescription)
                    .HasMaxLength(500);
                
                entity.Property(m => m.Keywords)
                    .HasMaxLength(200);

                // Configure one-to-one relationship
                entity.HasOne<Blog>()
                    .WithOne()
                    .HasForeignKey<BlogMetadata>(m => m.BlogId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Vehicle hierarchy with table sharing
            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.HasKey(v => v.Id);
                entity.ToTable("Vehicles");
                
                entity.Property(v => v.Make)
                    .HasMaxLength(50)
                    .IsRequired();
                
                entity.Property(v => v.Model)
                    .HasMaxLength(50)
                    .IsRequired();
            });

            modelBuilder.Entity<Car>(entity =>
            {
                entity.HasBaseType<Vehicle>();
                entity.ToTable("Vehicles"); // Share table with base Vehicle
                
                entity.Property(c => c.NumberOfDoors)
                    .IsRequired();
            });
        }
    }

    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime DateOfBirth { get; set; }
    }

    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime DateOfBirth { get; set; }
        public string? Department { get; set; }
        public decimal? Salary { get; set; }
    }

    public class Blog
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
    }

    public class BlogMetadata
    {
        public int BlogId { get; set; }
        public string? MetaDescription { get; set; }
        public string? Keywords { get; set; }
    }

    public class Vehicle
    {
        public int Id { get; set; }
        public string Make { get; set; } = "";
        public string Model { get; set; } = "";
    }

    public class Car : Vehicle
    {
        public int NumberOfDoors { get; set; }
    }
}
