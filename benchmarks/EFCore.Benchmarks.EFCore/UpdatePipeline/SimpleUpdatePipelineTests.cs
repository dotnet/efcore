// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore.Benchmarks.EFCore.Models.Orders;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Benchmarks.EFCore.UpdatePipeline
{
    public class SimpleUpdatePipelineTests
    {
        public abstract class Base
        {
            protected static readonly SimpleUpdatePipelineFixture _fixture = new SimpleUpdatePipelineFixture();
            protected OrdersContext _context;
            private IDbContextTransaction _transaction;
            private int _recordsAffected = -1;

            [Params(true, false)]
            public bool Async;

            [Params(true, false)]
            public bool Batching;

            [IterationSetup]
            public virtual void InitializeContext()
            {
                _context = _fixture.CreateContext(Batching);
                _transaction = _context.Database.BeginTransaction();
            }

            [IterationCleanup]
            public virtual void CleanupContext()
            {
                if (_recordsAffected != -1)
                {
                    Assert.Equal(1000, _recordsAffected);
                }

                _transaction.Dispose();
                _context.Dispose();
            }

            [Benchmark]
            public async Task UpdatePipeline()
            {
                _recordsAffected = Async
                    ? await _context.SaveChangesAsync()
                    : _context.SaveChanges();
            }
        }

        public class Insert : Base
        {
            [IterationSetup]
            public override void InitializeContext()
            {
                base.InitializeContext();

                var customers = _fixture.CreateCustomers(1000, setPrimaryKeys: false);
                _context.Customers.AddRange(customers);
            }
        }

        public class Update : Base
        {
            [IterationSetup]
            public override void InitializeContext()
            {
                base.InitializeContext();

                foreach (var customer in _context.Customers)
                {
                    customer.FirstName += " Modified";
                }
            }
        }

        public class Delete : Base
        {
            [IterationSetup]
            public override void InitializeContext()
            {
                base.InitializeContext();

                _context.Customers.RemoveRange(_context.Customers.ToList());
            }
        }

        public class Mixed : Base
        {
            [IterationSetup]
            public override void InitializeContext()
            {
                base.InitializeContext();

                var existingCustomers = _context.Customers.ToArray();
                var newCustomers = _fixture.CreateCustomers(333, setPrimaryKeys: false);
                _context.Customers.AddRange(newCustomers);

                for (var i = 0; i < 1000; i += 3)
                {
                    _context.Customers.Remove(existingCustomers[i]);
                }

                for (var i = 1; i < 1000; i += 3)
                {
                    existingCustomers[i].FirstName += " Modified";
                }
            }
        }

        public class SimpleUpdatePipelineFixture : OrdersFixture
        {
            public SimpleUpdatePipelineFixture()
                : base("Perf_UpdatePipeline_Simple", 0, 1000, 0, 0)
            {
            }

            public OrdersContext CreateContext(bool batching)
            {
                return new OrdersContext(ConnectionString, disableBatching: !batching);
            }
        }
    }
}
