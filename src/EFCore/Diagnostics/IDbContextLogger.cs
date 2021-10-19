// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A simple logging interface for Entity Framework events.
    ///     Used by <see cref="DbContextOptionsBuilder.LogTo(Action{string},LogLevel,DbContextLoggerOptions?)" />
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-simple-logging">EF Core simple logging</see> for more information.
    /// </remarks>
    public interface IDbContextLogger
    {
        /// <summary>
        ///     Logs the given <see cref="EventData" />.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This method is only called if <see cref="ShouldLog" /> returns true.
        ///     </para>
        ///     <para>
        ///         The specific subtype of the <see cref="EventData" /> argument is dependent on the event
        ///         being logged. See <see cref="CoreEventId" /> for the type of event data used for each core event.
        ///     </para>
        /// </remarks>
        /// <param name="eventData">The event to log.</param>
        void Log(EventData eventData);

        /// <summary>
        ///     Determines whether or not the given event should be logged.
        /// </summary>
        /// <param name="eventId">The ID of the event.</param>
        /// <param name="logLevel">The level of the event.</param>
        /// <returns>Returns <see langword="true" /> if the event should be logged; <see langword="false" /> if it should be filtered out.</returns>
        bool ShouldLog(EventId eventId, LogLevel logLevel);
    }
}
