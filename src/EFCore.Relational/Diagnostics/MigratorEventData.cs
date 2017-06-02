// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     The <see cref="DiagnosticSource" /> event payload for
    ///     <see cref="RelationalEventId" /> migration events.
    /// </summary>
    public class MigratorEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="migrator">
        ///     The <see cref="IMigrator" /> in use.
        /// </param>
        public MigratorEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] IMigrator migrator)
            : base(eventDefinition, messageGenerator)
        {
            Migrator = migrator;
        }

        /// <summary>
        ///     The <see cref="IMigrator" /> in use.
        /// </summary>
        public virtual IMigrator Migrator { get; }
    }
}
