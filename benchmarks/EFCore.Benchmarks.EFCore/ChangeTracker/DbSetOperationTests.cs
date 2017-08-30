// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore.Benchmarks.EFCore.Models.Orders;
using Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;

namespace Microsoft.EntityFrameworkCore.Benchmarks.EFCore.ChangeTracker
{
    public class DbSetOperationTests
    {
        public abstract class Base
        {
            private readonly DbSetOperationFixture _fixture = new DbSetOperationFixture();
            protected List<Customer> CustomersWithoutPk;
            protected List<Customer> CustomersWithPk;
            protected OrdersContext Context;

            [Params(true, false)]
            public bool AutoDetectChanges { get; set; }

            [GlobalSetup]
            public virtual void CreateCustomers()
            {
                CustomersWithoutPk = _fixture.CreateCustomers(20000, setPrimaryKeys: false);
                CustomersWithPk = _fixture.CreateCustomers(20000, setPrimaryKeys: true);
            }

            [IterationSetup]
            public virtual void InitializeContext()
            {
                Context = _fixture.CreateContext();
                Context.ChangeTracker.AutoDetectChangesEnabled = AutoDetectChanges;
            }

            [IterationCleanup]
            public virtual void CleanupContext()
            {
                Context.Dispose();
            }
        }

        public class AddDataVariations : Base
        {
            [Benchmark]
            public virtual void Add()
            {
                foreach (var customer in CustomersWithoutPk)
                {
                    Context.Customers.Add(customer);
                }
            }

            [Benchmark]
            public virtual void AddRange()
            {
                Context.Customers.AddRange(CustomersWithoutPk);
            }

            [Benchmark]
            public virtual void Attach()
            {
                foreach (var customer in CustomersWithPk)
                {
                    Context.Customers.Attach(customer);
                }
            }

            [Benchmark]
            public virtual void AttachRange()
            {
                Context.Customers.AttachRange(CustomersWithPk);
            }
        }

        public class ExistingDataVariations : Base
        {
            [IterationSetup]
            public override void InitializeContext()
            {
                base.InitializeContext();
                Context.Customers.AttachRange(CustomersWithPk);
            }

            [Benchmark]
            public virtual void Remove()
            {
                foreach (var customer in CustomersWithPk)
                {
                    Context.Customers.Remove(customer);
                }
            }

            [Benchmark]
            public virtual void RemoveRange()
            {
                Context.Customers.RemoveRange(CustomersWithPk);
            }

            [Benchmark]
            public virtual void Update()
            {
                foreach (var customer in CustomersWithPk)
                {
                    Context.Customers.Update(customer);
                }
            }

            [Benchmark]
            public virtual void UpdateRange()
            {
                Context.Customers.UpdateRange(CustomersWithPk);
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
