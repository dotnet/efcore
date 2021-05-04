// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel
{
    public class TeamSponsor
    {
        public int TeamId { get; set; }
        public int SponsorId { get; set; }

        public virtual Team Team { get; set; }
        public virtual Sponsor Sponsor { get; set; }
    }
}
