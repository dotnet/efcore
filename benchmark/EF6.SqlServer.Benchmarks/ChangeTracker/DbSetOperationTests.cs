// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;

#pragma warning disable CA1034 // Nested types should not be visible

namespace Microsoft.EntityFrameworkCore.Benchmarks.ChangeTracker
{
#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable
    public class DbSetOperationTests
#pragma warning restore CA1052 // Static holder types should be Static or NotInheritable
    {
        public abstract class DbSetOperationBase
        {
            private static readonly OrdersFixture _fixture
                = new OrdersFixture("Perf_ChangeTracker_DbSetOperation_EF6", 0, 0, 0, 0);

            protected List<Customer> _customersWithoutPk;
            protected List<Customer> _customersWithPk;
            protected OrdersContext _context;

            protected abstract bool AutoDetectChanges { get; }

            [GlobalSetup]
            public virtual void CreateCustomers()
            {
                _customersWithoutPk = _fixture.CreateCustomers(20000, setPrimaryKeys: false);
                _customersWithPk = _fixture.CreateCustomers(20000, setPrimaryKeys: true);
            }

            [IterationSetup]
            public virtual void InitializeContext()
            {
                _context = _fixture.CreateContext();
                _context.Configuration.AutoDetectChangesEnabled = AutoDetectChanges;
            }

            [IterationCleanup]
            public virtual void CleanupContext()
            {
                _context.Dispose();
            }
        }

        [DisplayName(nameof(AddDataVariations))]
        public abstract class AddDataVariations : DbSetOperationBase
        {
            [Benchmark]
            public virtual void Add()
            {
                foreach (var customer in _customersWithoutPk)
                {
                    _context.Customers.Add(customer);
                }
            }

            [Benchmark]
            public virtual void AddRange()
            {
                _context.Customers.AddRange(_customersWithoutPk);
            }

            [Benchmark]
            public virtual void Attach()
            {
                foreach (var customer in _customersWithPk)
                {
                    _context.Customers.Attach(customer);
                }
            }

            // Note: AttachRange() not implemented because there is no
            //       API for bulk attach in EF6.x
        }

        [DisplayName(nameof(ExistingDataVariations))]
        public abstract class ExistingDataVariations : DbSetOperationBase
        {
            [IterationSetup]
            public override void InitializeContext()
            {
                base.InitializeContext();
                _customersWithPk.ForEach(c => _context.Customers.Attach(c));
            }

            [Benchmark]
            public virtual void Remove()
            {
                foreach (var customer in _customersWithPk)
                {
                    _context.Customers.Remove(customer);
                }
            }

            [Benchmark]
            public virtual void RemoveRange()
            {
                _context.Customers.RemoveRange(_customersWithPk);
            }

            [Benchmark]
            public virtual void Update()
            {
                foreach (var customer in _customersWithPk)
                {
                    _context.Entry(customer).State = EntityState.Modified;
                }
            }

            // Note: UpdateRange() not implemented because there is no
            //       API for bulk update in EF6.x
        }

        [SingleRunJob]
        [Description("AutoDetectChanges=True")]
        public class AddDataVariationsWithAutoDetectChangesOn : AddDataVariations
        {
            protected override bool AutoDetectChanges => true;
        }

        [SingleRunJob]
        [Description("AutoDetectChanges=True")]
        public class ExistingDataVariationsWithAutoDetectChangesOn : ExistingDataVariations
        {
            protected override bool AutoDetectChanges => true;
        }

        [Description("AutoDetectChanges=False")]
        public class AddDataVariationsWithAutoDetectChangesOff : AddDataVariations
        {
            protected override bool AutoDetectChanges => false;
        }

        [Description("AutoDetectChanges=False")]
        public class ExistingDataVariationsWithAutoDetectChangesOff : ExistingDataVariations
        {
            protected override bool AutoDetectChanges => false;
        }
    }
}
