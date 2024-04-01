// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class AdHocQueryFiltersQueryTestBase : NonSharedModelTestBase
{
    protected override string StoreName
        => "AdHocQueryFiltersQueryTests";

    #region 10295

    [ConditionalFact]
    public virtual async Task Query_filter_with_contains_evaluates_correctly()
    {
        var contextFactory = await InitializeAsync<Context10295>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();
        var result = context.Entities.ToList();
        Assert.Single(result);
    }

    protected class Context10295(DbContextOptions options) : DbContext(options)
    {
        private readonly List<int> _ids = [1, 7];

        public DbSet<MyEntity10295> Entities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<MyEntity10295>().HasQueryFilter(x => !_ids.Contains(x.Id));

        public Task SeedAsync()
        {
            var e1 = new MyEntity10295 { Name = "Name1" };
            var e2 = new MyEntity10295 { Name = "Name2" };
            Entities.AddRange(e1, e2);
            return SaveChangesAsync();
        }

        public class MyEntity10295
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }

    #endregion

    #region 10301

    [ConditionalFact]
    public virtual async Task MultiContext_query_filter_test()
    {
        var contextFactory = await InitializeAsync<FilterContext10301>(seed: c => c.SeedAsync());

        using (var context = contextFactory.CreateContext())
        {
            Assert.Empty(context.Blogs.ToList());

            context.Tenant = 1;
            Assert.Single(context.Blogs.ToList());

            context.Tenant = 2;
            Assert.Equal(2, context.Blogs.Count());
        }
    }

    protected class FilterContextBase10301(DbContextOptions options) : DbContext(options)
    {
        public int Tenant { get; set; }

        public DbSet<Blog10301> Blogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Blog10301>().HasQueryFilter(e => e.SomeValue == Tenant);

        public Task SeedAsync()
        {
            AddRange(
                new Blog10301 { SomeValue = 1 },
                new Blog10301 { SomeValue = 2 },
                new Blog10301 { SomeValue = 2 }
            );

            return SaveChangesAsync();
        }

        public class Blog10301
        {
            public int Id { get; set; }
            public int SomeValue { get; set; }
        }
    }

    protected class FilterContext10301(DbContextOptions options) : FilterContextBase10301(options);

    #endregion

    #region 12170

    [ConditionalFact]
    public virtual async Task Weak_entities_with_query_filter_subquery_flattening()
    {
        var contextFactory = await InitializeAsync<Context12170>();
        using var context = contextFactory.CreateContext();
        var result = context.Definitions.Any();

        Assert.False(result);
    }

    protected class Context12170(DbContextOptions options) : DbContext(options)
    {
        public virtual DbSet<Definition12170> Definitions { get; set; }
        public virtual DbSet<DefinitionHistory12170> DefinitionHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Definition12170>().HasQueryFilter(md => md.ChangeInfo.RemovedPoint.Timestamp == null);
            modelBuilder.Entity<Definition12170>().HasOne(h => h.LatestHistoryEntry).WithMany();
            modelBuilder.Entity<Definition12170>().HasMany(h => h.HistoryEntries).WithOne(h => h.Definition);

            modelBuilder.Entity<DefinitionHistory12170>().OwnsOne(h => h.EndedPoint);
        }

        [Owned]
        public class OptionalChangePoint12170
        {
            public int Value { get; set; }
            public DateTime? Timestamp { get; set; }
        }

        [Owned]
        public class MasterChangeInfo12170
        {
            public bool Exists { get; set; }
            public virtual OptionalChangePoint12170 RemovedPoint { get; set; }
        }

        public class DefinitionHistory12170
        {
            public int Id { get; set; }
            public int MacGuffinDefinitionID { get; set; }
            public virtual Definition12170 Definition { get; set; }
            public OptionalChangePoint12170 EndedPoint { get; set; }
        }

        public class Definition12170
        {
            public int Id { get; set; }
            public virtual MasterChangeInfo12170 ChangeInfo { get; set; }

            public virtual ICollection<DefinitionHistory12170> HistoryEntries { get; set; }
            public virtual DefinitionHistory12170 LatestHistoryEntry { get; set; }
            public int? LatestHistoryEntryID { get; set; }
        }
    }

    #endregion

    #region 13517

    [ConditionalFact]
    public virtual async Task Query_filter_with_pk_fk_optimization()
    {
        var contextFactory = await InitializeAsync<Context13517>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();
        context.Entities.Select(
            s =>
                new Context13517.EntityDto13517
                {
                    Id = s.Id,
                    RefEntity = s.RefEntity == null
                        ? null
                        : new Context13517.RefEntityDto13517 { Id = s.RefEntity.Id, Public = s.RefEntity.Public },
                    RefEntityId = s.RefEntityId
                }).Single(p => p.Id == 1);
    }

    protected class Context13517(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity13517> Entities { get; set; }
        public DbSet<RefEntity13517> RefEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<RefEntity13517>().HasQueryFilter(f => f.Public);

        public Task SeedAsync()
        {
            var refEntity = new RefEntity13517 { Public = false };
            RefEntities.Add(refEntity);
            Entities.Add(new Entity13517 { RefEntity = refEntity });
            return SaveChangesAsync();
        }

        public class Entity13517
        {
            public int Id { get; set; }
            public int? RefEntityId { get; set; }
            public RefEntity13517 RefEntity { get; set; }
        }

        public class RefEntity13517
        {
            public int Id { get; set; }
            public bool Public { get; set; }
        }

        public class EntityDto13517
        {
            public int Id { get; set; }
            public int? RefEntityId { get; set; }
            public RefEntityDto13517 RefEntity { get; set; }
        }

        public class RefEntityDto13517
        {
            public int Id { get; set; }
            public bool Public { get; set; }
        }
    }

    #endregion

    #region 17253

    [ConditionalFact]
    public virtual async Task Self_reference_in_query_filter_works()
    {
        var contextFactory = await InitializeAsync<Context17253>(seed: c => c.SeedAsync());

        using (var context = contextFactory.CreateContext())
        {
            var query = context.EntitiesWithQueryFilterSelfReference.Where(e => e.Name != "Foo");
            var result = query.ToList();
        }

        using (var context = contextFactory.CreateContext())
        {
            var query = context.EntitiesReferencingEntityWithQueryFilterSelfReference.Where(e => e.Name != "Foo");
            var result = query.ToList();
        }
    }

    protected class Context17253(DbContextOptions options) : DbContext(options)
    {
        public DbSet<EntityWithQueryFilterSelfReference17253> EntitiesWithQueryFilterSelfReference { get; set; }

        public DbSet<EntityReferencingEntityWithQueryFilterSelfReference17253> EntitiesReferencingEntityWithQueryFilterSelfReference
        {
            get;
            set;
        }

        public DbSet<EntityWithQueryFilterCycle17253_1> EntitiesWithQueryFilterCycle1 { get; set; }
        public DbSet<EntityWithQueryFilterCycle17253_2> EntitiesWithQueryFilterCycle2 { get; set; }
        public DbSet<EntityWithQueryFilterCycle17253_3> EntitiesWithQueryFilterCycle3 { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EntityWithQueryFilterSelfReference17253>().HasQueryFilter(e => EntitiesWithQueryFilterSelfReference.Any());
            modelBuilder.Entity<EntityReferencingEntityWithQueryFilterSelfReference17253>()
                .HasQueryFilter(e => Set<EntityWithQueryFilterSelfReference17253>().Any());

            modelBuilder.Entity<EntityWithQueryFilterCycle17253_1>().HasQueryFilter(e => EntitiesWithQueryFilterCycle2.Any());
            modelBuilder.Entity<EntityWithQueryFilterCycle17253_2>().HasQueryFilter(e => Set<EntityWithQueryFilterCycle17253_3>().Any());
            modelBuilder.Entity<EntityWithQueryFilterCycle17253_3>().HasQueryFilter(e => EntitiesWithQueryFilterCycle1.Any());
        }

        public Task SeedAsync()
        {
            EntitiesWithQueryFilterSelfReference.Add(
                new EntityWithQueryFilterSelfReference17253 { Name = "EntityWithQueryFilterSelfReference" });
            EntitiesReferencingEntityWithQueryFilterSelfReference.Add(
                new EntityReferencingEntityWithQueryFilterSelfReference17253
                {
                    Name = "EntityReferencingEntityWithQueryFilterSelfReference"
                });

            EntitiesWithQueryFilterCycle1.Add(new EntityWithQueryFilterCycle17253_1 { Name = "EntityWithQueryFilterCycle1_1" });
            EntitiesWithQueryFilterCycle2.Add(new EntityWithQueryFilterCycle17253_2 { Name = "EntityWithQueryFilterCycle2_1" });
            EntitiesWithQueryFilterCycle3.Add(new EntityWithQueryFilterCycle17253_3 { Name = "EntityWithQueryFilterCycle3_1" });

            return SaveChangesAsync();
        }

        public class EntityWithQueryFilterSelfReference17253
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class EntityReferencingEntityWithQueryFilterSelfReference17253
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class EntityWithQueryFilterCycle17253_1
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class EntityWithQueryFilterCycle17253_2
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class EntityWithQueryFilterCycle17253_3
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }

    #endregion

    #region 18510

    [ConditionalFact]
    public virtual async Task Invoke_inside_query_filter_gets_correctly_evaluated_during_translation()
    {
        var contextFactory = await InitializeAsync<Context18510>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();
        context.TenantId = 1;

        var query1 = context.Entities.ToList();
        Assert.True(query1.All(x => x.TenantId == 1));

        context.TenantId = 2;
        var query2 = context.Entities.ToList();
        Assert.True(query2.All(x => x.TenantId == 2));
    }

    protected class Context18510(DbContextOptions options) : DbContext(options)
    {
        public DbSet<MyEntity18510> Entities { get; set; }

        public int TenantId { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MyEntity18510>().HasQueryFilter(x => x.Name != "Foo");

            var entityType = modelBuilder.Model.GetEntityTypes().Single(et => et.ClrType == typeof(MyEntity18510));
            var queryFilter = entityType.GetQueryFilter();
            Expression<Func<int>> tenantFunc = () => TenantId;
            var tenant = Expression.Invoke(tenantFunc);

            var efPropertyMethod = typeof(EF).GetTypeInfo().GetDeclaredMethod(nameof(EF.Property)).MakeGenericMethod(typeof(int));
            var prm = queryFilter.Parameters[0];
            var efPropertyMethodCall = Expression.Call(efPropertyMethod, prm, Expression.Constant("TenantId"));

            var updatedQueryFilter = Expression.Lambda(
                Expression.AndAlso(
                    queryFilter.Body,
                    Expression.Equal(
                        efPropertyMethodCall,
                        tenant)),
                prm);

            entityType.SetQueryFilter(updatedQueryFilter);
        }

        public Task SeedAsync()
        {
            var e1 = new MyEntity18510 { Name = "e1", TenantId = 1 };
            var e2 = new MyEntity18510 { Name = "e2", TenantId = 2 };
            var e3 = new MyEntity18510 { Name = "e3", TenantId = 2 };
            var e4 = new MyEntity18510 { Name = "Foo", TenantId = 2 };

            Entities.AddRange(e1, e2, e3, e4);
            return SaveChangesAsync();
        }

        public class MyEntity18510
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public int TenantId { get; set; }
        }
    }

    #endregion

    #region 18759

    [ConditionalFact]
    public virtual async Task Query_filter_with_null_constant()
    {
        var contextFactory = await InitializeAsync<Context18759>();
        using var context = contextFactory.CreateContext();
        var people = context.People.ToList();
    }

    protected class Context18759(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Person18759> People { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Person18759>().HasQueryFilter(p => p.UserDelete != null);

        public class Person18759
        {
            public int Id { get; set; }
            public User18759 UserDelete { get; set; }
        }

        public class User18759
        {
            public int Id { get; set; }
        }
    }

    #endregion

    #region 19708

    [ConditionalFact]
    public virtual async Task GroupJoin_SelectMany_gets_flattened()
    {
        var contextFactory = await InitializeAsync<Context19708>(seed: c => c.SeedAsync());
        using (var context = contextFactory.CreateContext())
        {
            var query = context.CustomerFilters.ToList();
        }

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Set<Context19708.CustomerView19708>().ToList();

            Assert.Collection(
                query,
                t => AssertCustomerView(t, 1, "First", 1, "FirstChild"),
                t => AssertCustomerView(t, 2, "Second", 2, "SecondChild1"),
                t => AssertCustomerView(t, 2, "Second", 3, "SecondChild2"),
                t => AssertCustomerView(t, 3, "Third", null, ""));

            static void AssertCustomerView(
                Context19708.CustomerView19708 actual,
                int id,
                string name,
                int? customerMembershipId,
                string customerMembershipName)
            {
                Assert.Equal(id, actual.Id);
                Assert.Equal(name, actual.Name);
                Assert.Equal(customerMembershipId, actual.CustomerMembershipId);
                Assert.Equal(customerMembershipName, actual.CustomerMembershipName);
            }
        }
    }

    protected class Context19708(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Customer19708> Customers { get; set; }
        public DbSet<CustomerMembership19708> CustomerMemberships { get; set; }
        public DbSet<CustomerFilter19708> CustomerFilters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CustomerFilter19708>()
                .HasQueryFilter(
                    e => (from a in (from c in Customers
                                     join cm in CustomerMemberships on c.Id equals cm.CustomerId into g
                                     from cm in g.DefaultIfEmpty()
                                     select new { c.Id, CustomerMembershipId = (int?)cm.Id })
                          where a.CustomerMembershipId != null && a.Id == e.CustomerId
                          select a).Count()
                        > 0)
                .HasKey(e => e.CustomerId);

#pragma warning disable CS0618 // Type or member is obsolete
            modelBuilder.Entity<CustomerView19708>().HasNoKey().ToQuery(Build_Customers_Sql_View_InMemory());
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public Task SeedAsync()
        {
            var customer1 = new Customer19708 { Name = "First" };
            var customer2 = new Customer19708 { Name = "Second" };
            var customer3 = new Customer19708 { Name = "Third" };

            var customerMembership1 = new CustomerMembership19708 { Name = "FirstChild", Customer = customer1 };
            var customerMembership2 = new CustomerMembership19708 { Name = "SecondChild1", Customer = customer2 };
            var customerMembership3 = new CustomerMembership19708 { Name = "SecondChild2", Customer = customer2 };

            AddRange(customer1, customer2, customer3);
            AddRange(customerMembership1, customerMembership2, customerMembership3);

            return SaveChangesAsync();
        }

        private Expression<Func<IQueryable<CustomerView19708>>> Build_Customers_Sql_View_InMemory()
        {
            Expression<Func<IQueryable<CustomerView19708>>> query = () =>
                from customer in Customers
                join customerMembership in CustomerMemberships on customer.Id equals customerMembership.CustomerId into
                    nullableCustomerMemberships
                from customerMembership in nullableCustomerMemberships.DefaultIfEmpty()
                select new CustomerView19708
                {
                    Id = customer.Id,
                    Name = customer.Name,
                    CustomerMembershipId = customerMembership != null ? customerMembership.Id : default(int?),
                    CustomerMembershipName = customerMembership != null ? customerMembership.Name : ""
                };
            return query;
        }

        public class Customer19708
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class CustomerMembership19708
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public int CustomerId { get; set; }
            public Customer19708 Customer { get; set; }
        }

        public class CustomerFilter19708
        {
            public int CustomerId { get; set; }
            public int CustomerMembershipId { get; set; }
        }

        public class CustomerView19708
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int? CustomerMembershipId { get; set; }
            public string CustomerMembershipName { get; set; }
        }
    }

    #endregion

    #region 26428

#nullable enable

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task IsDeleted_query_filter_with_conversion_to_int_works(bool async)
    {
        var contextFactory = await InitializeAsync<Context26428>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();

        var query = context.Suppliers.Include(s => s.Location).OrderBy(s => s.Name);

        var suppliers = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Equal(4, suppliers.Count);
        Assert.Single(suppliers.Where(e => e.Location != null));
    }

    protected class Context26428(DbContextOptions options) : DbContext(options)
    {
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

        public Task SeedAsync()
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

            return SaveChangesAsync();
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

    #endregion

    #region 27163

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Group_by_multiple_aggregate_joining_different_tables(bool async)
    {
        var contextFactory = await InitializeAsync<Context27163>();
        using var context = contextFactory.CreateContext();

        var query = context.Parents
            .GroupBy(x => new { })
            .Select(
                g => new
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
            .Select(
                g => new
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

    protected class Context27163(DbContextOptions options) : DbContext(options)
    {
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

    #endregion
}
