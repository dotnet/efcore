// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;
using Xunit;

#pragma warning disable CA1034 // Nested types should not be visible

namespace Microsoft.EntityFrameworkCore.Benchmarks.ChangeTracker
{
#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable
    public class FixupTests
#pragma warning restore CA1052 // Static holder types should be Static or NotInheritable
    {
        public abstract class FixupBase
        {
            private static readonly OrdersFixture _fixture
                = new OrdersFixture("Perf_ChangeTracker_Fixup_EF6", 0, 5000, 2, 0);

            protected List<Customer> _customers;
            protected List<Order> _ordersWithoutPk;
            protected List<Order> _ordersWithPk;
            protected OrdersContext _context;

            protected abstract bool AutoDetectChanges { get; }

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
                _context.Configuration.AutoDetectChangesEnabled = AutoDetectChanges;

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

        [DisplayName(nameof(ChildVariations))]
        public abstract class ChildVariations : FixupBase
        {
            [IterationSetup]
            public override void InitializeContext()
            {
                base.InitializeContext();

                _customers.ForEach(c => _context.Customers.Attach(c));
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

        [DisplayName(nameof(ParentVariations))]
        public abstract class ParentVariations : FixupBase
        {
            [IterationSetup]
            public override void InitializeContext()
            {
                base.InitializeContext();

                _ordersWithPk.ForEach(o => _context.Orders.Attach(o));
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

        [SingleRunJob, Description("AutoDetectChanges=True")]
        public class ChildVariationsWithAutoDetectChangesOn : ChildVariations
        {
            protected override bool AutoDetectChanges => true;
        }

        [SingleRunJob, Description("AutoDetectChanges=True")]
        public class ParentVariationsWithAutoDetectChangesOn : ParentVariations
        {
            protected override bool AutoDetectChanges => true;
        }

        [Description("AutoDetectChanges=False")]
        public class ChildVariationsWithAutoDetectChangesOff : ChildVariations
        {
            protected override bool AutoDetectChanges => false;
        }

        [Description("AutoDetectChanges=False")]
        public class ParentVariationsWithAutoDetectChangesOff : ParentVariations
        {
            protected override bool AutoDetectChanges => false;
        }
    }
}
