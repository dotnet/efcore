// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocQueryFiltersQueryInMemoryTest(NonSharedFixture fixture) : AdHocQueryFiltersQueryTestBase(fixture)
{
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

            modelBuilder.Entity<CustomerView19708>().HasNoKey().ToInMemoryQuery(Build_Customers_Sql_View_InMemory());
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

    protected override ITestStoreFactory TestStoreFactory
        => InMemoryTestStoreFactory.Instance;
}
