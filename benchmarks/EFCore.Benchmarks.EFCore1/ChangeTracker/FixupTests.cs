// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore.Benchmarks.EFCore1.Models.Orders;
using Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;
using Xunit;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace Microsoft.EntityFrameworkCore.Benchmarks.EFCore1.ChangeTracker
{
    public class FixupTests
    {
        public abstract class Base
        {
            private static readonly FixupFixture _fixture = new FixupFixture();
            protected List<Customer> _customers;
            protected List<Order> _ordersWithoutPk;
            protected List<Order> _ordersWithPk;
            protected OrdersContext _context;

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
                _context = _fixture.CreateContext();
                _context.ChangeTracker.AutoDetectChangesEnabled = AutoDetectChanges;

                _customers = _fixture.CreateCustomers(5000, setPrimaryKeys: true);
                _ordersWithoutPk = _fixture.CreateOrders(_customers, ordersPerCustomer: 2, setPrimaryKeys: false);
                _ordersWithPk = _fixture.CreateOrders(_customers, ordersPerCustomer: 2, setPrimaryKeys: true);
            }

            [IterationCleanup]
            public virtual void CleanupContext()
            {
                _context.Dispose();
            }
        }

        public class ChildVariations : Base
        {
            [IterationSetup]
            public override void InitializeContext()
            {
                base.InitializeContext();
                _context.Customers.AttachRange(_customers);
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

        public class ParentVariations : Base
        {
            [IterationSetup]
            public override void InitializeContext()
            {
                base.InitializeContext();
                _context.Orders.AttachRange(_ordersWithPk);
            }

            [Benchmark]
            public virtual void AddParents()
            {
                foreach (var customer in _customers)
                {
                    _context.Customers.Add(customer);
                }
            }

            [Benchmark]
            public virtual void AttachParents()
            {
                foreach (var customer in _customers)
                {
                    _context.Customers.Attach(customer);
                }
            }

            [Benchmark]
            public virtual void QueryParents()
            {
                _context.Customers.ToList();
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
