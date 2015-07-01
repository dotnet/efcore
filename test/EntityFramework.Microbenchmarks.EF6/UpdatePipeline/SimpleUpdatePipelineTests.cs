// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using EntityFramework.Microbenchmarks.Core;
using EntityFramework.Microbenchmarks.Core.Models.Orders;
using EntityFramework.Microbenchmarks.EF6.Models.Orders;
using Xunit;

namespace EntityFramework.Microbenchmarks.EF6.UpdatePipeline
{
    public class SimpleUpdatePipelineTests
    {
        private static readonly string _connectionString
            = $@"Server={BenchmarkConfig.Instance.BenchmarkDatabaseInstance};Database=Perf_UpdatePipeline_Simple_EF6;Integrated Security=True;MultipleActiveResultSets=true;";

        public SimpleUpdatePipelineTests()
        {
            new OrdersSeedData().EnsureCreated(
                _connectionString,
                productCount: 0,
                customerCount: 1000,
                ordersPerCustomer: 0,
                linesPerOrder: 0);
        }

        [Benchmark(Iterations = 100, WarmupIterations = 5)]
        public void Insert(MetricCollector collector)
        {
            using (var context = new OrdersContext(_connectionString))
            {
                using (context.Database.BeginTransaction())
                {
                    for (var i = 0; i < 1000; i++)
                    {
                        context.Customers.Add(new Customer { Name = "New Customer " + i });
                    }

                    collector.StartCollection();
                    var records = context.SaveChanges();
                    collector.StopCollection();

                    Assert.Equal(1000, records);
                }
            }
        }

        [Benchmark(Iterations = 100, WarmupIterations = 5)]
        public void Update(MetricCollector collector)
        {
            using (var context = new OrdersContext(_connectionString))
            {
                using (context.Database.BeginTransaction())
                {
                    foreach (var customer in context.Customers)
                    {
                        customer.Name += " Modified";
                    }

                    collector.StartCollection();
                    var records = context.SaveChanges();
                    collector.StopCollection();

                    Assert.Equal(1000, records);
                }
            }
        }

        [Benchmark(Iterations = 100, WarmupIterations = 5)]
        public void Delete(MetricCollector collector)
        {
            using (var context = new OrdersContext(_connectionString))
            {
                using (context.Database.BeginTransaction())
                {
                    foreach (var customer in context.Customers)
                    {
                        context.Customers.Remove(customer);
                    }

                    collector.StartCollection();
                    var records = context.SaveChanges();
                    collector.StopCollection();

                    Assert.Equal(1000, records);
                }
            }
        }

        [Benchmark(Iterations = 100, WarmupIterations = 5)]
        public void Mixed(MetricCollector collector)
        {
            using (var context = new OrdersContext(_connectionString))
            {
                using (context.Database.BeginTransaction())
                {
                    var customers = context.Customers.ToArray();

                    for (var i = 0; i < 333; i++)
                    {
                        context.Customers.Add(new Customer { Name = "New Customer " + i });
                    }

                    for (var i = 0; i < 1000; i += 3)
                    {
                        context.Customers.Remove(customers[i]);
                    }

                    for (var i = 1; i < 1000; i += 3)
                    {
                        customers[i].Name += " Modified";
                    }

                    collector.StartCollection();
                    var records = context.SaveChanges();
                    collector.StopCollection();

                    Assert.Equal(1000, records);
                }
            }
        }
    }
}
