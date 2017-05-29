// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for events that have
    ///     an associated snapshot name.
    /// </summary>
    public class SnapshotNameEventData : EventDataBase
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="snapshotName"> The snapshot name. </param>
        public SnapshotNameEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventDataBase, string> messageGenerator,
            [NotNull] string snapshotName)
            : base(eventDefinition, messageGenerator)
        {
            SnapshotName = snapshotName;
        }

        /// <summary>
        ///     The snapshot name.
        /// </summary>
        public virtual string SnapshotName { get; }
    }
}
