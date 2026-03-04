// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ConferencePlanner.ConferenceDTO;

#nullable disable

public class SearchResult
{
    public SearchResultType Type { get; set; }

    public SessionResponse Session { get; set; }

    public SpeakerResponse Speaker { get; set; }
}
