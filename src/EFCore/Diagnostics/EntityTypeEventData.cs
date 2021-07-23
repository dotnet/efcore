﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for events that have
    ///     an entity type.
    /// </summary>
    public class EntityTypeEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="entityType"> The entityType. </param>
        public EntityTypeEventData(
            EventDefinitionBase eventDefinition,
            Func<EventDefinitionBase, EventData, string> messageGenerator,
            IReadOnlyEntityType entityType)
            : base(eventDefinition, messageGenerator)
        {
            EntityType = entityType;
        }

        /// <summary>
        ///     The entity type.
        /// </summary>
        public virtual IReadOnlyEntityType EntityType { get; }
    }
}
