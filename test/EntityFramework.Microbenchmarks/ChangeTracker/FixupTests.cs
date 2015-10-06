// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using EntityFramework.Microbenchmarks.Core;
using EntityFramework.Microbenchmarks.Core.Models.Orders;
using EntityFramework.Microbenchmarks.Models.Orders;
using System.Linq;
using Xunit;

namespace EntityFramework.Microbenchmarks.ChangeTracker
{
    public class FixupTests : IClassFixture<FixupTests.FixupFixture>
    {
        private readonly FixupFixture _fixture;

        public FixupTests(FixupFixture fixture)
        {
            _fixture = fixture;
        }

        [Benchmark]
        public void AddChildren(IMetricCollector collector)
        {
            using (var context = _fixture.CreateContext())
            {
                var orders = new Order[1000];
                for (var i = 0; i < orders.Length; i++)
                {
                    context.Customers.Attach(new Customer { CustomerId = i + 1 });
                    orders[i] = new Order { CustomerId = i + 1 };
                }

                using (collector.StartCollection())
                {
                    foreach (var order in orders)
                    {
                        context.Orders.Add(order);
                    }
                }

                Assert.All(orders, o => Assert.NotNull(o.Customer));
            }
        }

        [Benchmark]
        public void AddParents(IMetricCollector collector)
        {
            using (var context = _fixture.CreateContext())
            {
                var customers = new Customer[1000];
                for (var i = 0; i < customers.Length; i++)
                {
                    customers[i] = new Customer { CustomerId = i + 1 };
                    context.Orders.Add(new Order { CustomerId = i + 1 });
                }

                using (collector.StartCollection())
                {
                    foreach (var customer in customers)
                    {
                        context.Customers.Add(customer);
                    }
                }

                Assert.All(customers, c => Assert.Equal(1, c.Orders.Count));
            }
        }

        [Benchmark]
        public void AttachChildren(IMetricCollector collector)
        {
            using (var context = _fixture.CreateContext())
            {
                var orders = new Order[1000];
                for (var i = 0; i < orders.Length; i++)
                {
                    context.Customers.Attach(new Customer { CustomerId = i + 1 });
                    orders[i] = new Order { OrderId = i + 1, CustomerId = i + 1 };
                }

                using (collector.StartCollection())
                {
                    foreach (var order in orders)
                    {
                        context.Orders.Attach(order);
                    }
                }

                Assert.All(orders, o => Assert.NotNull(o.Customer));
            }
        }

        [Benchmark]
        public void AttachParents(IMetricCollector collector)
        {
            using (var context = _fixture.CreateContext())
            {
                var customers = new Customer[1000];
                for (var i = 0; i < customers.Length; i++)
                {
                    customers[i] = new Customer { CustomerId = i + 1 };
                    context.Orders.Attach(new Order { OrderId = i + 1, CustomerId = i + 1 });
                }

                using (collector.StartCollection())
                {
                    foreach (var customer in customers)
                    {
                        context.Customers.Attach(customer);
                    }
                }

                Assert.All(customers, c => Assert.Equal(1, c.Orders.Count));
            }
        }

        [Benchmark]
        public void QueryChildren(IMetricCollector collector)
        {
            using (var context = _fixture.CreateContext())
            {
                context.Customers.ToList();

                collector.StartCollection();
                var orders = context.Orders.ToList();
                collector.StopCollection();

                Assert.Equal(1000, context.ChangeTracker.Entries<Customer>().Count());
                Assert.Equal(1000, context.ChangeTracker.Entries<Order>().Count());
                Assert.All(orders, o => Assert.NotNull(o.Customer));
            }
        }

        [Benchmark]
        public void QueryParents(IMetricCollector collector)
        {
            using (var context = _fixture.CreateContext())
            {
                context.Orders.ToList();

                collector.StartCollection();
                var customers = context.Customers.ToList();
                collector.StopCollection();

                Assert.Equal(1000, context.ChangeTracker.Entries<Customer>().Count());
                Assert.Equal(1000, context.ChangeTracker.Entries<Order>().Count());
                Assert.All(customers, c => Assert.Equal(1, c.Orders.Count));
            }
        }

        public class FixupFixture : OrdersFixture
        {
            public FixupFixture()
                : base("Perf_ChangeTracker_Fixup", 0, 1000, 1, 0)
            { }
        }
    }
}
