// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
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
            EventDefinitionBase eventDefinition,
            Func<EventDefinitionBase, EventData, string> messageGenerator,
            EntityEntry entityEntry,
            IProperty property,
            object? value)
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
        ///     The property.
        /// </summary>
        public new virtual IProperty Property => (IProperty)base.Property;

        /// <summary>
        ///     The value.
        /// </summary>
        public virtual object? Value { get; }
    }
}
