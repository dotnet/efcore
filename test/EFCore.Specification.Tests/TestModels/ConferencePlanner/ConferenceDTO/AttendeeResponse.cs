// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.ConferencePlanner.ConferenceDTO
{
    public class AttendeeResponse : Attendee
    {
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}
