// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class ShadowStateUpdateTest : IClassFixture<InMemoryFixture>
    {
        [Fact]
        public async Task Can_add_update_delete_end_to_end_using_partial_shadow_state()
        {
            var model = new Model();

            var customerType = model.AddEntityType(typeof(Customer));
            var property1 = customerType.AddProperty("Id", typeof(int));
            customerType.GetOrSetPrimaryKey(property1);
            customerType.AddProperty("Name", typeof(string));

            var optionsBuilder = new DbContextOptionsBuilder()
                .UseModel(model)
                .UseInMemoryDatabase(nameof(ShadowStateUpdateTest))
                .UseInternalServiceProvider(_fixture.ServiceProvider);

            var customer = new Customer
            {
                Id = 42
            };

            using (var context = new DbContext(optionsBuilder.Options))
            {
                context.Add(customer);

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

        private readonly InMemoryFixture _fixture;

        public ShadowStateUpdateTest(InMemoryFixture fixture)
        {
            _fixture = fixture;
        }

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
}
