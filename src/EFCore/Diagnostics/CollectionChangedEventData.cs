// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for events that indicate
    ///     a collection navigation property has had entities added and/or removed.
    /// </summary>
    public class CollectionChangedEventData : NavigationEventData, ICollectionChangedEventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="entityEntry"> The entry for the entity instance on which the property value has changed. </param>
        /// <param name="navigation"> The navigation property. </param>
        /// <param name="added"> The entities added to the collection. </param>
        /// <param name="removed"> The entities removed from the collection. </param>
        public CollectionChangedEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] EntityEntry entityEntry,
            [NotNull] INavigation navigation,
            [NotNull] IEnumerable<object> added,
            [NotNull] IEnumerable<object> removed)
            : base(eventDefinition, messageGenerator, navigation)
        {
            Check.NotNull(entityEntry, nameof(entityEntry));
            Check.NotNull(added, nameof(added));
            Check.NotNull(removed, nameof(removed));

            EntityEntry = entityEntry;
            Added = added;
            Removed = removed;
        }

        /// <summary>
        ///     The entry for the entity instance on which the navigation property has been added
        ///     to or removed from.
        /// </summary>
        public virtual EntityEntry EntityEntry { get; }

        /// <summary>
        ///     The entities added to the collection.
        /// </summary>
        public virtual IEnumerable<object> Added { get; }

        /// <summary>
        ///     The entities removed from the collection.
        /// </summary>
        public virtual IEnumerable<object> Removed { get; }
    }
}
