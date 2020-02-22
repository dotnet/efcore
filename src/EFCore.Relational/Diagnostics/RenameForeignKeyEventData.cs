// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     The <see cref="DiagnosticSource" /> event payload for
    ///     <see cref="RelationalEventId" /> rename foreign key events.
    /// </summary>
    public class RenameForeignKeyEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="renameForeignKeyOperation"> The <see cref="RenameForeignKeyOperation"/> causing the event. </param>
        public RenameForeignKeyEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData,string> messageGenerator,
            [NotNull] RenameForeignKeyOperation renameForeignKeyOperation
            )
            : base(eventDefinition, messageGenerator)
        {
            RenameForeignKeyOperation = renameForeignKeyOperation;
        }

        /// <summary>
        ///     The migration type.
        /// </summary>
        public virtual RenameForeignKeyOperation RenameForeignKeyOperation { get; }

    }
}
