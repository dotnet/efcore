// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query;

public abstract class NorthwindChangeTrackingQueryTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
{
    protected NorthwindChangeTrackingQueryTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

    protected TFixture Fixture { get; }

    [ConditionalFact]
    public virtual void Entity_reverts_when_state_set_to_unchanged()
    {
        using var context = CreateContext();
        var customer = context.Customers.First();

        Assert.NotEqual("425-882-8080", customer.Phone);

        var firstTrackedEntity = context.ChangeTracker.Entries<Customer>().Single();
        var originalPhoneNumber = customer.Phone;

        customer.Phone = "425-882-8080";

        context.ChangeTracker.DetectChanges();

        Assert.Equal(customer.CustomerID, firstTrackedEntity.Property(c => c.CustomerID).CurrentValue);
        Assert.Equal(EntityState.Modified, firstTrackedEntity.State);
        Assert.Equal("425-882-8080", firstTrackedEntity.Property(c => c.Phone).CurrentValue);

        firstTrackedEntity.State = EntityState.Unchanged;

        Assert.Equal(customer.CustomerID, firstTrackedEntity.Property(c => c.CustomerID).CurrentValue);
        Assert.Equal(originalPhoneNumber, firstTrackedEntity.Property(c => c.Phone).CurrentValue);
        Assert.Equal(EntityState.Unchanged, firstTrackedEntity.State);
    }

    [ConditionalFact]
    public virtual void Multiple_entities_can_revert()
    {
        using var context = CreateContext();
        var customerPostalCodes = context.Customers.Select(c => c.PostalCode).ToList();
        var customerRegion = context.Customers.Select(c => c.Region).ToList();

        foreach (var customer in context.Customers)
        {
            customer.PostalCode = "98052";
            customer.Region = "'Murica";
        }

        Assert.Equal(91, context.ChangeTracker.Entries().Count());
        Assert.Equal("98052", context.Customers.First().PostalCode);
        Assert.Equal("'Murica", context.Customers.First().Region);

        foreach (var entityEntry in context.ChangeTracker.Entries().ToList())
        {
            entityEntry.State = EntityState.Unchanged;
        }

        var newCustomerPostalCodes = context.Customers.Select(c => c.PostalCode);
        var newCustomerRegion = context.Customers.Select(c => c.Region);

        Assert.Equal(customerPostalCodes, newCustomerPostalCodes);
        Assert.Equal(customerRegion, newCustomerRegion);
    }

    [ConditionalFact]
    public virtual void Entity_does_not_revert_when_attached_on_DbContext()
    {
        using var context = CreateContext();
        var customer = context.Customers.First();
        var firstTrackedEntity = context.ChangeTracker.Entries<Customer>().Single();

        Assert.Equal(EntityState.Unchanged, firstTrackedEntity.State);
        Assert.NotEqual("425-882-8080", customer.Phone);
        Assert.NotEqual("425-882-8080", firstTrackedEntity.Property(c => c.Phone).OriginalValue);

        customer.Phone = "425-882-8080";
        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Modified, firstTrackedEntity.State);

        context.Attach(customer);

