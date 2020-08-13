// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public abstract class TestLoggerBase
    {
        public LogLevel EnabledFor { get; set; }
        public LogLevel? LoggedAt { get; set; }
        public EventId LoggedEvent { get; set; }
        public string Message { get; set; }

        public DiagnosticSource DiagnosticSource { get; } = new TestDiagnosticSource();
    }
}
