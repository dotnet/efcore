// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure.Internal;

public class ConcurrencyDetectorTest
{
    [Fact]
    public void MultipleContextOnSameThread_Should_Success()
    {
        var preparingContext = new Context();

        var customer = new Customer { FirstName = "John", LastName = "Doe" };

        preparingContext.Customers.Add(customer);
        preparingContext.SaveChanges();

        var order = new Order { CustomerId = customer.CustomerId, OrderDate = DateTime.Now };

        preparingContext.Orders.Add(order);
        preparingContext.SaveChanges();

        var context = new Context();

        var exception = Record.Exception(()
            => context.Orders.Select(o => new { Date = o.OrderDate, Name = GetCustomer(o.OrderId, context) }).ToArray());

        Assert.Null(exception);
    }

    [Fact] // Issue #22802
    public async Task Pooled_context_can_be_reused_after_critical_section_was_never_exited()
    {
        var factory = new PooledDbContextFactory<Context>(
            new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(nameof(Pooled_context_can_be_reused_after_critical_section_was_never_exited))
                .Options);

        // AsyncLocal changes made inside Task.Run do not flow back to the caller, so the next lease below sees a clean flow like a new request would
        var leaked = await Task.Run(
            async () =>
            {
                using var context = factory.CreateDbContext();
                await context.Customers.ToListAsync();
                context.GetService<IConcurrencyDetector>().EnterCriticalSection();

                return context;
            });

        using var context = factory.CreateDbContext();

        Assert.Same(leaked, context);
        await context.Customers.ToListAsync();
    }

    [Fact]
    public async Task ResetState_releases_critical_section_that_was_never_exited()
    {
        var detector = new ConcurrencyDetector();

        await Task.Run(() => detector.EnterCriticalSection());

        Assert.Equal(
            CoreStrings.ConcurrentMethodInvocation,
            Assert.Throws<InvalidOperationException>(() => detector.EnterCriticalSection()).Message);

        detector.ResetState();

        detector.EnterCriticalSection().Dispose();
    }

    [Fact]
    public async Task Nested_critical_sections_are_released_when_outermost_exits()
    {
        var detector = new ConcurrencyDetector();

        var outer = detector.EnterCriticalSection();
        detector.EnterCriticalSection().Dispose();

        await RunOnUnrelatedFlow(() => Assert.Throws<InvalidOperationException>(() => detector.EnterCriticalSection()));

        outer.Dispose();

        await RunOnUnrelatedFlow(() => detector.EnterCriticalSection().Dispose());
    }

    // Task.Run flows the caller's AsyncLocal state, so suppress it to behave like a different request
    private static Task RunOnUnrelatedFlow(Action action)
    {
        using (ExecutionContext.SuppressFlow())
        {
            return Task.Run(action);
        }
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
        public Context()
        {
        }

        public Context(DbContextOptions<Context> options)
            : base(options)
        {
        }

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseInMemoryDatabase(nameof(ConcurrencyDetectorTest));
            }
        }

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().HasKey(c => c.CustomerId);
            modelBuilder.Entity<Order>().HasKey(o => o.OrderId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId);
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<Customer> Customers { get; set; } = null!;

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<Order> Orders { get; set; } = null!;
    }

    public class Customer
    {
        public int CustomerId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public virtual ICollection<Order> Orders { get; set; } = [];
    }

    public class Order
    {
        private readonly ILazyLoader? _lazyLoader;
        private Customer? _customer;

        public Order()
        {
        }

        public Order(ILazyLoader lazyLoader)
            => _lazyLoader = lazyLoader;

        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public int CustomerId { get; set; }

        public Customer Customer
        {
            get => _lazyLoader.Load(this, ref _customer)!;
            set => _customer = value;
        }
    }
}
