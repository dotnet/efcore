// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ConferencePlanner;

#nullable disable

public class Session : ConferenceDTO.Session
{
    public virtual ICollection<SessionSpeaker> SessionSpeakers { get; set; }

    public virtual ICollection<SessionAttendee> SessionAttendees { get; set; }

    public Track Track { get; set; }
}
