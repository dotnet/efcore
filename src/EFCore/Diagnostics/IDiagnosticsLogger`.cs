// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     <para>
    ///         Combines <see cref="ILogger" /> and <see cref="DiagnosticSource" />
    ///         for use by all EF Core logging so that events can be sent to both <see cref="ILogger" />
    ///         for ASP.NET and <see cref="DiagnosticSource" /> for everything else.
    ///     </para>
    ///     <para>
    ///         Also intercepts messages such that warnings
    ///         can be either logged or thrown, and such that a decision as to whether to log
    ///         sensitive data or not can be made.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>. This means a single instance
    ///         is used by many <see cref="DbContext"/> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
    /// </summary>
    public interface IDiagnosticsLogger<TLoggerCategory> : IDiagnosticsLogger
        where TLoggerCategory : LoggerCategory<TLoggerCategory>, new()
    {
    }
}
