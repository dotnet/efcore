// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ConferencePlanner
{
    public class SessionAttendee
    {
        public int SessionId { get; set; }

        public Session Session { get; set; }

        public int AttendeeId { get; set; }

        public Attendee Attendee { get; set; }
    }
}
