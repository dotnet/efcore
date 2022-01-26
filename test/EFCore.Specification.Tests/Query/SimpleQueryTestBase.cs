// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace Microsoft.EntityFrameworkCore;

public abstract class SimpleQueryTestBase : NonSharedModelTestBase
{
    public static IEnumerable<object[]> IsAsyncData = new[] { new object[] { false }, new object[] { true } };

    protected override string StoreName
        => "SimpleQueryTests";

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Multiple_nested_reference_navigations(bool async)
    {
        var contextFactory = await InitializeAsync<Context24368>();
        using var context = contextFactory.CreateContext();
        var id = 1;
        var staff = await context.Staff.FindAsync(3);

        Assert.Equal(1, staff.ManagerId);

        var query = context.Appraisals
            .Include(ap => ap.Staff).ThenInclude(s => s.Manager)
            .Include(ap => ap.Staff).ThenInclude(s => s.SecondaryManager)
            .Where(ap => ap.Id == id);

        var appraisal = async
            ? await query.SingleOrDefaultAsync()
            : query.SingleOrDefault();

        Assert.Equal(1, staff.ManagerId);

        Assert.NotNull(appraisal);
        Assert.Same(staff, appraisal.Staff);
        Assert.NotNull(appraisal.Staff.Manager);
        Assert.Equal(1, appraisal.Staff.ManagerId);
        Assert.NotNull(appraisal.Staff.SecondaryManager);
        Assert.Equal(2, appraisal.Staff.SecondaryManagerId);
    }

