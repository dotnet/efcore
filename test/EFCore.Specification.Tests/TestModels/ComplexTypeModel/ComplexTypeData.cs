// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ComplexTypeModel;

#nullable enable

public class ComplexTypeData : ISetSource
{
    private readonly IReadOnlyList<Customer> _customers;
    private readonly IReadOnlyList<CustomerGroup> _customerGroups;

    public ComplexTypeData()
    {
        _customers = CreateCustomers();
        _customerGroups = CreateCustomerGroups(_customers);
    }

    public IQueryable<TEntity> Set<TEntity>()
        where TEntity : class
    {
        if (typeof(TEntity) == typeof(Customer))
        {
            return (IQueryable<TEntity>)_customers.AsQueryable();
        }

        if (typeof(TEntity) == typeof(CustomerGroup))
        {
            return (IQueryable<TEntity>)_customerGroups.AsQueryable();
        }

        throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
    }

    private static IReadOnlyList<Customer> CreateCustomers()
    {
        var address1 = new Address
        {
            AddressLine1 = "804 S. Lakeshore Road",
            ZipCode = 38654,
            Country = new Country { FullName = "United States", Code = "US" }
        };

        var customer1 = new Customer
        {
            Id = 1,
            Name = "Mona Cy",
            ShippingAddress = address1,
            BillingAddress = address1
        };

        var customer2 = new Customer
        {
            Id = 2,
            Name = "Antigonus Mitul",
            ShippingAddress = new Address
            {
                AddressLine1 = "72 Hickory Rd.",
                ZipCode = 07728,
                Country = new Country { FullName = "Germany", Code = "DE" }
            },
            BillingAddress = new Address
            {
                AddressLine1 = "79 Main St.",
                ZipCode = 29293,
                Country = new Country { FullName = "Germany", Code = "DE" }
            }
        };

        var address3 = new Address
        {
            AddressLine1 = "79 Main St.",
            ZipCode = 29293,
            Country = new Country { FullName = "Germany", Code = "DE" }
        };

        var customer3 = new Customer
        {
            Id = 3,
            Name = "Monty Elias",
            ShippingAddress = address3,
            BillingAddress = address3
        };

        return new List<Customer> { customer1, customer2, customer3 };
    }

    private static IReadOnlyList<CustomerGroup> CreateCustomerGroups(IReadOnlyList<Customer> customers)
    {
        var group1 = new CustomerGroup
        {
            Id = 1,
            RequiredCustomer = customers[0],
            OptionalCustomer = customers[1]
        };

        var group2 = new CustomerGroup
        {
            Id = 2,
            RequiredCustomer = customers[1],
            OptionalCustomer = customers[0]
        };

        var group3 = new CustomerGroup
        {
            Id = 3,
            RequiredCustomer = customers[0],
            OptionalCustomer = null
        };

        return new List<CustomerGroup>
        {
            group1,
            group2,
            group3
        };
    }

    public static void Seed(PoolableDbContext context)
    {
        var customers = CreateCustomers();
        var customerGroups = CreateCustomerGroups(customers);

        context.Set<Customer>().AddRange(customers);
        context.Set<CustomerGroup>().AddRange(customerGroups);

        context.SaveChanges();
    }
}
