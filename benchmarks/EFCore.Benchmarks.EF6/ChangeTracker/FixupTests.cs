// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore.Benchmarks.EF6.Models.Orders;
using Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;
using Xunit;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace Microsoft.EntityFrameworkCore.Benchmarks.EF6.ChangeTracker
{
    public class FixupTests
    {
        public abstract class Base
        {
            protected static readonly FixupFixture Fixture = new FixupFixture();
            protected List<Customer> Customers;
            protected List<Order> OrdersWithoutPk;
            protected List<Order> OrdersWithPk;
            protected OrdersContext Context;

            [Params(true, false)]
            public bool AutoDetectChanges { get; set; }

            [GlobalSetup]
            public virtual void CreateData()
            {
                Customers = Fixture.CreateCustomers(5000, setPrimaryKeys: true);
                OrdersWithoutPk = Fixture.CreateOrders(Customers, ordersPerCustomer: 2, setPrimaryKeys: false);
                OrdersWithPk = Fixture.CreateOrders(Customers, ordersPerCustomer: 2, setPrimaryKeys: true);

                using (var context = Fixture.CreateContext())
                {
                    Assert.Equal(5000, context.Customers.Count());
                    Assert.Equal(10000, context.Orders.Count());
                }
            }

            [IterationSetup]
            public virtual void InitializeContext()
            {
                Context = Fixture.CreateContext();
                Context.Configuration.AutoDetectChangesEnabled = AutoDetectChanges;
            }

            [IterationCleanup]
            public virtual void CleanupContext()
            {
                Context.Dispose();
            }
        }

        public class ChildVariation : Base
        {
            [IterationSetup]
            public override void InitializeContext()
            {
                base.InitializeContext();

                Customers.ForEach(c => Context.Customers.Attach(c));
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

        public class ParentVariation : Base
        {
            [IterationSetup]
            public override void InitializeContext()
            {
                base.InitializeContext();

                OrdersWithPk.ForEach(o => Context.Orders.Attach(o));
            }

            // Note: AddParents() not implemented because fixup to added parents
            //       only happens during SaveChanges for EF6.x (not during Add)

            [Benchmark]
            public virtual void AttachParents()
            {
                foreach (var customer in Customers)
                {
                    Context.Customers.Attach(customer);
                }
            }

            [Benchmark]
            public virtual void QueryChildren()
            {
                Context.Customers.ToList();
            }
        }

        public class FixupFixture : OrdersFixture
        {
            public FixupFixture()
                : base("Perf_ChangeTracker_Fixup_EF6", 0, 5000, 2, 0)
            {
            }
        }
    }
}
