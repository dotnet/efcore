// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable UnusedVariable
// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore;

#nullable disable

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
