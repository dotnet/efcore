// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for events that
    ///     specify the entities being saved and the rows affected.
    /// </summary>
    public class SaveChangesEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="entries"> Entries for the entities being saved. </param>
        /// <param name="rowsAffected"> The rows affected. </param>
        public SaveChangesEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] IEnumerable<IUpdateEntry> entries,
            int rowsAffected)
            : base(eventDefinition, messageGenerator)
        {
            Entries = entries;
            RowsAffected = rowsAffected;
        }

        /// <summary>
        ///     Entries for the entities being saved.
        /// </summary>
        public virtual IEnumerable<IUpdateEntry> Entries { get; }

        /// <summary>
        ///     The rows affected.
        /// </summary>
        public virtual int RowsAffected { get; }
    }
}
