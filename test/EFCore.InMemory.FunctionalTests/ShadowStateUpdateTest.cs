// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class ShadowStateUpdateTest(InMemoryFixture fixture) : IClassFixture<InMemoryFixture>
{
    [ConditionalFact]
    public async Task Can_add_update_delete_end_to_end_using_partial_shadow_state()
    {
        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
        var entityTypeBuilder = modelBuilder.Entity<Customer>();
        entityTypeBuilder.Property<string>("Name");

        var customerType = (IEntityType)entityTypeBuilder.Metadata;

        var optionsBuilder = new DbContextOptionsBuilder()
            .UseModel(modelBuilder.FinalizeModel())
            .UseInMemoryDatabase(nameof(ShadowStateUpdateTest))
            .UseInternalServiceProvider(_fixture.ServiceProvider);

        var customer = new Customer { Id = 42 };

        using (var context = new DbContext(optionsBuilder.Options))
        {
            await context.AddAsync(customer);

            context.Entry(customer).Property("Name").CurrentValue = "Daenerys";

            await context.SaveChangesAsync();

            context.Entry(customer).Property("Name").CurrentValue = "Changed!";
        }

        using (var context = new DbContext(optionsBuilder.Options))
        {
            var customerFromStore = context.Set<Customer>().Single();

            Assert.Equal(42, customerFromStore.Id);
            Assert.Equal(
                "Daenerys",
                (string)context.Entry(customerFromStore).Property("Name").CurrentValue);
        }

        using (var context = new DbContext(optionsBuilder.Options))
        {
            var customerEntry = context.Entry(customer).GetInfrastructure();
            customerEntry[customerType.FindProperty("Name")] = "Daenerys Targaryen";

            context.Update(customer);

            await context.SaveChangesAsync();
        }

        using (var context = new DbContext(optionsBuilder.Options))
        {
            var customerFromStore = context.Set<Customer>().Single();

            Assert.Equal(42, customerFromStore.Id);
            Assert.Equal(
                "Daenerys Targaryen",
                (string)context.Entry(customerFromStore).Property("Name").CurrentValue);
        }

        using (var context = new DbContext(optionsBuilder.Options))
        {
            context.Remove(customer);

            await context.SaveChangesAsync();
        }

        using (var context = new DbContext(optionsBuilder.Options))
        {
            Assert.Equal(0, context.Set<Customer>().Count());
        }
    }

    private readonly InMemoryFixture _fixture = fixture;

    private class Customer
    {
        private Customer(object[] values)
        {
            Id = (int)values[0];
        }

        public Customer()
        {
        }

        public int Id { get; set; }
    }
}
