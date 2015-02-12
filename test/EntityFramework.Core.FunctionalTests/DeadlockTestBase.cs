// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


#if NET45

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class DeadlockTestBase<TTestStore>
        where TTestStore : TestStore
    {
        protected abstract TTestStore CreateTestDatabase();

        protected abstract DbContext CreateContext(TTestStore testDatabase);

        protected abstract DbContext CreateContext();

        [Fact]
        public void ToListAsync_does_not_deadlock()
        {
            using (var context = CreateContext())
            {
                RunDeadlockTest(context.Set<Customer>().ToListAsync);
            }
        }

        [Fact]
        public void DbSet_LoadAsync_does_not_deadlock()
        {
            using (var context = CreateContext())
            {
                RunDeadlockTest(context.Set<Customer>().LoadAsync);
            }
        }
        
        [Fact]
        public void DbContext_SaveChangesAsync_does_not_deadlock()
        {
            using (var testDatabase = CreateTestDatabase())
            {
                using (var context = CreateContext(testDatabase))
                {
                    context.Set<Product>().Add(new Product { ProductID = 78, SupplierID = 1 });
                    RunDeadlockTest(() => context.SaveChangesAsync());
                }
            }
        }

        private void RunDeadlockTest(Func<Task> asyncOperation)
        {
            var dispatcherThread = new Thread(Dispatcher.Run) { Name = "Dispatcher" };
            dispatcherThread.Start();

            Dispatcher dispatcher = null;
            // Wait for dispatcher to start up
            while ((dispatcher = Dispatcher.FromThread(dispatcherThread)) == null)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(15));
            }

            try
            {
                Assert.Equal(
                    DispatcherOperationStatus.Completed,
                    dispatcher.InvokeAsync(
                        () => Assert.True(asyncOperation().Wait(TimeSpan.FromMinutes(1)), "Async operation resulted in a deadlock"))
                        .Wait(TimeSpan.FromMinutes(1.5)));
            }
            catch (TaskCanceledException)
            {
                // Sometimes thrown by the dispatcher, doesn't indicate a deadlock
            }
            finally
            {
                // Do our best to cleanup, but don't fail the test if not possible to do so in the allocated time
                dispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
                var startShutdownTime = DateTime.Now;
                while (!dispatcher.HasShutdownFinished
                       && DateTime.Now - startShutdownTime < TimeSpan.FromSeconds(10))
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(100));
                }

                Task.Run(() => dispatcherThread.Abort()).Wait(TimeSpan.FromSeconds(10));
            }
        }
    }
}

#endif
