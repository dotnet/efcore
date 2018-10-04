// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class CompiledQueryInMemoryTest : CompiledQueryTestBase<NorthwindQueryInMemoryFixture<NoopModelCustomizer>>
    {
        public CompiledQueryInMemoryTest(NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }
        
        public class MyServiceCollection : ServiceCollection
        {
            public MyServiceCollection(string dbName)
            {
                this.AddDbContext<NorthwindContext>(
                    o => o.UseInMemoryDatabase(dbName));
            }
        }

        [Fact]
        public void Query_Different_InMemoryDatabases()
        {
            var preCompiled = EF.CompileQuery<NorthwindContext, Customer>(ctx => ctx.Customers.SingleOrDefault());

            using (var services = new MyServiceCollection("FOO").BuildServiceProvider())
            {
                using (var scope = services.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var ctx = scope.ServiceProvider.GetRequiredService<NorthwindContext>();

                    ctx.Database.EnsureDeleted();
                    ctx.Database.EnsureCreated();

                    ctx.Add(new Customer { ContactName = "One" });

                    ctx.SaveChanges();

                    var item = preCompiled(ctx);
                    Assert.Equal("One", item?.ContactName);
                }

                using (var scope = services.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var ctx = scope.ServiceProvider.GetRequiredService<NorthwindContext>();
                    var item = preCompiled(ctx);
                    Assert.Equal("One", item?.ContactName);
                }
            }

            using (var services = new MyServiceCollection("BAR").BuildServiceProvider())
            {
                using (var scope = services.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var ctx = scope.ServiceProvider.GetRequiredService<NorthwindContext>();
                    ctx.Database.EnsureDeleted();
                    ctx.Database.EnsureCreated();

                    ctx.Add(new Customer { ContactName = "Two" });

                    ctx.SaveChanges();

                    var item = preCompiled(ctx);
                    Assert.Equal("Two", item?.ContactName);
                }

                using (var scope = services.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var ctx = scope.ServiceProvider.GetRequiredService<NorthwindContext>();
                    var item = preCompiled(ctx);
                    Assert.Equal("Two", item?.ContactName);
                }
            }
        }

        [Fact(Skip = "See issue#13857")]
        public override void DbQuery_query()
        {
            base.DbQuery_query();
        }

        [Fact(Skip = "See issue#13857")]
        public override Task DbQuery_query_async()
        {
            return base.DbQuery_query_async();
        }

        [Fact(Skip = "See issue#13857")]
        public override void DbQuery_query_first()
        {
            base.DbQuery_query_first();
        }

        [Fact(Skip = "See issue#13857")]
        public override Task DbQuery_query_first_async()
        {
            return base.DbQuery_query_first_async();
        }
    }
}
