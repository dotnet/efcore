// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Entity;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore.Benchmarks.EF6.Models.Orders;
using Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;

namespace Microsoft.EntityFrameworkCore.Benchmarks.EF6.ChangeTracker
{
    public class DbSetOperationTests
    {
        public abstract class Base
        {
            private readonly DbSetOperationFixture _fixture = new DbSetOperationFixture();

            protected List<Customer> CustomersWithoutPk;
            protected List<Customer> CustomersWithPk;
            protected OrdersContext Context;

            protected abstract bool AutoDetectChanges { get; }

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
                Context.Configuration.AutoDetectChangesEnabled = AutoDetectChanges;
            }

            [IterationCleanup]
            public virtual void CleanupContext()
            {
                Context.Dispose();
            }
        }

        public abstract class AddDataVariations : Base
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

            // Note: AttachRange() not implemented because there is no
            //       API for bulk attach in EF6.x
        }

        public abstract class ExistingDataVariations : Base
        {
            [IterationSetup]
            public override void InitializeContext()
            {
                base.InitializeContext();
                CustomersWithPk.ForEach(c => Context.Customers.Attach(c));
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
                    Context.Entry(customer).State = EntityState.Modified;
                }
            }

            // Note: UpdateRange() not implemented because there is no
            //       API for bulk update in EF6.x
        }

        [SingleRunBenchmarkJob]
        public class AddDataVariationsWithAutoDetectChangesOn : AddDataVariations
        {
            protected override bool AutoDetectChanges => true;
        }

        [SingleRunBenchmarkJob]
        public class ExistingDataVariationsWithAutoDetectChangesOn : ExistingDataVariations
        {
            protected override bool AutoDetectChanges => true;
        }

        [BenchmarkJob]
        [MemoryDiagnoser]
        public class AddDataVariationsWithAutoDetectChangesOff : AddDataVariations
        {
            protected override bool AutoDetectChanges => false;
        }

        [BenchmarkJob]
        [MemoryDiagnoser]
        public class ExistingDataVariationsWithAutoDetectChangesOff : ExistingDataVariations
        {
            protected override bool AutoDetectChanges => false;
        }

        public class DbSetOperationFixture : OrdersFixture
        {
            public DbSetOperationFixture()
                : base("Perf_ChangeTracker_DbSetOperation_EF6", 0, 0, 0, 0)
            {
            }
        }
    }
}
