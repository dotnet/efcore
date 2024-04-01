// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class AdHocAdvancedMappingsQueryTestBase : NonSharedModelTestBase
{
    protected override string StoreName
        => "AdHocAdvancedMappingsQueryTests";

    #region 9582

    [ConditionalFact]
    public virtual async Task Setting_IsUnicode_generates_unicode_literal_in_SQL()
    {
        var contextFactory = await InitializeAsync<Context9582>();
        using var context = contextFactory.CreateContext();
        var query = context.Set<Context9582.TipoServicio>().Where(xx => xx.Nombre.Contains("lla")).ToList();
    }

    private class Context9582(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TipoServicio>(
                builder =>
                {
                    builder.HasKey(ts => ts.Id);

                    builder.Property(ts => ts.Id).IsRequired();
                    builder.Property(ts => ts.Nombre).IsRequired().HasMaxLength(20);
                });

            foreach (var property in modelBuilder.Model.GetEntityTypes()
                         .SelectMany(e => e.GetProperties().Where(p => p.ClrType == typeof(string))))
            {
                property.SetIsUnicode(false);
            }
        }

        public class TipoServicio
        {
            public int Id { get; set; }
            public string Nombre { get; set; }
        }
    }

    #endregion

    #region 11835

    [ConditionalFact]
    public virtual async Task Projecting_correlated_collection_along_with_non_mapped_property()
    {
        var contextFactory = await InitializeAsync<Context11835>(seed: c => c.SeedAsync());
        using (var context = contextFactory.CreateContext())
        {
            var result = context.Blogs.Select(
                e => new
                {
                    e.Id,
                    e.Title,
                    FirstPostName = e.Posts.Where(i => i.Name.Contains("2")).ToList()
                }).ToList();
        }

        using (var context = contextFactory.CreateContext())
        {
            var result = context.Blogs.Select(
                e => new
                {
                    e.Id,
                    e.Title,
                    FirstPostName = e.Posts.OrderBy(i => i.Id).FirstOrDefault().Name
                }).ToList();
        }
    }

    private class Context11835(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }

        public Task SeedAsync()
        {
            var b1 = new Blog { Title = "B1" };
            var b2 = new Blog { Title = "B2" };
            var p11 = new Post { Name = "P11", Blog = b1 };
            var p12 = new Post { Name = "P12", Blog = b1 };
            var p13 = new Post { Name = "P13", Blog = b1 };
            var p21 = new Post { Name = "P21", Blog = b2 };
            var p22 = new Post { Name = "P22", Blog = b2 };

            Blogs.AddRange(b1, b2);
            Posts.AddRange(p11, p12, p13, p21, p22);
            return SaveChangesAsync();
        }

        public class Blog
        {
            public int Id { get; set; }

            [NotMapped]
            public string Title { get; set; }

            public List<Post> Posts { get; set; }
        }

        public class Post
        {
            public int Id { get; set; }
            public int BlogId { get; set; }
            public Blog Blog { get; set; }
            public string Name { get; set; }
        }
    }

    #endregion

    #region 15684

    [ConditionalFact]
    public virtual async Task Projection_failing_with_EnumToStringConverter()
    {
        var contextFactory = await InitializeAsync<Context15684>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();
        var query = from p in context.Products
                    join c in context.Categories on p.CategoryId equals c.Id into grouping
                    from c in grouping.DefaultIfEmpty()
                    select new Context15684.ProductDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        CategoryName = c == null ? "Other" : c.Name,
                        CategoryStatus = c == null ? Context15684.CategoryStatus.Active : c.Status
                    };
        var result = query.ToList();
        Assert.Equal(2, result.Count);
    }

    private class Context15684(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder
                .Entity<Category>()
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<CategoryStatus>());

        public Task SeedAsync()
        {
            Products.Add(
                new Product { Name = "Apple", Category = new Category { Name = "Fruit", Status = CategoryStatus.Active } });

            Products.Add(new Product { Name = "Bike" });

            return SaveChangesAsync();
        }

        public class Product
        {
            [Key]
            public int Id { get; set; }

            [Required]
            public string Name { get; set; }

            public int? CategoryId { get; set; }

            public Category Category { get; set; }
        }

        public class Category
        {
            [Key]
            public int Id { get; set; }

            [Required]
            public string Name { get; set; }

            public CategoryStatus Status { get; set; }
        }

        public class ProductDto
        {
            public string CategoryName { get; set; }
            public CategoryStatus CategoryStatus { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public enum CategoryStatus
        {
            Active = 0,
            Removed = 1
        }
    }

    #endregion

    #region 17276

    [ConditionalFact]
    public virtual async Task Expression_tree_constructed_via_interface_works()
    {
        var contextFactory = await InitializeAsync<Context17276>();
        using (var context = contextFactory.CreateContext())
        {
            var query = Context17276.List(context.RemovableEntities);
        }

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Parents
                .Where(p => EF.Property<bool>(EF.Property<Context17276.IRemovable>(p, "RemovableEntity"), "IsRemoved"))
                .ToList();
        }

        using (var context = contextFactory.CreateContext())
        {
            var query = context.RemovableEntities
                .Where(p => EF.Property<string>(EF.Property<Context17276.IOwned>(p, "OwnedEntity"), "OwnedValue") == "Abc")
                .ToList();
        }

        using (var context = contextFactory.CreateContext())
        {
            var specification = new Context17276.Specification<Context17276.Parent>(1);
            var entities = context.Set<Context17276.Parent>().Where(specification.Criteria).ToList();
        }
    }

    private class Context17276(DbContextOptions options) : DbContext(options)
    {
        public DbSet<RemovableEntity> RemovableEntities { get; set; }
        public DbSet<Parent> Parents { get; set; }

        public static List<T> List<T>(IQueryable<T> query)
            where T : IRemovable
            => query.Where(x => !x.IsRemoved).ToList();

        public interface IRemovable
        {
            bool IsRemoved { get; set; }

            string RemovedByUser { get; set; }

            DateTime? Removed { get; set; }
        }

        public class RemovableEntity : IRemovable
        {
            public int Id { get; set; }
            public bool IsRemoved { get; set; }
            public string RemovedByUser { get; set; }
            public DateTime? Removed { get; set; }
            public OwnedEntity OwnedEntity { get; set; }
        }

        public class Parent : IHasId<int>
        {
            public int Id { get; set; }
            public RemovableEntity RemovableEntity { get; set; }
        }

        [Owned]
        public class OwnedEntity : IOwned
        {
            public string OwnedValue { get; set; }
            public int Exists { get; set; }
        }

        public interface IHasId<out T>
        {
            T Id { get; }
        }

        public interface IOwned
        {
            string OwnedValue { get; }
            int Exists { get; }
        }

        public class Specification<T>(int id)
            where T : IHasId<int>
        {
            public Expression<Func<T, bool>> Criteria { get; } = t => t.Id == id;
        }
    }

    #endregion

    #region 17794

    [ConditionalFact]
    public virtual async Task Double_convert_interface_created_expression_tree()
    {
        var contextFactory = await InitializeAsync<Context17794>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();
        var expression = Context17794.HasAction17794<Context17794.Offer>(Context17794.Actions.Accepted);
        var query = context.Offers.Where(expression).Count();

        Assert.Equal(1, query);
    }

    private class Context17794(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Offer> Offers { get; set; }
        public DbSet<OfferAction> OfferActions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }

        public Task SeedAsync()
        {
            Add(new Offer { OfferActions = new List<OfferAction> { new() { Action = Actions.Accepted } } });
            return SaveChangesAsync();
        }

        public static Expression<Func<T, bool>> HasAction17794<T>(Actions action)
            where T : IOffer
        {
            Expression<Func<OfferAction, bool>> predicate = oa => oa.Action == action;

            return v => v.OfferActions.AsQueryable().Any(predicate);
        }

        public interface IOffer
        {
            ICollection<OfferAction> OfferActions { get; set; }
        }

        public class Offer : IOffer
        {
            public int Id { get; set; }

            public ICollection<OfferAction> OfferActions { get; set; }
        }

        public enum Actions
        {
            Accepted = 1,
            Declined = 2
        }

        public class OfferAction
        {
            public int Id { get; set; }

            [Required]
            public Offer Offer { get; set; }

            public int OfferId { get; set; }

            [Required]
            public Actions Action { get; set; }
        }
    }

    #endregion

    #region 18087

    [ConditionalFact]
    public virtual async Task Casts_are_removed_from_expression_tree_when_redundant()
    {
        var contextFactory = await InitializeAsync<Context18087>(seed: c => c.SeedAsync());

        using (var context = contextFactory.CreateContext())
        {
            var queryBase = (IQueryable)context.MockEntities;
            var id = 1;
            var query = queryBase.Cast<Context18087.IDomainEntity>().FirstOrDefault(x => x.Id == id);

            Assert.Equal(1, query.Id);
        }

        using (var context = contextFactory.CreateContext())
        {
            var queryBase = (IQueryable)context.MockEntities;
            var query = queryBase.Cast<object>().Count();

            Assert.Equal(3, query);
        }

        using (var context = contextFactory.CreateContext())
        {
            var queryBase = (IQueryable)context.MockEntities;
            var id = 1;

            var message = Assert.Throws<InvalidOperationException>(
                () => queryBase.Cast<Context18087.IDummyEntity>().FirstOrDefault(x => x.Id == id)).Message;

            Assert.Equal(
                CoreStrings.TranslationFailed(
                    @"DbSet<MockEntity>()    .Cast<IDummyEntity>()    .Where(e => e.Id == __id_0)"),
                message.Replace("\r", "").Replace("\n", ""));
        }
    }

    private class Context18087(DbContextOptions options) : DbContext(options)
    {
        public DbSet<MockEntity> MockEntities { get; set; }

        public Task SeedAsync()
        {
            AddRange(
                new MockEntity { Name = "Entity1", NavigationEntity = null },
                new MockEntity { Name = "Entity2", NavigationEntity = null },
                new MockEntity { Name = "NewEntity", NavigationEntity = null });

            return SaveChangesAsync();
        }

        public interface IDomainEntity
        {
            int Id { get; set; }
        }

        public interface IDummyEntity
        {
            int Id { get; set; }
        }

        public class MockEntity : IDomainEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public MockEntity NavigationEntity { get; set; }
        }
    }

    #endregion

    #region 18346

    [ConditionalFact]
    public virtual async Task Can_query_hierarchy_with_non_nullable_property_on_derived()
    {
        var contextFactory = await InitializeAsync<Context18346>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();
        var query = context.Businesses.ToList();
        Assert.Equal(3, query.Count);
    }

    private class Context18346(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Business> Businesses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Business>()
                .HasDiscriminator(x => x.Type)
                .HasValue<Shop>(BusinessType.Shop)
                .HasValue<Brand>(BusinessType.Brand);

        public Task SeedAsync()
        {
            var shop1 = new Shop { IsOnline = true, Name = "Amzn" };
            var shop2 = new Shop { IsOnline = false, Name = "Mom and Pop's Shoppe" };
            var brand = new Brand { Name = "Tsla" };
            Businesses.AddRange(shop1, shop2, brand);
            return SaveChangesAsync();
        }

        public abstract class Business
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public BusinessType Type { get; set; }
        }

        public class Shop : Business
        {
            public bool IsOnline { get; set; }
        }

        public class Brand : Business;

        public enum BusinessType
        {
            Shop,
            Brand,
        }
    }

    #endregion

    #region 26742

    [ConditionalTheory]
    [InlineData(null, "")]
    //[InlineData(0, " (Scale = 0)")] //https://github.com/dotnet/SqlClient/issues/1380 cause this test to fail, not EF
    [InlineData(1, " (Scale = 1)")]
    [InlineData(2, " (Scale = 2)")]
    [InlineData(3, " (Scale = 3)")]
    [InlineData(4, " (Scale = 4)")]
    [InlineData(5, " (Scale = 5)")]
    [InlineData(6, " (Scale = 6)")]
    [InlineData(7, " (Scale = 7)")]
    public virtual async Task Query_generates_correct_datetime2_parameter_definition(int? fractionalSeconds, string postfix)
    {
        var contextFactory = await InitializeAsync<Context26742>(
            onModelCreating: modelBuilder =>
            {
                if (fractionalSeconds.HasValue)
                {
                    modelBuilder.Entity<Context26742.Entity>().Property(p => p.DateTime).HasPrecision(fractionalSeconds.Value);
                }
            });

        var parameter = new DateTime(2021, 11, 12, 13, 14, 15).AddTicks(1234567);
        using var context = contextFactory.CreateContext();
        _ = context.Entities.Where(x => x.DateTime == parameter).Select(e => e.DateTime).FirstOrDefault();
    }

    [ConditionalTheory]
    [InlineData(null, "")]
    //[InlineData(0, " (Scale = 0)")] //https://github.com/dotnet/SqlClient/issues/1380 cause this test to fail, not EF
    [InlineData(1, " (Scale = 1)")]
    [InlineData(2, " (Scale = 2)")]
    [InlineData(3, " (Scale = 3)")]
    [InlineData(4, " (Scale = 4)")]
    [InlineData(5, " (Scale = 5)")]
    [InlineData(6, " (Scale = 6)")]
    [InlineData(7, " (Scale = 7)")]
    public virtual async Task Query_generates_correct_datetimeoffset_parameter_definition(int? fractionalSeconds, string postfix)
    {
        var contextFactory = await InitializeAsync<Context26742>(
            onModelCreating: modelBuilder =>
            {
                if (fractionalSeconds.HasValue)
                {
                    modelBuilder.Entity<Context26742.Entity>().Property(p => p.DateTimeOffset).HasPrecision(fractionalSeconds.Value);
                }
            });

        var parameter = new DateTimeOffset(new DateTime(2021, 11, 12, 13, 14, 15).AddTicks(1234567), TimeSpan.FromHours(10));
        using var context = contextFactory.CreateContext();
        _ = context.Entities.Where(x => x.DateTimeOffset == parameter).Select(e => e.DateTimeOffset).FirstOrDefault();
    }

    [ConditionalTheory]
    [InlineData(null, "")]
    //[InlineData(0, " (Scale = 0)")] //https://github.com/dotnet/SqlClient/issues/1380 cause this test to fail, not EF
    [InlineData(1, " (Scale = 1)")]
    [InlineData(2, " (Scale = 2)")]
    [InlineData(3, " (Scale = 3)")]
    [InlineData(4, " (Scale = 4)")]
    [InlineData(5, " (Scale = 5)")]
    [InlineData(6, " (Scale = 6)")]
    [InlineData(7, " (Scale = 7)")]
    public virtual async Task Query_generates_correct_timespan_parameter_definition(int? fractionalSeconds, string postfix)
    {
        var contextFactory = await InitializeAsync<Context26742>(
            onModelCreating: modelBuilder =>
            {
                if (fractionalSeconds.HasValue)
                {
                    modelBuilder.Entity<Context26742.Entity>().Property(p => p.TimeSpan).HasPrecision(fractionalSeconds.Value);
                }
            });

        var parameter = TimeSpan.Parse("12:34:56.7890123", CultureInfo.InvariantCulture);
        using var context = contextFactory.CreateContext();
        _ = context.Entities.Where(x => x.TimeSpan == parameter).Select(e => e.TimeSpan).FirstOrDefault();
    }

    private class Context26742(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity> Entities { get; set; }

        public class Entity
        {
            public int Id { get; set; }
            public TimeSpan TimeSpan { get; set; }
            public DateTime DateTime { get; set; }
            public DateTimeOffset DateTimeOffset { get; set; }
        }
    }

    #endregion

    #region 28196

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Hierarchy_query_with_abstract_type_sibling(bool async)
        => Hierarchy_query_with_abstract_type_sibling_helper(async, null);

    public virtual async Task Hierarchy_query_with_abstract_type_sibling_helper(bool async, Action<ModelBuilder> onModelCreating)
    {
        var contextFactory = await InitializeAsync<Context28196>(onModelCreating: onModelCreating, seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();
        var query = context.Animals.OfType<Context28196.Pet>().Where(a => a.Species.StartsWith("F"));
        var result = async
            ? await query.ToListAsync()
            : query.ToList();
    }

    protected class Context28196(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Animal> Animals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Animal>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<Pet>();
            modelBuilder.Entity<Cat>();
            modelBuilder.Entity<Dog>();
            modelBuilder.Entity<FarmAnimal>();
        }

        public Task SeedAsync()
        {
            AddRange(
                new Cat
                {
                    Id = 1,
                    Name = "Alice",
                    Species = "Felis catus",
                    EdcuationLevel = "MBA"
                },
                new Cat
                {
                    Id = 2,
                    Name = "Mac",
                    Species = "Felis catus",
                    EdcuationLevel = "BA"
                },
                new Dog
                {
                    Id = 3,
                    Name = "Toast",
                    Species = "Canis familiaris",
                    FavoriteToy = "Mr. Squirrel"
                },
                new FarmAnimal
                {
                    Id = 4,
                    Value = 100.0,
                    Species = "Ovis aries"
                });

            return SaveChangesAsync();
        }

        public abstract class Animal
        {
            public int Id { get; set; }
            public string Species { get; set; }
        }

        public class FarmAnimal : Animal
        {
            public double Value { get; set; }
        }

        public abstract class Pet : Animal
        {
            public string Name { get; set; }
        }

        public class Cat : Pet
        {
            public string EdcuationLevel { get; set; }
        }

        public class Dog : Pet
        {
            public string FavoriteToy { get; set; }
        }
    }

    #endregion
}
