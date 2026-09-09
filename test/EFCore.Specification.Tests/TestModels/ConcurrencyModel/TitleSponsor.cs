// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;

#nullable disable

public class TitleSponsor : Sponsor
{
    public class TitleSponsorProxy(ILazyLoader loader) : TitleSponsor(loader), IF1Proxy
    {
        public bool CreatedCalled { get; set; }
        public bool InitializingCalled { get; set; }
        public bool InitializedCalled { get; set; }
    }

    private readonly ILazyLoader _loader;
    private SponsorDetails _details;

    public TitleSponsor()
    {
    }

    private TitleSponsor(ILazyLoader loader)
    {
        _loader = loader;

        Assert.IsType<TitleSponsorProxy>(this);
    }

    public SponsorDetails Details
    {
        get => _loader.Load(this, ref _details);
        set => _details = value;
    }
}
