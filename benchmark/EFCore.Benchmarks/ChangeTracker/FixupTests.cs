// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;
using Xunit;

#pragma warning disable CA1034 // Nested types should not be visible

namespace Microsoft.EntityFrameworkCore.Benchmarks.ChangeTracker;
#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable
public class FixupTests
#pragma warning restore CA1052 // Static holder types should be Static or NotInheritable
{
    public abstract class FixupBase
    {
        private OrdersFixtureBase _fixture;

        protected List<Customer> _customers;
        protected List<Order> _ordersWithoutPk;
        protected List<Order> _ordersWithPk;
        protected OrdersContextBase _context;

        public abstract OrdersFixtureBase CreateFixture();

        [Params(true, false)]
        public virtual bool AutoDetectChanges { get; set; }

        [GlobalSetup]
        public virtual void CheckData()
        {
            _fixture = CreateFixture();
            _fixture.Initialize(0, 5000, 2, 0);

            using (var context = _fixture.CreateContext())
            {
                Assert.Equal(5000, context.Customers.Count());
                Assert.Equal(10000, context.Orders.Count());
            }
        }

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
            => _context.Dispose();
    }

    public abstract class ChildVariationsBase : FixupBase
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
            => _context.Orders.ToList();
    }

    public abstract class ParentVariationsBase : FixupBase
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
            => _context.Customers.ToList();
    }
}
