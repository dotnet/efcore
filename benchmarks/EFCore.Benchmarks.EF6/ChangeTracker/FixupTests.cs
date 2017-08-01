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
            private static readonly FixupFixture _fixture = new FixupFixture();

            protected List<Customer> Customers;
            protected List<Order> OrdersWithoutPk;
            protected List<Order> OrdersWithPk;
            protected OrdersContext Context;

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
                Context = _fixture.CreateContext();
                Context.Configuration.AutoDetectChangesEnabled = AutoDetectChanges;

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

        public abstract class ChildVariations : Base
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

        public abstract class ParentVariations : Base
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

        [SingleRunBenchmarkJob]
        public class ChildVariationsWithAutoDetectChangesOn : ChildVariations
        {
            protected override bool AutoDetectChanges => true;
        }

        [SingleRunBenchmarkJob]
        public class ParentVariationsWithAutoDetectChangesOn : ParentVariations
        {
            protected override bool AutoDetectChanges => true;
        }

        [BenchmarkJob]
        [MemoryDiagnoser]
        public class ChildVariationsWithAutoDetectChangesOff : ChildVariations
        {
            protected override bool AutoDetectChanges => false;
        }

        [BenchmarkJob]
        [MemoryDiagnoser]
        public class ParentVariationsWithAutoDetectChangesOff : ParentVariations
        {
            protected override bool AutoDetectChanges => false;
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
