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
    ///     an entity is being deleted because its required relationship to a parent has been severed.
    /// </summary>
    public class CascadeDeleteOrphanEventData : EntityEntryEventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="entityEntry"> The entity entry for the entity that is being deleted. </param>
        /// <param name="parentEntityTypes"> The entity type to which the relationship was severed. </param>
        /// <param name="state"> The state that the child is transitioning to--usually 'Deleted'. </param>
        public CascadeDeleteOrphanEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] EntityEntry entityEntry,
            [NotNull] IEntityType parentEntityTypes,
            EntityState state)
            : base(eventDefinition, messageGenerator, entityEntry)
        {
            Check.NotNull(parentEntityTypes, nameof(parentEntityTypes));

            ParentEntityType = parentEntityTypes;
            State = state;
        }

        /// <summary>
        ///     The state that the child is transitioning to--usually 'Deleted'.
        /// </summary>
        public virtual EntityState State { get; }

        /// <summary>
        ///     The entity type to which the relationship was severed.
        /// </summary>
        public virtual IEntityType ParentEntityType { get; }
    }
}