    protected class Context24368 : DbContext
    {
        public Context24368(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Appraisal> Appraisals { get; set; }
        public DbSet<Staff> Staff { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Staff>().HasIndex(e => e.ManagerId).IsUnique(false);
            modelBuilder.Entity<Staff>()
                .HasOne(a => a.Manager)
                .WithOne()
                .HasForeignKey<Staff>(s => s.ManagerId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Staff>().HasIndex(e => e.SecondaryManagerId).IsUnique(false);
            modelBuilder.Entity<Staff>()
                .HasOne(a => a.SecondaryManager)
                .WithOne()
                .HasForeignKey<Staff>(s => s.SecondaryManagerId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Staff>().HasData(
                new Staff
                {
                    Id = 1,
                    Email = "mgr1@company.com",
                    Logon = "mgr1",
                    Name = "Manager 1"
                },
                new Staff
                {
                    Id = 2,
                    Email = "mgr2@company.com",
                    Logon = "mgr2",
                    Name = "Manager 2",
                    ManagerId = 1
                },
                new Staff
                {
                    Id = 3,
                    Email = "emp@company.com",
                    Logon = "emp",
                    Name = "Employee",
                    ManagerId = 1,
                    SecondaryManagerId = 2
                }
            );

            modelBuilder.Entity<Appraisal>().HasData(
                new Appraisal
                {
                    Id = 1,
                    PeriodStart = new DateTimeOffset(new DateTime(2020, 1, 1).ToUniversalTime()),
                    PeriodEnd = new DateTimeOffset(new DateTime(2020, 12, 31).ToUniversalTime()),
                    StaffId = 3
                });
        }
    }

    protected class Appraisal
    {
        public int Id { get; set; }

        public int StaffId { get; set; }
        public Staff Staff { get; set; }

        public DateTimeOffset PeriodStart { get; set; }
        public DateTimeOffset PeriodEnd { get; set; }

        public bool Complete { get; set; }
        public bool Deleted { get; set; }
    }

    protected class Staff
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string Logon { get; set; }

        [MaxLength(150)]
        public string Email { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }

        public int? ManagerId { get; set; }
        public Staff Manager { get; set; }

        public int? SecondaryManagerId { get; set; }
        public Staff SecondaryManager { get; set; }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Comparing_enum_casted_to_byte_with_int_parameter(bool async)
    {
        var contextFactory = await InitializeAsync<Context21770>();
        using var context = contextFactory.CreateContext();
        var bitterTaste = Taste.Bitter;
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
        var query = context.IceCreams.Where(i => i.Taste == (byte)Taste.Bitter);

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
        Expression<Func<Food, byte?>> memberAccess = i => i.Taste;
        var predicate = Expression.Lambda<Func<Food, bool>>(
            Expression.Equal(
                Expression.Convert(memberAccess.Body, typeof(int?)),
                Expression.Convert(
                    Expression.Convert(Expression.Constant(Taste.Bitter, typeof(Taste)), typeof(int)),
                    typeof(int?))),
            memberAccess.Parameters);
        var query = context.Food.Where(predicate);

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

        var query = from f in context.Food
                    select new { Bar = f.Taste != null ? (Taste)f.Taste : (Taste?)null };

        var bitterFood = async
            ? await query.ToListAsync()
            : query.ToList();
    }

    protected class Context21770 : DbContext
    {
        public Context21770(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<IceCream> IceCreams { get; set; }
        public DbSet<Food> Food { get; set; }

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
    }

    protected enum Taste : byte
    {
        Sweet = 0,
        Bitter = 1,
    }

    protected class IceCream
    {
        public int IceCreamId { get; set; }
        public string Name { get; set; }
        public int Taste { get; set; }
    }

    protected class Food
    {
        public int Id { get; set; }
        public byte? Taste { get; set; }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Bool_discriminator_column_works(bool async)
    {
        var contextFactory = await InitializeAsync<Context24657>(seed: c => c.Seed());
        using var context = contextFactory.CreateContext();

        var query = context.Authors.Include(e => e.Blog);

        var authors = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Equal(2, authors.Count);
    }

    protected class Context24657 : DbContext
    {
        public Context24657(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Author> Authors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Blog>()
                .HasDiscriminator<bool>(nameof(Blog.IsPhotoBlog))
                .HasValue<DevBlog>(false)
                .HasValue<PhotoBlog>(true);

        public void Seed()
        {
            Add(new Author { Blog = new DevBlog { Title = "Dev Blog", } });
            Add(new Author { Blog = new PhotoBlog { Title = "Photo Blog", } });

            SaveChanges();
        }
    }

    protected class Author
    {
        public int Id { get; set; }
        public Blog Blog { get; set; }
    }

    protected abstract class Blog
    {
        public int Id { get; set; }
        public bool IsPhotoBlog { get; set; }
        public string Title { get; set; }
    }

    protected class DevBlog : Blog
    {
        public DevBlog()
        {
            IsPhotoBlog = false;
        }
    }

    protected class PhotoBlog : Blog
    {
        public PhotoBlog()
        {
            IsPhotoBlog = true;
        }

        public int NumberOfPhotos { get; set; }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Count_member_over_IReadOnlyCollection_works(bool async)
    {
        var contextFactory = await InitializeAsync<Context26433>(seed: c => c.Seed());
        using var context = contextFactory.CreateContext();

        var query = context.Authors
            .Select(a => new { BooksCount = a.Books.Count });

        var authors = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Equal(3, Assert.Single(authors).BooksCount);
    }

    protected class Context26433 : DbContext
    {
        public Context26433(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Book26433> Books { get; set; }
        public DbSet<Author26433> Authors { get; set; }

        public void Seed()
        {
            base.Add(
                new Author26433
                {
                    FirstName = "William",
                    LastName = "Shakespeare",
                    Books = new List<Book26433>
                    {
                        new() { Title = "Hamlet" },
                        new() { Title = "Othello" },
                        new() { Title = "MacBeth" }
                    }
                });

            SaveChanges();
        }
    }

    protected class Author26433
    {
        [Key]
        public int AuthorId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IReadOnlyCollection<Book26433> Books { get; set; }
    }

    protected class Book26433
    {
        [Key]
        public int BookId { get; set; }

        public string Title { get; set; }
        public int AuthorId { get; set; }
        public Author26433 Author { get; set; }
    }

#nullable enable

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task IsDeleted_query_filter_with_conversion_to_int_works(bool async)
    {
        var contextFactory = await InitializeAsync<Context26428>(seed: c => c.Seed());
        using var context = contextFactory.CreateContext();

        var query = context.Suppliers.Include(s => s.Location).OrderBy(s => s.Name);

        var suppliers = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Equal(4, suppliers.Count);
        Assert.Single(suppliers.Where(e => e.Location != null));
    }

    protected class Context26428 : DbContext
    {
        public Context26428(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Supplier> Suppliers
            => Set<Supplier>();

        public DbSet<Location> Locations
            => Set<Location>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Supplier>().Property(s => s.IsDeleted).HasConversion<int>();
            modelBuilder.Entity<Supplier>().HasQueryFilter(s => !s.IsDeleted);

            modelBuilder.Entity<Location>().Property(l => l.IsDeleted).HasConversion<int>();
            modelBuilder.Entity<Location>().HasQueryFilter(l => !l.IsDeleted);
        }

        public void Seed()
        {
            var activeAddress = new Location { Address = "Active address", IsDeleted = false };
            var deletedAddress = new Location { Address = "Deleted address", IsDeleted = true };

            var activeSupplier1 = new Supplier
            {
                Name = "Active supplier 1",
                IsDeleted = false,
                Location = activeAddress
            };
            var activeSupplier2 = new Supplier
            {
                Name = "Active supplier 2",
                IsDeleted = false,
                Location = deletedAddress
            };
            var activeSupplier3 = new Supplier { Name = "Active supplier 3", IsDeleted = false };
            var deletedSupplier = new Supplier { Name = "Deleted supplier", IsDeleted = false };

            AddRange(activeAddress, deletedAddress);
            AddRange(activeSupplier1, activeSupplier2, activeSupplier3, deletedSupplier);

            SaveChanges();
        }
    }

    protected class Supplier
    {
        public Guid SupplierId { get; set; }
        public string Name { get; set; } = null!;
        public Location? Location { get; set; }
        public bool IsDeleted { get; set; }
    }

    protected class Location
    {
        public Guid LocationId { get; set; }
        public string Address { get; set; } = null!;
        public bool IsDeleted { get; set; }
    }

#nullable disable

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Unwrap_convert_node_over_projection_when_translating_contains_over_subquery(bool async)
    {
        var contextFactory = await InitializeAsync<Context26593>(seed: c => c.Seed());
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
        var contextFactory = await InitializeAsync<Context26593>(seed: c => c.Seed());
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
        var contextFactory = await InitializeAsync<Context26593>(seed: c => c.Seed());
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

    protected class Context26593 : DbContext
    {
        public Context26593(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Membership> Memberships { get; set; }

        public void Seed()
        {
            var user = new User();
            var group = new Group();
            var membership = new Membership { Group = group, User = user };
            AddRange(user, group, membership);

            SaveChanges();
        }
    }

    protected class User
    {
        public int Id { get; set; }

        public ICollection<Membership> Memberships { get; set; }
    }

    protected class Group
    {
        public int Id { get; set; }

        public ICollection<Membership> Memberships { get; set; }
    }

    protected class Membership
    {
        public int Id { get; set; }
        public User User { get; set; }
        public int UserId { get; set; }
        public Group Group { get; set; }
        public int GroupId { get; set; }
    }

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

    protected class Context26587 : DbContext
    {
        public Context26587(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<OrderItem> OrderItems { get; set; }
    }

    protected class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public DateTime? ShippingDate { get; set; }
        public DateTime? CancellationDate { get; set; }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Enum_with_value_converter_matching_take_value(bool async)
    {
        var contextFactory = await InitializeAsync<Context26472>();
        using var context = contextFactory.CreateContext();
        var orderItemType = OrderItemType.MyType1;
        var query = context.Orders.Where(x => x.Items.Any()).OrderBy(e => e.Id).Take(1)
            .Select(e => e.Id)
            .Join(context.Orders, o => o, i => i.Id, (o, i) => i)
            .Select(entity => new
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

    protected class Context26472 : DbContext
    {
        public Context26472(DbContextOptions options)
               : base(options)
        {
        }

        public virtual DbSet<Order26472> Orders { get; set; }
        public virtual DbSet<OrderItem26472> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderItem26472>().Property(x => x.Type).HasConversion<string>();
        }
    }

    protected class Order26472
    {
        public int Id { get; set; }

        public virtual ICollection<OrderItem26472> Items { get; set; }
    }

    protected class OrderItem26472
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public OrderItemType Type { get; set; }
        public double Price { get; set; }
    }

    protected enum OrderItemType
    {
        Undefined = 0,
        MyType1 = 1,
        MyType2 = 2
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_Aggregate_over_navigations_repeated(bool async)
    {
        var contextFactory = await InitializeAsync<Context27083>(seed: c => c.Seed());
        using var context = contextFactory.CreateContext();

        var query = context
            .Set<TimeSheet>()
            .Where(x => x.OrderId != null)
            .GroupBy(x => x.OrderId)
            .Select(x => new
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
        var contextFactory = await InitializeAsync<Context27083>(seed: c => c.Seed());
        using var context = contextFactory.CreateContext();

        Expression<Func<Order, bool>> someFilterFromOutside = x => x.Number != "A1";

        var query = context
            .Set<Order>()
            .Where(someFilterFromOutside)
            .GroupBy(x => new { x.CustomerId, x.Number })
            .Select(x => new
            {
                x.Key.CustomerId,
                CustomerMinHourlyRate = context.Set<Order>().Where(n => n.CustomerId == x.Key.CustomerId).Min(h => h.HourlyRate),
                HourlyRate = x.Min(f => f.HourlyRate),
                Count = x.Count()
            });

        var orders = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Collection(orders.OrderBy(x => x.CustomerId),
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

    protected class Context27083 : DbContext
    {
        public Context27083(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<TimeSheet> TimeSheets { get; set; }
        public DbSet<Customer> Customers { get; set; }

        public void Seed()
        {
            var customerA = new Customer { Name = "Customer A" };
            var customerB = new Customer { Name = "Customer B" };

            var projectA = new Project { Customer = customerA };
            var projectB = new Project { Customer = customerB };

            var orderA1 = new Order { Number = "A1", Customer = customerA, HourlyRate = 10 };
            var orderA2 = new Order { Number = "A2", Customer = customerA, HourlyRate = 11 };
            var orderB1 = new Order { Number = "B1", Customer = customerB, HourlyRate = 20 };

            var timeSheetA = new TimeSheet { Order = orderA1, Project = projectA };
            var timeSheetB = new TimeSheet { Order = orderB1, Project = projectB };

            AddRange(customerA, customerB);
            AddRange(projectA, projectB);
            AddRange(orderA1, orderA2, orderB1);
            AddRange(timeSheetA, timeSheetB);
            SaveChanges();
        }
    }

    protected class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public List<Project> Projects { get; set; }
        public List<Order> Orders { get; set; }
    }

    protected class Order
    {
        public int Id { get; set; }
        public string Number { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public int HourlyRate { get; set; }
    }

    protected class Project
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }

        public Customer Customer { get; set; }
    }

    protected class TimeSheet
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }
        public Project Project { get; set; }

        public int? OrderId { get; set; }
        public Order Order { get; set; }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Aggregate_over_subquery_in_group_by_projection_2(bool async)
    {
        var contextFactory = await InitializeAsync<Context27094>();
        using var context = contextFactory.CreateContext();

        var query = from t in context.Table
                    group t.Id by t.Value into tg
                    select new
                    {
                        A = tg.Key,
                        B = context.Table.Where(t => t.Value == tg.Max() * 6).Max(t => (int?)t.Id),
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

        var query = from t in context.Table
                    group t.Id by t.Value into tg
                    select new
                    {
                        A = tg.Key,
                        B = tg.Sum(),
                        C = (from t in context.Table
                             group t.Id by t.Value into tg2
                             select tg.Sum() + tg2.Sum()
                             ).OrderBy(e => 1).FirstOrDefault()
                    };

        var orders = async
            ? await query.ToListAsync()
            : query.ToList();
    }

    protected class Context27094 : DbContext
    {
        public Context27094(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Table> Table { get; set; }
    }

    protected class Table
    {
        public int Id { get; set; }
        public int? Value { get; set; }
    }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Group_by_multiple_aggregate_joining_different_tables(bool async)
        {
            var contextFactory = await InitializeAsync<Context27163>();
            using var context = contextFactory.CreateContext();

            var query = context.Parents
                .GroupBy(x => new { })
                .Select(g => new
                {
                    Test1 = g
                        .Select(x => x.Child1.Value1)
                        .Distinct()
                        .Count(),
                    Test2 = g
                        .Select(x => x.Child2.Value2)
                        .Distinct()
                        .Count()
                });

            var orders = async
                ? await query.ToListAsync()
                : query.ToList();
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Group_by_multiple_aggregate_joining_different_tables_with_query_filter(bool async)
        {
            var contextFactory = await InitializeAsync<Context27163>();
            using var context = contextFactory.CreateContext();

            var query = context.Parents
                .GroupBy(x => new { })
                .Select(g => new
                {
                    Test1 = g
                        .Select(x => x.ChildFilter1.Value1)
                        .Distinct()
                        .Count(),
                    Test2 = g
                        .Select(x => x.ChildFilter2.Value2)
                        .Distinct()
                        .Count()
                });

            var orders = async
                ? await query.ToListAsync()
                : query.ToList();
        }

        protected class Context27163 : DbContext
        {
            public Context27163(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Parent> Parents { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<ChildFilter1>().HasQueryFilter(e => e.Filter1 == "Filter1");
                modelBuilder.Entity<ChildFilter2>().HasQueryFilter(e => e.Filter2 == "Filter2");
            }
        }

        public class Parent
        {
            public int Id { get; set; }
            public Child1 Child1 { get; set; }
            public Child2 Child2 { get; set; }
            public ChildFilter1 ChildFilter1 { get; set; }
            public ChildFilter2 ChildFilter2 { get; set; }
        }

        public class Child1
        {
            public int Id { get; set; }
            public string Value1 { get; set; }
        }

        public class Child2
        {
            public int Id { get; set; }
            public string Value2 { get; set; }
        }

        public class ChildFilter1
        {
            public int Id { get; set; }
            public string Filter1 { get; set; }
            public string Value1 { get; set; }
        }

        public class ChildFilter2
        {
            public int Id { get; set; }
            public string Filter2 { get; set; }
            public string Value2 { get; set; }
        }
}
