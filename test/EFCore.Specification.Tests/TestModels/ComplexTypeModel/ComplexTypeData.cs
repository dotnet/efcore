// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ComplexTypeModel;

public class ComplexTypeData : ISetSource
{
    private readonly IReadOnlyList<Customer> _customers;
    private readonly IReadOnlyList<CustomerGroup> _customerGroups;
    private readonly IReadOnlyList<ValuedCustomer> _valuedCustomers;
    private readonly IReadOnlyList<ValuedCustomerGroup> _valuedCustomerGroups;

    public ComplexTypeData()
    {
        _customers = CreateCustomers();
        _customerGroups = CreateCustomerGroups(_customers);
        _valuedCustomers = CreateValuedCustomers();
        _valuedCustomerGroups = CreateValuedCustomerGroups(_valuedCustomers);
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

        if (typeof(TEntity) == typeof(ValuedCustomer))
        {
            return (IQueryable<TEntity>)_valuedCustomers.AsQueryable();
        }

        if (typeof(TEntity) == typeof(ValuedCustomerGroup))
        {
            return (IQueryable<TEntity>)_valuedCustomerGroups.AsQueryable();
        }

        throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
    }

    private static IReadOnlyList<Customer> CreateCustomers()
    {
        var address1 = new Address
        {
            AddressLine1 = "804 S. Lakeshore Road",
            ZipCode = 38654,
            Country = new Country { FullName = "United States", Code = "US" },
            Tags = new List<string> { "foo", "bar" }
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
                Country = new Country { FullName = "Germany", Code = "DE" },
                Tags = new List<string> { "baz" }
            },
            BillingAddress = new Address
            {
                AddressLine1 = "79 Main St.",
                ZipCode = 29293,
                Country = new Country { FullName = "Germany", Code = "DE" },
                Tags = new List<string>
                {
                    "a1",
                    "a2",
                    "a3"
                }
            }
        };

        var address3 = new Address
        {
            AddressLine1 = "79 Main St.",
            ZipCode = 29293,
            Country = new Country { FullName = "Germany", Code = "DE" },
            Tags = new List<string> { "foo", "moo" }
        };

        var customer3 = new Customer
        {
            Id = 3,
            Name = "Monty Elias",
            ShippingAddress = address3,
            BillingAddress = address3
        };

        return new List<Customer>
        {
            customer1,
            customer2,
            customer3
        };
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

    private static IReadOnlyList<ValuedCustomer> CreateValuedCustomers()
    {
        var address1 = new AddressStruct
        {
            AddressLine1 = "804 S. Lakeshore Road",
            ZipCode = 38654,
            Country = new CountryStruct { FullName = "United States", Code = "US" }
        };

        var customer1 = new ValuedCustomer
        {
            Id = 1,
            Name = "Mona Cy",
            ShippingAddress = address1,
            BillingAddress = address1
        };

        var customer2 = new ValuedCustomer
        {
            Id = 2,
            Name = "Antigonus Mitul",
            ShippingAddress = new AddressStruct
            {
                AddressLine1 = "72 Hickory Rd.",
                ZipCode = 07728,
                Country = new CountryStruct { FullName = "Germany", Code = "DE" }
            },
            BillingAddress = new AddressStruct
            {
                AddressLine1 = "79 Main St.",
                ZipCode = 29293,
                Country = new CountryStruct { FullName = "Germany", Code = "DE" }
            }
        };

        var address3 = new AddressStruct
        {
            AddressLine1 = "79 Main St.",
            ZipCode = 29293,
            Country = new CountryStruct { FullName = "Germany", Code = "DE" }
        };

        var customer3 = new ValuedCustomer
        {
            Id = 3,
            Name = "Monty Elias",
            ShippingAddress = address3,
            BillingAddress = address3
        };

        return new List<ValuedCustomer>
        {
            customer1,
            customer2,
            customer3
        };
    }

    private static IReadOnlyList<ValuedCustomerGroup> CreateValuedCustomerGroups(IReadOnlyList<ValuedCustomer> customers)
    {
        var group1 = new ValuedCustomerGroup
        {
            Id = 1,
            RequiredCustomer = customers[0],
            OptionalCustomer = customers[1]
        };

        var group2 = new ValuedCustomerGroup
        {
            Id = 2,
            RequiredCustomer = customers[1],
            OptionalCustomer = customers[0]
        };

        var group3 = new ValuedCustomerGroup
        {
            Id = 3,
            RequiredCustomer = customers[0],
            OptionalCustomer = null
        };

        return new List<ValuedCustomerGroup>
        {
            group1,
            group2,
            group3
        };
    }

    public static Task SeedAsync(PoolableDbContext context)
    {
        var customers = CreateCustomers();
        var customerGroups = CreateCustomerGroups(customers);
        var valuedCustomers = CreateValuedCustomers();
        var valuedCustomerGroups = CreateValuedCustomerGroups(valuedCustomers);

        context.AddRange(customers);
        context.AddRange(customerGroups);
        context.AddRange(valuedCustomers);
        context.AddRange(valuedCustomerGroups);

        return context.SaveChangesAsync();
    }
}
