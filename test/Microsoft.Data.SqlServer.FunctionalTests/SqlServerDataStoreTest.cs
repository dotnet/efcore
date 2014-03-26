// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Xunit;

namespace Microsoft.Data.SqlServer.FunctionalTests
{
    public class SqlServerDataStoreTest
    {
        public class Customer
        {
            public string CustomerID { get; set; }
            public string CompanyName { get; set; }
            public string ContactName { get; set; }
            public string ContactTitle { get; set; }
            public string Address { get; set; }
            public string City { get; set; }
            public string Region { get; set; }
            public string PostalCode { get; set; }
            public string Country { get; set; }
            public string Phone { get; set; }
            public string Fax { get; set; }
        }

        [Fact]
        public async Task Can_async_read_rows()
        {
            var model = CreateModel();

            using (var testDatabase = await TestDatabase.Northwind())
            {
                var sqlServerDataStore
                    = new SqlServerDataStore(testDatabase.Connection.ConnectionString);

                Assert.Equal(91, await sqlServerDataStore.Query<Customer>(model, new FakeStateManager()).CountAsync());
            }
        }

        private class FakeStateManager : StateManager
        {
        }

        private static Model CreateModel()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .StorageName("Customers")
                .Properties(ps =>
                    {
                        ps.Property(c => c.CustomerID);
                        ps.Property(c => c.CompanyName);
                        ps.Property(c => c.ContactName);
                        ps.Property(c => c.ContactTitle);
                        ps.Property(c => c.Address);
                        ps.Property(c => c.City);
                        ps.Property(c => c.Region);
                        ps.Property(c => c.PostalCode);
                        ps.Property(c => c.Country);
                        ps.Property(c => c.Phone);
                        ps.Property(c => c.Fax);
                    });

            return model;
        }
    }
}
