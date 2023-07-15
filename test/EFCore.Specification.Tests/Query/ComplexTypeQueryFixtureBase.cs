// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ComplexTypeModel;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable enable

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

    protected override void Seed(PoolableDbContext context)
        => ComplexTypeData.Seed(context);

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
    }

    public Func<DbContext> GetContextCreator()
        => () => CreateContext();

    public ISetSource GetExpectedData()
        => _expectedData ??= new ComplexTypeData();

    public IReadOnlyDictionary<Type, object> EntitySorters { get; } = new Dictionary<Type, Func<object, object?>>
    {
        { typeof(Customer), e => ((Customer)e).Id },
        { typeof(CustomerGroup), e => ((CustomerGroup)e).Id },

        // Complex types - still need comparers for cases where they are projected directly
        { typeof(Address), e => ((Address?)e)?.ZipCode },
        { typeof(Country), e => ((Country?)e)?.Code }
    }.ToDictionary(e => e.Key, e => (object)e.Value);

    public IReadOnlyDictionary<Type, object> EntityAsserters { get; } = new Dictionary<Type, Action<object, object>>
    {
        {
            typeof(Customer), (e, a) =>
            {
                Assert.Equal(e == null, a == null);
                if (a is not null && e is not null)
                {
                    AssertCustomer((Customer)e, (Customer)a);
                }
            }
        },
        {
            typeof(Address), (e, a) =>
            {
                Assert.Equal(e == null, a == null);
                if (a is not null && e is not null)
                {
                    AssertAddress((Address)e, (Address)e);
                }
            }
        },
        {
            typeof(Country), (e, a) =>
            {
                Assert.Equal(e == null, a == null);
                if (a is not null && e is not null)
                {
                    AssertCountry((Country)e, (Country)e);
                }
            }
        },
        {
            typeof(CustomerGroup), (e, a) =>
            {
                Assert.Equal(e == null, a == null);
                if (a is not null && e is not null)
                {
                    var ee = (CustomerGroup)e;
                    var aa = (CustomerGroup)a;

                    AssertCustomer(ee.RequiredCustomer, aa.RequiredCustomer);
                    AssertCustomer(ee.OptionalCustomer, aa.OptionalCustomer);
                }
            }
        }
    }.ToDictionary(e => e.Key, e => (object)e.Value);

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
}
