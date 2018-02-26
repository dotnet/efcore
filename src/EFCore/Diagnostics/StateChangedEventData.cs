// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for events that indicate
    ///     a change of a tracked entity from one <see cref="EntityState" /> to another.
    /// </summary>
    public class StateChangedEventData : EntityEntryEventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="entityEntry"> The entity entry. </param>
        /// <param name="oldState"> The old state. </param>
        /// <param name="newState"> The new state. </param>
        public StateChangedEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] EntityEntry entityEntry,
            EntityState oldState,
            EntityState newState)
            : base(eventDefinition, messageGenerator, entityEntry)
        {
            OldState = oldState;
            NewState = newState;
        }

        /// <summary>
        ///     The old state.
        /// </summary>
        public virtual EntityState OldState { get; }

        /// <summary>
        ///     The new state.
        /// </summary>
        public virtual EntityState NewState { get; }
    }
}
