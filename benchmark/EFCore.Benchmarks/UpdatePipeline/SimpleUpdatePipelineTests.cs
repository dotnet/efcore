// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

#pragma warning disable CA1034 // Nested types should not be visible

namespace Microsoft.EntityFrameworkCore.Benchmarks.UpdatePipeline;
#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable
public class SimpleUpdatePipelineTests
#pragma warning restore CA1052 // Static holder types should be Static or NotInheritable
{
    public abstract class UpdatePipelineBase
    {
        protected OrdersFixtureBase _fixture;
        protected OrdersContextBase _context;
        private IDbContextTransaction _transaction;
        private int _recordsAffected = -1;

        public abstract OrdersFixtureBase CreateFixture();

        [Params(true, false)]
        public virtual bool Async { get; set; }

        [Params(true, false)]
        public virtual bool Batching { get; set; }

        [GlobalSetup]
        public virtual void InitializeFixture()
        {
            _fixture = CreateFixture();
            _fixture.Initialize(0, 1000, 0, 0);
            _context = _fixture.CreateContext(disableBatching: Batching);
        }

        public virtual void InitializeContext()
            => _transaction = _context.Database.BeginTransaction();

        [IterationCleanup]
        public virtual void CleanupContext()
        {
            if (_recordsAffected != -1)
            {
                Assert.Equal(1000, _recordsAffected);
            }

            _transaction.Dispose();
            _context.ChangeTracker.Clear();
        }

        [Benchmark]
        public virtual async Task UpdatePipeline()
            => _recordsAffected = Async
                ? await _context.SaveChangesAsync()
                : _context.SaveChanges();
    }

    public abstract class InsertBase : UpdatePipelineBase
    {
        [IterationSetup]
        public override void InitializeContext()
        {
            base.InitializeContext();

            var customers = _fixture.CreateCustomers(1000, setPrimaryKeys: false);
            _context.Customers.AddRange(customers);
        }
    }

    public abstract class UpdateBase : UpdatePipelineBase
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

    public abstract class DeleteBase : UpdatePipelineBase
    {
        [IterationSetup]
        public override void InitializeContext()
        {
            base.InitializeContext();

            _context.Customers.RemoveRange(_context.Customers.ToList());
        }
    }

    public abstract class MixedBase : UpdatePipelineBase
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
}
