// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for events that indicate
    ///     a changed property value.
    /// </summary>
    public class PropertyChangedEventData : PropertyEventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="entityEntry"> The entry for the entity instance on which the property value has changed. </param>
        /// <param name="property"> The property. </param>
        /// <param name="oldValue"> The old value. </param>
        /// <param name="newValue"> The new value. </param>
        public PropertyChangedEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] EntityEntry entityEntry,
            [NotNull] IProperty property,
            [CanBeNull] object oldValue,
            [CanBeNull] object newValue)
            : base(eventDefinition, messageGenerator, property)
        {
            Check.NotNull(entityEntry, nameof(entityEntry));

            EntityEntry = entityEntry;
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        ///     The entry for the entity instance on which the property value has changed.
        /// </summary>
        public virtual EntityEntry EntityEntry { get; }

        /// <summary>
        ///     The old value.
        /// </summary>
        public virtual object OldValue { get; }

        /// <summary>
        ///     The new value.
        /// </summary>
        public virtual object NewValue { get; }
    }
}
