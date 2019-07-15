// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.TestModels.ConferencePlanner
{
    public class SessionSpeaker
    {
        public int SessionId { get; set; }

        public Session Session { get; set; }

        public int SpeakerId { get; set; }

        public Speaker Speaker { get; set; }
    }
}
