// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for events that have
    ///     an associated <see cref="ModelSnapshot" /> and file name.
    /// </summary>
    public class ModelSnapshotFileNameEventData : EventDataBase
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="snapshot"> The <see cref="ModelSnapshot" />. </param>
        /// <param name="fileName"> The file name. </param>
        public ModelSnapshotFileNameEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventDataBase, string> messageGenerator,
            [NotNull] ModelSnapshot snapshot,
            [NotNull] string fileName)
            : base(eventDefinition, messageGenerator)
        {
            Snapshot = snapshot;
            FileName = fileName;
        }

        /// <summary>
        ///     The <see cref="ModelSnapshot" />.
        /// </summary>
        public virtual ModelSnapshot Snapshot { get; }

        /// <summary>
        ///     The file name.
        /// </summary>
        public virtual string FileName { get; }
    }
}