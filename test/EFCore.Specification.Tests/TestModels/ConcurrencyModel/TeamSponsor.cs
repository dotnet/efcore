// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;

public class TeamSponsor
{
    public int TeamId { get; set; }
    public int SponsorId { get; set; }

    public virtual Team Team { get; set; }
    public virtual Sponsor Sponsor { get; set; }
}
