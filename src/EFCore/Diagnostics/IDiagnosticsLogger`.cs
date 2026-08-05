// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     Combines <see cref="ILogger" /> and <see cref="DiagnosticSource" />
///     for use by all EF Core logging so that events can be sent to both <see cref="ILogger" />
///     for ASP.NET and <see cref="DiagnosticSource" /> for everything else.
/// </summary>
/// <remarks>
///     <para>
///         Also intercepts messages such that warnings can be either logged or thrown, and such that a decision as to whether to log
///         sensitive data or not can be made.
///     </para>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public interface IDiagnosticsLogger<TLoggerCategory> : IDiagnosticsLogger
    where TLoggerCategory : LoggerCategory<TLoggerCategory>, new();
