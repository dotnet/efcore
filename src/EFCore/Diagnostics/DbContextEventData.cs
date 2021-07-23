// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for events that reference
    ///     a <see cref="DbContext" />.
    /// </summary>
    public class DbContextEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="context"> The current <see cref="DbContext" />, or null if not known. </param>
        public DbContextEventData(
            EventDefinitionBase eventDefinition,
            Func<EventDefinitionBase, EventData, string> messageGenerator,
            DbContext? context)
            : base(eventDefinition, messageGenerator)
        {
            Context = context;
        }

        /// <summary>
        ///     The current <see cref="DbContext" />.
        /// </summary>
        public virtual DbContext? Context { get; }
    }
}
