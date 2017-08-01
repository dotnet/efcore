// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore.Benchmarks.EFCore.Models.Orders;
using Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;
using Xunit;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace Microsoft.EntityFrameworkCore.Benchmarks.EFCore.ChangeTracker
{
    public class FixupTests
    {
        public abstract class Base
        {
            private static readonly FixupFixture _fixture = new FixupFixture();
            protected List<Customer> Customers;
            protected List<Order> OrdersWithoutPk;
            protected List<Order> OrdersWithPk;
            protected OrdersContext Context;

            [Params(true, false)]
            public bool AutoDetectChanges { get; set; }

            [GlobalSetup]
            public virtual void CheckData()
            {
                using (var context = _fixture.CreateContext())
                {
                    Assert.Equal(5000, context.Customers.Count());
                    Assert.Equal(10000, context.Orders.Count());
                }
            }

            [IterationSetup]
            public virtual void InitializeContext()
            {
                Context = _fixture.CreateContext();
                Context.ChangeTracker.AutoDetectChangesEnabled = AutoDetectChanges;

                Customers = _fixture.CreateCustomers(5000, setPrimaryKeys: true);
                OrdersWithoutPk = _fixture.CreateOrders(Customers, ordersPerCustomer: 2, setPrimaryKeys: false);
                OrdersWithPk = _fixture.CreateOrders(Customers, ordersPerCustomer: 2, setPrimaryKeys: true);
            }

            [IterationCleanup]
            public virtual void CleanupContext()
            {
                Context.Dispose();
            }
        }

        public class ChildVariations : Base
        {
            [IterationSetup]
            public override void InitializeContext()
            {
                base.InitializeContext();
                Context.Customers.AttachRange(Customers);
            }

            [Benchmark]
            public virtual void AddChildren()
            {
                foreach (var order in OrdersWithoutPk)
                {
                    Context.Orders.Add(order);
                }
            }

            [Benchmark]
            public virtual void AttachChildren()
            {
                foreach (var order in OrdersWithPk)
                {
                    Context.Orders.Attach(order);
                }
            }

            [Benchmark]
            public virtual void QueryChildren()
            {
                Context.Orders.ToList();
            }
        }

        public class ParentVariations : Base
        {
            [IterationSetup]
            public override void InitializeContext()
            {
                base.InitializeContext();
                Context.Orders.AttachRange(OrdersWithPk);
            }

            [Benchmark]
            public virtual void AddParents()
            {
                foreach (var customer in Customers)
                {
                    Context.Customers.Add(customer);
                }
            }

            [Benchmark]
            public virtual void AttachParents()
            {
                foreach (var customer in Customers)
                {
                    Context.Customers.Attach(customer);
                }
            }

            [Benchmark]
            public virtual void QueryParents()
            {
                Context.Customers.ToList();
            }
        }

        public class FixupFixture : OrdersFixture
        {
            public FixupFixture()
                : base("Perf_ChangeTracker_Fixup", 0, 5000, 2, 0)
            {
            }
        }
    }
}
