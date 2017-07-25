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
        public class ChildVariation
        {
            private static readonly FixupFixture _fixture = new FixupFixture();
            private List<Customer> _customers;
            private List<Order> _ordersWithoutPk;
            private List<Order> _ordersWithPk;
            private OrdersContext _context;

            [Params(true, false)]
            public bool AutoDetectChanges { get; set; }

            [GlobalSetup]
            public virtual void CreateData()
            {
                _customers = _fixture.CreateCustomers(5000, setPrimaryKeys: true);
                _ordersWithoutPk = _fixture.CreateOrders(_customers, ordersPerCustomer: 2, setPrimaryKeys: false);
                _ordersWithPk = _fixture.CreateOrders(_customers, ordersPerCustomer: 2, setPrimaryKeys: true);

                using (var context = _fixture.CreateContext())
                {
                    Assert.Equal(5000, context.Customers.Count());
                    Assert.Equal(10000, context.Orders.Count());
                }
            }

            [IterationSetup]
            public virtual void InitializeContext()
            {
                _context = _fixture.CreateContext();
                _context.Configuration.AutoDetectChangesEnabled = AutoDetectChanges;
                _customers.ForEach(c => _context.Customers.Attach(c));
            }

            [IterationCleanup]
            public virtual void CleanupContext()
            {
                _context.Dispose();
            }

            [Benchmark]
            public virtual void AddChildren()
            {
                foreach (var order in _ordersWithoutPk)
                {
                    _context.Orders.Add(order);
                }
            }

            [Benchmark]
            public virtual void AttachChildren()
            {
                foreach (var order in _ordersWithPk)
                {
                    _context.Orders.Attach(order);
                }
            }

            [Benchmark]
            public virtual void QueryChildren()
            {
                _context.Orders.ToList();
            }
        }

        public class ParentVariation
        {
            private static readonly FixupFixture _fixture = new FixupFixture();
            private List<Customer> _customers;
            private List<Order> _orders;
            private OrdersContext _context;

            [Params(true, false)]
            public bool AutoDetectChanges { get; set; }

            [GlobalSetup]
            public virtual void CreateData()
            {
                _customers = _fixture.CreateCustomers(5000, setPrimaryKeys: true);
                _orders = _fixture.CreateOrders(_customers, ordersPerCustomer: 2, setPrimaryKeys: true);
            }

            [IterationSetup]
            public virtual void InitializeContext()
            {
                _context = _fixture.CreateContext();
                _context.Configuration.AutoDetectChangesEnabled = AutoDetectChanges;
                _orders.ForEach(o => _context.Orders.Attach(o));
            }

            [IterationCleanup]
            public virtual void CleanupContext()
            {
                _context.Dispose();
            }

            // Note: AddParents() not implemented because fixup to added parents
            //       only happens during SaveChanges for EF6.x (not during Add)

            [Benchmark]
            public virtual void AttachParents()
            {
                foreach (var customer in _customers)
                {
                    _context.Customers.Attach(customer);
                }
            }

            [Benchmark]
            public virtual void QueryChildren()
            {
                _context.Customers.ToList();
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
