// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ComplexTypeModel;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class ComplexTypeQueryFixtureBase : SharedStoreFixtureBase<PoolableDbContext>, IQueryFixtureBase
{
    protected override string StoreName
        => "ComplexTypeQueryTest";

    private ComplexTypeData? _expectedData;

    public override PoolableDbContext CreateContext()
    {
        var context = base.CreateContext();
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        return context;
    }

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).ConfigureWarnings(wcb => wcb.Throw());

    protected override Task SeedAsync(PoolableDbContext context)
        => ComplexTypeData.SeedAsync(context);

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.Entity<Customer>(
            cb =>
            {
                cb.Property(c => c.Id).ValueGeneratedNever();

                cb.ComplexProperty(c => c.ShippingAddress, sab => sab.ComplexProperty(sa => sa.Country));
                cb.ComplexProperty(c => c.BillingAddress, sab => sab.ComplexProperty(sa => sa.Country));
            });

        modelBuilder.Entity<CustomerGroup>(
            cgb =>
            {
                cgb.Property(cg => cg.Id).ValueGeneratedNever();
                cgb.Navigation(cg => cg.RequiredCustomer).AutoInclude();
                cgb.Navigation(cg => cg.OptionalCustomer).AutoInclude();
            });

        modelBuilder.Entity<ValuedCustomer>(
            cb =>
            {
                cb.Property(c => c.Id).ValueGeneratedNever();

                cb.ComplexProperty(c => c.ShippingAddress, sab => sab.ComplexProperty(sa => sa.Country));
                cb.ComplexProperty(c => c.BillingAddress, sab => sab.ComplexProperty(sa => sa.Country));
            });

        modelBuilder.Entity<ValuedCustomerGroup>(
            cgb =>
            {
                cgb.Property(cg => cg.Id).ValueGeneratedNever();
                cgb.Navigation(cg => cg.RequiredCustomer).AutoInclude();
                cgb.Navigation(cg => cg.OptionalCustomer).AutoInclude();
            });
    }

    public Func<DbContext> GetContextCreator()
        => () => CreateContext();

    public ISetSource GetExpectedData()
        => _expectedData ??= new ComplexTypeData();

    public IReadOnlyDictionary<Type, object> EntitySorters { get; } = new Dictionary<Type, object>
    {
        { typeof(Customer), (Func<Customer, object>)(e => e.Id) },
        { typeof(CustomerGroup), (Func<CustomerGroup, object>)(e => e.Id) },
        { typeof(ValuedCustomer), (Func<ValuedCustomer, object>)(e => e.Id) },
        { typeof(ValuedCustomerGroup), (Func<ValuedCustomerGroup, object>)(e => e.Id) },

        // Complex types - still need comparers for cases where they are projected directly
        { typeof(Address), (Func<Address, object>)(e => e.ZipCode) },
        { typeof(Country), (Func<Country, object>)(e => e.Code) },
        { typeof(AddressStruct), (Func<AddressStruct, object>)(e => e.ZipCode) },
        { typeof(CountryStruct), (Func<CountryStruct, object>)(e => e.Code) }
    }.ToDictionary(e => e.Key, e => e.Value);

    public IReadOnlyDictionary<Type, object> EntityAsserters { get; } = new Dictionary<Type, object>
    {
        {
            typeof(Customer), (Customer e, Customer a) =>
            {
                Assert.Equal(e == null, a == null);
                if (a is not null && e is not null)
                {
                    AssertCustomer(e, a);
                }
            }
        },
        {
            typeof(Address), (Address e, Address a) =>
            {
                Assert.Equal(e == null, a == null);
                if (a is not null && e is not null)
                {
                    AssertAddress(e, a);
                }
            }
        },
        {
            typeof(Country), (Country e, Country a) =>
            {
                Assert.Equal(e == null, a == null);
                if (a is not null && e is not null)
                {
                    AssertCountry(e, a);
                }
            }
        },
        {
            typeof(CustomerGroup), (CustomerGroup e, CustomerGroup a) =>
            {
                Assert.Equal(e == null, a == null);
                if (a is not null && e is not null)
                {
                    AssertCustomer(e.RequiredCustomer, a.RequiredCustomer);
                    AssertCustomer(e.OptionalCustomer, a.OptionalCustomer);
                }
            }
        },
        {
            typeof(ValuedCustomer), (ValuedCustomer e, ValuedCustomer a) =>
            {
                Assert.Equal(e == null, a == null);
                if (a is not null && e is not null)
                {
                    AssertValuedCustomer(e, a);
                }
            }
        },
        {
            typeof(AddressStruct), (AddressStruct e, AddressStruct a) =>
            {
                AssertAddressStruct(e, a);
            }
        },
        {
            typeof(CountryStruct), (CountryStruct e, CountryStruct a) =>
            {
                AssertCountryStruct(e, e);
            }
        },
        {
            typeof(ValuedCustomerGroup), (ValuedCustomerGroup e, ValuedCustomerGroup a) =>
            {
                Assert.Equal(e == null, a == null);
                if (a is not null && e is not null)
                {
                    AssertValuedCustomer(e.RequiredCustomer, a.RequiredCustomer);
                    AssertValuedCustomer(e.OptionalCustomer, a.OptionalCustomer);
                }
            }
        }
    }.ToDictionary(e => e.Key, e => e.Value);

    private static void AssertCustomer(Customer? expected, Customer? actual)
    {
        if (expected is not null && actual is not null)
        {
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Name, actual.Name);
            AssertAddress(expected.ShippingAddress, actual.ShippingAddress);
            AssertAddress(expected.BillingAddress, actual.BillingAddress);
        }
        else
        {
            Assert.Equal(expected is null, actual is null);
        }
    }

    private static void AssertAddress(Address? expected, Address? actual)
    {
        if (expected is not null && actual is not null)
        {
            Assert.Equal(expected.AddressLine1, actual.AddressLine1);
            Assert.Equal(expected.AddressLine2, actual.AddressLine2);
            Assert.Equal(expected.ZipCode, actual.ZipCode);

            AssertCountry(expected.Country, actual.Country);
        }
        else
        {
            Assert.Equal(expected is null, actual is null);
        }
    }

    private static void AssertCountry(Country? expected, Country? actual)
    {
        if (expected is not null && actual is not null)
        {
            Assert.Equal(expected.FullName, actual.FullName);
            Assert.Equal(expected.Code, actual.Code);
        }
        else
        {
            Assert.Equal(expected is null, actual is null);
        }
    }

    private static void AssertValuedCustomer(ValuedCustomer? expected, ValuedCustomer? actual)
    {
        if (expected is not null && actual is not null)
        {
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Name, actual.Name);
            AssertAddressStruct(expected.ShippingAddress, actual.ShippingAddress);
            AssertAddressStruct(expected.BillingAddress, actual.BillingAddress);
        }
        else
        {
            Assert.Equal(expected is null, actual is null);
        }
    }

    private static void AssertAddressStruct(object? expected, object? actual)
    {
        if (expected is AddressStruct expectedStruct && actual is AddressStruct actualStruct)
        {
            Assert.Equal(expectedStruct.AddressLine1, actualStruct.AddressLine1);
            Assert.Equal(expectedStruct.AddressLine2, actualStruct.AddressLine2);
            Assert.Equal(expectedStruct.ZipCode, actualStruct.ZipCode);

            AssertCountryStruct(expectedStruct.Country, actualStruct.Country);
        }
        else
        {
            Assert.Equal(expected is null, actual is null);
        }
    }

    private static void AssertCountryStruct(object? expected, object? actual)
    {
        if (expected is CountryStruct expectedStruct && actual is CountryStruct actualStruct)
        {
            Assert.Equal(expectedStruct.FullName, actualStruct.FullName);
            Assert.Equal(expectedStruct.Code, actualStruct.Code);
        }
        else
        {
            Assert.Equal(expected is null, actual is null);
        }
    }
}
