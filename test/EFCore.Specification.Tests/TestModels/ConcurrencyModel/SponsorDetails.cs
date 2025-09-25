// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;

public class SponsorDetails
{
    public class SponsorDetailsProxy(
        int days,
        decimal space) : SponsorDetails(days, space), IF1Proxy
    {
        public bool CreatedCalled { get; set; }
        public bool InitializingCalled { get; set; }
        public bool InitializedCalled { get; set; }
    }

    public SponsorDetails()
    {
    }

    private SponsorDetails(int days, decimal space)
    {
        Days = days;
        Space = space;

        Assert.IsType<SponsorDetailsProxy>(this);
    }

    public int Days { get; set; }
    public decimal Space { get; set; }
}
