// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable UnusedVariable
// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class ConcurrencyDetectorEnabledTestBase<TFixture> : ConcurrencyDetectorTestBase<TFixture>
    where TFixture : ConcurrencyDetectorTestBase<TFixture>.ConcurrencyDetectorFixtureBase, new()
{
    protected ConcurrencyDetectorEnabledTestBase(TFixture fixture)
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
                c.Products.Add(new Product { Id = 2, Name = "Unicorn Replacement Horn Pack" });
                return async ? await c.SaveChangesAsync() : c.SaveChanges();
            });

        using var ctx = CreateContext();
        var newProduct = await ctx.Products.SingleOrDefaultAsync(p => p.Id == 2);
        Assert.Null(newProduct);
    }

    protected override async Task ConcurrencyDetectorTest(Func<ConcurrencyDetectorDbContext, Task<object>> test)
    {
        using var context = CreateContext();

        var concurrencyDetector = context.GetService<IConcurrencyDetector>();
        IDisposable disposer = null;

        await Task.Run(() => disposer = concurrencyDetector.EnterCriticalSection());

        using (disposer)
        {
            Exception ex = await Assert.ThrowsAsync<InvalidOperationException>(() => test(context));

            Assert.Equal(CoreStrings.ConcurrentMethodInvocation, ex.Message);
        }
    }
}
