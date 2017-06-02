// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for error events that reference
    ///     a <see cref="DbContext" />.
    /// </summary>
    public class DbContextErrorEventData : DbContextEventData, IErrorEventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="context"> The current <see cref="DbContext" />. </param>
        /// <param name="exception"> The exception that triggered this event. </param>
        public DbContextErrorEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] DbContext context,
            [NotNull] Exception exception)
            : base(eventDefinition, messageGenerator, context)
        {
            Exception = exception;
        }

        /// <summary>
        ///     The exception that triggered this event.
        /// </summary>
        public virtual Exception Exception { get; }
    }
}
