// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.SqlServer.FunctionalTests
{
    public class SqlServerEndToEndTest
    {
        [Fact]
        public async Task Can_run_linq_query_on_entity_set()
        {
            using (await TestDatabase.Northwind())
            {
                using (var db = new NorthwindContext())
                {
                    var results = db.Customers
                        .Where(c => c.CompanyName.StartsWith("A"))
                        .OrderByDescending(c => c.CustomerID)
                        .ToList();

                    Assert.Equal(4, results.Count);
                    Assert.Equal("AROUT", results[0].CustomerID);
                    Assert.Equal("ANTON", results[1].CustomerID);
                    Assert.Equal("ANATR", results[2].CustomerID);
                    Assert.Equal("ALFKI", results[3].CustomerID);

                    Assert.Equal("(171) 555-6750", results[0].Fax);
                    Assert.Null(results[1].Fax);
                    Assert.Equal("(5) 555-3745", results[2].Fax);
                    Assert.Equal("030-0076545", results[3].Fax);
                }
            }
        }

        [Fact]
        public async Task Can_enumerate_entity_set()
        {
            using (await TestDatabase.Northwind())
            {
                using (var db = new NorthwindContext())
                {
                    var results = new List<Customer>();
                    foreach (var item in db.Customers)
                    {
                        results.Add(item);
                    }

                    Assert.Equal(91, results.Count);
                    Assert.Equal("ALFKI", results[0].CustomerID);
                    Assert.Equal("Alfreds Futterkiste", results[0].CompanyName);
                }
            }
        }

        [Fact]
        public async Task Can_save_changes()
        {
            using (var testDatabase = await TestDatabase.Scratch())
            {
                await testDatabase.ExecuteNonQueryAsync(
                    @"CREATE TABLE [dbo].[Blog](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](max) NULL,
    CONSTRAINT [PK_Blogging] PRIMARY KEY CLUSTERED ( [Id] ASC ))");

                await testDatabase.ExecuteNonQueryAsync(@"INSERT INTO [dbo].[Blog] (Id, Name) VALUES (1, 'Blog to Update')");
                await testDatabase.ExecuteNonQueryAsync(@"INSERT INTO [dbo].[Blog] (Id, Name) VALUES (2, 'Blog to Delete')");

                var config = new EntityConfigurationBuilder()
                    .UseSqlServer(testDatabase.Connection.ConnectionString)
                    .BuildConfiguration();

                using (var db = new BloggingContext(config))
                {
                    db.ChangeTracker.Entry(new Blog { Id = 1, Name = "Blog is Updated" }).State = EntityState.Modified;
                    db.ChangeTracker.Entry(new Blog { Id = 2, Name = "Blog to Delete" }).State = EntityState.Deleted;
                    db.Blogs.Add(new Blog { Id = 3, Name = "Blog to Insert" });
                    db.SaveChanges();

                    var rows = await testDatabase.ExecuteScalarAsync<int>(
                        @"SELECT Count(*) FROM [dbo].[Blog] WHERE Id = 1 AND Name = 'Blog is Updated'",
                        CancellationToken.None);

                    Assert.Equal(1, rows);

                    rows = await testDatabase.ExecuteScalarAsync<int>(
                        @"SELECT Count(*) FROM [dbo].[Blog] WHERE Id = 2",
                        CancellationToken.None);

                    Assert.Equal(0, rows);

                    rows = await testDatabase.ExecuteScalarAsync<int>(
                        @"SELECT Count(*) FROM [dbo].[Blog] WHERE Id = 3 AND Name = 'Blog to Insert'",
                        CancellationToken.None);

                    Assert.Equal(1, rows);
                }
            }
        }

        private class NorthwindContext : EntityContext
        {
            public EntitySet<Customer> Customers { get; set; }

            protected override void OnConfiguring(EntityConfigurationBuilder builder)
            {
                builder.UseSqlServer(TestDatabase.NorthwindConnectionString);
            }

            protected override void OnModelCreating(ModelBuilder builder)
            {
                builder
                    .Entity<Customer>()
                    .Key(c => c.CustomerID)
                    .StorageName("Customers");
            }
        }

        private class Customer
        {
            public string CustomerID { get; set; }
            public string CompanyName { get; set; }
            public string Fax { get; set; }
        }

        private class BloggingContext : EntityContext
        {
            public BloggingContext(EntityConfiguration config)
                : base(config)
            {
            }

            public EntitySet<Blog> Blogs { get; set; }
        }

        private class Blog
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
