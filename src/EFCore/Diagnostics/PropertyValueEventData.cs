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
    ///     a property value.
    /// </summary>
    public class PropertyValueEventData : PropertyEventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="entityEntry"> The entry for the entity instance on which the property value has changed. </param>
        /// <param name="property"> The property. </param>
        /// <param name="value"> The old value. </param>
        public PropertyValueEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] EntityEntry entityEntry,
            [NotNull] IProperty property,
            [CanBeNull] object value)
            : base(eventDefinition, messageGenerator, property)
        {
            Check.NotNull(entityEntry, nameof(entityEntry));

            EntityEntry = entityEntry;
            Value = value;
        }

        /// <summary>
        ///     The entry for the entity instance.
        /// </summary>
        public virtual EntityEntry EntityEntry { get; }

        /// <summary>
        ///     The value.
        /// </summary>
        public virtual object Value { get; }
    }
}
