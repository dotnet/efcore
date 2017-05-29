// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for events that have
    ///     associated <see cref="MigrationOperation" />s.
    /// </summary>
    public class MigrationOperationsEventData : EventDataBase
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="operations"> The operations. </param>
        public MigrationOperationsEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventDataBase, string> messageGenerator,
            [NotNull] IEnumerable<MigrationOperation> operations)
            : base(eventDefinition, messageGenerator)
        {
            Operations = operations;
        }

        /// <summary>
        ///     The operations.
        /// </summary>
        public virtual IEnumerable<MigrationOperation> Operations { get; }
    }
}