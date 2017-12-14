// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     The <see cref="DiagnosticSource" /> event payload for
    ///     <see cref="CoreEventId" /> execution strategy events.
    /// </summary>
    public class ExecutionStrategyEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="exceptionsEncountered">
        ///     The exceptions that have been caught during the execution of an operation.
        /// </param>
        /// <param name="delay"> The delay before retrying the operation. </param>
        /// <param name="async">
        ///     Indicates whether or not the command was executed asynchronously.
        /// </param>
        public ExecutionStrategyEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] IReadOnlyList<Exception> exceptionsEncountered,
            TimeSpan delay,
            bool async)
            : base(eventDefinition, messageGenerator)
        {
            ExceptionsEncountered = exceptionsEncountered;
            Delay = delay;
            IsAsync = async;
        }

        /// <summary>
        ///     A correlation ID that identifies the <see cref="DbConnection" /> instance being used.
        /// </summary>
        public virtual IReadOnlyList<Exception> ExceptionsEncountered { get; }

        /// <summary>
        ///     The delay before retrying the operation.
        /// </summary>
        public virtual TimeSpan Delay { get; }

        /// <summary>
        ///     Indicates whether or not the operation is being executed asynchronously.
        /// </summary>
        public virtual bool IsAsync { get; }
    }
}
