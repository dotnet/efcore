// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore.Benchmarks.EFCore1.Models.Orders;
using Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;

namespace Microsoft.EntityFrameworkCore.Benchmarks.EFCore1.ChangeTracker
{
    public class DbSetOperationTests
    {
        public abstract class Base
        {
            private readonly DbSetOperationFixture _fixture = new DbSetOperationFixture();
            protected List<Customer> _customersWithoutPk;
            protected List<Customer> _customersWithPk;
            protected OrdersContext _context;

            [Params(true, false)]
            public bool AutoDetectChanges { get; set; }

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
                _context.ChangeTracker.AutoDetectChangesEnabled = AutoDetectChanges;
            }

            [IterationCleanup]
            public virtual void CleanupContext()
            {
                _context.Dispose();
            }
        }

        public class AddDataVariations : Base
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

            [Benchmark]
            public virtual void AttachRange()
            {
                _context.Customers.AttachRange(_customersWithPk);
            }
        }

        public class ExistingDataVariations : Base
        {
            [IterationSetup]
            public override void InitializeContext()
            {
                base.InitializeContext();
                _context.Customers.AttachRange(_customersWithPk);
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
                    _context.Customers.Update(customer);
                }
            }

            [Benchmark]
            public virtual void UpdateRange()
            {
                _context.Customers.UpdateRange(_customersWithPk);
            }
        }

        public class DbSetOperationFixture : OrdersFixture
        {
            public DbSetOperationFixture()
                : base("Perf_ChangeTracker_DbSetOperation", 0, 0, 0, 0)
            {
            }
        }
    }
}
