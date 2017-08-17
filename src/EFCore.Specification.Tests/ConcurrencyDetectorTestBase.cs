// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class ConcurrencyDetectorTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : NorthwindQueryFixtureBase, new()
    {
        protected ConcurrencyDetectorTestBase(TFixture fixture)
        {
            Fixture = fixture;
        }

        protected TFixture Fixture { get; }

        protected NorthwindContext CreateContext() => Fixture.CreateContext();

        [Fact]
        public virtual async Task SaveChanges_logs_concurrent_access_nonasync()
        {
            await ConcurrencyDetectorTest(
                c =>
                    {
                        c.SaveChanges();
                        return Task.FromResult(false);
                    },
                async: false);
        }

        [Fact]
        public virtual async Task SaveChanges_logs_concurrent_access_async()
        {
            await ConcurrencyDetectorTest(c => c.SaveChangesAsync(), async: true);
        }

        [Fact(Skip = "Failed during 2.0.1 branching")]
        public virtual async Task Find_logs_concurrent_access_nonasync()
        {
            await ConcurrencyDetectorTest(
                c =>
                    {
                        c.Products.Find(1);
                        return Task.FromResult(false);
                    },
                async: false);
        }

        [Fact(Skip = "#9074")]
        public virtual async Task Find_logs_concurrent_access_async()
        {
            await ConcurrencyDetectorTest(c => c.Products.FindAsync(1), async: true);
        }

        [Fact]
        public virtual async Task Count_logs_concurrent_access_nonasync()
        {
            await ConcurrencyDetectorTest(
                c =>
                    {
                        c.Products.Count();
                        return Task.FromResult(false);
                    },
                async: false);
        }

        [Fact(Skip = "#9074")]
        public virtual async Task Count_logs_concurrent_access_async()
        {
            await ConcurrencyDetectorTest(c => c.Products.ToListAsync(), async: true);
        }

        [Fact]
        public virtual async Task ToList_logs_concurrent_access_nonasync()
        {
            await ConcurrencyDetectorTest(
                c =>
                    {
                        c.Products.Count();
                        return Task.FromResult(false);
                    },
                async: false);
        }

        [Fact]
        public virtual async Task ToList_logs_concurrent_access_async()
        {
            await ConcurrencyDetectorTest(c => c.Products.ToListAsync(), async: true);
        }

        protected virtual async Task ConcurrencyDetectorTest(Func<NorthwindContext, Task> test, bool async)
        {
            using (var context = CreateContext())
            {
                context.Products.Add(new Product());

                context.GetService<IConcurrencyDetector>().EnterCriticalSection();

                Exception ex;
                if (async)
                {
                    ex = await Assert.ThrowsAsync<InvalidOperationException>(() => test(context));
                }
                else
                {
                    ex = Assert.Throws<InvalidOperationException>(() => { test(context); });
                }

                Assert.Equal(CoreStrings.ConcurrentMethodInvocation, ex.Message);
            }
        }
    }
}
