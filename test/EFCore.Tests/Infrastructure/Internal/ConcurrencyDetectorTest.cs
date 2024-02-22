// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure.Internal;

public class ConcurrencyDetectorTest
{
    [ConditionalFact]
    public void MultipleContextOnSameThread_Should_Success()
    {
        var preparingContext = new Context();

        var customer = new Customer
        {
            FirstName = "John",
            LastName = "Doe"
        };

        preparingContext.Customers.Add(customer);
        preparingContext.SaveChanges();

        var order = new Order
        {
            CustomerId = customer.CustomerId,
            OrderDate = DateTime.Now
        };

        preparingContext.Orders.Add(order);
        preparingContext.SaveChanges();

        var context  = new Context();

        var exception = Record.Exception(() => context.Orders.Select(o => new { Date = o.OrderDate, Name = GetCustomer(o.OrderId, context) }).ToArray());

        Assert.Null(exception);
    }

    // method imitates the procedure of loading entities into the cache within a separate context and then retrieves them by attaching them to the original context.
    private static Customer GetCustomer(int orderId, Context originalCtx)
    {
        var context = new Context();
        var orders = context.Orders.ToArray();
        var order = orders.First(o => o.OrderId == orderId);

        context.Entry(order).State = EntityState.Detached;
        context.Dispose();

        var orderCopy = new Order(originalCtx.GetService<ILazyLoader>())
        {
            OrderId = order.OrderId,
            OrderDate = order.OrderDate,
            CustomerId = order.CustomerId
        };

        var entity = originalCtx.Attach(orderCopy);

        entity.State = EntityState.Unchanged;

        return orderCopy.Customer;
    }

    private class Context : DbContext
    {
        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInMemoryDatabase(nameof(ConcurrencyDetectorTest));

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().HasKey(c => c.CustomerId);
            modelBuilder.Entity<Order>().HasKey(o => o.OrderId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId);
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
    }

    public class Customer
    {
        public int CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }

    public class Order
    {
        private readonly ILazyLoader _lazyLoader;
        private Customer _customer;

        public Order()
        {
        }

        public Order(ILazyLoader lazyLoader)
        {
            _lazyLoader = lazyLoader;
        }

        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public int CustomerId { get; set; }

        public Customer Customer
        {
            get => _lazyLoader.Load(this, ref _customer);
            set => _customer = value;
        }
    }
}
