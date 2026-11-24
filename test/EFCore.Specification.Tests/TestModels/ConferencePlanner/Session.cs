// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ConferencePlanner;

public class Session : ConferenceDTO.Session
{
    public virtual ICollection<SessionSpeaker> SessionSpeakers { get; set; } = null!;

    public virtual ICollection<SessionAttendee> SessionAttendees { get; set; } = null!;

    public Track Track { get; set; } = null!;
}
