// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
namespace Microsoft.EntityFrameworkCore.TestModels.ConferencePlanner.ConferenceDTO
{
    public class SearchResult
    {
        public SearchResultType Type { get; set; }

        public SessionResponse Session { get; set; }

        public SpeakerResponse Speaker { get; set; }
    }
}
