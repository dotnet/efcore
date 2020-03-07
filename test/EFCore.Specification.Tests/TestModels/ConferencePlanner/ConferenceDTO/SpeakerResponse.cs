// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.ConferencePlanner.ConferenceDTO
{
    public class SpeakerResponse : Speaker
    {
        // TODO: Set order of JSON proeprties so this shows up last not first
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}
