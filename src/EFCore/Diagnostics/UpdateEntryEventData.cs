﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for events that have
    ///     an entity update entry.
    /// </summary>
    public class UpdateEntryEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="entityEntry"> The entry for the entity instance on which the property value has changed. </param>
        public UpdateEntryEventData(
            EventDefinitionBase eventDefinition,
            Func<EventDefinitionBase, EventData, string> messageGenerator,
            IUpdateEntry entityEntry)
            : base(eventDefinition, messageGenerator)
        {
            EntityEntry = entityEntry;
        }

        /// <summary>
        ///     The entry for the entity instance on which the property value has changed.
        /// </summary>
        public virtual IUpdateEntry EntityEntry { get; }
    }
}
