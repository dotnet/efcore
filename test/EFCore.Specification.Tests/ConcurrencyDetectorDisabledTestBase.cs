// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Xunit;

// ReSharper disable UnusedVariable
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public abstract class ConcurrencyDetectorDisabledTestBase<TFixture> : ConcurrencyDetectorTestBase<TFixture>
        where TFixture : ConcurrencyDetectorTestBase<TFixture>.ConcurrencyDetectorFixtureBase, new()
    {
        protected ConcurrencyDetectorDisabledTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task SaveChanges(bool async)
        {
            await ConcurrencyDetectorTest(
                async c =>
                {
                    c.Products.Add(new Product { Id = 3, Name = "Unicorn Horseshoe Protection Pack" });
                    return async ? await c.SaveChangesAsync() : c.SaveChanges();
                });

            using var ctx = CreateContext();
            var newProduct = await ctx.Products.FindAsync(3);
            Assert.NotNull(newProduct);
            ctx.Products.Remove(newProduct);
            await ctx.SaveChangesAsync();
        }

        protected override async Task ConcurrencyDetectorTest(Func<ConcurrencyDetectorDbContext, Task<object>> test)
        {
            using var context = CreateContext();

            var concurrencyDetector = context.GetService<IConcurrencyDetector>();
            IDisposable disposer = null;

            await Task.Run(() => disposer = concurrencyDetector.EnterCriticalSection());

            using (disposer)
            {
                await test(context);
            }
        }
    }
}
