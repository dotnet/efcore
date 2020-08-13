// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for events that reference
    ///     a <see cref="DbContext" />.
    /// </summary>
    public class DbContextEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="context"> The current <see cref="DbContext" />, or null if not known. </param>
        public DbContextEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [CanBeNull] DbContext context)
            : base(eventDefinition, messageGenerator)
        {
            Context = context;
        }

        /// <summary>
        ///     The current <see cref="DbContext" />.
        /// </summary>
        public virtual DbContext Context { get; }
    }
}
