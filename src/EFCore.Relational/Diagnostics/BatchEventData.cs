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
    ///     The <see cref="DiagnosticSource" /> event payload for
    ///     <see cref="RelationalEventId" /> batch events.
    /// </summary>
    public class BatchEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="entries"> The entries being updated. </param>
        /// <param name="commandCount"> The command count. </param>
        public BatchEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] IEnumerable<IUpdateEntry> entries,
            int commandCount)
            : base(eventDefinition, messageGenerator)
        {
            Entries = entries;
            CommandCount = commandCount;
        }

        /// <summary>
        ///     The entries being updated.
        /// </summary>
        public virtual IEnumerable<IUpdateEntry> Entries { get; }

        /// <summary>
        ///     The command count.
        /// </summary>
        public virtual int CommandCount { get; }
    }
}
