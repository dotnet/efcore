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
    public class ReferenceChangedEventData : NavigationEventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="entityEntry"> The entry for the entity instance on which the property value has changed. </param>
        /// <param name="navigation"> The navigation property. </param>
        /// <param name="oldReferencedEntity"> The old referenced entity. </param>
        /// <param name="newReferencedEntity"> The new referenced entity. </param>
        public ReferenceChangedEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] EntityEntry entityEntry,
            [NotNull] INavigation navigation,
            [CanBeNull] object oldReferencedEntity,
            [CanBeNull] object newReferencedEntity)
            : base(eventDefinition, messageGenerator, navigation)
        {
            Check.NotNull(entityEntry, nameof(entityEntry));

            EntityEntry = entityEntry;
            OldReferencedEntity = oldReferencedEntity;
            NewReferencedEntity = newReferencedEntity;
        }

        /// <summary>
        ///     The entry for the entity instance on which the navigation property value has changed.
        /// </summary>
        public virtual EntityEntry EntityEntry { get; }

        /// <summary>
        ///     The old referenced entity.
        /// </summary>
        public virtual object OldReferencedEntity { get; }

        /// <summary>
        ///     The new referenced entity.
        /// </summary>
        public virtual object NewReferencedEntity { get; }
    }
}
