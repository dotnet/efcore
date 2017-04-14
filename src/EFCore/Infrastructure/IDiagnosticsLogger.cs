// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     Combines <see cref="IInterceptingLogger{TLoggerCategory}" /> and <see cref="DiagnosticSource" />
    ///     for use by all EF Core logging so that events can be sent to both <see cref="ILogger" />
    ///     for ASP.NET and <see cref="DiagnosticSource" /> for everything else.
    /// </summary>
    public interface IDiagnosticsLogger<TLoggerCategory>
        where TLoggerCategory : LoggerCategory<TLoggerCategory>, new()
    {
        /// <summary>
        ///     The <see cref="IInterceptingLogger{TLoggerCategory}" />.
        /// </summary>
        IInterceptingLogger<TLoggerCategory> Logger { get; }

        /// <summary>
        ///     The <see cref="DiagnosticSource" />.
        /// </summary>
        DiagnosticSource DiagnosticSource { get; }
    }
}
