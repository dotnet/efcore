// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract partial class MergeOptionTestBase<TFixture>(TFixture fixture) : IClassFixture<TFixture>
    where TFixture : MergeOptionTestBase<TFixture>.MergeOptionFixtureBase
{
    protected TFixture Fixture { get; } = fixture;

    protected DbContext CreateContext() => Fixture.CreateContext();

    protected abstract void UseTransaction(DbContext context, Action<DbContext> testAction);

    protected abstract Task UseTransactionAsync(DbContext context, Func<DbContext, Task> testAction);

    protected virtual void ClearLog()
    {
    }

    protected virtual void RecordLog()
    {
    }

    [ConditionalFact]
    public virtual void Can_use_Refresh_with_OverwriteChanges()
    {
        using var context = CreateContext();
        
        UseTransaction(context, ctx =>
        {
            var product = ctx.Set<Product>().First();
            
            product.Name = "Modified locally";
            Assert.Equal("Modified locally", product.Name);

            var newName = "Changed in database";
            UpdateProductNameInDatabase(ctx, product.Id, newName);

            var refreshed = ctx.Set<Product>()
                .Where(p => p.Id == product.Id)
                .Refresh(MergeOption.OverwriteChanges)
                .First();

            Assert.Same(product, refreshed);
            Assert.Equal(newName, refreshed.Name);
            Assert.Equal(newName, ctx.Entry(product).Property(p => p.Name).OriginalValue);
            Assert.Equal(EntityState.Unchanged, ctx.Entry(product).State);
        });
    }

    [ConditionalFact]
    public virtual async Task Can_use_Refresh_with_OverwriteChanges_async()
    {
        using var context = CreateContext();
        
        await UseTransactionAsync(context, async ctx =>
        {
            var product = await ctx.Set<Product>().FirstAsync();
            
            product.Name = "Modified locally";
            Assert.Equal("Modified locally", product.Name);

            var newName = "Changed in database";
            await UpdateProductNameInDatabaseAsync(ctx, product.Id, newName);

            var refreshed = await ctx.Set<Product>()
                .Where(p => p.Id == product.Id)
                .Refresh(MergeOption.OverwriteChanges)
                .FirstAsync();

            Assert.Same(product, refreshed);
            Assert.Equal(newName, refreshed.Name);
            Assert.Equal(newName, ctx.Entry(product).Property(p => p.Name).OriginalValue);
            Assert.Equal(EntityState.Unchanged, ctx.Entry(product).State);
        });
    }

    [ConditionalFact]
    public virtual void Refresh_with_PreserveChanges_keeps_local_modifications()
    {
        using var context = CreateContext();
        
        UseTransaction(context, ctx =>
        {
            var product = ctx.Set<Product>().First();
            
            product.Price = 999.99m;
            Assert.Equal(999.99m, product.Price);
            ctx.Entry(product).Property(p => p.Price).IsModified = true;

            var newPrice = 123.45m;
            UpdateProductPriceInDatabase(ctx, product.Id, newPrice);

            var refreshed = ctx.Set<Product>()
                .Where(p => p.Id == product.Id)
                .Refresh(MergeOption.PreserveChanges)
                .First();

            Assert.Same(product, refreshed);
            Assert.Equal(999.99m, refreshed.Price);
            Assert.Equal(newPrice, ctx.Entry(product).Property(p => p.Price).OriginalValue);
            Assert.Equal(EntityState.Modified, ctx.Entry(product).State);
        });
    }

    [ConditionalFact]
    public virtual void Refresh_throws_on_non_tracking_query()
    {
        using var context = CreateContext();

        Assert.Throws<InvalidOperationException>(() =>
            context.Set<Product>()
                .AsNoTracking()
                .Refresh(MergeOption.OverwriteChanges)
                .ToList());
    }

    [ConditionalFact]
    public virtual void Refresh_throws_on_multiple_merge_options()
    {
        using var context = CreateContext();

        Assert.Throws<InvalidOperationException>(() =>
            context.Set<Product>()
                .Refresh(MergeOption.OverwriteChanges)
                .Refresh(MergeOption.PreserveChanges)
                .ToList());
    }

    [ConditionalFact]
    public virtual void Refresh_works_with_ToList()
    {
        using var context = CreateContext();
        
        UseTransaction(context, ctx =>
        {
            var products = ctx.Set<Product>().ToList();
            var firstProduct = products.First();
            
            firstProduct.Name = "Modified";

            UpdateProductNameInDatabase(ctx, firstProduct.Id, "Updated");

            var refreshed = ctx.Set<Product>()
                .Refresh(MergeOption.OverwriteChanges)
                .ToList();

            Assert.Equal("Updated", firstProduct.Name);
        });
    }

    [ConditionalFact]
    public virtual void Refresh_works_with_FirstOrDefault()
    {
        using var context = CreateContext();
        
        UseTransaction(context, ctx =>
        {
            var product = ctx.Set<Product>().First();
            product.Name = "Modified";

            UpdateProductNameInDatabase(ctx, product.Id, "Updated");

            var refreshed = ctx.Set<Product>()
                .Where(p => p.Id == product.Id)
                .Refresh(MergeOption.OverwriteChanges)
                .FirstOrDefault();

            Assert.Same(product, refreshed);
            Assert.Equal("Updated", refreshed.Name);
        });
    }

    [ConditionalFact]
    public virtual void Refresh_works_with_Include()
    {
        using var context = CreateContext();
        
        UseTransaction(context, ctx =>
        {
            var order = ctx.Set<Order>()
                .Include(o => o.OrderDetails)
                .First();
            
            order.CustomerName = "Modified";

            UpdateOrderCustomerNameInDatabase(ctx, order.Id, "Updated");

            var refreshed = ctx.Set<Order>()
                .Include(o => o.OrderDetails)
                .Where(o => o.Id == order.Id)
                .Refresh(MergeOption.OverwriteChanges)
                .First();

            Assert.Same(order, refreshed);
            Assert.Equal("Updated", refreshed.CustomerName);
        });
    }

    [ConditionalFact]
    public virtual void Refresh_with_modified_property()
    {
        using var context = CreateContext();
        
        UseTransaction(context, ctx =>
        {
            var product = ctx.Set<Product>().First();
            product.Price = 100m;
            product.Quantity = 5;

            ctx.SaveChanges();

            UpdateProductInDatabase(ctx, product.Id, 200m, 10);

            var refreshed = ctx.Set<Product>()
                .Where(p => p.Id == product.Id)
                .Refresh(MergeOption.OverwriteChanges)
                .First();

            Assert.Equal(200m, refreshed.Price);
            Assert.Equal(10, refreshed.Quantity);
        });
    }

    [ConditionalFact]
    public virtual void EntityEntry_Reload_with_MergeOption_OverwriteChanges()
    {
        using var context = CreateContext();
        
        UseTransaction(context, ctx =>
        {
            var product = ctx.Set<Product>().First();
            product.Name = "Modified";

            UpdateProductNameInDatabase(ctx, product.Id, "Updated");

            ctx.Entry(product).Reload(MergeOption.OverwriteChanges);

            Assert.Equal("Updated", product.Name);
            Assert.Equal(EntityState.Unchanged, ctx.Entry(product).State);
        });
    }

    [ConditionalFact]
    public virtual async Task EntityEntry_ReloadAsync_with_MergeOption_OverwriteChanges()
    {
        using var context = CreateContext();
        
        await UseTransactionAsync(context, async ctx =>
        {
            var product = await ctx.Set<Product>().FirstAsync();
            product.Name = "Modified";

            await UpdateProductNameInDatabaseAsync(ctx, product.Id, "Updated");

            await ctx.Entry(product).ReloadAsync(MergeOption.OverwriteChanges);

            Assert.Equal("Updated", product.Name);
            Assert.Equal(EntityState.Unchanged, ctx.Entry(product).State);
        });
    }

    [ConditionalFact]
    public virtual void EntityEntry_Reload_with_MergeOption_PreserveChanges()
    {
        using var context = CreateContext();
        
        UseTransaction(context, ctx =>
        {
            var product = ctx.Set<Product>().First();
            
            product.Price = 999.99m;

            UpdateProductPriceInDatabase(ctx, product.Id, 123.45m);

            ctx.Entry(product).Reload(MergeOption.PreserveChanges);

            Assert.Equal(999.99m, product.Price);
            Assert.Equal(123.45m, ctx.Entry(product).Property(p => p.Price).OriginalValue);
            Assert.Equal(EntityState.Modified, ctx.Entry(product).State);
        });
    }

    [ConditionalFact]
    public virtual void Refresh_many_to_many_relationship()
    {
        using var context = CreateContext();
        
        UseTransaction(context, ctx =>
        {
            var student = ctx.Set<Student>().Include(s => s.Courses).First();
            var originalCourseCount = student.Courses.Count;
            
            var courseToAdd = ctx.Set<Course>().First(c => !student.Courses.Contains(c));
            
            AddStudentCourseInDatabase(ctx, student.Id, courseToAdd.Id);

            var coll = ctx.Entry(student).Collection(s => s.Courses);
            coll.IsLoaded = false;
            coll.Load();

            Assert.Equal(originalCourseCount + 1, student.Courses.Count);
            Assert.Contains(student.Courses, c => c.Id == courseToAdd.Id);
        });
    }

    [ConditionalFact]
    public virtual async Task Refresh_many_to_many_relationship_async()
    {
        using var context = CreateContext();
        
        await UseTransactionAsync(context, async ctx =>
        {
            var student = await ctx.Set<Student>().Include(s => s.Courses).FirstAsync();
            var originalCourseCount = student.Courses.Count();
            
            var courseToAdd = await ctx.Set<Course>().FirstAsync(c => !student.Courses.Contains(c));
            
            await AddStudentCourseInDatabaseAsync(ctx, student.Id, courseToAdd.Id);

            var coll = ctx.Entry(student).Collection(s => s.Courses);
            coll.IsLoaded = false;
            await coll.LoadAsync();

            Assert.Equal(originalCourseCount + 1, student.Courses.Count);
            Assert.Contains(student.Courses, c => c.Id == courseToAdd.Id);
        });
    }

    [ConditionalFact]
    public virtual void Refresh_with_shadow_property()
    {
        using var context = CreateContext();
        
        UseTransaction(context, ctx =>
        {
            var book = ctx.Set<Book>().First();
            var originalPublisher = ctx.Entry(book).Property("Publisher").CurrentValue;
            
            var newPublisher = "Updated Publisher";
            UpdateBookPublisherInDatabase(ctx, book.Id, newPublisher);

            var refreshed = ctx.Set<Book>()
                .Where(b => b.Id == book.Id)
                .Refresh(MergeOption.OverwriteChanges)
                .First();

            Assert.Same(book, refreshed);
            Assert.Equal(newPublisher, ctx.Entry(book).Property("Publisher").CurrentValue);
        });
    }

    [ConditionalFact]
    public virtual void Refresh_respects_global_query_filter()
    {
        using var context = CreateContext();
        
        UseTransaction(context, ctx =>
        {
            var activeCategories = ctx.Set<Category>().ToList();
            
            Assert.All(activeCategories, c => Assert.True(c.IsActive));
            Assert.DoesNotContain(activeCategories, c => c.Id == 2);
        });
    }

    [ConditionalFact]
    public virtual void Refresh_with_primitive_collection()
    {
        using var context = CreateContext();
        
        UseTransaction(context, ctx =>
        {
            var product = ctx.Set<Product>().First();
            var originalTags = product.Tags.ToList();
            
            var newTags = new List<string> { "newTag1", "newTag2", "newTag3" };
            UpdateProductTagsInDatabase(ctx, product.Id, newTags);

            var refreshed = ctx.Set<Product>()
                .Where(p => p.Id == product.Id)
                .Refresh(MergeOption.OverwriteChanges)
                .First();

            Assert.Equal(3, refreshed.Tags.Count);
            Assert.Contains("newTag1", refreshed.Tags);
            Assert.Contains("newTag2", refreshed.Tags);
        });
    }

    [ConditionalFact]
    public virtual void Refresh_with_enum_value_converter()
    {
        using var context = CreateContext();
        
        UseTransaction(context, ctx =>
        {
            var product = ctx.Set<Product>().First();
            product.Status = ProductStatus.Active;
            
            UpdateProductStatusInDatabase(ctx, product.Id, ProductStatus.Discontinued);

            var refreshed = ctx.Set<Product>()
                .Where(p => p.Id == product.Id)
                .Refresh(MergeOption.OverwriteChanges)
                .First();

            Assert.Equal(ProductStatus.Discontinued, refreshed.Status);
        });
    }

    [ConditionalFact]
    public virtual void Refresh_entity_in_different_states()
    {
        using var context = CreateContext();
        
        UseTransaction(context, ctx =>
        {
            var unchangedProduct = ctx.Set<Product>().OrderBy(p => p.Id).First();
            var modifiedProduct = ctx.Set<Product>().OrderBy(p => p.Id).Skip(1).First();
            modifiedProduct.Name = "Modified Name";
            
            var newProduct = new Product { Id = 999, Name = "New Product", Price = 99.99m, Quantity = 10, Status = ProductStatus.Active, Tags = [] };
            ctx.Add(newProduct);

            Assert.Equal(EntityState.Unchanged, ctx.Entry(unchangedProduct).State);
            Assert.Equal(EntityState.Modified, ctx.Entry(modifiedProduct).State);
            Assert.Equal(EntityState.Added, ctx.Entry(newProduct).State);

            UpdateProductNameInDatabase(ctx, unchangedProduct.Id, "DB Updated");

            var refreshed = ctx.Set<Product>()
                .Refresh(MergeOption.OverwriteChanges)
                .ToList();

            Assert.Equal("DB Updated", unchangedProduct.Name);
            Assert.Equal(EntityState.Unchanged, ctx.Entry(unchangedProduct).State);
        });
    }

    [ConditionalFact]
    public virtual void Refresh_with_ThenInclude()
    {
        using var context = CreateContext();
        
        UseTransaction(context, ctx =>
        {
            var student = ctx.Set<Student>()
                .Include(s => s.Courses)
                .ThenInclude(c => c.Students)
                .First();
            
            student.Name = "Modified Name";

            UpdateStudentNameInDatabase(ctx, student.Id, "Updated Name");

            var refreshed = ctx.Set<Student>()
                .Include(s => s.Courses)
                .ThenInclude(c => c.Students)
                .Where(s => s.Id == student.Id)
                .Refresh(MergeOption.OverwriteChanges)
                .First();

            Assert.Same(student, refreshed);
            Assert.Equal("Updated Name", refreshed.Name);
            Assert.NotEmpty(refreshed.Courses);
        });
    }

    [ConditionalFact]
    public virtual void Refresh_PreserveChanges_with_unchanged_entity()
    {
        using var context = CreateContext();
        
        UseTransaction(context, ctx =>
        {
            var product = ctx.Set<Product>().First();
            var originalName = product.Name;
            
            Assert.Equal(EntityState.Unchanged, ctx.Entry(product).State);

            UpdateProductNameInDatabase(ctx, product.Id, "DB Modified");

            var refreshed = ctx.Set<Product>()
                .Where(p => p.Id == product.Id)
                .Refresh(MergeOption.PreserveChanges)
                .First();

            Assert.Equal("DB Modified", refreshed.Name);
            Assert.Equal(EntityState.Unchanged, ctx.Entry(product).State);
        });
    }

    [ConditionalFact]
    public virtual void Refresh_PreserveChanges_modified_property_not_overwritten()
    {
        using var context = CreateContext();
        
        UseTransaction(context, ctx =>
        {
            var product = ctx.Set<Product>().First();
            var originalPrice = product.Price;
            
            product.Price = 999.99m;
            product.Name = "Modified Name";
            ctx.Entry(product).Property(p => p.Price).IsModified = true;
            ctx.Entry(product).Property(p => p.Name).IsModified = true;
            
            var newPrice = 123.45m;
            var newName = "DB Name";
            UpdateProductInDatabase(ctx, product.Id, newPrice, product.Quantity);
            UpdateProductNameInDatabase(ctx, product.Id, newName);

            var refreshed = ctx.Set<Product>()
                .Where(p => p.Id == product.Id)
                .Refresh(MergeOption.PreserveChanges)
                .First();

            Assert.Equal(999.99m, refreshed.Price);
            Assert.Equal("Modified Name", refreshed.Name);
            Assert.Equal(newPrice, ctx.Entry(product).Property(p => p.Price).OriginalValue);
            Assert.Equal(newName, ctx.Entry(product).Property(p => p.Name).OriginalValue);
            Assert.Equal(EntityState.Modified, ctx.Entry(product).State);
        });
    }

    [ConditionalFact]
    public virtual void Refresh_unchanged_with_mismatched_original_value()
    {
        using var context = CreateContext();
        
        UseTransaction(context, ctx =>
        {
            var product = ctx.Set<Product>().First();
            var currentName = product.Name;
            
            Assert.Equal(EntityState.Unchanged, ctx.Entry(product).State);
            
            ctx.Entry(product).Property(p => p.Name).OriginalValue = "Different Original";
            ctx.Entry(product).Property(p => p.Name).IsModified = false;

            UpdateProductNameInDatabase(ctx, product.Id, "DB Updated Name");

            var refreshed = ctx.Set<Product>()
                .Where(p => p.Id == product.Id)
                .Refresh(MergeOption.PreserveChanges)
                .First();

            Assert.Equal("DB Updated Name", refreshed.Name);
            Assert.Equal("DB Updated Name", ctx.Entry(product).Property(p => p.Name).OriginalValue);
            Assert.Equal(EntityState.Unchanged, ctx.Entry(product).State);
        });
    }

    [ConditionalFact]
    public virtual void Refresh_modified_with_matching_original_value()
    {
        using var context = CreateContext();
        
        UseTransaction(context, ctx =>
        {
            var product = ctx.Set<Product>().First();
            var dbValue = product.Price;
            
            product.Price = 500.00m;
            Assert.Equal(EntityState.Modified, ctx.Entry(product).State);
            
            var originalValueInDb = ctx.Entry(product).Property(p => p.Price).OriginalValue;
            Assert.Equal(dbValue, originalValueInDb);

            var refreshed = ctx.Set<Product>()
                .Where(p => p.Id == product.Id)
                .Refresh(MergeOption.PreserveChanges)
                .First();

            Assert.Equal(500.00m, refreshed.Price);
            Assert.Equal(dbValue, ctx.Entry(product).Property(p => p.Price).OriginalValue);
            Assert.Equal(EntityState.Modified, ctx.Entry(product).State);
        });
    }

    [ConditionalFact]
    public virtual void Refresh_with_owned_entity()
    {
        using var context = CreateContext();
        
        UseTransaction(context, ctx =>
        {
            var order = ctx.Set<Order>().First();
            var originalShippingCity = order.ShippingAddress.City;
            
            order.ShippingAddress.City = "Modified City";

            UpdateOrderShippingCityInDatabase(ctx, order.Id, "DB City");

            var refreshed = ctx.Set<Order>()
                .Where(o => o.Id == order.Id)
                .Refresh(MergeOption.OverwriteChanges)
                .First();

            Assert.Equal("DB City", refreshed.ShippingAddress.City);
        });
    }

    [ConditionalFact]
    public virtual void Refresh_with_tph_inheritance()
    {
        using var context = CreateContext();
        
        UseTransaction(context, ctx =>
        {
            var premiumProduct = ctx.Set<PremiumProduct>().First();
            var originalRewardPoints = premiumProduct.RewardPoints;
            
            premiumProduct.RewardPoints = 9999;

            UpdatePremiumProductRewardPointsInDatabase(ctx, premiumProduct.Id, 5000);

            var refreshed = ctx.Set<PremiumProduct>()
                .Where(p => p.Id == premiumProduct.Id)
                .Refresh(MergeOption.OverwriteChanges)
                .First();

            Assert.Equal(5000, refreshed.RewardPoints);
            Assert.Equal(EntityState.Unchanged, ctx.Entry(refreshed).State);
        });
    }

    [ConditionalFact]
    public virtual async Task Refresh_with_streaming_query()
    {
        using var context = CreateContext();
        
        await UseTransactionAsync(context, async ctx =>
        {
            var count = 0;
            await foreach (var product in ctx.Set<Product>()
                .Refresh(MergeOption.AppendOnly)
                .AsAsyncEnumerable())
            {
                Assert.NotNull(product.Name);
                count++;
                if (count >= 2)
                    break;
            }

            Assert.True(count >= 2);
        });
    }

    [ConditionalFact]
    public virtual void Refresh_same_entity_projected_multiple_times()
    {
        using var context = CreateContext();
        
        UseTransaction(context, ctx =>
        {
            var result = ctx.Set<Product>()
                .Select(p => new { First = p, Second = p })
                .Refresh(MergeOption.AppendOnly)
                .First();

            Assert.Same(result.First, result.Second);
        });
    }

    protected abstract void AddStudentCourseInDatabase(DbContext context, int studentId, int courseId);
    protected abstract Task AddStudentCourseInDatabaseAsync(DbContext context, int studentId, int courseId);
    protected abstract void UpdateBookPublisherInDatabase(DbContext context, int bookId, string newPublisher);
    protected abstract void UpdateProductTagsInDatabase(DbContext context, int productId, List<string> newTags);
    protected abstract void UpdateProductStatusInDatabase(DbContext context, int productId, ProductStatus newStatus);
    protected abstract void UpdateStudentNameInDatabase(DbContext context, int studentId, string newName);
    protected abstract void UpdateOrderShippingCityInDatabase(DbContext context, int orderId, string newCity);
    protected abstract void UpdatePremiumProductRewardPointsInDatabase(DbContext context, int productId, int newRewardPoints);

    protected abstract void UpdateProductNameInDatabase(DbContext context, int id, string newName);
    protected abstract Task UpdateProductNameInDatabaseAsync(DbContext context, int id, string newName);
    protected abstract void UpdateProductPriceInDatabase(DbContext context, int id, decimal newPrice);
    protected abstract void UpdateOrderCustomerNameInDatabase(DbContext context, int id, string newName);
    protected abstract void UpdateProductInDatabase(DbContext context, int id, decimal newPrice, int newQuantity);

    public abstract class MergeOptionFixtureBase : SharedStoreFixtureBase<PoolableDbContext>
    {
        protected override string StoreName => "MergeOptionTest";

        protected override bool RecreateStore
            => true;

        protected override Type ContextType { get; } = typeof(MergeOptionContext);

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<Product>(b =>
            {
                b.Property(p => p.Id).ValueGeneratedNever();
                b.Property(p => p.Name).IsRequired();
            });

            modelBuilder.Entity<Order>(b =>
            {
                b.Property(o => o.Id).ValueGeneratedNever();
                b.HasMany(o => o.OrderDetails).WithOne(od => od.Order).HasForeignKey(od => od.OrderId);
                b.OwnsOne(o => o.ShippingAddress, a =>
                {
                    a.Property(addr => addr.City).IsRequired();
                });
            });

            modelBuilder.Entity<OrderDetail>(b =>
            {
                b.Property(od => od.Id).ValueGeneratedNever();
            });

            modelBuilder.Entity<Student>(b =>
            {
                b.Property(s => s.Id).ValueGeneratedNever();
                b.HasMany(s => s.Courses).WithMany(c => c.Students);
            });

            modelBuilder.Entity<Course>(b =>
            {
                b.Property(c => c.Id).ValueGeneratedNever();
            });

            modelBuilder.Entity<Book>(b =>
            {
                b.Property(bk => bk.Id).ValueGeneratedNever();
                b.Property<DateTime>("CreatedDate");
                b.Property<string>("Publisher").HasMaxLength(100);
            });

            modelBuilder.Entity<Category>(b =>
            {
                b.Property(c => c.Id).ValueGeneratedNever();
                b.HasQueryFilter(c => c.IsActive);
            });

            modelBuilder.Entity<PremiumProduct>(b =>
            {
                b.HasBaseType<Product>();
            });
        }

        protected override async Task SeedAsync(PoolableDbContext context)
        {
            await context.Database.EnsureCreatedResilientlyAsync();

            var product1 = new Product { Id = 1, Name = "Product 1", Price = 10.99m, Quantity = 100, Status = ProductStatus.Active, Tags = ["tag1", "tag2"] };
            var product2 = new Product { Id = 2, Name = "Product 2", Price = 20.99m, Quantity = 50, Status = ProductStatus.Active, Tags = ["tag2", "tag3"] };
            var product3 = new Product { Id = 3, Name = "Product 3", Price = 30.99m, Quantity = 25, Status = ProductStatus.Inactive, Tags = ["tag3"] };
            var premiumProduct1 = new PremiumProduct { Id = 4, Name = "Premium Product 1", Price = 99.99m, Quantity = 10, Status = ProductStatus.Active, Tags = ["premium"], RewardPoints = 1000 };

            var order1 = new Order 
            { 
                Id = 1, 
                CustomerName = "Customer 1",
                ShippingAddress = new Address { Street = "123 Main St", City = "City1", PostalCode = "12345" },
                OrderDetails = new List<OrderDetail>
                {
                    new() { Id = 1, ProductId = 1, Quantity = 2 },
                    new() { Id = 2, ProductId = 2, Quantity = 1 }
                }
            };

            var order2 = new Order 
            { 
                Id = 2, 
                CustomerName = "Customer 2",
                ShippingAddress = new Address { Street = "456 Oak Ave", City = "City2", PostalCode = "67890" },
                OrderDetails = new List<OrderDetail>
                {
                    new() { Id = 3, ProductId = 3, Quantity = 3 }
                }
            };

            var course1 = new Course { Id = 1, Name = "Math", Description = "Mathematics" };
            var course2 = new Course { Id = 2, Name = "Science", Description = "Natural Sciences" };
            var course3 = new Course { Id = 3, Name = "History", Description = "World History" };

            var student1 = new Student { Id = 1, Name = "John", Email = "john@test.com", Courses = [course1, course2] };
            var student2 = new Student { Id = 2, Name = "Jane", Email = "jane@test.com", Courses = [course2, course3] };

            var book1 = new Book { Id = 1, Title = "Book 1" };
            var book2 = new Book { Id = 2, Title = "Book 2" };

            var category1 = new Category { Id = 1, Name = "Active Category", IsActive = true };
            var category2 = new Category { Id = 2, Name = "Inactive Category", IsActive = false };

            context.AddRange(product1, product2, product3, premiumProduct1, order1, order2);
            context.AddRange(student1, student2, course3);
            context.AddRange(book1, book2);
            context.AddRange(category1, category2);

            // Set shadow properties for books
            context.Entry(book1).Property("CreatedDate").CurrentValue = DateTime.UtcNow.AddDays(-30);
            context.Entry(book1).Property("Publisher").CurrentValue = "Publisher A";
            context.Entry(book2).Property("CreatedDate").CurrentValue = DateTime.UtcNow.AddDays(-15);
            context.Entry(book2).Property("Publisher").CurrentValue = "Publisher B";

            await context.SaveChangesAsync();
        }
    }

    protected class MergeOptionContext(DbContextOptions options) : PoolableDbContext(options);

    protected class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public List<string> Tags { get; set; }
        public ProductStatus Status { get; set; }
    }

    protected class PremiumProduct : Product
    {
        public int RewardPoints { get; set; }
    }

    protected enum ProductStatus
    {
        Active,
        Inactive,
        Discontinued
    }

    protected class Order
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
        public Address ShippingAddress { get; set; }
    }

    protected class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
    }

    protected class OrderDetail
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    protected class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public List<Course> Courses { get; set; }
    }

    protected class Course
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Student> Students { get; set; }
    }

    protected class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }

    }

    protected class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }
}
