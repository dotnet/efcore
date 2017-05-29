// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for events that have
    ///     an associated <see cref="Migrations.Migration" /> and file name.
    /// </summary>
    public class MigrationFileNameEventData : EventDataBase
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="migration"> The <see cref="Migration" />. </param>
        /// <param name="fileName"> The file name. </param>
        public MigrationFileNameEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventDataBase, string> messageGenerator,
            [NotNull] Migration migration,
            [NotNull] string fileName)
            : base(eventDefinition, messageGenerator)
        {
            Migration = migration;
            FileName = fileName;
        }

        /// <summary>
        ///     The <see cref="Migration" />.
        /// </summary>
        public virtual Migration Migration { get; }

        /// <summary>
        ///     The file name.
        /// </summary>
        public virtual string FileName { get; }
    }
}