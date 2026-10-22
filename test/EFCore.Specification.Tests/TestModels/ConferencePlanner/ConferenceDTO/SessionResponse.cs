// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ConferencePlanner.ConferenceDTO;

#nullable disable

public class SessionResponse : Session
{
    public Track Track { get; set; }

    public List<Speaker> Speakers { get; set; } = [];
}
