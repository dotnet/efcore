// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

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