        Assert.Equal(customer.CustomerID, firstTrackedEntity.Property(c => c.CustomerID).CurrentValue);
        Assert.Equal(EntityState.Unchanged, firstTrackedEntity.State);
        Assert.Equal("425-882-8080", firstTrackedEntity.Property(c => c.Phone).CurrentValue);
        Assert.Equal("425-882-8080", firstTrackedEntity.Property(c => c.Phone).OriginalValue);
    }

    [ConditionalFact]
    public virtual void Entity_does_not_revert_when_attached_on_DbSet()
    {
        using var context = CreateContext();
        var customer = context.Customers.First();
        var firstTrackedEntity = context.ChangeTracker.Entries<Customer>().Single();

        Assert.Equal(EntityState.Unchanged, firstTrackedEntity.State);
        Assert.NotEqual("425-882-8080", customer.Phone);
        Assert.NotEqual("425-882-8080", firstTrackedEntity.Property(c => c.Phone).OriginalValue);

        customer.Phone = "425-882-8080";
        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Modified, firstTrackedEntity.State);

        context.Customers.Attach(customer);

        Assert.Equal(customer.CustomerID, firstTrackedEntity.Property(c => c.CustomerID).CurrentValue);
        Assert.Equal(EntityState.Unchanged, firstTrackedEntity.State);
        Assert.Equal("425-882-8080", firstTrackedEntity.Property(c => c.Phone).CurrentValue);
        Assert.Equal("425-882-8080", firstTrackedEntity.Property(c => c.Phone).OriginalValue);
    }

    // ReSharper disable PossibleMultipleEnumeration
    [ConditionalFact]
    public virtual void Entity_range_does_not_revert_when_attached_dbContext()
    {
        using var context = CreateContext();
        var customers = context.Customers.OrderBy(c => c.CustomerID).Take(2);

        var customer0 = customers.First();
        var customer1 = customers.Skip(1).First();

        var trackedEntity0 = context.ChangeTracker.Entries<Customer>().First();
        var trackedEntity1 = context.ChangeTracker.Entries<Customer>().Skip(1).First();

        Assert.Equal(EntityState.Unchanged, trackedEntity0.State);
        Assert.Equal(EntityState.Unchanged, trackedEntity1.State);
        Assert.NotEqual("425-882-8080", customer0.Phone);
        Assert.NotEqual("425-882-8080", customer1.Phone);
        Assert.NotEqual("425-882-8080", trackedEntity0.Property(c => c.Phone).OriginalValue);
        Assert.NotEqual("425-882-8080", trackedEntity1.Property(c => c.Phone).OriginalValue);

        customer0.Phone = "425-882-8080";
        customer1.Phone = "425-882-8080";
        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Modified, trackedEntity0.State);
        Assert.Equal(EntityState.Modified, trackedEntity1.State);

        context.AttachRange(customers);

        Assert.Equal(customer0.CustomerID, trackedEntity0.Property(c => c.CustomerID).CurrentValue);
        Assert.Equal(customer1.CustomerID, trackedEntity1.Property(c => c.CustomerID).CurrentValue);
        Assert.Equal(EntityState.Unchanged, trackedEntity0.State);
        Assert.Equal(EntityState.Unchanged, trackedEntity1.State);
        Assert.Equal("425-882-8080", trackedEntity0.Property(c => c.Phone).CurrentValue);
        Assert.Equal("425-882-8080", trackedEntity1.Property(c => c.Phone).CurrentValue);
        Assert.Equal("425-882-8080", trackedEntity0.Property(c => c.Phone).OriginalValue);
        Assert.Equal("425-882-8080", trackedEntity1.Property(c => c.Phone).OriginalValue);
    }

    [ConditionalFact]
    public virtual void Entity_range_does_not_revert_when_attached_dbSet()
    {
        using var context = CreateContext();
        var customers = context.Customers.OrderBy(c => c.CustomerID).Take(2);

        var customer0 = customers.First();
        var customer1 = customers.Skip(1).First();

        var trackedEntity0 = context.ChangeTracker.Entries<Customer>().First();
        var trackedEntity1 = context.ChangeTracker.Entries<Customer>().Skip(1).First();

        Assert.Equal(EntityState.Unchanged, trackedEntity0.State);
        Assert.Equal(EntityState.Unchanged, trackedEntity1.State);
        Assert.NotEqual("425-882-8080", customer0.Phone);
        Assert.NotEqual("425-882-8080", customer1.Phone);
        Assert.NotEqual("425-882-8080", trackedEntity0.Property(c => c.Phone).OriginalValue);
        Assert.NotEqual("425-882-8080", trackedEntity1.Property(c => c.Phone).OriginalValue);

        customer0.Phone = "425-882-8080";
        customer1.Phone = "425-882-8080";
        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Modified, trackedEntity0.State);
        Assert.Equal(EntityState.Modified, trackedEntity1.State);

        context.Customers.AttachRange(customers);

        Assert.Equal(customer0.CustomerID, trackedEntity0.Property(c => c.CustomerID).CurrentValue);
        Assert.Equal(customer1.CustomerID, trackedEntity1.Property(c => c.CustomerID).CurrentValue);
        Assert.Equal(EntityState.Unchanged, trackedEntity0.State);
        Assert.Equal(EntityState.Unchanged, trackedEntity1.State);
        Assert.Equal("425-882-8080", trackedEntity0.Property(c => c.Phone).CurrentValue);
        Assert.Equal("425-882-8080", trackedEntity1.Property(c => c.Phone).CurrentValue);
        Assert.Equal("425-882-8080", trackedEntity0.Property(c => c.Phone).OriginalValue);
        Assert.Equal("425-882-8080", trackedEntity1.Property(c => c.Phone).OriginalValue);
    }
    // ReSharper restore PossibleMultipleEnumeration

    [ConditionalFact]
    public virtual void Can_disable_and_reenable_query_result_tracking()
    {
        using var context = CreateContext();
        Assert.Equal(QueryTrackingBehavior.TrackAll, context.ChangeTracker.QueryTrackingBehavior);

        var query = context.Employees.OrderBy(e => e.EmployeeID);

        var results = query.Take(1).ToList();

        Assert.Single(results);
        Assert.Single(context.ChangeTracker.Entries());

        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        results = query.Skip(1).Take(1).ToList();

        Assert.Single(results);
        Assert.Single(context.ChangeTracker.Entries());

        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

        results = query.ToList();

        Assert.Equal(9, results.Count);
        Assert.Equal(9, context.ChangeTracker.Entries().Count());
    }

    [ConditionalFact]
    public virtual void Can_disable_and_reenable_query_result_tracking_starting_with_NoTracking()
    {
        using var context = CreateNoTrackingContext();
        Assert.Equal(QueryTrackingBehavior.NoTracking, context.ChangeTracker.QueryTrackingBehavior);

        var query = context.Employees.OrderBy(e => e.EmployeeID);

        var results = query.Take(1).ToList();

        Assert.Single(results);
        Assert.Empty(context.ChangeTracker.Entries());

        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

        results = query.Skip(1).Take(1).ToList();

        Assert.Single(results);
        Assert.Single(context.ChangeTracker.Entries());
    }

    [ConditionalFact]
    public virtual void Can_disable_and_reenable_query_result_tracking_query_caching()
    {
        using (var context = CreateContext())
        {
            Assert.Equal(QueryTrackingBehavior.TrackAll, context.ChangeTracker.QueryTrackingBehavior);

            var results = context.Employees.ToList();

            Assert.Equal(9, results.Count);
            Assert.Equal(9, context.ChangeTracker.Entries().Count());
        }

        using (var context = CreateContext())
        {
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var results = context.Employees.ToList();

            Assert.Equal(9, results.Count);
            Assert.Empty(context.ChangeTracker.Entries());

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
        }
    }

    [ConditionalFact]
    public virtual void Can_disable_and_reenable_query_result_tracking_query_caching_using_options()
    {
        using (var context = CreateContext())
        {
            Assert.Equal(QueryTrackingBehavior.TrackAll, context.ChangeTracker.QueryTrackingBehavior);

            var results = context.Employees.ToList();

            Assert.Equal(9, results.Count);
            Assert.Equal(9, context.ChangeTracker.Entries().Count());
        }

        using (var context = CreateNoTrackingContext())
        {
            Assert.Equal(QueryTrackingBehavior.NoTracking, context.ChangeTracker.QueryTrackingBehavior);

            var results = context.Employees.ToList();

            Assert.Equal(9, results.Count);
            Assert.Empty(context.ChangeTracker.Entries());
        }
    }

    [ConditionalFact]
    public virtual void Can_disable_and_reenable_query_result_tracking_query_caching_single_context()
    {
        using var context = CreateContext();
        Assert.Equal(QueryTrackingBehavior.TrackAll, context.ChangeTracker.QueryTrackingBehavior);

        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        var results = context.Employees.ToList();

        Assert.Equal(9, results.Count);
        Assert.Empty(context.ChangeTracker.Entries());

        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

        results = context.Employees.ToList();

        Assert.Equal(9, results.Count);
        Assert.Equal(9, context.ChangeTracker.Entries().Count());
    }

    [ConditionalFact]
    public virtual void AsTracking_switches_tracking_on_when_off_in_options()
    {
        using var context = CreateNoTrackingContext();
        var results = context.Employees.AsTracking().ToList();

        Assert.Equal(9, results.Count);
        Assert.Equal(9, context.ChangeTracker.Entries().Count());
    }

    [ConditionalFact]
    public virtual void Precedence_of_tracking_modifiers()
    {
        using var context = CreateContext();
        var results = context.Employees.AsNoTracking().AsTracking().ToList();

        Assert.Equal(9, results.Count);
        Assert.Equal(9, context.ChangeTracker.Entries().Count());
    }

    [ConditionalFact]
    public virtual void Precedence_of_tracking_modifiers2()
    {
        using var context = CreateContext();
        var results = context.Employees.AsTracking().AsNoTracking().ToList();

        Assert.Equal(9, results.Count);
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [ConditionalFact]
    public virtual void Precedence_of_tracking_modifiers3()
    {
        using var context = CreateContext();
        var customers
            = (from c in context.Set<Customer>().AsNoTracking()
               join o in context.Set<Order>().AsTracking()
                   on c.CustomerID equals o.CustomerID
               where c.CustomerID == "ALFKI"
               select o)
            .ToList();

        Assert.Equal(6, customers.Count);
        Assert.Equal(6, context.ChangeTracker.Entries().Count());
    }

    [ConditionalFact]
    public virtual void Precedence_of_tracking_modifiers4()
    {
        using var context = CreateContext();
        var customers
            = (from c in context.Set<Customer>().AsTracking()
               join o in context.Set<Order>().AsNoTracking()
                   on c.CustomerID equals o.CustomerID
               where c.CustomerID == "ALFKI"
               select o)
            .ToList();

        Assert.Equal(6, customers.Count);
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [ConditionalFact]
    public virtual void Precedence_of_tracking_modifiers5()
    {
        using var context = CreateContext();
        var customers
            = (from c in context.Set<Customer>().AsTracking()
               join o in context.Set<Order>()
                   on c.CustomerID equals o.CustomerID
               where c.CustomerID == "ALFKI"
               select o)
            .AsNoTracking()
            .ToList();

        Assert.Equal(6, customers.Count);
        Assert.Empty(context.ChangeTracker.Entries());
    }

    protected NorthwindContext CreateContext()
        => Fixture.CreateContext();

    protected virtual NorthwindContext CreateNoTrackingContext()
        => new(
            new DbContextOptionsBuilder(Fixture.CreateOptions())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).Options);
}
