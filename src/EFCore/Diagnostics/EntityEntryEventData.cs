// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for events that reference
    ///     a <see cref="EntityEntry" />.
    /// </summary>
    public class EntityEntryEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="entityEntry"> The entity entry. </param>
        public EntityEntryEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] EntityEntry entityEntry)
            : base(eventDefinition, messageGenerator)
        {
            Check.NotNull(entityEntry, nameof(entityEntry));

            EntityEntry = entityEntry;
        }

        /// <summary>
        ///     The entity entry.
        /// </summary>
        public virtual EntityEntry EntityEntry { get; }
    }
}
