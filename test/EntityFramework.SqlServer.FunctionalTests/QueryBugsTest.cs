// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class QueryBugsTest
    {
        [Fact]
        public async Task First_ix_async_bug_603()
        {
            using (var context = new MyContext())
            {
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();

                context.Products.Add(new Product { Name = "Product 1" });
                context.SaveChanges();
            }

            using (var ctx = new MyContext())
            {
                var product = await ctx.Products.FirstAsync();

                ctx.Products.Remove(product);

                await ctx.SaveChangesAsync();
            }
        }

        [Fact]
        public async Task First_or_default_ix_async_bug_603()
        {
            using (var context = new MyContext())
            {
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();

                context.Products.Add(new Product { Name = "Product 1" });
                context.SaveChanges();
            }

            using (var ctx = new MyContext())
            {
                var product = await ctx.Products.FirstOrDefaultAsync();

                ctx.Products.Remove(product);

                await ctx.SaveChangesAsync();
            }
        }

        private class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class MyContext : DbContext
        {
            public DbSet<Product> Products { get; set; }

            protected override void OnConfiguring(DbContextOptions options)
            {
                options.UseSqlServer(SqlServerTestDatabase.CreateConnectionString("Repro603"));
            }
        }
    }
}
