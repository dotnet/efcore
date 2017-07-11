// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class DatabaseFacadeTest
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Methods_delegate_to_configured_store_creator(bool async)
        {
            var creator = new FakeDatabaseCreator();

            var context = InMemoryTestHelpers.Instance.CreateContext(
                new ServiceCollection().AddSingleton<IDatabaseCreator>(creator));

            if (async)
            {
                Assert.True(await context.Database.EnsureCreatedAsync());
                Assert.Equal(1, creator.EnsureCreatedAsyncCount);
                Assert.True(await context.Database.EnsureDeletedAsync());
                Assert.Equal(1, creator.EnsureDeletedAsyncCount);
            }
            else
            {
                Assert.True(context.Database.EnsureCreated());
                Assert.Equal(1, creator.EnsureCreatedCount);
                Assert.True(context.Database.EnsureDeleted());
                Assert.Equal(1, creator.EnsureDeletedCount);
            }
        }

        private class FakeDatabaseCreator : IDatabaseCreator
        {
            public int EnsureDeletedCount;
            public int EnsureDeletedAsyncCount;
            public int EnsureCreatedCount;
            public int EnsureCreatedAsyncCount;

            public bool EnsureDeleted()
            {
                EnsureDeletedCount++;
                return true;
            }

            public Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                EnsureDeletedAsyncCount++;
                return Task.FromResult(true);
            }

            public bool EnsureCreated()
            {
                EnsureCreatedCount++;
                return true;
            }

            public Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                EnsureCreatedAsyncCount++;
                return Task.FromResult(true);
            }
        }

        [Fact]
        public void Can_get_IServiceProvider()
        {
            using (var context = InMemoryTestHelpers.Instance.CreateContext())
            {
                Assert.Same(
                    ((IInfrastructure<IServiceProvider>)context).Instance,
                    ((IInfrastructure<IServiceProvider>)context.Database).Instance);
            }
        }

        [Fact]
        public void Can_get_DatabaseCreator()
        {
            using (var context = InMemoryTestHelpers.Instance.CreateContext())
            {
                Assert.Same(
                    context.GetService<IDatabaseCreator>(),
                    context.Database.GetService<IDatabaseCreator>());
            }
        }

        [Fact]
        public void Can_get_Model()
        {
            using (var context = InMemoryTestHelpers.Instance.CreateContext())
            {
                Assert.Same(context.GetService<IModel>(), context.Database.GetService<IModel>());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Can_begin_transaction(bool async)
        {
            var transaction = new FakeDbContextTransaction();

            var context = InMemoryTestHelpers.Instance.CreateContext(
                new ServiceCollection().AddSingleton<IDbContextTransactionManager>(
                    new FakeDbContextTransactionManager(transaction)));

            Assert.Same(
                transaction,
                async
                    ? await context.Database.BeginTransactionAsync()
                    : context.Database.BeginTransaction());
        }

        private class FakeDbContextTransactionManager : IDbContextTransactionManager
        {
            private readonly FakeDbContextTransaction _transaction;
            
            public FakeDbContextTransactionManager(FakeDbContextTransaction transaction)
            {
                _transaction = transaction;
            }

            public int CommitCalls;
            public int RollbackCalls;

            public IDbContextTransaction BeginTransaction() 
                => _transaction;

            public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default(CancellationToken))
                => Task.FromResult<IDbContextTransaction>(_transaction);

            public void CommitTransaction() => CommitCalls++;
            public void RollbackTransaction() => RollbackCalls++;
            public IDbContextTransaction CurrentTransaction => _transaction;

            public void ResetState() => throw new NotImplementedException();
        }

        private class FakeDbContextTransaction : IDbContextTransaction
        {
            public void Dispose() => throw new NotImplementedException();
            public Guid TransactionId { get; }
            public void Commit() => throw new NotImplementedException();
            public void Rollback() => throw new NotImplementedException();
        }

        [Fact]
        public void Can_commit_transaction()
        {
            var manager = new FakeDbContextTransactionManager(new FakeDbContextTransaction());

            var context = InMemoryTestHelpers.Instance.CreateContext(
                new ServiceCollection().AddSingleton<IDbContextTransactionManager>(manager));

            context.Database.CommitTransaction();

            Assert.Equal(1, manager.CommitCalls);
        }

        [Fact]
        public void Can_roll_back_transaction()
        {
            var manager = new FakeDbContextTransactionManager(new FakeDbContextTransaction());

            var context = InMemoryTestHelpers.Instance.CreateContext(
                new ServiceCollection().AddSingleton<IDbContextTransactionManager>(manager));

            context.Database.RollbackTransaction();

            Assert.Equal(1, manager.RollbackCalls);
        }

        [Fact]
        public void Can_get_current_transaction()
        {
            var transaction = new FakeDbContextTransaction();

            var context = InMemoryTestHelpers.Instance.CreateContext(
                new ServiceCollection().AddSingleton<IDbContextTransactionManager>(
                    new FakeDbContextTransactionManager(transaction)));

            Assert.Same(transaction, context.Database.CurrentTransaction);
        }

        [Fact]
        public void Cannot_use_DatabaseFacade_after_dispose()
        {
            var context = InMemoryTestHelpers.Instance.CreateContext();
            var facade = context.Database;
            context.Dispose();

            Assert.Throws<ObjectDisposedException>(() => context.Database.GetService<IModel>());

            foreach (var methodInfo in facade.GetType().GetMethods(BindingFlags.Public))
            {
                Assert.Throws<ObjectDisposedException>(() => methodInfo.Invoke(facade, null));
            }
        }
    }
}
