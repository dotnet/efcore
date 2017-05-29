// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Migrations.Design;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for events that have
    ///     an associated <see cref="Migrations.Design.ScaffoldedMigration" /> and file name.
    /// </summary>
    public class ScaffoldedMigrationEventData : EventDataBase
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="scaffoldedMigration"> The <see cref="Migrations.Design.ScaffoldedMigration" />. </param>
        /// <param name="fileName"> The file name. </param>
        public ScaffoldedMigrationEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventDataBase, string> messageGenerator,
            [NotNull] ScaffoldedMigration scaffoldedMigration,
            [NotNull] string fileName)
            : base(eventDefinition, messageGenerator)
        {
            ScaffoldedMigration = scaffoldedMigration;
            FileName = fileName;
        }

        /// <summary>
        ///     The <see cref="Migrations.Design.ScaffoldedMigration" />.
        /// </summary>
        public virtual ScaffoldedMigration ScaffoldedMigration { get; }

        /// <summary>
        ///     The file name.
        /// </summary>
        public virtual string FileName { get; }
    }
}