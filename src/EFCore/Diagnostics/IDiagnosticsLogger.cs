// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     Combines <see cref="ILogger" /> and <see cref="DiagnosticSource" />
///     for use by all EF Core logging so that events can be sent to both <see cref="ILogger" />
///     for ASP.NET and <see cref="DiagnosticSource" /> for everything else.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public interface IDiagnosticsLogger
{
    /// <summary>
    ///     Entity Framework logging options.
    /// </summary>
    ILoggingOptions Options { get; }

    /// <summary>
    ///     Caching for logging definitions.
    /// </summary>
    LoggingDefinitions Definitions { get; }

    /// <summary>
    ///     Gets a value indicating whether sensitive information should be written
    ///     to the underlying logger. This also has the side effect of writing a warning
    ///     to the log the first time sensitive data is logged.
    /// </summary>
    bool ShouldLogSensitiveData();

    /// <summary>
    ///     The underlying <see cref="ILogger" />.
    /// </summary>
    ILogger Logger { get; }

    /// <summary>
    ///     The <see cref="DiagnosticSource" />.
    /// </summary>
    DiagnosticSource DiagnosticSource { get; }

    /// <summary>
    ///     The <see cref="IDbContextLogger" />.
    /// </summary>
    IDbContextLogger DbContextLogger { get; }

    /// <summary>
    ///     Holds registered interceptors, if any.
    /// </summary>
    IInterceptors? Interceptors { get; }

    /// <summary>
    ///     Checks whether or not the message should be sent to the <see cref="ILogger" />.
    /// </summary>
    /// <param name="definition">The definition of the event to log.</param>
    /// <returns>
    ///     <see langword="true" /> if <see cref="ILogger" /> logging is enabled and the event should not be ignored;
    ///     <see langword="false" /> otherwise.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] // Because hot path for logging
    bool ShouldLog(EventDefinitionBase definition)
        // No null checks; low-level code in hot path for logging.
        => definition.WarningBehavior == WarningBehavior.Throw
            || (Logger.IsEnabled(definition.Level)
                && definition.WarningBehavior != WarningBehavior.Ignore);

    /// <summary>
    ///     Dispatches the given <see cref="EventData" /> to a <see cref="DiagnosticSource" />, if enabled, and
    ///     a <see cref="IDbContextLogger" />, if enabled.
    /// </summary>
    /// <param name="definition">The definition of the event to log.</param>
    /// <param name="eventData">The event data.</param>
    /// <param name="diagnosticSourceEnabled">True to dispatch to a <see cref="DiagnosticSource" />; <see langword="false" /> otherwise.</param>
    /// <param name="simpleLogEnabled">True to dispatch to a <see cref="IDbContextLogger" />; <see langword="false" /> otherwise.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] // Because hot path for logging
    void DispatchEventData(
        EventDefinitionBase definition,
        EventData eventData,
        bool diagnosticSourceEnabled,
        bool simpleLogEnabled)
    {
        // No null checks; low-level code in hot path for logging.

        if (diagnosticSourceEnabled)
        {
            DiagnosticSource.Write(definition.EventId.Name!, eventData);
        }

        if (simpleLogEnabled)
        {
            DbContextLogger.Log(eventData);
        }
    }

    /// <summary>
    ///     Determines whether or not an <see cref="EventData" /> instance is needed based on whether or
    ///     not there is a <see cref="DiagnosticSource" /> or an <see cref="IDbContextLogger" /> enabled for
    ///     the given event.
    /// </summary>
    /// <param name="definition">The definition of the event.</param>
    /// <param name="diagnosticSourceEnabled">
    ///     Set to <see langword="true" /> if a <see cref="DiagnosticSource" /> is enabled;
    ///     <see langword="false" /> otherwise.
    /// </param>
    /// <param name="simpleLogEnabled">
    ///     True to <see langword="true" /> if a <see cref="IDbContextLogger" /> is enabled; <see langword="false" />
    ///     otherwise.
    /// </param>
    /// <returns><see langword="true" /> if either a diagnostic source or a LogTo logger is enabled; <see langword="false" /> otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] // Because hot path for logging
    bool NeedsEventData(
        EventDefinitionBase definition,
        out bool diagnosticSourceEnabled,
        out bool simpleLogEnabled)
    {
        // No null checks; low-level code in hot path for logging.

        diagnosticSourceEnabled = DiagnosticSource.IsEnabled(definition.EventId.Name!);

        simpleLogEnabled = definition.WarningBehavior == WarningBehavior.Log
            && DbContextLogger.ShouldLog(definition.EventId, definition.Level);

        return diagnosticSourceEnabled
            || simpleLogEnabled;
    }

    /// <summary>
    ///     Determines whether or not an <see cref="EventData" /> instance is needed based on whether or
    ///     not there is a <see cref="DiagnosticSource" />, an <see cref="IDbContextLogger" />, or an <see cref="IInterceptor" /> enabled for
    ///     the given event.
    /// </summary>
    /// <param name="definition">The definition of the event.</param>
    /// <param name="interceptor">The <see cref="IInterceptor" /> to use if enabled; otherwise null.</param>
    /// <param name="diagnosticSourceEnabled">
    ///     Set to <see langword="true" /> if a <see cref="DiagnosticSource" /> is enabled;
    ///     <see langword="false" /> otherwise.
    /// </param>
    /// <param name="simpleLogEnabled">
    ///     True to <see langword="true" /> if a <see cref="IDbContextLogger" /> is enabled; <see langword="false" />
    ///     otherwise.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if either a diagnostic source, a LogTo logger, or an interceptor is enabled; <see langword="false" />
    ///     otherwise.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] // Because hot path for logging
    bool NeedsEventData<TInterceptor>(
        EventDefinitionBase definition,
        out TInterceptor? interceptor,
        out bool diagnosticSourceEnabled,
        out bool simpleLogEnabled)
        where TInterceptor : class, IInterceptor
    {
        // No null checks; low-level code in hot path for logging.

        diagnosticSourceEnabled = DiagnosticSource.IsEnabled(definition.EventId.Name!);

        interceptor = Interceptors?.Aggregate<TInterceptor>();

        simpleLogEnabled = definition.WarningBehavior == WarningBehavior.Log
            && DbContextLogger.ShouldLog(definition.EventId, definition.Level);

        return diagnosticSourceEnabled
            || simpleLogEnabled
            || interceptor != null;
    }
}
