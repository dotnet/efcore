// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for events that reference
    ///     a <see cref="DbContext" /> type.
    /// </summary>
    /// <remarks>
    ///     For more information, see <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see>.
    /// </remarks>
    public class DbContextTypeEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="contextType"> The current <see cref="DbContext" />. </param>
        public DbContextTypeEventData(
            EventDefinitionBase eventDefinition,
            Func<EventDefinitionBase, EventData, string> messageGenerator,
            Type contextType)
            : base(eventDefinition, messageGenerator)
        {
            ContextType = contextType;
        }

        /// <summary>
        ///     The current <see cref="DbContext" />.
        /// </summary>
        public virtual Type ContextType { get; }
    }
}
