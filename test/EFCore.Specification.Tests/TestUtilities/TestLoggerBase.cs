// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

#nullable disable

public abstract class TestLoggerBase
{
    public LogLevel EnabledFor { get; set; }
    public LogLevel? LoggedAt { get; set; }
    public EventId LoggedEvent { get; set; }
    public string Message { get; set; }

    public IDbContextLogger DbContextLogger { get; } = new TestDbContextLogger();

    public DiagnosticSource DiagnosticSource { get; } = new TestDiagnosticSource();
}
