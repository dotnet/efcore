// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class AdHocMiscellaneousQueryTestBase : NonSharedModelTestBase
{
    protected override string StoreName
        => "AdHocMiscellaneousQueryTests";

    #region 603

    [ConditionalFact]
    public virtual async Task First_FirstOrDefault_ix_async()
    {
        var contextFactory = await InitializeAsync<Context603>();
        using (var context = contextFactory.CreateContext())
        {
            var product = await context.Products.OrderBy(p => p.Id).FirstAsync();
            context.Products.Remove(product);
            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateContext())
        {
            context.Products.Add(new Context603.Product { Name = "Product 1" });
            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateContext())
        {
            var product = await context.Products.OrderBy(p => p.Id).FirstOrDefaultAsync();
            context.Products.Remove(product);
            await context.SaveChangesAsync();
        }
    }

    private class Context603(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Product>()
                .HasData(new Product { Id = 1, Name = "Product 1" });

        public class Product
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }

    #endregion

    #region 6901

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Left_join_with_missing_key_values_on_both_sides(bool async)
    {
        var contextFactory = await InitializeAsync<Context6901>();
        using var context = contextFactory.CreateContext();

        var customers
            = from customer in context.Customers
              join postcode in context.Postcodes
                  on customer.PostcodeID equals postcode.PostcodeID into custPCTmp
              from custPC in custPCTmp.DefaultIfEmpty()
              select new
              {
                  customer.CustomerID,
                  customer.CustomerName,
                  TownName = custPC == null ? string.Empty : custPC.TownName,
                  PostcodeValue = custPC == null ? string.Empty : custPC.PostcodeValue
              };

        var results = customers.ToList();

        Assert.Equal(5, results.Count);
        Assert.True(results[3].CustomerName != results[4].CustomerName);
    }

    private class Context6901(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>(
                c =>
                {
                    c.HasKey(x => x.CustomerID);
                    c.Property(c => c.CustomerID).ValueGeneratedNever();
                    c.Property(c => c.CustomerName).HasMaxLength(120).IsUnicode(false);
                    c.HasData(
                        new Customer
                        {
                            CustomerID = 1,
                            CustomerName = "Sam Tippet",
                            PostcodeID = 5
                        },
                        new Customer
                        {
                            CustomerID = 2,
                            CustomerName = "William Greig",
                            PostcodeID = 2
                        },
                        new Customer
                        {
                            CustomerID = 3,
                            CustomerName = "Steve Jones",
                            PostcodeID = 3
                        },
                        new Customer { CustomerID = 4, CustomerName = "Jim Warren" },
                        new Customer
                        {
                            CustomerID = 5,
                            CustomerName = "Andrew Smith",
                            PostcodeID = 5
                        });
                });

            modelBuilder.Entity<Postcode>(
                p =>
                {
                    p.HasKey(x => x.PostcodeID);
                    p.Property(c => c.PostcodeID).ValueGeneratedNever();
                    p.Property(c => c.PostcodeValue).HasMaxLength(100).IsUnicode(false);
                    p.Property(c => c.TownName).HasMaxLength(255).IsUnicode(false);
                    p.HasData(
                        new Postcode
                        {
                            PostcodeID = 2,
                            PostcodeValue = "1000",
                            TownName = "Town 1"
                        },
                        new Postcode
                        {
                            PostcodeID = 3,
                            PostcodeValue = "2000",
                            TownName = "Town 2"
                        },
                        new Postcode
                        {
                            PostcodeID = 4,
                            PostcodeValue = "3000",
                            TownName = "Town 3"
                        },
                        new Postcode
                        {
                            PostcodeID = 5,
                            PostcodeValue = "4000",
                            TownName = "Town 4"
                        });
                });
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Postcode> Postcodes { get; set; }
    }

    public class Customer
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
        public int? PostcodeID { get; set; }
    }

    public class Postcode
    {
        public int PostcodeID { get; set; }
        public string PostcodeValue { get; set; }
        public string TownName { get; set; }
    }

    #endregion

    #region 6986

    [ConditionalFact]
    public virtual async Task Shadow_property_with_inheritance()
    {
        var contextFactory = await InitializeAsync<Context6986>(seed: c => c.SeedAsync());

        using (var context = contextFactory.CreateContext())
        {
            // can_query_base_type_when_derived_types_contain_shadow_properties
            var query = context.Contacts.ToList();

            Assert.Equal(4, query.Count);
            Assert.Equal(2, query.OfType<Context6986.EmployerContact>().Count());
            Assert.Single(query.OfType<Context6986.ServiceOperatorContact>());
        }

        using (var context = contextFactory.CreateContext())
        {
            // can_include_dependent_to_principal_navigation_of_derived_type_with_shadow_fk
            var query = context.Contacts.OfType<Context6986.ServiceOperatorContact>().Include(e => e.ServiceOperator)
                .ToList();

            Assert.Single(query);
            Assert.NotNull(query[0].ServiceOperator);
        }

        using (var context = contextFactory.CreateContext())
        {
            // can_project_shadow_property_using_ef_property
            var query = context.Contacts.OfType<Context6986.ServiceOperatorContact>().Select(
                c => new { c, Prop = EF.Property<int>(c, "ServiceOperatorId") }).ToList();

            Assert.Single(query);
            Assert.Equal(1, query[0].Prop);
        }
    }

    private class Context6986(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<EmployerContact> EmployerContacts { get; set; }
        public DbSet<Employer> Employers { get; set; }
        public DbSet<ServiceOperatorContact> ServiceOperatorContacts { get; set; }
        public DbSet<ServiceOperator> ServiceOperators { get; set; }

        public async Task SeedAsync()
        {
            ServiceOperators.Add(new ServiceOperator());
            Employers.AddRange(
                new Employer { Name = "UWE" },
                new Employer { Name = "Hewlett Packard" });

            await SaveChangesAsync();

            Contacts.AddRange(
                new ServiceOperatorContact
                {
                    UserName = "service.operator@esoterix.co.uk", ServiceOperator = ServiceOperators.OrderBy(o => o.Id).First()
                },
                new EmployerContact
                {
                    UserName = "uwe@esoterix.co.uk", Employer = Employers.OrderBy(e => e.Id).First(e => e.Name == "UWE")
                },
                new EmployerContact
                {
                    UserName = "hp@esoterix.co.uk", Employer = Employers.OrderBy(e => e.Id).First(e => e.Name == "Hewlett Packard")
                },
                new Contact { UserName = "noroles@esoterix.co.uk" });

            await SaveChangesAsync();
        }

        public class EmployerContact : Contact
        {
            [Required]
            public Employer Employer { get; set; }
        }

        public class ServiceOperatorContact : Contact
        {
            [Required]
            public ServiceOperator ServiceOperator { get; set; }
        }

        public class Contact
        {
            public int Id { get; set; }
            public string UserName { get; set; }
            public bool IsPrimary { get; set; }
        }

        public class Employer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<EmployerContact> Contacts { get; set; }
        }

        public class ServiceOperator
        {
            public int Id { get; set; }
            public List<ServiceOperatorContact> Contacts { get; set; }
        }
    }

    #endregion

    #region 7222

    [ConditionalFact]
    public virtual async Task Inlined_dbcontext_is_not_leaking()
    {
        var contextFactory = await InitializeAsync<Context7222>();
        using (var context = contextFactory.CreateContext())
        {
            var entities = context.Blogs.Select(b => context.ClientMethod(b)).ToList();
        }

        using (var context = contextFactory.CreateContext())
        {
            Assert.Throws<InvalidOperationException>(() => context.RunQuery());
        }
    }

    private class Context7222(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Blog> Blogs { get; set; }

        public void RunQuery()
            => Blogs.Select(b => ClientMethod(b)).ToList();

        public int ClientMethod(Blog blog)
            => blog.Id;

        public class Blog
        {
            public int Id { get; set; }
        }
    }

    #endregion

    #region 7359

    [ConditionalFact]
    public virtual async Task Discriminator_type_is_handled_correctly()
    {
        var contextFactory = await InitializeAsync<Context7359>(seed: c => c.SeedAsync());

        using (var ctx = contextFactory.CreateContext())
        {
            var query = ctx.Products.OfType<Context7359.SpecialProduct>().ToList();

            Assert.Single(query);
        }

        using (var ctx = contextFactory.CreateContext())
        {
            var query = ctx.Products.Where(p => p is Context7359.SpecialProduct).ToList();

            Assert.Single(query);
        }
    }

    private class Context7359(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SpecialProduct>();
            modelBuilder.Entity<Product>()
                .HasDiscriminator<int?>("Discriminator")
                .HasValue(0)
                .HasValue<SpecialProduct>(1);
        }

        public Task SeedAsync()
        {
            Add(new Product { Name = "Product1" });
            Add(new SpecialProduct { Name = "SpecialProduct" });
            return SaveChangesAsync();
        }

        public class Product
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        public class SpecialProduct : Product;
    }

    #endregion

    #region 7983

    [ConditionalFact]
    public virtual async Task New_instances_in_projection_are_not_shared_across_results()
    {
        var contextFactory = await InitializeAsync<Context7983>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();
        var list = context.Posts.Select(p => new Context7983.PostDTO().From(p)).ToList();

        Assert.Equal(3, list.Count);
        Assert.Equal(new[] { "First", "Second", "Third" }, list.Select(dto => dto.Title));
    }

    private class Context7983(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }

        public Task SeedAsync()
        {
            Add(
                new Blog
                {
                    Posts = new List<Post>
                    {
                        new() { Title = "First" },
                        new() { Title = "Second" },
                        new() { Title = "Third" }
                    }
                });

            return SaveChangesAsync();
        }

        public class Blog
        {
            public int Id { get; set; }
            public string Title { get; set; }

            public ICollection<Post> Posts { get; set; }
        }

        public class Post
        {
            public int Id { get; set; }
            public string Title { get; set; }

            public int? BlogId { get; set; }
            public Blog Blog { get; set; }
        }

        public class PostDTO
        {
            public string Title { get; set; }

            public PostDTO From(Post post)
            {
                Title = post.Title;
                return this;
            }
        }
    }

    #endregion

    #region 8538

    [ConditionalFact]
    public virtual async Task Enum_has_flag_applies_explicit_cast_for_constant()
    {
        var contextFactory = await InitializeAsync<Context8538>(seed: c => c.SeedAsync());

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Entities.Where(e => e.Permission.HasFlag(Context8538.Permission.READ_WRITE)).ToList();
            Assert.Single(query);
        }

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Entities.Where(e => e.PermissionShort.HasFlag(Context8538.PermissionShort.READ_WRITE)).ToList();
            Assert.Single(query);
        }
    }

    [ConditionalFact]
    public virtual async Task Enum_has_flag_does_not_apply_explicit_cast_for_non_constant()
    {
        var contextFactory = await InitializeAsync<Context8538>(seed: c => c.SeedAsync());

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Entities.Where(e => e.Permission.HasFlag(e.Permission)).ToList();
            Assert.Equal(3, query.Count);
        }

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Entities.Where(e => e.PermissionByte.HasFlag(e.PermissionByte)).ToList();
            Assert.Equal(3, query.Count);
        }
    }

    private class Context8538(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity> Entities { get; set; }

        public Task SeedAsync()
        {
            AddRange(
                new Entity
                {
                    Permission = Permission.NONE,
                    PermissionByte = PermissionByte.NONE,
                    PermissionShort = PermissionShort.NONE
                },
                new Entity
                {
                    Permission = Permission.READ_ONLY,
                    PermissionByte = PermissionByte.READ_ONLY,
                    PermissionShort = PermissionShort.READ_ONLY
                },
                new Entity
                {
                    Permission = Permission.READ_WRITE,
                    PermissionByte = PermissionByte.READ_WRITE,
                    PermissionShort = PermissionShort.READ_WRITE
                }
            );

            return SaveChangesAsync();
        }

        public class Entity
        {
            public int Id { get; set; }
            public Permission Permission { get; set; }
            public PermissionByte PermissionByte { get; set; }
            public PermissionShort PermissionShort { get; set; }
        }

        [Flags]
        public enum PermissionByte : byte
        {
            NONE = 1,
            READ_ONLY = 2,
            READ_WRITE = 4
        }

        [Flags]
        public enum PermissionShort : short
        {
            NONE = 1,
            READ_ONLY = 2,
            READ_WRITE = 4
        }

        [Flags]
        public enum Permission : long
        {
            NONE = 0x01,
            READ_ONLY = 0x02,
            READ_WRITE = 0x400000000 // 36 bits
        }
    }

    #endregion

    #region 8909

    [ConditionalFact]
    public virtual async Task Variable_from_closure_is_parametrized()
    {
        var contextFactory = await InitializeAsync<Context8909>();
        using (var context = contextFactory.CreateContext())
        {
            context.Cache.Compact(1);

            var id = 1;
            context.Entities.Where(c => c.Id == id).ToList();
            Assert.Equal(2, context.Cache.Count);

            id = 2;
            context.Entities.Where(c => c.Id == id).ToList();
            Assert.Equal(2, context.Cache.Count);
        }

        using (var context = contextFactory.CreateContext())
        {
            context.Cache.Compact(1);

            var id = 0;
            // ReSharper disable once AccessToModifiedClosure
            Expression<Func<Context8909.Entity, bool>> whereExpression = c => c.Id == id;

            id = 1;
            context.Entities.Where(whereExpression).ToList();
            Assert.Equal(2, context.Cache.Count);

            id = 2;
            context.Entities.Where(whereExpression).ToList();
            Assert.Equal(2, context.Cache.Count);
        }

        using (var context = contextFactory.CreateContext())
        {
            context.Cache.Compact(1);

            var id = 0;
            // ReSharper disable once AccessToModifiedClosure
            Expression<Func<Context8909.Entity, bool>> whereExpression = c => c.Id == id;
            Expression<Func<Context8909.Entity, bool>> containsExpression =
                c => context.Entities.Where(whereExpression).Select(e => e.Id).Contains(c.Id);

            id = 1;
            context.Entities.Where(containsExpression).ToList();
            Assert.Equal(2, context.Cache.Count);

            id = 2;
            context.Entities.Where(containsExpression).ToList();
            Assert.Equal(2, context.Cache.Count);
        }
    }

    [ConditionalFact]
    public virtual async Task Relational_command_cache_creates_new_entry_when_parameter_nullability_changes()
    {
        var contextFactory = await InitializeAsync<Context8909>();
        using var context = contextFactory.CreateContext();
        context.Cache.Compact(1);

        var name = "A";

        context.Entities.Where(e => e.Name == name).ToList();
        Assert.Equal(2, context.Cache.Count);

        name = null;
        context.Entities.Where(e => e.Name == name).ToList();
        Assert.Equal(3, context.Cache.Count);
    }

    [ConditionalFact]
    public virtual async Task Query_cache_entries_are_evicted_as_necessary()
    {
        var contextFactory = await InitializeAsync<Context8909>();
        using var context = contextFactory.CreateContext();
        context.Cache.Compact(1);
        Assert.Equal(0, context.Cache.Count);

        var entityParam = Expression.Parameter(typeof(Context8909.Entity), "e");
        var idPropertyInfo = context.Model.FindEntityType((typeof(Context8909.Entity)))
            .FindProperty(nameof(Context8909.Entity.Id))
            .PropertyInfo;
        for (var i = 0; i < 1100; i++)
        {
            var conditionBody = Expression.Equal(
                Expression.MakeMemberAccess(entityParam, idPropertyInfo),
                Expression.Constant(i));
            var whereExpression = Expression.Lambda<Func<Context8909.Entity, bool>>(conditionBody, entityParam);
            context.Entities.Where(whereExpression).GetEnumerator();
        }

        Assert.True(context.Cache.Count <= 1024);
    }

    [ConditionalFact]
    public virtual async Task Explicitly_compiled_query_does_not_add_cache_entry()
    {
        var parameter = Expression.Parameter(typeof(Context8909.Entity));
        var predicate = Expression.Lambda<Func<Context8909.Entity, bool>>(
            Expression.MakeBinary(
                ExpressionType.Equal,
                Expression.PropertyOrField(parameter, "Id"),
                Expression.Constant(1)),
            parameter);
        var query = EF.CompileQuery((Context8909 context) => context.Set<Context8909.Entity>().SingleOrDefault(predicate));

        var contextFactory = await InitializeAsync<Context8909>();

        using (var context = contextFactory.CreateContext())
        {
            context.Cache.Compact(1);
            Assert.Equal(0, context.Cache.Count);

            query(context);

            // 1 entry for RelationalCommandCache
            Assert.Equal(1, context.Cache.Count);
        }
    }

    private class Context8909(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity> Entities { get; set; }

        public MemoryCache Cache
        {
            get
            {
                var compiledQueryCache = this.GetService<ICompiledQueryCache>();

                return (MemoryCache)typeof(CompiledQueryCache)
                    .GetField("_memoryCache", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.GetValue(compiledQueryCache);
            }
        }

        public class Entity
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }

    #endregion

    #region 9468

    [ConditionalFact]
    public virtual async Task Conditional_expression_with_conditions_does_not_collapse_if_nullable_bool()
    {
        var contextFactory = await InitializeAsync<Context9468>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();
        var query = context.Carts.Select(
            t => new { Processing = t.Configuration != null ? !t.Configuration.Processed : (bool?)null }).ToList();

        Assert.Single(query.Where(t => t.Processing == null));
        Assert.Single(query.Where(t => t.Processing == true));
        Assert.Single(query.Where(t => t.Processing == false));
    }

    private class Context9468(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Cart> Carts { get; set; }

        public Task SeedAsync()
        {
            AddRange(
                new Cart(),
                new Cart { Configuration = new Configuration { Processed = true } },
                new Cart { Configuration = new Configuration() }
            );

            return SaveChangesAsync();
        }

        public class Cart
        {
            public int Id { get; set; }
            public int? ConfigurationId { get; set; }
            public Configuration Configuration { get; set; }
        }

        public class Configuration
        {
            public int Id { get; set; }
            public bool Processed { get; set; }
        }
    }

    #endregion

    #region 11104

    [ConditionalFact]
    public virtual async Task QueryBuffer_requirement_is_computed_when_querying_base_type_while_derived_type_has_shadow_prop()
    {
        var contextFactory = await InitializeAsync<Context11104>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();
        var query = context.Bases.ToList();

        var derived1 = Assert.Single(query);
        Assert.Equal(typeof(Context11104.Derived1), derived1.GetType());
    }

    private class Context11104(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Base> Bases { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Base>()
                .HasDiscriminator(x => x.IsTwo)
                .HasValue<Derived1>(false)
                .HasValue<Derived2>(true);

        public Task SeedAsync()
        {
            AddRange(new Derived1 { IsTwo = false });
            return SaveChangesAsync();
        }

        public abstract class Base
        {
            public int Id { get; set; }
            public bool IsTwo { get; set; }
        }

        public class Derived1 : Base
        {
            public Stuff MoreStuff { get; set; }
        }

        public class Derived2 : Base;

        public class Stuff
        {
            public int Id { get; set; }
        }
    }

    #endregion

    #region 11885

    [ConditionalFact]
    public virtual async Task Average_with_cast()
    {
        var contextFactory = await InitializeAsync<Context11885>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();
        var prices = context.Prices.ToList();

        Assert.Equal(prices.Average(e => e.Price), context.Prices.Average(e => e.Price));
        Assert.Equal(prices.Average(e => e.IntColumn), context.Prices.Average(e => e.IntColumn));
        Assert.Equal(prices.Average(e => e.NullableIntColumn), context.Prices.Average(e => e.NullableIntColumn));
        Assert.Equal(prices.Average(e => e.LongColumn), context.Prices.Average(e => e.LongColumn));
        Assert.Equal(prices.Average(e => e.NullableLongColumn), context.Prices.Average(e => e.NullableLongColumn));
        Assert.Equal(prices.Average(e => e.FloatColumn), context.Prices.Average(e => e.FloatColumn));
        Assert.Equal(prices.Average(e => e.NullableFloatColumn), context.Prices.Average(e => e.NullableFloatColumn));
        Assert.Equal(prices.Average(e => e.DoubleColumn), context.Prices.Average(e => e.DoubleColumn));
        Assert.Equal(prices.Average(e => e.NullableDoubleColumn), context.Prices.Average(e => e.NullableDoubleColumn));
        Assert.Equal(prices.Average(e => e.DecimalColumn), context.Prices.Average(e => e.DecimalColumn));
        Assert.Equal(prices.Average(e => e.NullableDecimalColumn), context.Prices.Average(e => e.NullableDecimalColumn));
    }

    private class Context11885(DbContextOptions options) : DbContext(options)
    {
        public DbSet<PriceEntity> Prices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<PriceEntity>(
                b =>
                {
                    b.Property(e => e.Price).HasPrecision(18, 8);
                    b.Property(e => e.DecimalColumn).HasPrecision(18, 2);
                    b.Property(e => e.NullableDecimalColumn).HasPrecision(18, 2);
                });

        public Task SeedAsync()
        {
            AddRange(
                new PriceEntity
                {
                    IntColumn = 1,
                    NullableIntColumn = 1,
                    LongColumn = 1000,
                    NullableLongColumn = 1000,
                    FloatColumn = 0.1F,
                    NullableFloatColumn = 0.1F,
                    DoubleColumn = 0.000001,
                    NullableDoubleColumn = 0.000001,
                    DecimalColumn = 1.0m,
                    NullableDecimalColumn = 1.0m,
                    Price = 0.00112000m
                },
                new PriceEntity
                {
                    IntColumn = 2,
                    NullableIntColumn = 2,
                    LongColumn = 2000,
                    NullableLongColumn = 2000,
                    FloatColumn = 0.2F,
                    NullableFloatColumn = 0.2F,
                    DoubleColumn = 0.000002,
                    NullableDoubleColumn = 0.000002,
                    DecimalColumn = 2.0m,
                    NullableDecimalColumn = 2.0m,
                    Price = 0.00232111m
                },
                new PriceEntity
                {
                    IntColumn = 3,
                    LongColumn = 3000,
                    FloatColumn = 0.3F,
                    DoubleColumn = 0.000003,
                    DecimalColumn = 3.0m,
                    Price = 0.00345223m
                }
            );

            return SaveChangesAsync();
        }

        public class PriceEntity
        {
            public int Id { get; set; }
            public int IntColumn { get; set; }
            public int? NullableIntColumn { get; set; }
            public long LongColumn { get; set; }
            public long? NullableLongColumn { get; set; }
            public float FloatColumn { get; set; }
            public float? NullableFloatColumn { get; set; }
            public double DoubleColumn { get; set; }
            public double? NullableDoubleColumn { get; set; }
            public decimal DecimalColumn { get; set; }
            public decimal? NullableDecimalColumn { get; set; }
            public decimal Price { get; set; }
        }
    }

    #endregion

    #region 12274

    [ConditionalFact]
    public virtual async Task Parameterless_ctor_on_inner_DTO_gets_called_for_every_row()
    {
        var contextFactory = await InitializeAsync<Context12274>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();
        var results = context.Entities.Select(
            x =>
                new Context12274.OuterDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    Inner = new Context12274.InnerDTO()
                }).ToList();
        Assert.Equal(4, results.Count);
        Assert.False(ReferenceEquals(results[0].Inner, results[1].Inner));
        Assert.False(ReferenceEquals(results[1].Inner, results[2].Inner));
        Assert.False(ReferenceEquals(results[2].Inner, results[3].Inner));
    }

    private class Context12274(DbContextOptions options) : DbContext(options)
    {
        public DbSet<MyEntity> Entities { get; set; }

        public Task SeedAsync()
        {
            var e1 = new MyEntity { Name = "1" };
            var e2 = new MyEntity { Name = "2" };
            var e3 = new MyEntity { Name = "3" };
            var e4 = new MyEntity { Name = "4" };

            Entities.AddRange(e1, e2, e3, e4);
            return SaveChangesAsync();
        }

        public class MyEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class OuterDTO
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public InnerDTO Inner { get; set; }
        }

        public class InnerDTO;
    }

    #endregion

    #region 12549

    [ConditionalFact]
    public virtual async Task Union_and_insert_works_correctly_together()
    {
        var contextFactory = await InitializeAsync<Context12549>();

        using (var context = contextFactory.CreateContext())
        {
            var id1 = 1;
            var id2 = 2;

            var ids1 = context.Set<Context12549.Table1>()
                .Where(x => x.Id == id1)
                .Select(x => x.Id);

            var ids2 = context.Set<Context12549.Table2>()
                .Where(x => x.Id == id2)
                .Select(x => x.Id);

            var results = ids1.Union(ids2).ToList();

            context.AddRange(
                new Context12549.Table1(),
                new Context12549.Table2(),
                new Context12549.Table1(),
                new Context12549.Table2());

            await context.SaveChangesAsync();
        }
    }

    private class Context12549(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Table1> Tables1 { get; set; }
        public DbSet<Table2> Tables2 { get; set; }

        public class Table1
        {
            public int Id { get; set; }
        }

        public class Table2
        {
            public int Id { get; set; }
        }
    }

    #endregion

    #region 15215

    [ConditionalFact]
    public virtual async Task Repeated_parameters_in_generated_query_sql()
    {
        var contextFactory = await InitializeAsync<Context15215>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();
        var k = 1;
        var a = context.Autos.Where(e => e.Id == k).First();
        var b = context.Autos.Where(e => e.Id == k + 1).First();

        var equalQuery = (from d in context.EqualAutos
                          where (d.Auto == a && d.AnotherAuto == b)
                              || (d.Auto == b && d.AnotherAuto == a)
                          select d).ToList();

        Assert.Single(equalQuery);
    }

    private class Context15215(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Auto> Autos { get; set; }
        public DbSet<EqualAuto> EqualAutos { get; set; }

        public async Task SeedAsync()
        {
            for (var i = 0; i < 10; i++)
            {
                Add(new Auto { Name = "Auto " + i });
            }

            await SaveChangesAsync();

            AddRange(
                new EqualAuto { Auto = await Autos.FindAsync(1), AnotherAuto = await Autos.FindAsync(2) },
                new EqualAuto { Auto = await Autos.FindAsync(5), AnotherAuto = await Autos.FindAsync(4) });

            await SaveChangesAsync();
        }

        public class Auto
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class EqualAuto
        {
            public int Id { get; set; }
            public Auto Auto { get; set; }
            public Auto AnotherAuto { get; set; }
        }
    }

    #endregion

    #region 19253

    [ConditionalFact]
    public virtual async Task Operators_combine_nullability_of_entity_shapers()
    {
        var contextFactory = await InitializeAsync<Context19253>(seed: c => c.SeedAsync());

        using (var context = contextFactory.CreateContext())
        {
            Expression<Func<Context19253.A, string>> leftKeySelector = x => x.forkey;
            Expression<Func<Context19253.B, string>> rightKeySelector = y => y.forkey;

            var query = context.As.GroupJoin(
                    context.Bs,
                    leftKeySelector,
                    rightKeySelector,
                    (left, rightg) => new { left, rightg })
                .SelectMany(
                    r => r.rightg.DefaultIfEmpty(),
                    (x, y) => new Context19253.JoinResult<Context19253.A, Context19253.B> { Left = x.left, Right = y })
                .Concat(
                    context.Bs.GroupJoin(
                            context.As,
                            rightKeySelector,
                            leftKeySelector,
                            (right, leftg) => new { leftg, right })
                        .SelectMany(
                            l => l.leftg.DefaultIfEmpty(),
                            (x, y) => new Context19253.JoinResult<Context19253.A, Context19253.B> { Left = y, Right = x.right })
                        .Where(z => z.Left.Equals(null)))
                .ToList();

            Assert.Equal(3, query.Count);
        }

        using (var context = contextFactory.CreateContext())
        {
            Expression<Func<Context19253.A, string>> leftKeySelector = x => x.forkey;
            Expression<Func<Context19253.B, string>> rightKeySelector = y => y.forkey;

            var query = context.As.GroupJoin(
                    context.Bs,
                    leftKeySelector,
                    rightKeySelector,
                    (left, rightg) => new { left, rightg })
                .SelectMany(
                    r => r.rightg.DefaultIfEmpty(),
                    (x, y) => new Context19253.JoinResult<Context19253.A, Context19253.B> { Left = x.left, Right = y })
                .Union(
                    context.Bs.GroupJoin(
                            context.As,
                            rightKeySelector,
                            leftKeySelector,
                            (right, leftg) => new { leftg, right })
                        .SelectMany(
                            l => l.leftg.DefaultIfEmpty(),
                            (x, y) => new Context19253.JoinResult<Context19253.A, Context19253.B> { Left = y, Right = x.right })
                        .Where(z => z.Left.Equals(null)))
                .ToList();

            Assert.Equal(3, query.Count);
        }

        using (var context = contextFactory.CreateContext())
        {
            Expression<Func<Context19253.A, string>> leftKeySelector = x => x.forkey;
            Expression<Func<Context19253.B, string>> rightKeySelector = y => y.forkey;

            var query = context.As.GroupJoin(
                    context.Bs,
                    leftKeySelector,
                    rightKeySelector,
                    (left, rightg) => new { left, rightg })
                .SelectMany(
                    r => r.rightg.DefaultIfEmpty(),
                    (x, y) => new Context19253.JoinResult<Context19253.A, Context19253.B> { Left = x.left, Right = y })
                .Except(
                    context.Bs.GroupJoin(
                            context.As,
                            rightKeySelector,
                            leftKeySelector,
                            (right, leftg) => new { leftg, right })
                        .SelectMany(
                            l => l.leftg.DefaultIfEmpty(),
                            (x, y) => new Context19253.JoinResult<Context19253.A, Context19253.B> { Left = y, Right = x.right }))
                .ToList();

            Assert.Single(query);
        }

        using (var context = contextFactory.CreateContext())
        {
            Expression<Func<Context19253.A, string>> leftKeySelector = x => x.forkey;
            Expression<Func<Context19253.B, string>> rightKeySelector = y => y.forkey;

            var query = context.As.GroupJoin(
                    context.Bs,
                    leftKeySelector,
                    rightKeySelector,
                    (left, rightg) => new { left, rightg })
                .SelectMany(
                    r => r.rightg.DefaultIfEmpty(),
                    (x, y) => new Context19253.JoinResult<Context19253.A, Context19253.B> { Left = x.left, Right = y })
                .Intersect(
                    context.Bs.GroupJoin(
                            context.As,
                            rightKeySelector,
                            leftKeySelector,
                            (right, leftg) => new { leftg, right })
                        .SelectMany(
                            l => l.leftg.DefaultIfEmpty(),
                            (x, y) => new Context19253.JoinResult<Context19253.A, Context19253.B> { Left = y, Right = x.right }))
                .ToList();

            Assert.Single(query);
        }
    }

    private class Context19253(DbContextOptions options) : DbContext(options)
    {
        public DbSet<A> As { get; set; }
        public DbSet<B> Bs { get; set; }

        public Task SeedAsync()
        {
            var tmp_a = new[]
            {
                new()
                {
                    a = "a0",
                    a1 = "a1",
                    forkey = "a"
                },
                new A
                {
                    a = "a2",
                    a1 = "a1",
                    forkey = "d"
                },
            };
            var tmp_b = new[]
            {
                new()
                {
                    b = "b0",
                    b1 = "b1",
                    forkey = "a"
                },
                new B
                {
                    b = "b2",
                    b1 = "b1",
                    forkey = "c"
                },
            };
            As.AddRange(tmp_a);
            Bs.AddRange(tmp_b);
            return SaveChangesAsync();
        }

        public class JoinResult<TLeft, TRight>
        {
            public TLeft Left { get; set; }

            public TRight Right { get; set; }
        }

        public class A
        {
            public int Id { get; set; }
            public string a { get; set; }
            public string a1 { get; set; }
            public string forkey { get; set; }
        }

        public class B
        {
            public int Id { get; set; }
            public string b { get; set; }
            public string b1 { get; set; }
            public string forkey { get; set; }
        }
    }

    #endregion

    #region 21770

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Comparing_enum_casted_to_byte_with_int_parameter(bool async)
    {
        var contextFactory = await InitializeAsync<Context21770>();
        using var context = contextFactory.CreateContext();
        var bitterTaste = Context21770.Taste.Bitter;
        var query = context.IceCreams.Where(i => i.Taste == (byte)bitterTaste);

        var bitterIceCreams = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Single(bitterIceCreams);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Comparing_enum_casted_to_byte_with_int_constant(bool async)
    {
        var contextFactory = await InitializeAsync<Context21770>();
        using var context = contextFactory.CreateContext();
        var query = context.IceCreams.Where(i => i.Taste == (byte)Context21770.Taste.Bitter);

        var bitterIceCreams = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Single(bitterIceCreams);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Comparing_byte_column_to_enum_in_vb_creating_double_cast(bool async)
    {
        var contextFactory = await InitializeAsync<Context21770>();
        using var context = contextFactory.CreateContext();
        Expression<Func<Context21770.Food, byte?>> memberAccess = i => i.Taste;
        var predicate = Expression.Lambda<Func<Context21770.Food, bool>>(
            Expression.Equal(
                Expression.Convert(memberAccess.Body, typeof(int?)),
                Expression.Convert(
                    Expression.Convert(Expression.Constant(Context21770.Taste.Bitter, typeof(Context21770.Taste)), typeof(int)),
                    typeof(int?))),
            memberAccess.Parameters);
        var query = context.Foods.Where(predicate);

        var bitterFood = async
            ? await query.ToListAsync()
            : query.ToList();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Null_check_removal_in_ternary_maintain_appropriate_cast(bool async)
    {
        var contextFactory = await InitializeAsync<Context21770>();
        using var context = contextFactory.CreateContext();

        var query = from f in context.Foods
                    select new { Bar = f.Taste != null ? (Context21770.Taste)f.Taste : (Context21770.Taste?)null };

        var bitterFood = async
            ? await query.ToListAsync()
            : query.ToList();
    }

    private class Context21770(DbContextOptions options) : DbContext(options)
    {
        public DbSet<IceCream> IceCreams { get; set; }
        public DbSet<Food> Foods { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IceCream>(
                entity =>
                {
                    entity.HasData(
                        new IceCream
                        {
                            IceCreamId = 1,
                            Name = "Vanilla",
                            Taste = (byte)Taste.Sweet
                        },
                        new IceCream
                        {
                            IceCreamId = 2,
                            Name = "Chocolate",
                            Taste = (byte)Taste.Sweet
                        },
                        new IceCream
                        {
                            IceCreamId = 3,
                            Name = "Match",
                            Taste = (byte)Taste.Bitter
                        });
                });

            modelBuilder.Entity<Food>(
                entity =>
                {
                    entity.HasData(new Food { Id = 1, Taste = null });
                });
        }

        public enum Taste : byte
        {
            Sweet = 0,
            Bitter = 1,
        }

        public class IceCream
        {
            public int IceCreamId { get; set; }
            public string Name { get; set; }
            public int Taste { get; set; }
        }

        public class Food
        {
            public int Id { get; set; }
            public byte? Taste { get; set; }
        }
    }

    #endregion

    #region 22841

    [ConditionalFact]
    public async virtual Task SaveChangesAsync_accepts_changes_with_ConfigureAwait_true()
    {
        var contextFactory = await InitializeAsync<Context22841>();

        using var context = contextFactory.CreateContext();
        var observableThing = new Context22841.ObservableThing();

        using var trackingSynchronizationContext = new SingleThreadSynchronizationContext();
        var origSynchronizationContext = SynchronizationContext.Current;
        SynchronizationContext.SetSynchronizationContext(trackingSynchronizationContext);

        // Do a dispatch once to make sure we're in the new synchronization context. This is necessary in case the below happens
        // to complete synchronously, which shouldn't happen in principle - but just to be safe.
        await Task.Delay(1).ConfigureAwait(true);

        bool? isMySyncContext = null;
        Action callback = () => isMySyncContext =
            SynchronizationContext.Current == trackingSynchronizationContext
            && Thread.CurrentThread == trackingSynchronizationContext.Thread;
        observableThing.Event += callback;

        try
        {
            await context.AddAsync(observableThing);
            await context.SaveChangesAsync();
        }
        finally
        {
            observableThing.Event -= callback;
            SynchronizationContext.SetSynchronizationContext(origSynchronizationContext);
        }

        Assert.True(isMySyncContext);
    }

    private class Context22841(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder
                .Entity<ObservableThing>()
                .Property(o => o.Id)
                .UsePropertyAccessMode(PropertyAccessMode.Property);

        public DbSet<ObservableThing> ObservableThings { get; set; }

        public class ObservableThing
        {
            public int Id
            {
                get => _id;
                set
                {
                    _id = value;
                    Event?.Invoke();
                }
            }

            private int _id;

            public event Action Event;
        }
    }

    #endregion

    #region 24657

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Bool_discriminator_column_works(bool async)
    {
        var contextFactory = await InitializeAsync<Context24657>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();

        var query = context.Authors.Include(e => e.Blog);

        var authors = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Equal(2, authors.Count);
    }

    private class Context24657(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Author> Authors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Blog>()
                .HasDiscriminator<bool>(nameof(Blog.IsPhotoBlog))
                .HasValue<DevBlog>(false)
                .HasValue<PhotoBlog>(true);

        public Task SeedAsync()
        {
            Add(new Author { Blog = new DevBlog { Title = "Dev Blog", } });
            Add(new Author { Blog = new PhotoBlog { Title = "Photo Blog", } });

            return SaveChangesAsync();
        }

        public class Author
        {
            public int Id { get; set; }
            public Blog Blog { get; set; }
        }

        public abstract class Blog
        {
            public int Id { get; set; }
            public bool IsPhotoBlog { get; set; }
            public string Title { get; set; }
        }

        public class DevBlog : Blog
        {
            public DevBlog()
            {
                IsPhotoBlog = false;
            }
        }

        public class PhotoBlog : Blog
        {
            public PhotoBlog()
            {
                IsPhotoBlog = true;
            }

            public int NumberOfPhotos { get; set; }
        }
    }

    #endregion

    #region 26593

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Unwrap_convert_node_over_projection_when_translating_contains_over_subquery(bool async)
    {
        var contextFactory = await InitializeAsync<Context26593>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();

        var currentUserId = 1;

        var currentUserGroupIds = context.Memberships
            .Where(m => m.UserId == currentUserId)
            .Select(m => m.GroupId);

        var hasMembership = context.Memberships
            .Where(m => currentUserGroupIds.Contains(m.GroupId))
            .Select(m => m.User);

        var query = context.Users
            .Select(u => new { HasAccess = hasMembership.Contains(u) });

        var users = async
            ? await query.ToListAsync()
            : query.ToList();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Unwrap_convert_node_over_projection_when_translating_contains_over_subquery_2(bool async)
    {
        var contextFactory = await InitializeAsync<Context26593>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();

        var currentUserId = 1;

        var currentUserGroupIds = context.Memberships
            .Where(m => m.UserId == currentUserId)
            .Select(m => m.Group);

        var hasMembership = context.Memberships
            .Where(m => currentUserGroupIds.Contains(m.Group))
            .Select(m => m.User);

        var query = context.Users
            .Select(u => new { HasAccess = hasMembership.Contains(u) });

        var users = async
            ? await query.ToListAsync()
            : query.ToList();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Unwrap_convert_node_over_projection_when_translating_contains_over_subquery_3(bool async)
    {
        var contextFactory = await InitializeAsync<Context26593>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();

        var currentUserId = 1;

        var currentUserGroupIds = context.Memberships
            .Where(m => m.UserId == currentUserId)
            .Select(m => m.GroupId);

        var hasMembership = context.Memberships
            .Where(m => currentUserGroupIds.Contains(m.GroupId))
            .Select(m => m.User);

        var query = context.Users
            .Select(u => new { HasAccess = hasMembership.Any(e => e == u) });

        var users = async
            ? await query.ToListAsync()
            : query.ToList();
    }

    private class Context26593(DbContextOptions options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Membership> Memberships { get; set; }

        public Task SeedAsync()
        {
            var user = new User();
            var group = new Group();
            var membership = new Membership { Group = group, User = user };
            AddRange(user, group, membership);

            return SaveChangesAsync();
        }

        public class User
        {
            public int Id { get; set; }

            public ICollection<Membership> Memberships { get; set; }
        }

        public class Group
        {
            public int Id { get; set; }

            public ICollection<Membership> Memberships { get; set; }
        }

        public class Membership
        {
            public int Id { get; set; }
            public User User { get; set; }
            public int UserId { get; set; }
            public Group Group { get; set; }
            public int GroupId { get; set; }
        }
    }

    #endregion

    #region 26587

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_aggregate_on_right_side_of_join(bool async)
    {
        var contextFactory = await InitializeAsync<Context26587>();
        using var context = contextFactory.CreateContext();

        var orderId = 123456;

        var orderItems = context.OrderItems.Where(o => o.OrderId == orderId);
        var items = orderItems
            .GroupBy(
                o => o.OrderId,
                (o, g) => new
                {
                    Key = o, IsPending = g.Max(y => y.ShippingDate == null && y.CancellationDate == null ? o : (o - 10000000))
                })
            .OrderBy(e => e.Key);

        var query = orderItems
            .Join(items, x => x.OrderId, x => x.Key, (x, y) => x)
            .OrderBy(x => x.OrderId);

        var users = async
            ? await query.ToListAsync()
            : query.ToList();
    }

    private class Context26587(DbContextOptions options) : DbContext(options)
    {
        public DbSet<OrderItem> OrderItems { get; set; }

        public class OrderItem
        {
            public int Id { get; set; }
            public int OrderId { get; set; }
            public DateTime? ShippingDate { get; set; }
            public DateTime? CancellationDate { get; set; }
        }
    }

    #endregion

    #region 26472

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Enum_with_value_converter_matching_take_value(bool async)
    {
        var contextFactory = await InitializeAsync<Context26472>();
        using var context = contextFactory.CreateContext();
        var orderItemType = Context26472.OrderItemType.MyType1;
        var query = context.Orders.Where(x => x.Items.Any()).OrderBy(e => e.Id).Take(1)
            .Select(e => e.Id)
            .Join(context.Orders, o => o, i => i.Id, (o, i) => i)
            .Select(
                entity => new
                {
                    entity.Id,
                    SpecialSum = entity.Items.Where(x => x.Type == orderItemType)
                        .Select(x => x.Price)
                        .FirstOrDefault()
                });

        var result = async
            ? await query.ToListAsync()
            : query.ToList();
    }

    private class Context26472(DbContextOptions options) : DbContext(options)
    {
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<OrderItem>().Property(x => x.Type).HasConversion<string>();

        public class Order
        {
            public int Id { get; set; }

            public virtual ICollection<OrderItem> Items { get; set; }
        }

        public class OrderItem
        {
            public int Id { get; set; }
            public int OrderId { get; set; }
            public OrderItemType Type { get; set; }
            public double Price { get; set; }
        }

        public enum OrderItemType
        {
            Undefined = 0,
            MyType1 = 1,
            MyType2 = 2
        }
    }

    #endregion

    #region 27083

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_Aggregate_over_navigations_repeated(bool async)
    {
        var contextFactory = await InitializeAsync<Context27083>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();

        var query = context
            .Set<Context27083.TimeSheet>()
            .Where(x => x.OrderId != null)
            .GroupBy(x => x.OrderId)
            .Select(
                x => new
                {
                    HourlyRate = x.Min(f => f.Order.HourlyRate),
                    CustomerId = x.Min(f => f.Project.Customer.Id),
                    CustomerName = x.Min(f => f.Project.Customer.Name),
                });

        var timeSheets = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Equal(2, timeSheets.Count);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Aggregate_over_subquery_in_group_by_projection(bool async)
    {
        var contextFactory = await InitializeAsync<Context27083>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();

        Expression<Func<Context27083.Order, bool>> someFilterFromOutside = x => x.Number != "A1";

        var query = context
            .Set<Context27083.Order>()
            .Where(someFilterFromOutside)
            .GroupBy(x => new { x.CustomerId, x.Number })
            .Select(
                x => new
                {
                    x.Key.CustomerId,
                    CustomerMinHourlyRate =
                        context.Set<Context27083.Order>().Where(n => n.CustomerId == x.Key.CustomerId).Min(h => h.HourlyRate),
                    HourlyRate = x.Min(f => f.HourlyRate),
                    Count = x.Count()
                });

        var orders = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Collection(
            orders.OrderBy(x => x.CustomerId),
            t =>
            {
                Assert.Equal(1, t.CustomerId);
                Assert.Equal(10, t.CustomerMinHourlyRate);
                Assert.Equal(11, t.HourlyRate);
                Assert.Equal(1, t.Count);
            },
            t =>
            {
                Assert.Equal(2, t.CustomerId);
                Assert.Equal(20, t.CustomerMinHourlyRate);
                Assert.Equal(20, t.HourlyRate);
                Assert.Equal(1, t.Count);
            });
    }

    private class Context27083(DbContextOptions options) : DbContext(options)
    {
        public DbSet<TimeSheet> TimeSheets { get; set; }
        public DbSet<Customer> Customers { get; set; }

        public Task SeedAsync()
        {
            var customerA = new Customer { Name = "Customer A" };
            var customerB = new Customer { Name = "Customer B" };

            var projectA = new Project { Customer = customerA };
            var projectB = new Project { Customer = customerB };

            var orderA1 = new Order
            {
                Number = "A1",
                Customer = customerA,
                HourlyRate = 10
            };
            var orderA2 = new Order
            {
                Number = "A2",
                Customer = customerA,
                HourlyRate = 11
            };
            var orderB1 = new Order
            {
                Number = "B1",
                Customer = customerB,
                HourlyRate = 20
            };

            var timeSheetA = new TimeSheet { Order = orderA1, Project = projectA };
            var timeSheetB = new TimeSheet { Order = orderB1, Project = projectB };

            AddRange(customerA, customerB);
            AddRange(projectA, projectB);
            AddRange(orderA1, orderA2, orderB1);
            AddRange(timeSheetA, timeSheetB);
            return SaveChangesAsync();
        }

        public class Customer
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public List<Project> Projects { get; set; }
            public List<Order> Orders { get; set; }
        }

        public class Order
        {
            public int Id { get; set; }
            public string Number { get; set; }

            public int CustomerId { get; set; }
            public Customer Customer { get; set; }

            public int HourlyRate { get; set; }
        }

        public class Project
        {
            public int Id { get; set; }
            public int CustomerId { get; set; }

            public Customer Customer { get; set; }
        }

        public class TimeSheet
        {
            public int Id { get; set; }

            public int ProjectId { get; set; }
            public Project Project { get; set; }

            public int? OrderId { get; set; }
            public Order Order { get; set; }
        }
    }

    #endregion

    #region 27094

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Aggregate_over_subquery_in_group_by_projection_2(bool async)
    {
        var contextFactory = await InitializeAsync<Context27094>();
        using var context = contextFactory.CreateContext();

        var query = from t in context.Tables
                    group t.Id by t.Value
                    into tg
                    select new
                    {
                        A = tg.Key, B = context.Tables.Where(t => t.Value == tg.Max() * 6).Max(t => (int?)t.Id),
                    };

        var orders = async
            ? await query.ToListAsync()
            : query.ToList();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Group_by_aggregate_in_subquery_projection_after_group_by(bool async)
    {
        var contextFactory = await InitializeAsync<Context27094>();
        using var context = contextFactory.CreateContext();

        var query = from t in context.Tables
                    group t.Id by t.Value
                    into tg
                    select new
                    {
                        A = tg.Key,
                        B = tg.Sum(),
                        C = (from t in context.Tables
                             group t.Id by t.Value
                             into tg2
                             select tg.Sum() + tg2.Sum()
                            ).OrderBy(e => 1).FirstOrDefault()
                    };

        var orders = async
            ? await query.ToListAsync()
            : query.ToList();
    }

    private class Context27094(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Table> Tables { get; set; }

        public class Table
        {
            public int Id { get; set; }
            public int? Value { get; set; }
        }
    }

    #endregion

    #region 26744

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Subquery_first_member_compared_to_null(bool async)
    {
        var contextFactory = await InitializeAsync<Context26744>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();

        var query = context.Parents
            .Where(
                p => p.Children.Any(c => c.SomeNullableDateTime == null)
                    && p.Children.Where(c => c.SomeNullableDateTime == null)
                        .OrderBy(c => c.SomeInteger)
                        .First().SomeOtherNullableDateTime
                    != null)
            .Select(
                p => p.Children.Where(c => c.SomeNullableDateTime == null)
                    .OrderBy(c => c.SomeInteger)
                    .First().SomeOtherNullableDateTime);

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Single(result);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task SelectMany_where_Select(bool async)
    {
        var contextFactory = await InitializeAsync<Context26744>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();

        var query = context.Parents
            .SelectMany(
                p => p.Children
                    .Where(c => c.SomeNullableDateTime == null)
                    .OrderBy(c => c.SomeInteger)
                    .Take(1))
            .Where(c => c.SomeOtherNullableDateTime != null)
            .Select(c => c.SomeNullableDateTime);

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Single(result);
    }

    private class Context26744(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Parent> Parents { get; set; }

        public Task SeedAsync()
        {
            Add(
                new Parent { Children = [new() { SomeInteger = 1, SomeOtherNullableDateTime = new DateTime(2000, 11, 18) }] });

            Add(new Parent { Children = [new() { SomeInteger = 1, }] });
            return SaveChangesAsync();
        }

        public class Parent
        {
            public int Id { get; set; }
            public List<Child> Children { get; set; }
        }

        public class Child
        {
            public int Id { get; set; }
            public int SomeInteger { get; set; }
            public DateTime? SomeNullableDateTime { get; set; }
            public DateTime? SomeOtherNullableDateTime { get; set; }
            public Parent Parent { get; set; }
        }
    }

    #endregion

    #region 27343

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Flattened_GroupJoin_on_interface_generic(bool async)
    {
        var contextFactory = await InitializeAsync<Context27343>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();
        var entitySet = context.Parents.AsQueryable<Context27343.IDocumentType>();
        var query = from p in entitySet
                    join c in context.Set<Context27343.Child>()
                        on p.Id equals c.Id into grouping
                    from c in grouping.DefaultIfEmpty()
                    select c;

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Empty(result);
    }

    private class Context27343(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Parent> Parents { get; set; }

        public Task SeedAsync()
            => SaveChangesAsync();

        public interface IDocumentType
        {
            public int Id { get; }
        }

        public class Parent : IDocumentType
        {
            public int Id { get; set; }
            public List<Child> Children { get; set; }
        }

        public class Child
        {
            public int Id { get; set; }
            public int SomeInteger { get; set; }
            public DateTime? SomeNullableDateTime { get; set; }
            public DateTime? SomeOtherNullableDateTime { get; set; }
            public Parent Parent { get; set; }
        }
    }

    #endregion

    #region 28039

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Pushdown_does_not_add_grouping_key_to_projection_when_distinct_is_applied(bool async)
    {
        var contextFactory = await InitializeAsync<Context28039>();
        using var db = contextFactory.CreateContext();

        var queryResults = (from i in db.IndexDatas.Where(a => a.Parcel == "some condition")
                                .Select(a => new Context28039.SearchResult { ParcelNumber = a.Parcel, RowId = a.RowId })
                            group i by new { i.ParcelNumber, i.RowId }
                            into grp
                            where grp.Count() == 1
                            select grp.Key.ParcelNumber).Distinct();

        var jsonLookup = (from dcv in db.TableDatas.Where(a => a.TableId == 123)
                          join wos in queryResults
                              on dcv.ParcelNumber equals wos
                          orderby dcv.ParcelNumber
                          select dcv.JSON).Take(123456);

        var result = async
            ? await jsonLookup.ToListAsync()
            : jsonLookup.ToList();
    }

    private class Context28039(DbContextOptions options) : DbContext(options)
    {
        public DbSet<IndexData> IndexDatas { get; set; }
        public DbSet<TableData> TableDatas { get; set; }

        public class TableData : EntityBase
        {
            public int TableId { get; set; }
            public string ParcelNumber { get; set; }
            public short RowId { get; set; }
            public string JSON { get; set; }
        }

        public abstract class EntityBase
        {
            [Key]
            public int ID { get; set; }
        }

        public class IndexData : EntityBase
        {
            public string Parcel { get; set; }
            public int RowId { get; set; }
        }

        public class SearchResult
        {
            public string ParcelNumber { get; set; }
            public int RowId { get; set; }
            public string DistinctValue { get; set; }
        }
    }

    #endregion

    #region 31961

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Filter_on_nested_DTO_with_interface_gets_simplified_correctly(bool async)
    {
        var contextFactory = await InitializeAsync<Context31961>();
        using var context = contextFactory.CreateContext();

        var query = await context.Customers
            .Select(
                m => new Context31961.CustomerDto()
                {
                    Id = m.Id,
                    CompanyId = m.CompanyId,
                    Company = m.Company != null
                        ? new Context31961.CompanyDto()
                        {
                            Id = m.Company.Id,
                            CompanyName = m.Company.CompanyName,
                            CountryId = m.Company.CountryId,
                            Country = new Context31961.CountryDto()
                            {
                                Id = m.Company.Country.Id, CountryName = m.Company.Country.CountryName,
                            },
                        }
                        : null,
                })
            .Where(m => m.Company.Country.CountryName == "COUNTRY")
            .ToListAsync();
    }

    private class Context31961(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Customer> Customers { get; set; }

        public DbSet<Company> Companies { get; set; }

        public DbSet<Country> Countries { get; set; }

        public class Customer
        {
            public string Id { get; set; } = string.Empty;

            public string CompanyId { get; set; }

            public Company Company { get; set; }
        }

        public class Country
        {
            public string Id { get; set; } = string.Empty;

            public string CountryName { get; set; } = string.Empty;
        }

        public class Company
        {
            public string Id { get; set; } = string.Empty;

            public string CompanyName { get; set; } = string.Empty;

            public string CountryId { get; set; }

            public Country Country { get; set; }
        }

        public interface ICustomerDto
        {
            string Id { get; set; }

            string CompanyId { get; set; }

            ICompanyDto Company { get; set; }
        }

        public interface ICountryDto
        {
            string Id { get; set; }

            string CountryName { get; set; }
        }

        public interface ICompanyDto
        {
            string Id { get; set; }

            string CompanyName { get; set; }

            string CountryId { get; set; }

            ICountryDto Country { get; set; }
        }

        public class CustomerDto : ICustomerDto
        {
            public string Id { get; set; } = string.Empty;

            public string CompanyId { get; set; }

            public ICompanyDto Company { get; set; }
        }

        public class CountryDto : ICountryDto
        {
            public string Id { get; set; } = string.Empty;

            public string CountryName { get; set; } = string.Empty;
        }

        public class CompanyDto : ICompanyDto
        {
            public string Id { get; set; } = string.Empty;

            public string CompanyName { get; set; } = string.Empty;

            public string CountryId { get; set; }

            public ICountryDto Country { get; set; }
        }
    }

    #endregion
}
