// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestDbContextLogger : IDbContextLogger
    {
        public LogLevel? LoggedAt { get; set; }
        public EventId LoggedEvent { get; set; }

        public void Log(EventData eventData)
        {
            LoggedAt = eventData.LogLevel;
            LoggedEvent = eventData.EventId;
        }

        public bool ShouldLog(EventId eventId, LogLevel logLevel)
            => true;
    }
}
