// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ConferencePlanner;

public class SessionSpeaker
{
    public int SessionId { get; set; }

    public Session Session { get; set; } = null!;

    public int SpeakerId { get; set; }

    public Speaker Speaker { get; set; } = null!;
}
