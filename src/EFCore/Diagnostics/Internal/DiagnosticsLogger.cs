// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;

namespace Microsoft.EntityFrameworkCore.Diagnostics.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class DiagnosticsLogger<TLoggerCategory> : IDiagnosticsLogger<TLoggerCategory>
    where TLoggerCategory : LoggerCategory<TLoggerCategory>, new()
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public DiagnosticsLogger(
        ILoggerFactory loggerFactory,
        ILoggingOptions loggingOptions,
        DiagnosticSource diagnosticSource,
        LoggingDefinitions loggingDefinitions,
        IDbContextLogger contextLogger,
        IInterceptors? interceptors = null)
    {
        DiagnosticSource = diagnosticSource;
        Definitions = loggingDefinitions;
        DbContextLogger = contextLogger;
        Logger = loggerFactory.CreateLogger(new TLoggerCategory());
        Options = loggingOptions;
        Interceptors = interceptors;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ILoggingOptions Options { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ILogger Logger { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IInterceptors? Interceptors { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DiagnosticSource DiagnosticSource { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual LoggingDefinitions Definitions { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IDbContextLogger DbContextLogger { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool ShouldLogSensitiveData()
    {
        var options = Options;

        if (options is { IsSensitiveDataLoggingEnabled: true, IsSensitiveDataLoggingWarned: false })
        {
            this.SensitiveDataLoggingEnabledWarning();

            options.IsSensitiveDataLoggingWarned = true;
        }

        return options.IsSensitiveDataLoggingEnabled;
    }

    /// <summary>
    ///     Dispatches the given <see cref="EventData" /> to a <see cref="DiagnosticSource" />, if enabled, and
    ///     a <see cref="IDbContextLogger" />, if enabled.
    /// </summary>
    /// <param name="definition">The definition of the event to log.</param>
    /// <param name="eventData">The event data.</param>
    /// <param name="diagnosticSourceEnabled">True to dispatch to a <see cref="DiagnosticSource" />; <see langword="false" /> otherwise.</param>
    /// <param name="simpleLogEnabled">True to dispatch to a <see cref="IDbContextLogger" />; <see langword="false" /> otherwise.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] // Because hot path for logging
    protected void DispatchEventData(
        EventDefinitionBase definition,
        EventData eventData,
        bool diagnosticSourceEnabled,
        bool simpleLogEnabled)
        => ((IDiagnosticsLogger)this).DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

    /// <summary>
    ///     Checks whether or not the message should be sent to the <see cref="ILogger" />.
    /// </summary>
    /// <param name="definition">The definition of the event to log.</param>
    /// <returns>
    ///     <see langword="true" /> if <see cref="ILogger" /> logging is enabled and the event should not be ignored;
    ///     <see langword="false" /> otherwise.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] // Because hot path for logging
    protected bool ShouldLog(EventDefinitionBase definition)
        => ((IDiagnosticsLogger)this).ShouldLog(definition);

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
    protected bool NeedsEventData<TInterceptor>(
        EventDefinitionBase definition,
        out TInterceptor? interceptor,
        out bool diagnosticSourceEnabled,
        out bool simpleLogEnabled)
        where TInterceptor : class, IInterceptor
        => ((IDiagnosticsLogger)this).NeedsEventData(definition, out interceptor, out diagnosticSourceEnabled, out simpleLogEnabled);
}
