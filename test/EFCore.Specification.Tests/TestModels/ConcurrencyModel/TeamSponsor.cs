// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;

#nullable disable

public class TeamSponsor
{
    public class TeamSponsorProxy : TeamSponsor, IF1Proxy
    {
        public bool CreatedCalled { get; set; }
        public bool InitializingCalled { get; set; }
        public bool InitializedCalled { get; set; }
    }

    public int TeamId { get; set; }
    public int SponsorId { get; set; }

    public virtual Team Team { get; set; }
    public virtual Sponsor Sponsor { get; set; }
}
