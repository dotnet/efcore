// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     <para>
    ///         Extensions on <see cref="IDiagnosticsLogger" /> for use by database providers when logging events.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers. It is generally not used in application code.
    ///     </para>
    /// </summary>
    public static class DiagnosticsLoggerExtensions
    {
        /// <summary>
        ///     Checks whether or not the message should be sent to the <see cref="ILogger"/>.
        /// </summary>
        /// <param name="diagnostics"> The <see cref="IDiagnosticsLogger" /> being used. </param>
        /// <param name="definition"> The definition of the event to log. </param>
        /// <returns> True if <see cref="ILogger"/> logging is enabled and the event should not be ignored; false otherwise. </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] // Because hot path for logging
        public static bool ShouldLog(
                [NotNull] this IDiagnosticsLogger diagnostics,
                [NotNull] EventDefinitionBase definition)
            // No null checks; low-level code in hot path for logging.
            => definition.WarningBehavior == WarningBehavior.Throw
                || (diagnostics.Logger.IsEnabled(definition.Level)
                    && definition.WarningBehavior != WarningBehavior.Ignore);

        /// <summary>
        ///     Dispatches the given <see cref="EventData" /> to a <see cref="DiagnosticSource" />, if enabled, and
        ///     a <see cref="IDbContextLogger" />, if enabled.
        /// </summary>
        /// <param name="diagnostics"> The <see cref="IDiagnosticsLogger" /> being used. </param>
        /// <param name="definition"> The definition of the event to log. </param>
        /// <param name="eventData"> The event data. </param>
        /// <param name="diagnosticSourceEnabled"> True to dispatch to a <see cref="DiagnosticSource" />; false otherwise. </param>
        /// <param name="simpleLogEnabled"> True to dispatch to a <see cref="IDbContextLogger" />; false otherwise. </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] // Because hot path for logging
        public static void DispatchEventData(
            [NotNull] this IDiagnosticsLogger diagnostics,
            [NotNull] EventDefinitionBase definition,
            [NotNull] EventData eventData,
            bool diagnosticSourceEnabled,
            bool simpleLogEnabled)
        {
            // No null checks; low-level code in hot path for logging.

            if (diagnosticSourceEnabled)
            {
                diagnostics.DiagnosticSource.Write(definition.EventId.Name, eventData);
            }

            if (simpleLogEnabled)
            {
                diagnostics.DbContextLogger.Log(eventData);
            }
        }

        /// <summary>
        ///     Determines whether or not an <see cref="EventData" /> instance is needed based on whether or
        ///     not there is a <see cref="DiagnosticSource" /> or an <see cref="IDbContextLogger" /> enabled for
        ///     the given event.
        /// </summary>
        /// <param name="diagnostics"> The <see cref="IDiagnosticsLogger" /> being used. </param>
        /// <param name="definition"> The definition of the event. </param>
        /// <param name="diagnosticSourceEnabled"> Set to true if a <see cref="DiagnosticSource" /> is enabled; false otherwise. </param>
        /// <param name="simpleLogEnabled"> True to true if a <see cref="IDbContextLogger" /> is enabled; false otherwise. </param>
        /// <returns> True if either a diagnostic source or a LogTo logger is enabled; false otherwise. </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] // Because hot path for logging
        public static bool NeedsEventData(
            [NotNull] this IDiagnosticsLogger diagnostics,
            [NotNull] EventDefinitionBase definition,
            out bool diagnosticSourceEnabled,
            out bool simpleLogEnabled)
        {
            // No null checks; low-level code in hot path for logging.

            diagnosticSourceEnabled = diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name);

            simpleLogEnabled = definition.WarningBehavior == WarningBehavior.Log
                && diagnostics.DbContextLogger.ShouldLog(definition.EventId, definition.Level);

            return diagnosticSourceEnabled
                || simpleLogEnabled;
        }

        /// <summary>
        ///     Determines whether or not an <see cref="EventData" /> instance is needed based on whether or
        ///     not there is a <see cref="DiagnosticSource" />, an <see cref="IDbContextLogger" />, or an <see cref="IInterceptor" /> enabled for
        ///     the given event.
        /// </summary>
        /// <param name="diagnostics"> The <see cref="IDiagnosticsLogger" /> being used. </param>
        /// <param name="definition"> The definition of the event. </param>
        /// <param name="interceptor"> The <see cref="IInterceptor" /> to use if enabled; otherwise null. </param>
        /// <param name="diagnosticSourceEnabled"> Set to true if a <see cref="DiagnosticSource" /> is enabled; false otherwise. </param>
        /// <param name="simpleLogEnabled"> True to true if a <see cref="IDbContextLogger" /> is enabled; false otherwise. </param>
        /// <returns> True if either a diagnostic source, a LogTo logger, or an interceptor is enabled; false otherwise. </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] // Because hot path for logging
        public static bool NeedsEventData<TInterceptor>(
            [NotNull] this IDiagnosticsLogger diagnostics,
            [NotNull] EventDefinitionBase definition,
            [CanBeNull] out TInterceptor interceptor,
            out bool diagnosticSourceEnabled,
            out bool simpleLogEnabled)
            where TInterceptor : class, IInterceptor
        {
            // No null checks; low-level code in hot path for logging.

            diagnosticSourceEnabled = diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name);

            interceptor = diagnostics.Interceptors?.Aggregate<TInterceptor>();

            simpleLogEnabled = definition.WarningBehavior == WarningBehavior.Log
                && diagnostics.DbContextLogger.ShouldLog(definition.EventId, definition.Level);

            return diagnosticSourceEnabled
                || simpleLogEnabled
                || interceptor != null;
        }
    }
}
