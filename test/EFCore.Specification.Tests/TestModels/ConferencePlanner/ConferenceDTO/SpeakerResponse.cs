// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ConferencePlanner.ConferenceDTO;

public class SpeakerResponse : Speaker
{
    // TODO: Set order of JSON proeprties so this shows up last not first
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}
