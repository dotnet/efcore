// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ConferencePlanner;

#nullable disable

public class Attendee : ConferenceDTO.Attendee
{
    public virtual ICollection<SessionAttendee> SessionsAttendees { get; set; }
}
