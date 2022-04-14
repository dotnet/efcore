// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;

public class TitleSponsor : Sponsor
{
    private readonly ILazyLoader _loader;
    private SponsorDetails _details;

    public TitleSponsor()
    {
    }

    private TitleSponsor(ILazyLoader loader)
    {
        _loader = loader;
    }

    public SponsorDetails Details
    {
        get => _loader.Load(this, ref _details);
        set => _details = value;
    }
}
