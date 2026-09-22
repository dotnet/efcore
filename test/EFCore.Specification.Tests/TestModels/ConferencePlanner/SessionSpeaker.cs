// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ConferencePlanner;

#nullable disable

public class SessionSpeaker
{
    public int SessionId { get; set; }

    public Session Session { get; set; }

    public int SpeakerId { get; set; }

    public Speaker Speaker { get; set; }
}
