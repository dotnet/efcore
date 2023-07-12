// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ConferencePlanner;

public class Track : ConferenceDTO.Track
{
    public virtual ICollection<Session> Sessions { get; set; }
}
