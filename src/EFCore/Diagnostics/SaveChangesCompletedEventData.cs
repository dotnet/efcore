// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for events that indicate
    ///     <see cref="DbContext.SaveChanges()" /> has completed.
    /// </summary>
    public class SaveChangesCompletedEventData : DbContextEventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="context"> The current <see cref="DbContext" />. </param>
        /// <param name="entitiesSavedCount"> The number of entities saved to the database. </param>
        public SaveChangesCompletedEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] DbContext context,
            int entitiesSavedCount)
            : base(eventDefinition, messageGenerator, context)
        {
            EntitiesSavedCount = entitiesSavedCount;
        }

        /// <summary>
        ///     The number of entities saved to the database.
        /// </summary>
        public virtual int EntitiesSavedCount { get; }
    }
}
