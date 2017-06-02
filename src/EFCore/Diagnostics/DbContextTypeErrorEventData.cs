// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for error events that reference
    ///     a <see cref="DbContext" /> type.
    /// </summary>
    public class DbContextTypeErrorEventData : DbContextTypeEventData, IErrorEventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="contextType"> The type of the current <see cref="DbContext" />. </param>
        /// <param name="exception"> The exception that triggered this event. </param>
        public DbContextTypeErrorEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] Type contextType,
            [NotNull] Exception exception)
            : base(eventDefinition, messageGenerator, contextType)
        {
            Exception = exception;
        }

        /// <summary>
        ///     The exception that triggered this event.
        /// </summary>
        public virtual Exception Exception { get; }
    }
}
