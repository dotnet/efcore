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
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public interface IDiagnosticsLogger<TLoggerCategory> : IDiagnosticsLogger
        where TLoggerCategory : LoggerCategory<TLoggerCategory>, new()
    {
        /// <summary>
        ///     Holds registered interceptors, if any.
        /// </summary>
        IInterceptors Interceptors { get; }
    }
}
