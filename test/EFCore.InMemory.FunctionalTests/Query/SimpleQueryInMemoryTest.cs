// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class SimpleQueryInMemoryTest : SimpleQueryTestBase
{
    public async override Task Multiple_nested_reference_navigations(bool async)
    {
        var contextFactory = await InitializeAsync<Context24368>();
        using var context = contextFactory.CreateContext();
        var id = 1;
        var staff = await context.Staff.FindAsync(3);

        Assert.Equal(1, staff.ManagerId);

        var query = context.Appraisals
            .Include(ap => ap.Staff).ThenInclude(s => s.Manager)
            .Include(ap => ap.Staff).ThenInclude(s => s.SecondaryManager)
            .Where(ap => ap.Id == id);

        var appraisal = async
            ? await query.SingleOrDefaultAsync()
            : query.SingleOrDefault();

        Assert.Equal(1, staff.ManagerId); // InMemory has different behavior

        Assert.NotNull(appraisal);
        Assert.Same(staff, appraisal.Staff);
        Assert.NotNull(appraisal.Staff.SecondaryManager);
        Assert.Equal(2, appraisal.Staff.SecondaryManagerId);
    }
    
    protected override ITestStoreFactory TestStoreFactory
        => InMemoryTestStoreFactory.Instance;
}
